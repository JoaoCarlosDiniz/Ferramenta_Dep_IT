using System.Diagnostics;
using System.Text.RegularExpressions;

namespace IT
{
    public class WingetUninstallForm : DevExpress.XtraEditors.XtraForm
    {
        private CheckedListBox checkedListBoxApps;
        private Label labelStatus;
        private DevExpress.XtraEditors.SimpleButton buttonUninstall;
        private DevExpress.XtraEditors.SimpleButton buttonCancel;
        private List<WingetApp> installedApps;

        public WingetUninstallForm()
        {
            InitializeComponent();
            this.Load += WingetUninstallForm_Load;
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WingetUninstallForm));
            checkedListBoxApps = new CheckedListBox();
            labelStatus = new Label();
            buttonUninstall = new DevExpress.XtraEditors.SimpleButton();
            buttonCancel = new DevExpress.XtraEditors.SimpleButton();
            SuspendLayout();
            // 
            // checkedListBoxApps
            // 
            checkedListBoxApps.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            checkedListBoxApps.FormattingEnabled = true;
            checkedListBoxApps.Location = new Point(12, 35);
            checkedListBoxApps.Name = "checkedListBoxApps";
            checkedListBoxApps.Size = new Size(774, 238);
            checkedListBoxApps.TabIndex = 0;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(12, 9);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(200, 15);
            labelStatus.TabIndex = 3;
            labelStatus.Text = "A procurar aplicações instaladas...";
            // 
            // buttonUninstall
            // 
            buttonUninstall.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("buttonUninstall.ImageOptions.SvgImage");
            buttonUninstall.Location = new Point(692, 310);
            buttonUninstall.Name = "buttonUninstall";
            buttonUninstall.Size = new Size(44, 44);
            buttonUninstall.TabIndex = 4;
            buttonUninstall.Click += buttonUninstall_Click;
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
            // WingetUninstallForm
            // 
            Appearance.Options.UseFont = true;
            ClientSize = new Size(798, 356);
            Controls.Add(buttonCancel);
            Controls.Add(buttonUninstall);
            Controls.Add(labelStatus);
            Controls.Add(checkedListBoxApps);
            IconOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("WingetUninstallForm.IconOptions.SvgImage");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WingetUninstallForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Desinstalar Aplicações com Winget";
            ResumeLayout(false);
            PerformLayout();
        }

        private async void WingetUninstallForm_Load(object sender, EventArgs e)
        {
            buttonUninstall.Enabled = false;
            installedApps = await GetInstalledAppsAsync();

            if (installedApps.Any())
            {
                labelStatus.Text = "Selecione as aplicações para desinstalar:";
                checkedListBoxApps.Items.AddRange(installedApps.ToArray());
                buttonUninstall.Enabled = true;
            }
            else
            {
                labelStatus.Text = "Nenhuma aplicação encontrada.";
            }
        }

        private Task<List<WingetApp>> GetInstalledAppsAsync()
        {
            return Task.Run(() =>
            {
                var apps = new List<WingetApp>();
                string rawOutput = "";
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = "list --accept-source-agreements",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8
                };

                using (var process = Process.Start(processStartInfo))
                {
                    rawOutput = process.StandardOutput.ReadToEnd();
                    string errorOutput = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    var lines = rawOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    bool headerFound = false;
                    
                    var appRegex = new Regex(@"^(.*?)\s{2,}(.*?)\s{2,}(.*?)\s{2,}", RegexOptions.Compiled);

                    foreach (var line in lines)
                    {
                        if (!headerFound && (line.Contains("Name") || line.Contains("Nome")) && (line.Contains("Id") || line.Contains("ID")))
                        {
                            headerFound = true;
                            continue;
                        }

                        if (headerFound && !line.StartsWith("---"))
                        {
                            var match = appRegex.Match(line);
                            if (match.Success)
                            {
                                string name = match.Groups[1].Value.Trim();
                                string id = match.Groups[2].Value.Trim();
                                string version = match.Groups[3].Value.Trim();

                                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(id))
                                {
                                    apps.Add(new WingetApp
                                    {
                                        Name = name,
                                        Id = id,
                                        VersaoAtual = version,
                                        VersaoDisponivel = null
                                    });
                                }
                            }
                        }
                    }
                }

                return apps;
            });
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            var selectedApps = checkedListBoxApps.CheckedItems.OfType<WingetApp>().ToList();
            if (!selectedApps.Any())
            {
                MessageBox.Show("Nenhuma aplicação foi selecionada para desinstalação.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Tem a certeza que pretende desinstalar as {selectedApps.Count} aplicações selecionadas?", "Confirmar Desinstalação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            this.Enabled = false;
            labelStatus.Text = "A desinstalar aplicações selecionadas...";
            Application.DoEvents();

            foreach (var app in selectedApps)
            {
                labelStatus.Text = $"A desinstalar: {app.Name}...";
                Application.DoEvents();
                RunCommandAsAdmin($"/c winget uninstall --id \"{app.Id}\" --accept-source-agreements --accept-package-agreements",
                                  $"'{app.Name}' desinstalado com sucesso.",
                                  $"Falha ao desinstalar '{app.Name}'.");
            }

            MessageBox.Show("Processo de desinstalação concluído.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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