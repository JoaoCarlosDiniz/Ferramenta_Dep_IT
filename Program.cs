using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace IT
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
                MessageBox.Show("O .NET Desktop Runtime v9.0 não está instalado. A instalaçã será iniciada agora.", "Pré-requisito ausente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Substitua "dotnet-installer.exe" pelo nome exato do seu instalador.
                string installerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Requisitos", "dotnet-installer.exe");

                if (File.Exists(installerPath))
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(installerPath)
                        {
                            UseShellExecute = true // UseShellExecute = true para elevar privil�gios (UAC) se necess�rio
                        };
                        Process.Start(startInfo)?.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ocorreu um erro ao tentar executar o instalador: {ex.Message}", "Erro de Instala��o", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return; // Sai da aplica��o se a instala��o falhar
                    }
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Verifica se uma vers�o espec�fica do .NET Desktop Runtime est� instalada.
        /// </summary>
        /// <param name="requiredVersion">A vers�o principal a ser verificada (ex: "9.0").</param>
        /// <returns>True se a vers�o ou uma mais recente estiver instalada, caso contr�rio, false.</returns>
        private static bool IsNetDesktopRuntimeInstalled(string requiredVersion)
        {
            try
            {
                if (!Version.TryParse(requiredVersion, out Version requiredVer))
                {
                    return false; // Vers�o requerida inv�lida
                }
                int requiredMajor = requiredVer.Major;

                // Lista de todos os poss�veis caminhos base e arquiteturas
                var basePaths = new[]
                {
                    @"SOFTWARE\dotnet\Setup\InstalledVersions",
                    @"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions" // Para instala��es 32-bit em SO 64-bit
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
                                    // As vers�es est�o nos nomes dos valores (ex: "9.0.9")
                                    foreach (var valueName in key.GetValueNames())
                                    {
                                        // Tenta fazer o parse da vers�o a partir do nome do valor
                                        if (Version.TryParse(valueName.Split('-')[0], out Version installedVersion) && installedVersion.Major == requiredMajor)
                                        {
                                            return true; // Encontrou uma vers�o compat�vel
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
                // Se ocorrer um erro ao ler o registro, assume que n�o est� instalado para seguran�a.
                return false;
            }

            return false; // N�o encontrou em nenhum dos caminhos verificados
        }
    }
}