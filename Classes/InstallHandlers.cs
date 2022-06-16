using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace VRCLinuxAssistant.Classes
{
    public class InstallHandlers
    {
        public static bool IsMelonLoaderInstalled()
        {
            return File.Exists(Path.Combine(App.VRChatInstallDirectory, "version.dll")) && File.Exists(Path.Combine(App.VRChatInstallDirectory, "MelonLoader", "Dependencies", "Bootstrap.dll"));
        }
        
        public static bool RemoveMelonLoader()
        {
            MainWindow.MainTextBlock.Text = $"Uninstalling MelonLoader...";

            try
            {
                var versionDllPath = Path.Combine(App.VRChatInstallDirectory, "version.dll");
                var melonLoaderDirPath = Path.Combine(App.VRChatInstallDirectory, "MelonLoader");

                if (File.Exists(versionDllPath))
                    File.Delete(versionDllPath);
                if (Directory.Exists(melonLoaderDirPath))
                    Directory.Delete(melonLoaderDirPath, true);
            }
            catch (Exception ex)
            {
                MainWindow.MainTextBlock.Text = $"Uninstalling MelonLoader failed.\n{ex.Message}";
                return false;
            }

            return true;
        }
        
        public static async Task InstallMelonLoader()
        {
            if (!RemoveMelonLoader()) return;

            try
            {
                MainWindow.MainTextBlock.Text = $"Downloading MelonLoader...";

                using var installerZip = await DownloadFileToMemory("https://github.com/LavaGang/MelonLoader/releases/latest/download/MelonLoader.x64.zip");
                using var zipReader = new ZipArchive(installerZip, ZipArchiveMode.Read);

                MainWindow.MainTextBlock.Text = "Unpacking MelonLoader...";

                foreach (var zipArchiveEntry in zipReader.Entries)
                {
                    var targetFileName = Path.Combine(App.VRChatInstallDirectory, zipArchiveEntry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                    using var targetFile = File.OpenWrite(targetFileName);
                    using var entryStream = zipArchiveEntry.Open();
                    await entryStream.CopyToAsync(targetFile);
                }

                Directory.CreateDirectory(Path.Combine(App.VRChatInstallDirectory, "Mods"));
                Directory.CreateDirectory(Path.Combine(App.VRChatInstallDirectory, "Plugins"));
                
                MainWindow.MainTextBlock.Text = "MelonLoader Installed!";
                
            }
            catch (Exception ex)
            {
                MainWindow.MainTextBlock.Text = "Failed to install MelonLoader "+ ex;
            }
        }
        
        internal static async Task<Stream> DownloadFileToMemory(string link)
        {
            var client = Http.HttpClient;
            using var resp = await client.GetAsync(link);
            var newStream = new MemoryStream();
            await resp.Content.CopyToAsync(newStream);
            newStream.Position = 0;
            return newStream;
        }
        
        public static async Task InstallMod(Mod mod)
        {
            string downloadLink = mod.versions[0].downloadLink;

            if (string.IsNullOrEmpty(downloadLink))
            {
                //TODO: MessageBoxes
                return;
            }

            if (mod.installedFilePath != null)
                File.Delete(mod.installedFilePath);

            string targetFilePath = "";

            using (var resp = await Http.HttpClient.GetAsync(downloadLink))
            {
                var stream = new MemoryStream();
                await resp.Content.CopyToAsync(stream);
                stream.Position = 0;

                targetFilePath = Path.Combine(App.VRChatInstallDirectory, mod.versions[0].IsPlugin ? "Plugins" : "Mods",
                    mod.versions[0].IsBroken ? "Broken" : (mod.versions[0].IsRetired ? "Retired" : ""), resp.RequestMessage.RequestUri.Segments.Last());

                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));

                using var targetFile = File.OpenWrite(targetFilePath);
                await stream.CopyToAsync(targetFile);
            }

            mod.ListItem.IsInstalled = true;
            mod.installedFilePath = targetFilePath;
            mod.ListItem.InstalledVersion = mod.versions[0].modVersion;
            mod.ListItem.InstalledModInfo = mod;
        }
    }
}