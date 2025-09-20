using DevExpress.XtraBars.Ribbon;
using Microsoft.Win32;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Compression;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;

namespace IT
{
    public partial class MainForm : RibbonForm
    {
        #region Variaveis
        private NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        private string Admin = ConfigurationManager.AppSettings["Admin"];
        private string SenhaAdmin = ConfigurationManager.AppSettings["SenhaAdmin"];
        private string NomeDominio = ConfigurationManager.AppSettings["NomeDominio"];
        private string Administrador = ConfigurationManager.AppSettings["Administrador"];
        private string SenhaAdministrador = ConfigurationManager.AppSettings["SenhaAdministrador"];

        private string LstSrvDC = ConfigurationManager.AppSettings["ListServerDC"];
        private List<string> ListServerDC = new List<string>();

        private string LstSrvGateway = ConfigurationManager.AppSettings["ServidorGateway"];
        private List<string> ServidorGateway = new List<string>();

        private string Estilo;

        private double TotalRAM = 0;
        private int RAMUso = 0;

        private double TotalDisco = 0;
        private int DiscoUso = 0;
        #endregion

        public MainForm()
        {
            InitializeComponent();

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize"))
                {
                    if (key != null)
                    {
                        object registryValue = key.GetValue("AppsUseLightTheme");
                        if (registryValue != null && (int)registryValue == 0)
                        {
                            ckEstilo.Checked = true;
                        }
                        else
                        {
                            ckEstilo.Checked = false;
                        }
                    }
                    else
                    {
                        ckEstilo.Checked = false;
                    }
                }
            }
            catch (Exception)
            {
                ckEstilo.Checked = false;
            }


            ListServerDC = !string.IsNullOrEmpty(LstSrvDC)
                ? LstSrvDC.Split(';').Select(s => s.Trim()).ToList()
                : new List<string>();

            ServidorGateway = !string.IsNullOrEmpty(LstSrvGateway)
                ? LstSrvGateway.Split(';').Select(s => s.Trim()).ToList()
                : new List<string>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Actualizar_DashBoard();
        }

        private void btDashBoardRefresh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Actualizar_DashBoard();
        }

        private void btActualiza_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Actualizar_DashBoard();
        }

        #region Domínio
        private void btDominio_Teste_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                if (!string.IsNullOrEmpty(domainName) && domainName.Contains("."))
                {
                    Domain domain = Domain.GetCurrentDomain();
                    DomainController dc = domain.FindDomainController();

                    MessageBox.Show($"O computador pertence ao domínio: {domain.Name}\nServidor de AD: {dc.Name}", "Teste de Domínio", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    dc.Dispose();
                    domain.Dispose();
                }
                else
                {
                    JoinDomain();
                }
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                JoinDomain();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar o domínio: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JoinDomain()
        {
            var result = MessageBox.Show("O computador não pertence a um domínio." + Environment.NewLine + "Deseja adicioná-lo agora?", "Adicionar ao Domínio", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                string selectedDC = ShowSelectDialog("Selecionar Servidor de DC", "Escolha o servidor de DC:", ListServerDC);
                if (string.IsNullOrEmpty(selectedDC))
                {
                    return; // User cancelled
                }

                try
                {
                    ManagementObject computerSystem = new ManagementObject("Win32_ComputerSystem.Name='" + Environment.MachineName + "'");
                    ManagementBaseObject inParams = computerSystem.GetMethodParameters("JoinDomainOrWorkgroup");
                    inParams["Name"] = NomeDominio;
                    inParams["Password"] = SenhaAdministrador;
                    inParams["UserName"] = $"{NomeDominio}\\{Administrador}";
                    inParams["DomainControllerName"] = selectedDC;
                    inParams["FJoinOptions"] = 1; // 1 (Join Domain) + 2 (Create Account) = 3

                    ManagementBaseObject outParams = computerSystem.InvokeMethod("JoinDomainOrWorkgroup", inParams, null);
                    uint returnValue = (uint)outParams["ReturnValue"];

                    if (returnValue == 0)
                    {
                        MessageBox.Show("O computador foi adicionado ao domínio com sucesso." + Environment.NewLine + "É necessário reiniciar para aplicar as alterações.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Falha ao adicionar o computador ao domínio." + Environment.NewLine + "Código de erro: {returnValue}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao tentar adicionar o computador ao domínio: {ex.Message}\n\nCertifique-se de que o programa está a ser executado como administrador.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btFixTime_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string selectedServer = ShowSelectDialog("Selecionar Servidor de Tempo", "Escolha o servidor de tempo (NTP):", ServidorGateway);
            if (string.IsNullOrEmpty(selectedServer))
            {
                return; // User cancelled
            }

            if (MessageBox.Show($"Tem a certeza que pretende sincronizar o tempo com o servidor {selectedServer}?", "Sincronização de Tempo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    MessageBox.Show("O serviço de tempo será reiniciado e configurado." + Environment.NewLine + "Este processo pode exigir privilégios de administrador.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Parar o serviço de tempo
                    await RunCommandAsAdmin("/c net stop w32time", "Serviço de tempo parado com sucesso.", "Falha ao parar o serviço de tempo.");

                    // Configurar o servidor de tempo
                    string configCommand = $"/c w32tm /config /manualpeerlist:\"{selectedServer}\" /syncfromflags:manual /reliable:yes /update";
                    await RunCommandAsAdmin(configCommand, "Configuração de servidor de tempo atualizada.", "Falha ao configurar o servidor de tempo.");

                    // Iniciar o serviço de tempo
                    await RunCommandAsAdmin("/c net start w32time", "Serviço de tempo iniciado com sucesso.", "Falha ao iniciar o serviço de tempo.");

                    // Forçar a ressincronização
                    await RunCommandAsAdmin("/c w32tm /resync /force", "Sincronização de tempo forçada.", "Falha ao forçar a sincronização.");

                    // Consultar e exibir o status
                    string status = await RunCommandAndGetOutput("w32tm /query /status");
                    string source = await RunCommandAndGetOutput("w32tm /query /source");

                    MessageBox.Show($"Sincronização concluída.\n\nStatus:\n{status}\nFonte:\n{source}", "Resultado da Sincronização", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro durante o processo de sincronização: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Windows
        private void btWindows_Teste_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<string> options = new List<string> { "Limpeza de arquivos temporários", "Verificação de Discos" };
            string selectedOption = ShowSelectDialog("Testes do Windows", "Escolha o teste a executar:", options);

            if (!string.IsNullOrEmpty(selectedOption))
            {
                if (selectedOption == "Limpeza de arquivos temporários")
                {
                    CleanTempFiles();
                }
                else if (selectedOption == "Verificação de Discos")
                {
                    CheckDisks();
                }
            }
        }

        private void CleanTempFiles()
        {
            if (MessageBox.Show("Tem a certeza que pretende limpar os ficheiros temporários?", "Limpeza de Ficheiros Temporários", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    string tempPath = Path.GetTempPath();
                    int filesDeleted = 0;
                    int foldersDeleted = 0;
                    long totalSize = 0;

                    System.IO.DirectoryInfo di = new DirectoryInfo(tempPath);

                    foreach (FileInfo file in di.GetFiles())
                    {
                        try
                        {
                            totalSize += file.Length;
                            file.Delete();
                            filesDeleted++;
                        }
                        catch { }
                    }

                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        try
                        {
                            totalSize += GetDirectorySize(dir);
                            dir.Delete(true);
                            foldersDeleted++;
                        }
                        catch { }
                    }

                    MessageBox.Show($"Limpeza concluída!\n\nFicheiros apagados: {filesDeleted}\nPastas apagadas: {foldersDeleted}\nEspaço libertado: {totalSize / (1024 * 1024)} MB", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro durante a limpeza: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            FileInfo[] fis = dir.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = dir.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }
            return size;
        }

        private void CheckDisks()
        {
            try
            {
                StringBuilder diskInfo = new StringBuilder();
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                List<string> fixedDrives = new List<string>();
                bool cDriveFound = false;

                foreach (DriveInfo d in allDrives)
                {
                    diskInfo.AppendLine($"Disco {d.Name}");
                    if (d.IsReady)
                    {
                        diskInfo.AppendLine($"  Tipo de disco: {d.DriveType}");
                        if (d.DriveType == DriveType.Fixed)
                        {
                            fixedDrives.Add(d.Name);
                            if (d.Name.StartsWith("C:", StringComparison.OrdinalIgnoreCase))
                            {
                                cDriveFound = true;
                            }
                        }
                        diskInfo.AppendLine($"  Nome do volume: {d.VolumeLabel}");
                        diskInfo.AppendLine($"  Sistema de ficheiros: {d.DriveFormat}");
                        diskInfo.AppendLine($"  Espaço total: {d.TotalSize / (1024 * 1024 * 1024)} GB");
                        diskInfo.AppendLine($"  Espaço livre: {d.AvailableFreeSpace / (1024 * 1024 * 1024)} GB");
                    }
                    else
                    {
                        diskInfo.AppendLine("  O disco não está pronto.");
                    }
                    diskInfo.AppendLine();
                }

                MessageBox.Show(diskInfo.ToString(), "Informação dos Discos", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (cDriveFound)
                {
                    var chkdskResult = MessageBox.Show("Deseja executar o CHKDSK na drive C:?", "Verificação de Disco (CHKDSK)", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (chkdskResult == DialogResult.Yes)
                    {
                        RunChkdsk();
                    }
                }

                if (fixedDrives.Count > 0)
                {
                    var result = MessageBox.Show("Deseja executar a verificação de ficheiros de sistema (sfc /scannow) nos discos fixos encontrados?", "Verificação de Ficheiros de Sistema", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        RunSfcScannow();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao verificar os discos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunChkdsk()
        {
            try
            {
                MessageBox.Show("A verificação de disco (chkdsk C: /f) será agendada para a próxima reinicialização." + Environment.NewLine + "Este processo pode exigir privilégios de administrador.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c echo S | chkdsk C: /f")
                {
                    Verb = "runas", // Request administrator privileges
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                MessageBox.Show("O CHKDSK foi agendado para a próxima reinicialização.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao agendar o chkdsk: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RunSfcScannow()
        {
            try
            {
                MessageBox.Show("A verificação de ficheiros de sistema (sfc /scannow) será iniciada." + Environment.NewLine + "Este processo pode demorar algum tempo e pode exigir privilégios de administrador." + Environment.NewLine + "Por favor, aguarde.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c sfc /scannow")
                {
                    Verb = "runas", // Request administrator privileges
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                MessageBox.Show("A verificação de ficheiros de sistema foi concluída.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao executar sfc /scannow: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btResolveRDC_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Esta operação irá modificar o registo do Windows para corrigir uma vulnerabilidade de segurança do RDC (CredSSP)." + Environment.NewLine + "Deseja continuar?", "Resolver Vulnerabilidade RDC", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string command = "/c REG ADD HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\\CredSSP\\Parameters /v AllowEncryptionOracle /t REG_DWORD /d 2 /f";
                string successMessage = "A correção para a vulnerabilidade de segurança do RDC foi aplicada com sucesso.";
                string errorMessage = "Ocorreu um erro ao aplicar a correção para o RDC.";

                await RunCommandAsAdmin(command, successMessage, errorMessage);
            }
        }

        private void btCheckRAM_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                MessageBox.Show("A ferramenta de Diagnóstico de Memória do Windows será iniciada." + Environment.NewLine + "Este processo pode exigir privilégios de administrador.", "Diagnóstico de Memória", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                ProcessStartInfo psi = new ProcessStartInfo("mdsched.exe")
                {
                    Verb = "runas", // Solicita privilégios de administrador
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process.Start(psi);
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // A operação foi cancelada pelo utilizador
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao tentar iniciar o Diagnóstico de Memória: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btBackupDRV_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.Filter = "ZIP file (*.zip)|*.zip";
                saveFileDialog.Title = "Guardar Backup de Drivers";
                saveFileDialog.FileName = $"Backup_Drivers_{Environment.MachineName}_{DateTime.Now:yyyyMMdd}.zip";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return; // Utilizador cancelou
                }

                string tempExportPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempExportPath);

                try
                {
                    MessageBox.Show("A exportar os drivers do sistema. Este processo pode demorar alguns minutos e requer privilégios de administrador.", "Backup de Drivers", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    string pnputilPath = "pnputil.exe";
                    // Se a aplicação for 32-bit a correr num SO 64-bit, usar o caminho Sysnative para evitar o redirecionamento de ficheiros.
                    if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
                    {
                        pnputilPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Sysnative", "pnputil.exe");
                    }

                    string command = $"{pnputilPath} /export-driver * \"{tempExportPath}\"";
                    string output = await RunCommandAndGetOutput(command);

                    if (output.StartsWith("Comando falhou"))
                    {
                        MessageBox.Show(output, "Erro ao Exportar Drivers", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Criar o ficheiro ZIP
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                    ZipFile.CreateFromDirectory(tempExportPath, saveFileDialog.FileName);

                    MessageBox.Show($"Backup de drivers criado com sucesso em:\n{saveFileDialog.FileName}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro durante o backup dos drivers: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Limpar a pasta temporária
                    if (Directory.Exists(tempExportPath))
                    {
                        Directory.Delete(tempExportPath, true);
                    }
                }
            }
        }

        private async void btCriarPontoRestauro_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende criar um ponto de restauro do sistema?" + Environment.NewLine + "Esta ação pode demorar alguns minutos e requer privilégios de administrador.", "Criar Ponto de Restauro", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string command = "/c powershell -command \"Checkpoint-Computer -Description 'Ponto_Restauracao_TI' -RestorePointType 'MODIFY_SETTINGS'\"";
                string successMessage = "O ponto de restauro 'Ponto_Restauracao_TI' foi criado com sucesso.";
                string errorMessage = "Ocorreu um erro ao criar o ponto de restauro." + Environment.NewLine + "Verifique se a Proteção do Sistema está ativada para a drive C:.";

                await RunCommandAsAdmin(command, successMessage, errorMessage);
            }
        }

        private void btManutencaoWIN_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Esta operação irá executar uma sequência de comandos de manutenção:" + Environment.NewLine + "1. sfc /scannow" + Environment.NewLine + "2. DISM /Online /Cleanup-Image /RestoreHealth" + Environment.NewLine + Environment.NewLine + "Este processo pode demorar bastante tempo, requer uma ligação à internet e privilégios de administrador." + Environment.NewLine + "Deseja continuar?", "Manutenção Completa do Windows", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // 1. Executar sfc /scannow
                    RunSfcScannow();

                    // 2. Executar DISM
                    MessageBox.Show("A verificação SFC foi concluída. A seguir, a ferramenta DISM será iniciada numa nova janela." + Environment.NewLine + "Por favor, aguarde a conclusão do processo.", "Manutenção do Windows (DISM)", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c DISM /Online /Cleanup-Image /RestoreHealth")
                    {
                        Verb = "runas", // Solicita privilégios de administrador
                        UseShellExecute = true,
                        CreateNoWindow = false // Mostra a janela da consola para o utilizador ver o progresso
                    };

                    Process process = Process.Start(psi);
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show("A operação DISM foi concluída com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"A operação DISM terminou com um código de erro: {process.ExitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // A operação foi cancelada pelo utilizador
                {
                    MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro durante a manutenção: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Utilizador
        private void btUtilizador_Teste_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UpdateAppsWithWinget();
        }

        private void UpdateAppsWithWinget()
        {
            try
            {
                using (var updateForm = new WingetUpdateForm())
                {
                    updateForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao iniciar o processo de atualização: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btUser_Desisntalar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            UninstallAppsWithWinget();
        }

        private void UninstallAppsWithWinget()
        {
            try
            {
                // É necessário criar um novo formulário 'WingetUninstallForm'
                // com a lógica para listar e desinstalar aplicações via Winget.
                using (var uninstallForm = new WingetUninstallForm())
                {
                    uninstallForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao iniciar o processo de desinstalação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btResetPool_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende reiniciar o serviço de impressão e limpar a fila de impressão?", "Reset Spooler de Impressão", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    MessageBox.Show("O serviço de impressão será reiniciado." + Environment.NewLine + "Este processo pode exigir privilégios de administrador.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    int exitCode;

                    // Parar o serviço de spooler de impressão
                    exitCode = await RunCommand("/c net stop spooler");
                    if (exitCode != 0)
                    {
                        MessageBox.Show($"Falha ao parar o serviço de impressão." + Environment.NewLine + "Código de erro: {exitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Tenta iniciar o serviço mesmo que a paragem tenha falhado, para garantir que não fica parado
                        await RunCommand("/c net start spooler");
                        return;
                    }

                    // Limpar a pasta de spool
                    exitCode = await RunCommand("/c del %systemroot%\\system32\\spool\\printers\\* /Q /F /S");
                    // O comando del pode retornar 1 se não encontrar ficheiros, o que não é um erro neste contexto.
                    if (exitCode != 0 && exitCode != 1)
                    {
                        MessageBox.Show($"Falha ao limpar a fila de impressão. Código de erro: {exitCode}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Mesmo que a limpeza falhe, o serviço deve ser reiniciado.
                    }

                    // Iniciar o serviço de spooler de impressão
                    exitCode = await RunCommand("/c net start spooler");
                    if (exitCode != 0)
                    {
                        MessageBox.Show($"Falha ao iniciar o serviço de impressão." + Environment.NewLine + "Código de erro: {exitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    MessageBox.Show("O serviço de impressão foi reiniciado e a fila de impressão foi limpa com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
                {
                    MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Tenta garantir que o serviço fica a correr se o utilizador cancelar a meio
                    await RunCommand("/c net start spooler");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao reiniciar o serviço de impressão: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Tenta garantir que o serviço fica a correr em caso de erro
                    await RunCommand("/c net start spooler");
                }
            }
        }
        #endregion

        #region Rede
        private void btRede_Teste_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<string> options = new List<string>
            {
                "Testar comunicação com DC",
                "Limpar cache de DNS",
                "Limpar cache ARP",
                "Limpar NetBIOS"
            };

            string selectedOption = ShowSelectDialog("Testes de Rede", "Escolha o teste a executar:", options);

            if (!string.IsNullOrEmpty(selectedOption))
            {
                switch (selectedOption)
                {
                    case "Testar comunicação com DC":
                        TestarComunicacaoDC();
                        break;
                    case "Limpar cache de DNS":
                        LimparCacheDNS();
                        break;
                    case "Limpar cache ARP":
                        LimparCacheARP();
                        break;
                    case "Limpar NetBIOS":
                        LimparNetBIOS();
                        break;
                }
            }
        }

        private void TestarComunicacaoDC()
        {
            string selectedDC = ShowSelectDialog("Selecionar Servidor de DC", "Escolha o servidor de DC para testar:", ListServerDC);
            if (string.IsNullOrEmpty(selectedDC))
            {
                return; // User cancelled
            }

            try
            {
                using (Ping pingSender = new Ping())
                {
                    PingReply reply = pingSender.Send(selectedDC);

                    if (reply.Status == IPStatus.Success)
                    {
                        MessageBox.Show($"Comunicação com {selectedDC} bem-sucedida.\n\nEndereço: {reply.Address}\nTempo de resposta: {reply.RoundtripTime}ms", "Teste de Comunicação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Falha na comunicação com {selectedDC}.\n\nStatus: {reply.Status}", "Teste de Comunicação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao tentar comunicar com {selectedDC}: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LimparCacheDNS()
        {
            await RunCommandAsAdmin("/c ipconfig /flushdns", "O cache de DNS foi limpo com sucesso.", "Ocorreu um erro ao limpar o cache de DNS.");
        }

        private async void LimparCacheARP()
        {
            await RunCommandAsAdmin("/c arp -d *", "O cache ARP foi limpo com sucesso.", "Ocorreu um erro ao limpar o cache ARP.");
        }

        private async void LimparNetBIOS()
        {
            // Códigos de sucesso para 'nbtstat -R'. 0 = sucesso, 1 = cache não precisava de ser limpo.
            int[] nbtstatSuccessCodes = new[] { 0, 1 };
            await RunCommandAsAdmin("/c nbtstat -R", "O cache NetBIOS foi limpo com sucesso ou não necessitava de limpeza.", "Ocorreu um erro ao limpar o cache de NetBIOS.", nbtstatSuccessCodes);
        }

        private void btCorreccaoAdmin_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<string> options = new List<string>
            {
                "Redefinir Senha da Conta do Computador",
                "Ativar Acesso Administrativo Remoto"
            };

            string selectedOption = ShowSelectDialog("Correções Administrativas", "Escolha a correção a aplicar:", options);

            if (string.IsNullOrEmpty(selectedOption))
            {
                return; // User cancelled
            }

            switch (selectedOption)
            {
                case "Redefinir Senha da Conta do Computador":
                    ResetComputerAccountPassword();
                    break;
                case "Ativar Acesso Administrativo Remoto":
                    EnableRemoteAdminAccess();
                    break;
            }
        }

        private async void ResetComputerAccountPassword()
        {
            if (string.IsNullOrEmpty(Admin) || string.IsNullOrEmpty(SenhaAdmin))
            {
                MessageBox.Show("O nome de utilizador e/ou a senha do administrador local não estão configurados no ficheiro App.config.", "Configuração em Falta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show($"Esta operação irá criar ou redefinir a senha do utilizador local '{Admin}'.\nDeseja continuar?", "Gerir Administrador Local", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                // Executa o comando para verificar a existência do utilizador.
                // O método RunCommand agora é usado para obter diretamente o código de saída.
                int exitCode = await RunCommand($"/c net user {Admin}");

                switch (exitCode)
                {
                    case 0: // Utilizador existe
                        // Redefine a senha
                        string resetPasswordCommand = $"/c net user {Admin} \"{SenhaAdmin}\"";
                        await RunCommandAsAdmin(resetPasswordCommand, $"A senha do utilizador '{Admin}' foi redefinida com sucesso.", $"Falha ao redefinir a senha do utilizador '{Admin}'.");
                        break;

                    case 2: // Utilizador não existe
                        // Cria o utilizador e adiciona-o ao grupo de Administradores
                        string createUserCommand = $"/c net user {Admin} /add";
                        bool userCreated = await RunCommandAsAdminInternal(createUserCommand, $"Falha ao criar o utilizador '{Admin}'.");
                        resetPasswordCommand = $"/c net user {Admin} \"{SenhaAdmin}\"";
                        await RunCommandAsAdmin(resetPasswordCommand, $"A senha do utilizador '{Admin}' foi redefinida com sucesso.", $"Falha ao redefinir a senha do utilizador '{Admin}'.");

                        if (userCreated)
                        {
                            string addToAdminGroupCommand = $"/c net localgroup Administradores {Admin} /add";
                            bool userAddedToGroup = await RunCommandAsAdminInternal(addToAdminGroupCommand, $"Falha ao adicionar '{Admin}' ao grupo de Administradores.");

                            if (userAddedToGroup)
                            {
                                MessageBox.Show($"Utilizador '{Admin}' criado e adicionado ao grupo de Administradores com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        break;

                    default: // Outro erro
                        MessageBox.Show($"Ocorreu um erro inesperado ao verificar o utilizador '{Admin}'.\nCódigo de saída: {exitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao gerir a conta de administrador local: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void EnableRemoteAdminAccess()
        {
            if (MessageBox.Show("Esta operação irá ativar serviços, regras de firewall e alterar o registo para permitir acesso administrativo remoto." + Environment.NewLine + "Deseja continuar?", "Ativar Acesso Administrativo Remoto", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Códigos de sucesso para o comando 'sc start'. 0 = sucesso, 1056 = serviço já em execução.
                int[] serviceStartSuccessCodes = new[] { 0, 1056 };
                // Códigos de sucesso para 'netsh'. 0 = sucesso, 1 = Nenhuma regra correspondeu (pode significar que já está OK).
                int[] netshSuccessCodes = new[] { 0, 1 };

                // Ativar serviços necessários
                await RunCommandAsAdmin("/c sc config lanmanserver start= auto", "Serviço 'Server' configurado para iniciar automaticamente.", "Falha ao configurar o serviço 'Server'.");
                await RunCommandAsAdmin("/c sc start lanmanserver", "Serviço 'Server' iniciado ou já em execução.", "Falha ao iniciar o serviço 'Server'.", serviceStartSuccessCodes);
                await RunCommandAsAdmin("/c sc config lanmanworkstation start= auto", "Serviço 'Workstation' configurado para iniciar automaticamente.", "Falha ao configurar o serviço 'Workstation'.");
                await RunCommandAsAdmin("/c sc start lanmanworkstation", "Serviço 'Workstation' iniciado ou já em execução.", "Falha ao iniciar o serviço 'Workstation'.", serviceStartSuccessCodes);

                // Ativar regras de firewall
                await RunCommandAsAdmin("/c netsh advfirewall firewall set rule group=\"Descoberta de Rede\" new enable=Yes", "Regras de firewall para 'Descoberta de Rede' ativadas ou já ativas.", "Falha ao ativar regras de firewall para 'Descoberta de Rede'.", netshSuccessCodes);
                await RunCommandAsAdmin("/c netsh advfirewall firewall set rule group=\"Partilha de Ficheiros e Impressoras\" new enable=Yes", "Regras de firewall para 'Partilha de Ficheiros e Impressoras' ativadas ou já ativas.", "Falha ao ativar regras de firewall para 'Partilha de Ficheiros e Impressoras'.", netshSuccessCodes);

                // Criar/alterar registo para permitir acesso remoto com contas locais
                await RunCommandAsAdmin("/c reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" /v LocalAccountTokenFilterPolicy /t REG_DWORD /d 1 /f", "Registo para acesso remoto com contas locais configurado.", "Falha ao configurar o registo para acesso remoto.");

                MessageBox.Show("A configuração de acesso administrativo remoto foi concluída.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btHost_Livre_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende restaurar o ficheiro HOSTS e remover todas as regras de bloqueio da Internet?" + Environment.NewLine + "Esta ação irá remover todas as entradas personalizadas e requer privilégios de administrador.", "Desbloquear Internet e Restaurar HOSTS", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DesbloqueioTotal();
            }
        }

        private async void DesbloqueioTotal()
        {
            // Restaurar a política de firewall padrão (Permitir saída)
            string restoreFirewallPolicyCommand = "/c netsh advfirewall set allprofiles firewallpolicy allowinbound,allowoutbound";
            await RunCommandAsAdminInternal(restoreFirewallPolicyCommand, "Ocorreu um erro ao restaurar a política do firewall.");

            // Remover regras de firewall específicas
            string[] ruleNamesToDelete = { "BloqueioTotalInternet_IT_Tool", "Allow_DNS_IT_Tool", "Allow_Google_IT_Tool" };
            foreach (var ruleName in ruleNamesToDelete)
            {
                string removeRuleCommand = $"/c netsh advfirewall firewall delete rule name=\"{ruleName}\"";
                await RunCommandAsAdminInternal(removeRuleCommand, $"Ocorreu um erro ao remover a regra '{ruleName}'.", new[] { 0, 1, 2 });
            }

            // Restaurar o ficheiro HOSTS
            string HostOriginal = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Origem.txt"));
            string hostsContent = HostOriginal;
            await ApplyHostsFile(hostsContent, "O acesso à Internet foi totalmente restaurado com sucesso.", "Ocorreu um erro ao restaurar o ficheiro HOSTS.");
        }

        private async void btHost_BloqueioTotal_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende bloquear completamente o acesso à Internet?" + Environment.NewLine + "Esta ação irá criar uma regra no Firewall do Windows e requer privilégios de administrador.", "Bloqueio Total da Internet", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Primeiro, removemos qualquer regra antiga com o mesmo nome para evitar conflitos.
                string deleteOldRuleCommand = "/c netsh advfirewall firewall delete rule name=\"BloqueioTotalInternet_IT_Tool\"";
                await RunCommandAsAdminInternal(deleteOldRuleCommand, "Ocorreu um erro ao remover a regra antiga.", new[] { 0, 1, 2 });

                // Adicionamos a nova regra que bloqueia todo o tráfego de saída.
                string command = "/c netsh advfirewall firewall add rule name=\"BloqueioTotalInternet_IT_Tool\" dir=out action=block";
                string successMessage = "O acesso à Internet foi bloqueado com sucesso através do Firewall do Windows.";
                string errorMessage = "Ocorreu um erro ao tentar bloquear o acesso à Internet.";

                await RunCommandAsAdmin(command, successMessage, errorMessage);
            }
        }

        private async void btHost_TotalGmail_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Pretende bloquear toda a Internet, exceto os serviços Google (Gmail, OAuth)?" + Environment.NewLine + "Esta ação irá modificar as políticas do Firewall do Windows e requer privilégios de administrador.", "Bloqueio com Exceção para Google", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Limpa regras anteriores para garantir um estado limpo
                DesbloqueioTotal();

                // Define a política de saída padrão para bloquear
                string blockOutboundPolicy = "/c netsh advfirewall set allprofiles firewallpolicy allowinbound,blockoutbound";
                bool policySet = await RunCommandAsAdminInternal(blockOutboundPolicy, "Falha ao definir a política de bloqueio de saída do firewall.");
                if (!policySet) return;

                // Permite tráfego DNS (essencial)
                string allowDns = "/c netsh advfirewall firewall add rule name=\"Allow_DNS_IT_Tool\" dir=out action=allow protocol=UDP remoteport=53";
                await RunCommandAsAdminInternal(allowDns, "Falha ao criar regra de permissão para DNS.");

                // Permite os IPs da Google (obtidos da fonte oficial _spf.google.com)
                // Esta lista pode precisar de ser atualizada periodicamente.
                string googleIPs = "35.190.247.0/24,64.233.160.0/19,66.102.0.0/20,66.249.80.0/20,72.14.192.0/18,74.125.0.0/16,108.177.8.0/21,173.194.0.0/16,209.85.128.0/17,216.58.192.0/19,216.239.32.0/19";
                string allowGoogle = $"netsh advfirewall firewall add rule name=\"Allow_Google_IT_Tool\" dir=out action=allow remoteip={googleIPs}";
                await RunCommandAsAdminInternal($"/c {allowGoogle}", "Falha ao criar regra de permissão para os IPs da Google.");

                MessageBox.Show("Acesso à Internet bloqueado, com exceção para os serviços Google e DNS.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void btHost_RedesSociais_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende bloquear o acesso às Redes Sociais?", "Bloqueio às Redes Sociais", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 1. Restaurar a política de firewall padrão (Permitir saída)
                string restoreFirewallPolicyCommand = "/c netsh advfirewall set allprofiles firewallpolicy allowinbound,allowoutbound";
                await RunCommandAsAdminInternal(restoreFirewallPolicyCommand, "Ocorreu um erro ao restaurar a política do firewall.");

                // 2. Remover regras de firewall específicas
                string[] ruleNamesToDelete = { "BloqueioTotalInternet_IT_Tool", "Allow_DNS_IT_Tool", "Allow_Google_IT_Tool" };
                foreach (var ruleName in ruleNamesToDelete)
                {
                    string removeRuleCommand = $"/c netsh advfirewall firewall delete rule name=\"{ruleName}\"";
                    await RunCommandAsAdminInternal(removeRuleCommand, $"Ocorreu um erro ao remover a regra '{ruleName}'.", new[] { 0, 1, 2 });
                }

                // 3. Restaurar o ficheiro HOSTS
                string HostOriginal = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Origem.txt"));
                string hostsContent = HostOriginal + Environment.NewLine;
                string RedesSociaisHosts = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "RedesSociais.txt"));
                hostsContent += Environment.NewLine + RedesSociaisHosts;

                await ApplyHostsFile(hostsContent, "O acesso às Redes Sociais foi feito com sucesso.", "Ocorreu um erro ao restaurar o ficheiro HOSTS.");
            }
        }

        private async void btHost_Stream_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende bloquear o acesso aos Sites de Streaming?", "Bloqueio aos Sites de Streaming", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 1. Restaurar a política de firewall padrão (Permitir saída)
                string restoreFirewallPolicyCommand = "/c netsh advfirewall set allprofiles firewallpolicy allowinbound,allowoutbound";
                await RunCommandAsAdminInternal(restoreFirewallPolicyCommand, "Ocorreu um erro ao restaurar a política do firewall.");

                // 2. Remover regras de firewall específicas
                string[] ruleNamesToDelete = { "BloqueioTotalInternet_IT_Tool", "Allow_DNS_IT_Tool", "Allow_Google_IT_Tool" };
                foreach (var ruleName in ruleNamesToDelete)
                {
                    string removeRuleCommand = $"/c netsh advfirewall firewall delete rule name=\"{ruleName}\"";
                    await RunCommandAsAdminInternal(removeRuleCommand, $"Ocorreu um erro ao remover a regra '{ruleName}'.", new[] { 0, 1, 2 });
                }

                // 3. Restaurar o ficheiro HOSTS
                string HostOriginal = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Origem.txt"));
                string hostsContent = HostOriginal + Environment.NewLine;
                string StreamingHosts = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Streaming.txt"));
                hostsContent += Environment.NewLine + StreamingHosts;

                await ApplyHostsFile(hostsContent, "O acesso aos Sites de Streaming foi feito com sucesso.", "Ocorreu um erro ao restaurar o ficheiro HOSTS.");
            }
        }

        private async void btHost_RedesSociaisStream_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende bloquear o acesso às Redes Sociais e aos Sites de Streaming?", "Bloqueio às Redes Sociais e Streaming", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 1. Restaurar a política de firewall padrão (Permitir saída)
                string restoreFirewallPolicyCommand = "/c netsh advfirewall set allprofiles firewallpolicy allowinbound,allowoutbound";
                await RunCommandAsAdminInternal(restoreFirewallPolicyCommand, "Ocorreu um erro ao restaurar a política do firewall.");

                // 2. Remover regras de firewall específicas
                string[] ruleNamesToDelete = { "BloqueioTotalInternet_IT_Tool", "Allow_DNS_IT_Tool", "Allow_Google_IT_Tool" };
                foreach (var ruleName in ruleNamesToDelete)
                {
                    string removeRuleCommand = $"/c netsh advfirewall firewall delete rule name=\"{ruleName}\"";
                    await RunCommandAsAdminInternal(removeRuleCommand, $"Ocorreu um erro ao remover a regra '{ruleName}'.", new[] { 0, 1, 2 });
                }

                // 3. Restaurar o ficheiro HOSTS
                string HostOriginal = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Origem.txt"));
                string hostsContent = HostOriginal + Environment.NewLine;
                string RedesSociaisHosts = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "RedesSociais.txt"));
                hostsContent += Environment.NewLine + RedesSociaisHosts;
                string StreamingHosts = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "Streaming.txt"));
                hostsContent += Environment.NewLine + StreamingHosts;

                await ApplyHostsFile(hostsContent, "O acesso às Redes Sociais e aos Sites de Streaming foi feito com sucesso.", "Ocorreu um erro ao restaurar o ficheiro HOSTS.");
            }
        }
        #endregion

        #region Funções Comuns
        private void Actualizar_DashBoard()
        {
            // Nome do PC
            txNomePC.Text = Environment.MachineName;

            // Informação do Processador
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        txProcessador.Text = obj["Name"]?.ToString();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                txProcessador.Text = "N/A";
            }

            // Edição e Versão do Windows
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        txWINEdicao.Text = obj["Caption"]?.ToString();
                        txWINVersao.Text = obj["Version"]?.ToString();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                txWINEdicao.Text = "N/A";
                txWINVersao.Text = "N/A";
            }

            // Informações de Rede
            try
            {
                lbRedeIP.Text = "N/A";
                lbRedeMask.Text = "N/A";
                lbRedeGateway.Text = "N/A";

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        IPInterfaceProperties properties = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                lbRedeIP.Text = ip.Address.ToString();
                                lbRedeMask.Text = ip.IPv4Mask.ToString();
                                break; // Encontrou o IPv4, pode sair do loop de IPs
                            }
                        }

                        if (properties.GatewayAddresses.Any())
                        {
                            lbRedeGateway.Text = properties.GatewayAddresses.FirstOrDefault(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address.ToString();
                        }

                        if (lbRedeIP.Text != "N/A")
                        {
                            break; // Encontrou uma interface válida, pode sair do loop de interfaces
                        }
                    }
                }
            }
            catch (Exception)
            {
                lbRedeIP.Text = "N/A";
                lbRedeMask.Text = "N/A";
                lbRedeGateway.Text = "N/A";
            }

            // Total de RAM Instalada
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["TotalPhysicalMemory"] != null)
                        {
                            double ramBytes = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                            TotalRAM = Math.Round(ramBytes / (1024 * 1024 * 1024));
                            lbMemTotal.Text = $"{TotalRAM} GB";
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
                lbMemTotal.Text = "N/A";
            }

            // RAM em uso
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["TotalVisibleMemorySize"] != null && obj["FreePhysicalMemory"] != null)
                        {
                            ulong totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]); // in KB
                            ulong freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]); // in KB
                            ulong usedMemory = totalMemory - freeMemory;
                            RAMUso = (int)((usedMemory * 100) / totalMemory);
                            var FreeRAM = freeMemory / (1024 * 1024);
                            lbMemLivre.Text = $"{FreeRAM.ToString("N0")} GB";
                            var UsedRAM = usedMemory / (1024 * 1024);
                            lbMemUso.Text = $"{UsedRAM.ToString("N0")} GB";
                        }
                        break;
                    }
                }
            }
            catch (Exception)
            {
                RAMUso = 0;
                lbMemLivre.Text = "N/A";
                lbMemUso.Text = "N/A";
            }

            ponteiroRAMClaro.Value = RAMUso;
            ponteiroRAMEscuro.Value = RAMUso;

            // Informações do Disco C:
            try
            {
                DriveInfo cDrive = new DriveInfo("C");
                if (cDrive.IsReady)
                {
                    TotalDisco = Math.Round((double)cDrive.TotalSize / (1024 * 1024 * 1024));
                    double freeSpace = cDrive.AvailableFreeSpace;
                    var FreeDisco = freeSpace / (1024 * 1024 * 1024);
                    double usedSpace = cDrive.TotalSize - freeSpace;
                    var UsedDisco = usedSpace / (1024 * 1024 * 1024);
                    DiscoUso = (int)(usedSpace * 100 / cDrive.TotalSize);
                    lbDiskTotal.Text = $"{TotalDisco.ToString("N0")} GB";
                    lbDiskUso.Text = $"{UsedDisco.ToString("N0")} GB";
                    lbDiskLivre.Text = $"{FreeDisco.ToString("N0")} GB";
                }
            }
            catch (Exception)
            {
                DiscoUso = 0;
                lbDiskTotal.Text = "N/A";
                lbDiskUso.Text = "N/A";
                lbDiskLivre.Text = "N/A";
            }

            ponteiroDiscoClaro.Value = DiscoUso;
            ponteiroDiscoEscuro.Value = DiscoUso;
        }

        private async Task<int> RunCommand(string command)
        {
            Process process = Process.Start(new ProcessStartInfo("cmd.exe", command)
            {
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            await process.WaitForExitAsync();
            return process.ExitCode;
        }

        private async Task RunCommandAsAdmin(string command, string successMessage, string errorMessage, int[] successExitCodes = null)
        {
            if (successExitCodes == null)
            {
                successExitCodes = new[] { 0 };
            }

            try
            {
                Process process = Process.Start(new ProcessStartInfo("cmd.exe", command)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                await process.WaitForExitAsync();

                if (successExitCodes.Contains(process.ExitCode))
                {
                    MessageBox.Show(successMessage, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"{errorMessage}\nCódigo de saída: {process.ExitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessage}\nDetalhes: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> RunCommandAsAdminInternal(string command, string errorMessage, int[] successExitCodes = null)
        {
            if (successExitCodes == null)
            {
                successExitCodes = new[] { 0 };
            }

            try
            {
                Process process = Process.Start(new ProcessStartInfo("cmd.exe", command)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                await process.WaitForExitAsync();

                if (successExitCodes.Contains(process.ExitCode))
                {
                    return true;
                }
                else
                {
                    MessageBox.Show($"{errorMessage}\nCódigo de saída: {process.ExitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
            {
                MessageBox.Show("A operação foi cancelada pelo utilizador.", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{errorMessage}\nDetalhes: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<string> RunCommandAndGetOutput(string command, int[] successExitCodes = null)
        {
            if (successExitCodes == null)
            {
                successExitCodes = new[] { 0 };
            }

            string output = "";
            string tempOutputFile = Path.GetTempFileName();
            try
            {
                Process process = Process.Start(new ProcessStartInfo("cmd.exe", $"/c {command} > \"{tempOutputFile}\" 2>&1")
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                await process.WaitForExitAsync();

                output = File.ReadAllText(tempOutputFile);

                if (!successExitCodes.Contains(process.ExitCode))
                {
                    return $"Comando falhou com código de saída: {process.ExitCode}\n{output}";
                }
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223) // Operation was canceled by the user
            {
                return "A operação foi cancelada pelo utilizador.";
            }
            catch (Exception ex)
            {
                return $"Erro ao executar o comando: {ex.Message}";
            }
            finally
            {
                if (File.Exists(tempOutputFile))
                {
                    File.Delete(tempOutputFile);
                }
            }
            return output;
        }

        private async Task ApplyHostsFile(string content, string successMessage, string errorMessage)
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFilePath, content, Encoding.UTF8);

                string hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers\\etc\\hosts");
                string command = $"/c copy /Y \"{tempFilePath}\" \"{hostsPath}\"";

                bool success = await RunCommandAsAdminInternal(command, errorMessage);
                if (success)
                {
                    // Após modificar o hosts, é boa prática limpar a cache de DNS para que as alterações tenham efeito imediato.
                    await RunCommandAsAdminInternal("/c ipconfig /flushdns", "Falha ao limpar a cache de DNS.");
                    MessageBox.Show(successMessage, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro inesperado: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        private string ShowSelectDialog(string NomeOpcao, string TextoOpcao, List<string> Opcoes)
        {
            using (DevExpress.XtraEditors.XtraForm prompt = new DevExpress.XtraEditors.XtraForm())
            {
                prompt.Width = 300;
                prompt.Height = 150;
                prompt.Text = NomeOpcao;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.IconOptions.Icon = this.IconOptions.Icon;
                prompt.IconOptions.SvgImage = this.IconOptions.SvgImage;
                Label textLabel = new Label() { Left = 20, Top = 20, Text = TextoOpcao, Width = 250 };
                ComboBox comboBox = new ComboBox() { Left = 20, Top = 50, Width = 250 };
                comboBox.DataSource = Opcoes;
                Button confirmation = new Button() { Text = "Ok", Left = 195, Width = 75, Top = 80, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(comboBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? comboBox.SelectedItem.ToString() : string.Empty;
            }
        }

        private void ShowOutputDialog(string title, string content)
        {
            using (DevExpress.XtraEditors.XtraForm outputForm = new DevExpress.XtraEditors.XtraForm())
            {
                outputForm.Width = 800;
                outputForm.Height = 600;
                outputForm.Text = title;
                outputForm.MaximizeBox = false;
                outputForm.StartPosition = FormStartPosition.CenterScreen;
                outputForm.IconOptions.Icon = this.IconOptions.Icon;
                outputForm.IconOptions.SvgImage = this.IconOptions.SvgImage;

                DevExpress.XtraEditors.MemoEdit memoEdit = new DevExpress.XtraEditors.MemoEdit();
                memoEdit.Dock = DockStyle.Fill;
                memoEdit.Text = content;
                memoEdit.Properties.ReadOnly = true;
                memoEdit.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

                outputForm.Controls.Add(memoEdit);
                outputForm.ShowDialog(this);
            }
        }
        #endregion

        #region Estilo
        private void ckEstilo_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (ckEstilo.Checked)
            {
                Estilo = "Escuro";
                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("WXI", "Darkness");
                panelInfoPC.LookAndFeel.SetSkinStyle("WXI", "Darkness");
                panelInfoRAM.LookAndFeel.SetSkinStyle("WXI", "Darkness");
                gaugeControlRAMClaro.Visible = false;
                gaugeControlRAMEscuro.Visible = true;
                panelInfoDisco.LookAndFeel.SetSkinStyle("WXI", "Darkness");
                gaugeControlDiscoClaro.Visible = false;
                gaugeControlDiscoEscuro.Visible = true;
                this.BackgroundImage = Image.FromFile(@"Imgs\FundoEscuro.png");
            }
            else
            {
                Estilo = "Claro";
                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("WXI", "Clearness");
                panelInfoPC.LookAndFeel.SetSkinStyle("WXI", "Clearness");
                panelInfoRAM.LookAndFeel.SetSkinStyle("WXI", "Clearness");
                gaugeControlRAMClaro.Visible = true;
                gaugeControlRAMEscuro.Visible = false;
                panelInfoDisco.LookAndFeel.SetSkinStyle("WXI", "Clearness");
                gaugeControlDiscoClaro.Visible = true;
                gaugeControlDiscoEscuro.Visible = false;
                this.BackgroundImage = Image.FromFile(@"Imgs\FundoClaro.png");
            }
        }
        #endregion
    }

    public class WingetApp
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string VersaoAtual { get; set; }
        public string VersaoDisponivel { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(VersaoAtual) && !string.IsNullOrEmpty(VersaoDisponivel))
            {
                return $"{Name} (Versão: {VersaoAtual} -> {VersaoDisponivel})";
            }
            else if (!string.IsNullOrEmpty(VersaoAtual) && string.IsNullOrEmpty(VersaoDisponivel))
            {
                return $"{Name} (Versão: {VersaoAtual})";
            }
            else
            {
                return $"{Name}";
            }
        }
    }
}