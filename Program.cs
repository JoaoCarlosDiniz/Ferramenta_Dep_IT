using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace Ferramenta_IT
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!IsNetDesktopRuntimeInstalled("9.0"))
            {
                MessageBox.Show("O .NET Desktop Runtime v9.0 não está instalado. O instalador será iniciado agora.", "Pré-requisito ausente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Substitua "dotnet-installer.exe" pelo nome exato do seu instalador.
                string installerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "dotnet-installer.exe");

                if (File.Exists(installerPath))
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(installerPath)
                        {
                            UseShellExecute = true // UseShellExecute = true para elevar privilégios (UAC) se necessário
                        };
                        Process.Start(startInfo)?.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocorreu um erro ao tentar executar o instalador: {ex.Message}", "Erro de Instalação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // Sai da aplicação se a instalação falhar
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Dashboard());
        }

        /// <summary>
        /// Verifica se uma versão específica do .NET Desktop Runtime está instalada.
        /// </summary>
        /// <param name="requiredVersion">A versão principal a ser verificada (ex: "9.0").</param>
        /// <returns>True se a versão ou uma mais recente estiver instalada, caso contrário, false.</returns>
        private static bool IsNetDesktopRuntimeInstalled(string requiredVersion)
        {
            try
            {
                if (!Version.TryParse(requiredVersion, out Version requiredVer))
                {
                    return false; // Versão requerida inválida
                }
                int requiredMajor = requiredVer.Major;

                // Lista de todos os possíveis caminhos base e arquiteturas
                var basePaths = new[]
                {
                    @"SOFTWARE\dotnet\Setup\InstalledVersions",
                    @"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions" // Para instalações 32-bit em SO 64-bit
                };
                var architectures = new[] { "x64", "x86" };
                var fxNames = new[] { "Microsoft.WindowsDesktop.App", "Microsoft.NETCore.App" };

                foreach (var basePath in basePaths)
                {
                    foreach (var arch in architectures)
                    {
                        foreach (var fxName in fxNames)
                        {
                            string path = $@"{basePath}\{arch}\sharedfx\{fxName}";
                            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
                            {
                                if (key != null)
                                {
                                    // As versões estão nos nomes dos valores (ex: "9.0.9")
                                    foreach (var valueName in key.GetValueNames())
                                    {
                                        // Tenta fazer o parse da versão a partir do nome do valor
                                        if (Version.TryParse(valueName.Split('-')[0], out Version installedVersion) && installedVersion.Major == requiredMajor)
                                        {
                                            return true; // Encontrou uma versão compatível
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Se ocorrer um erro ao ler o registro, assume que não está instalado para segurança.
                return false;
            }

            return false; // Não encontrou em nenhum dos caminhos verificados
        }
    }
}