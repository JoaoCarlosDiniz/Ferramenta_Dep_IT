using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ferramenta_IT
{
    public partial class FormDesinstalar : Form
    {
        private List<WingetApp> upgradableApps;

        public FormDesinstalar(bool Estilo)
        {
            InitializeComponent();
            if (Estilo)
            {
                this.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Label || ctrl is Button || ctrl is CheckedListBox)
                    {
                        ctrl.ForeColor = System.Drawing.Color.WhiteSmoke;
                        if (ctrl is CheckedListBox clb)
                        {
                            clb.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
                        }
                    }
                }
                buttonCancel.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                buttonUpdate.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
                buttonCancel.ForeColor = System.Drawing.Color.WhiteSmoke;
                buttonUpdate.ForeColor = System.Drawing.Color.WhiteSmoke;
            }
            else
            {
                this.BackColor = SystemColors.Control;
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Label || ctrl is Button || ctrl is CheckedListBox)
                    {
                        ctrl.ForeColor = System.Drawing.Color.Black;
                        if (ctrl is CheckedListBox clb)
                        {
                            clb.BackColor = SystemColors.Control;
                        }
                    }
                }
                buttonCancel.BackColor = SystemColors.Control;
                buttonUpdate.BackColor = SystemColors.Control;
                buttonCancel.ForeColor = System.Drawing.Color.Black;
                buttonUpdate.ForeColor = System.Drawing.Color.Black;
            }
        }

        private async void FormActualizar_LoadAsync(object sender, EventArgs e)
        {
            buttonUpdate.Enabled = false;
            upgradableApps = await GetInstalledAppsAsync();

            if (upgradableApps.Any())
            {
                labelStatus.Text = "Nenhuma aplicação encontrada.";
                checkedListBoxApps.Items.AddRange(upgradableApps.ToArray());
                buttonUpdate.Enabled = true;
            }
            else
            {
                labelStatus.Text = "Nenhuma atualização encontrada.";
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
