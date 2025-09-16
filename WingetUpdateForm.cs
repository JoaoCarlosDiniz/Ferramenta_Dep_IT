using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IT
{
    public class WingetUpdateForm : DevExpress.XtraEditors.XtraForm
    {
        private CheckedListBox checkedListBoxApps;
        private Label labelStatus;
        private DevExpress.XtraEditors.SimpleButton buttonUpdate;
        private DevExpress.XtraEditors.SimpleButton buttonCancel;
        private List<WingetApp> upgradableApps;

        public WingetUpdateForm()
        {
            InitializeComponent();
            this.Load += WingetUpdateForm_Load;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WingetUpdateForm));
            checkedListBoxApps = new CheckedListBox();
            labelStatus = new Label();
            buttonUpdate = new DevExpress.XtraEditors.SimpleButton();
            buttonCancel = new DevExpress.XtraEditors.SimpleButton();
            SuspendLayout();
            // 
            // checkedListBoxApps
            // 
            checkedListBoxApps.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            checkedListBoxApps.FormattingEnabled = true;
            checkedListBoxApps.Location = new Point(12, 35);
            checkedListBoxApps.Name = "checkedListBoxApps";
            checkedListBoxApps.Size = new Size(774, 166);
            checkedListBoxApps.TabIndex = 0;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(12, 9);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(148, 15);
            labelStatus.TabIndex = 3;
            labelStatus.Text = "A procurar atualizações...";
            // 
            // buttonUpdate
            // 
            buttonUpdate.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("buttonUpdate.ImageOptions.SvgImage");
            buttonUpdate.Location = new Point(692, 310);
            buttonUpdate.Name = "buttonUpdate";
            buttonUpdate.Size = new Size(44, 44);
            buttonUpdate.TabIndex = 4;
            buttonUpdate.Click += buttonUpdate_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("buttonCancel.ImageOptions.SvgImage");
            buttonCancel.Location = new Point(742, 310);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(44, 44);
            buttonCancel.TabIndex = 5;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // WingetUpdateForm
            // 
            Appearance.Options.UseFont = true;
            ClientSize = new Size(798, 356);
            Controls.Add(buttonCancel);
            Controls.Add(buttonUpdate);
            Controls.Add(labelStatus);
            Controls.Add(checkedListBoxApps);
            Font = new Font("Calibri", 9.75F);
            IconOptions.Image = (Image)resources.GetObject("WingetUpdateForm.IconOptions.Image");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WingetUpdateForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Atualizar Aplicativos com Winget";
            ResumeLayout(false);
            PerformLayout();
        }

        private async void WingetUpdateForm_Load(object sender, EventArgs e)
        {
            buttonUpdate.Enabled = false;
            upgradableApps = await GetUpgradableAppsAsync();

            if (upgradableApps.Any())
            {
                labelStatus.Text = "Selecione os aplicativos para atualizar:";
                checkedListBoxApps.Items.AddRange(upgradableApps.ToArray());
                buttonUpdate.Enabled = true;
            }
            else
            {
                labelStatus.Text = "Nenhuma atualização encontrada.";
            }
        }

        private Task<List<WingetApp>> GetUpgradableAppsAsync()
        {
            return Task.Run(() =>
            {
                var apps = new List<WingetApp>();
                string rawOutput = "";
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = "upgrade --location PT --accept-source-agreements",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                using (var process = Process.Start(processStartInfo))
                {
                    rawOutput = process.StandardOutput.ReadToEnd();
                    rawOutput += process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    var lines = rawOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    bool headerFound = false;
                    
                    // Regex to parse the line. It looks for at least two spaces between columns.
                    // It captures: 1. Name, 2. ID, 3. Current Version, 4. Available Version
                    var appRegex = new Regex(@"^(.*?)\s{2,}(.*?)\s{2,}(.*?)\s{2,}(.*?)\s{2,}", RegexOptions.Compiled);

                    foreach (var line in lines)
                    {
                        if (!headerFound && (line.Contains("Name") || line.Contains("Nome")) && (line.Contains("Id") || line.Contains("ID")))
                        {
                            headerFound = true;
                            continue;
                        }

                        if (headerFound && !line.StartsWith("---") && !line.Contains("upgrades available") && !line.Contains("atualizações disponíveis"))
                        {
                            var match = appRegex.Match(line);
                            if (match.Success)
                            {
                                string name = match.Groups[1].Value.Trim();
                                string id = match.Groups[2].Value.Trim();
                                string currentVersion = match.Groups[3].Value.Trim();
                                string availableVersion = match.Groups[4].Value.Trim().Split(' ')[0]; // Take first part of available version

                                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(id))
                                {
                                    apps.Add(new WingetApp
                                    {
                                        Name = name,
                                        Id = id,
                                        VersaoAtual = currentVersion,
                                        VersaoDisponivel = availableVersion
                                    });
                                }
                            }
                        }
                    }
                }

                if (!apps.Any() && !string.IsNullOrWhiteSpace(rawOutput))
                {
                    // Se nenhuma aplicação for encontrada, mostra a saída bruta para depuração.
                    MessageBox.Show("Nenhuma atualização encontrada. Saída do Winget:\n\n" + rawOutput, "Diagnóstico do Winget", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return apps;
            });
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            var selectedApps = checkedListBoxApps.CheckedItems.OfType<WingetApp>().ToList();
            if (!selectedApps.Any())
            {
                MessageBox.Show("Nenhum aplicativo foi selecionado para atualização.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Enabled = false;
            labelStatus.Text = "A atualizar aplicativos selecionados...";
            Application.DoEvents();

            foreach (var app in selectedApps)
            {
                labelStatus.Text = $"A atualizar: {app.Name}...";
                Application.DoEvents();
                RunCommandAsAdmin($"/c winget upgrade --id \"{app.Id}\" --location PT --accept-package-agreements --accept-source-agreements",
                                  $"'{app.Name}' atualizado com sucesso.",
                                  $"Falha ao atualizar '{app.Name}'.");
            }

            MessageBox.Show("Processo de atualização concluído.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void RunCommandAsAdmin(string command, string successMessage, string errorMessage)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", command)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"{errorMessage}\nCódigo de saída: {process.ExitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessage}\nDetalhes: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}