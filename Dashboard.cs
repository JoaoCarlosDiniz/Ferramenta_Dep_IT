using Microsoft.Win32;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;

namespace Ferramenta_IT
{
    public partial class Dashboard : Form
    {
        #region Variaveis
        private NameValueCollection AppSettings = ConfigurationManager.AppSettings;

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

        public Dashboard()
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

        private void Dashboard_Load(object sender, EventArgs e)
        {
            Actualizar_DashBoard();
        }

        private void actualizarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Actualizar_DashBoard();
        }

        #region Domínio
        private void testeDeDominioToolStripMenuItem_Click(object sender, EventArgs e)
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
                    inParams["FJoinOptions"] = 3; // 1 (Join Domain) + 2 (Create Account) = 3

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

        private void sincronizaçãoDeTempoToolStripMenuItem_Click(object sender, EventArgs e)
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
                    RunCommandAsAdmin("/c net stop w32time", "Serviço de tempo parado com sucesso.", "Falha ao parar o serviço de tempo.");

                    // Configurar o servidor de tempo
                    string configCommand = $"/c w32tm /config /manualpeerlist:\"{selectedServer}\" /syncfromflags:manual /reliable:yes /update";
                    RunCommandAsAdmin(configCommand, "Configuração de servidor de tempo atualizada.", "Falha ao configurar o servidor de tempo.");

                    // Iniciar o serviço de tempo
                    RunCommandAsAdmin("/c net start w32time", "Serviço de tempo iniciado com sucesso.", "Falha ao iniciar o serviço de tempo.");

                    // Forçar a ressincronização
                    RunCommandAsAdmin("/c w32tm /resync /force", "Sincronização de tempo forçada.", "Falha ao forçar a sincronização.");

                    // Consultar e exibir o status
                    string status = RunCommandAndGetOutput("/c w32tm /query /status");
                    string source = RunCommandAndGetOutput("/c w32tm /query /source");

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
        private void testeDeDiscoEWindowsToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void correcãoDoErroRDCCredSSPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Esta operação irá modificar o registo do Windows para corrigir uma vulnerabilidade de segurança do RDC (CredSSP)." + Environment.NewLine + "Deseja continuar?", "Resolver Vulnerabilidade RDC", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string command = "/c REG ADD HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\\CredSSP\\Parameters /v AllowEncryptionOracle /t REG_DWORD /d 2 /f";
                string successMessage = "A correção para a vulnerabilidade de segurança do RDC foi aplicada com sucesso.";
                string errorMessage = "Ocorreu um erro ao aplicar a correção para o RDC.";

                RunCommandAsAdmin(command, successMessage, errorMessage);
            }
        }
        #endregion

        #region Utilizador
        private void actualizarComWingetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateAppsWithWinget();
        }

        private void UpdateAppsWithWinget()
        {
            try
            {
                using (var updateForm = new FormActualizar(ckEstilo.Checked))
                {
                    updateForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao iniciar o processo de atualização: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void reiniciarServiçoDeImpressãoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tem a certeza que pretende reiniciar o serviço de impressão e limpar a fila de impressão?", "Reset Spooler de Impressão", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    MessageBox.Show("O serviço de impressão será reiniciado." + Environment.NewLine + "Este processo pode exigir privilégios de administrador.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    int exitCode;

                    // Parar o serviço de spooler de impressão
                    exitCode = RunCommand("/c net stop spooler");
                    if (exitCode != 0)
                    {
                        MessageBox.Show($"Falha ao parar o serviço de impressão." + Environment.NewLine + "Código de erro: {exitCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Tenta iniciar o serviço mesmo que a paragem tenha falhado, para garantir que não fica parado
                        RunCommand("/c net start spooler");
                        return;
                    }

                    // Limpar a pasta de spool
                    exitCode = RunCommand("/c del %systemroot%\\system32\\spool\\printers\\* /Q /F /S");
                    // O comando del pode retornar 1 se não encontrar ficheiros, o que não é um erro neste contexto.
                    if (exitCode != 0 && exitCode != 1)
                    {
                        MessageBox.Show($"Falha ao limpar a fila de impressão. Código de erro: {exitCode}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Mesmo que a limpeza falhe, o serviço deve ser reiniciado.
                    }

                    // Iniciar o serviço de spooler de impressão
                    exitCode = RunCommand("/c net start spooler");
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
                    RunCommand("/c net start spooler");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao reiniciar o serviço de impressão: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Tenta garantir que o serviço fica a correr em caso de erro
                    RunCommand("/c net start spooler");
                }
            }
        }
        #endregion

        #region Rede
        private void testeDeComunicaçãoEAcessosÀRedeToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void LimparCacheDNS()
        {
            RunCommandAsAdmin("/c ipconfig /flushdns", "O cache de DNS foi limpo com sucesso.", "Ocorreu um erro ao limpar o cache de DNS.");
        }

        private void LimparCacheARP()
        {
            RunCommandAsAdmin("/c arp -d *", "O cache ARP foi limpo com sucesso.", "Ocorreu um erro ao limpar o cache ARP.");
        }

        private void LimparNetBIOS()
        {
            RunCommandAsAdmin("/c nbtstat -R", "O cache NetBIOS foi limpo com sucesso.", "Ocorreu um erro ao limpar o cache de NetBIOS.");
        }

        private void correcçãoDoAdminToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ResetComputerAccountPassword()
        {
            string selectedDC = ShowSelectDialog("Selecionar Servidor de DC", "Escolha o servidor de DC para redefinir a senha da conta do computador:", ListServerDC);
            if (string.IsNullOrEmpty(selectedDC))
            {
                return; // User cancelled
            }

            if (MessageBox.Show($"Tem a certeza que pretende redefinir a senha da conta do computador no domínio através do servidor {selectedDC}?", "Redefinir Senha da Conta do Computador", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string command = $"/c netdom resetpwd /server:{selectedDC} /userd:{NomeDominio}\\{Administrador} /passwordd:{SenhaAdministrador}";
                string successMessage = "A senha da conta do computador foi redefinida com sucesso.";
                string errorMessage = "Ocorreu um erro ao redefinir a senha da conta do computador.";

                RunCommandAsAdmin(command, successMessage, errorMessage);
            }
        }

        private void EnableRemoteAdminAccess()
        {
            if (MessageBox.Show("Esta operação irá ativar serviços, regras de firewall e alterar o registo para permitir acesso administrativo remoto." + Environment.NewLine + "Deseja continuar?", "Ativar Acesso Administrativo Remoto", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Ativar serviços necessários
                RunCommandAsAdmin("/c sc config lanmanserver start= auto", "Serviço 'Server' configurado para iniciar automaticamente.", "Falha ao configurar o serviço 'Server'.");
                RunCommandAsAdmin("/c sc start lanmanserver", "Serviço 'Server' iniciado.", "Falha ao iniciar o serviço 'Server'.");
                RunCommandAsAdmin("/c sc config lanmanworkstation start= auto", "Serviço 'Workstation' configurado para iniciar automaticamente.", "Falha ao configurar o serviço 'Workstation'.");
                RunCommandAsAdmin("/c sc start lanmanworkstation", "Serviço 'Workstation' iniciado.", "Falha ao iniciar o serviço 'Workstation'.");

                // Ativar regras de firewall
                RunCommandAsAdmin("/c netsh advfirewall firewall set rule group=\"Descoberta de Rede\" new enable=Yes", "Regras de firewall para 'Descoberta de Rede' ativadas.", "Falha ao ativar regras de firewall para 'Descoberta de Rede'.");
                RunCommandAsAdmin("/c netsh advfirewall firewall set rule group=\"Partilha de Ficheiros e Impressoras\" new enable=Yes", "Regras de firewall para 'Partilha de Ficheiros e Impressoras' ativadas.", "Falha ao ativar regras de firewall para 'Partilha de Ficheiros e Impressoras'.");

                // Criar/alterar registo para permitir acesso remoto com contas locais
                RunCommandAsAdmin("/c reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System\" /v LocalAccountTokenFilterPolicy /t REG_DWORD /d 1 /f", "Registo para acesso remoto com contas locais configurado.", "Falha ao configurar o registo para acesso remoto.");

                MessageBox.Show("A configuração de acesso administrativo remoto foi concluída.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        }

        private void RunCommandAsAdmin(string command, string successMessage, string errorMessage)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", command)
                {
                    Verb = "runas", // Request administrator privileges
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                if (process.ExitCode == 0)
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

        private string RunCommandAndGetOutput(string command)
        {
            string output = "";
            try
            {
                string tempOutputFile = Path.GetTempFileName();
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command} > \"{tempOutputFile}\"")
                {
                    Verb = "runas", // Request administrator privileges
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process process = Process.Start(psi);
                process.WaitForExit();

                output = File.ReadAllText(tempOutputFile);
                File.Delete(tempOutputFile);

                if (process.ExitCode != 0)
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
            return output;
        }

        private int RunCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", command)
            {
                Verb = "runas", // Request administrator privileges
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process process = Process.Start(psi);
            process.WaitForExit();
            return process.ExitCode;
        }

        private string ShowSelectDialog(string NomeOpcao, string TextoOpcao, List<string> Opcoes)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 150;
                prompt.Text = NomeOpcao;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.Icon = this.Icon;
                Label textLabel = new Label() { Left = 20, Top = 20, Text = TextoOpcao, Width = 250 };
                ComboBox comboBox = new ComboBox() { Left = 20, Top = 50, Width = 250 };
                comboBox.DataSource = Opcoes;
                Button confirmation = new Button() { Text = "Ok", Left = 195, Width = 75, Top = 80, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(comboBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;
                if (ckEstilo.Checked)
                {
                    prompt.BackColor = Color.FromArgb(50, 50, 50);
                    textLabel.ForeColor = Color.WhiteSmoke;
                    comboBox.ForeColor = Color.WhiteSmoke;
                    comboBox.BackColor = Color.FromArgb(70, 70, 70);
                    confirmation.ForeColor = Color.WhiteSmoke;
                    confirmation.BackColor = Color.FromArgb(70, 70, 70);
                }
                else
                {
                    prompt.BackColor = SystemColors.Control;
                    textLabel.ForeColor = Color.Black;
                    comboBox.ForeColor = Color.Black;
                    comboBox.BackColor = SystemColors.Control;
                    confirmation.ForeColor = Color.Black;
                    confirmation.BackColor = SystemColors.Control;
                }

                return prompt.ShowDialog() == DialogResult.OK ? comboBox.SelectedItem.ToString() : string.Empty;
            }
        }
        #endregion

        #region Estilo
        private void ckEstilo_CheckedChanged(object sender, EventArgs e)
        {
            if (ckEstilo.Checked)
            {
                Estilo = "Escuro";
                this.BackColor = Color.FromArgb(50, 50, 50);
                var CorLetras = Color.WhiteSmoke;
                barraMenu.Renderer = new ToolStripProfessionalRenderer(new DarkThemeColorTable());
                barraMenu.ForeColor = CorLetras;
                for (int i = 0; i < barraMenu.Items.Count; i++)
                {
                    if (barraMenu.Items[i] is ToolStripMenuItem menuItem)
                    {
                        menuItem.ForeColor = CorLetras;
                        foreach (ToolStripItem subItem in menuItem.DropDownItems)
                        {
                            subItem.ForeColor = CorLetras;
                        }
                    }
                }
                ckEstilo.ForeColor = CorLetras;

                InfoPC.ForeColor = CorLetras;
                label1.ForeColor = CorLetras;
                label2.ForeColor = CorLetras;
                label3.ForeColor = CorLetras;
                label3.ForeColor = CorLetras;
                txNomePC.ForeColor = CorLetras;
                txProcessador.ForeColor = CorLetras;
                txWINEdicao.ForeColor = CorLetras;
                txWINVersao.ForeColor = CorLetras;

                InfoRede.ForeColor = CorLetras;
                label11.ForeColor = CorLetras;
                label12.ForeColor = CorLetras;
                label13.ForeColor = CorLetras;
                lbRedeIP.ForeColor = CorLetras;
                lbRedeMask.ForeColor = CorLetras;
                lbRedeGateway.ForeColor = CorLetras;

                InfoMemoria.ForeColor = CorLetras;
                label8.ForeColor = CorLetras;
                label9.ForeColor = CorLetras;
                label10.ForeColor = CorLetras;
                lbMemTotal.ForeColor = CorLetras;
                lbMemUso.ForeColor = CorLetras;
                lbMemLivre.ForeColor = CorLetras;

                InfoDisco.ForeColor = CorLetras;
                label14.ForeColor = CorLetras;
                label15.ForeColor = CorLetras;
                label16.ForeColor = CorLetras;
                lbDiskTotal.ForeColor = CorLetras;
                lbDiskUso.ForeColor = CorLetras;
                lbDiskLivre.ForeColor = CorLetras;

                this.BackgroundImage = Image.FromFile(@"Imgs\FundoEscuro.png");
            }
            else
            {
                Estilo = "Claro";
                this.BackColor = SystemColors.Control;
                var CorLetras = Color.Black;
                barraMenu.Renderer = new ToolStripProfessionalRenderer(new LightThemeColorTable());
                barraMenu.ForeColor = CorLetras;
                for (int i = 0; i < barraMenu.Items.Count; i++)
                {
                    if (barraMenu.Items[i] is ToolStripMenuItem menuItem)
                    {
                        menuItem.ForeColor = CorLetras;
                        foreach (ToolStripItem subItem in menuItem.DropDownItems)
                        {
                            subItem.ForeColor = CorLetras;
                        }
                    }
                }
                ckEstilo.ForeColor = CorLetras;

                InfoPC.ForeColor = CorLetras;
                label1.ForeColor = CorLetras;
                label2.ForeColor = CorLetras;
                label3.ForeColor = CorLetras;
                label3.ForeColor = CorLetras;
                txNomePC.ForeColor = CorLetras;
                txProcessador.ForeColor = CorLetras;
                txWINEdicao.ForeColor = CorLetras;
                txWINVersao.ForeColor = CorLetras;

                InfoRede.ForeColor = CorLetras;
                label11.ForeColor = CorLetras;
                label12.ForeColor = CorLetras;
                label13.ForeColor = CorLetras;
                lbRedeIP.ForeColor = CorLetras;
                lbRedeMask.ForeColor = CorLetras;
                lbRedeGateway.ForeColor = CorLetras;

                InfoMemoria.ForeColor = CorLetras;
                label8.ForeColor = CorLetras;
                label9.ForeColor = CorLetras;
                label10.ForeColor = CorLetras;
                lbMemTotal.ForeColor = CorLetras;
                lbMemUso.ForeColor = CorLetras;
                lbMemLivre.ForeColor = CorLetras;

                InfoDisco.ForeColor = CorLetras;
                label14.ForeColor = CorLetras;
                label15.ForeColor = CorLetras;
                label16.ForeColor = CorLetras;
                lbDiskTotal.ForeColor = CorLetras;
                lbDiskUso.ForeColor = CorLetras;
                lbDiskLivre.ForeColor = CorLetras;

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
            return $"{Name} (Versão: {VersaoAtual} -> {VersaoDisponivel})";
        }
    }

    public class LightThemeColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => Color.FromArgb(240, 240, 240);
        public override Color MenuStripGradientEnd => Color.FromArgb(240, 240, 240);
        public override Color ToolStripDropDownBackground => Color.White;
        public override Color ImageMarginGradientBegin => Color.FromArgb(240, 240, 240);
        public override Color ImageMarginGradientEnd => Color.FromArgb(240, 240, 240);
        public override Color MenuItemSelected => Color.FromArgb(220, 220, 220);
        public override Color MenuItemBorder => Color.FromArgb(160, 160, 160);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(220, 220, 220);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(220, 220, 220);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(200, 200, 200);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(200, 200, 200);
    }

    public class DarkThemeColorTable : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color MenuStripGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 30);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color MenuItemSelected => Color.FromArgb(70, 70, 70);
        public override Color MenuItemBorder => Color.FromArgb(100, 100, 100);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 70);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 70);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(50, 50, 50);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(50, 50, 50);
    }
}