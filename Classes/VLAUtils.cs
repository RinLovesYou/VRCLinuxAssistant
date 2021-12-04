using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace VRCLinuxAssistant.Classes
{
    public class VLAUtils
    {
        public static string ExePath = Process.GetCurrentProcess().MainModule.FileName;
        public class Constants
        {
            public const string VRChatAppId = "438100";
            public const string VRCMGModsJson = "https://api.vrcmg.com/v0/mods.json";
            public const string WeebCDNAPIURL = "https://pat.assistant.moe/api/v1.0/";
            public const string MD5Spacer = "                                 ";
            public static readonly char[] IllegalCharacters = new char[]
            {
                '<', '>', ':', '/', '\\', '|', '?', '*', '"',
                '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
                '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000d',
                '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
                '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001f',
            };
        }
        
        public static string GetInstallDir()
        {
            string InstallDir = Program.VLAConfig.VRCPath;
            if (!string.IsNullOrEmpty(InstallDir)
                && Directory.Exists(InstallDir)
                && Directory.Exists(Path.Combine(InstallDir, "VRChat_Data", "Plugins"))
                && File.Exists(Path.Combine(InstallDir, "VRChat.exe")))
            {
                return InstallDir;
            }
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Please select your VRChat install directory";
            var result = dialog.ShowAsync(MainWindow.Instance);
            while (!result.IsCompleted) { }

            if (result.IsCanceled || result.IsFaulted)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    MainWindow.MainTextBlock.Text = "There was an error getting your VRChat Folder. Please try again."; 
                });
            }
            InstallDir = result.Result;
            if (!string.IsNullOrEmpty(InstallDir)
                && Directory.Exists(InstallDir)
                && Directory.Exists(Path.Combine(InstallDir, "VRChat_Data", "Plugins"))
                && File.Exists(Path.Combine(InstallDir, "VRChat.exe")))
            {
                return InstallDir;
            }
            Dispatcher.UIThread.Post(() =>
            {
                MainWindow.MainTextBlock.Text = "There was an error getting your VRChat Folder. Please try again."; 
            });
            return null;
        }
        
        public static async Task Download(string link, string output)
        {
            var client = new HttpClient();
            var resp = await client.GetAsync(link);
            using (var stream = await resp.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }

        public static void SaveConfig()
        {
            try
            {
                File.WriteAllText("vrcla.config.json", JsonConvert.SerializeObject(Program.VLAConfig));
            }
            catch (Exception e)
            {
                MainWindow.MainTextBlock.Text = "There was an error saving your config. Settings may not persist.";
                
            }
        }
        
        public static string GetVersion()
        {
            string filename = Path.Combine(App.VRChatInstallDirectory, "VRChat_Data", "globalgamemanagers");
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] file = File.ReadAllBytes(filename);
                byte[] bytes = new byte[32];

                fs.Read(file, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                int index = Encoding.UTF8.GetString(file).IndexOf("public.app-category.games") + 136;

                Array.Copy(file, index, bytes, 0, 32);
                string version = Encoding.UTF8.GetString(bytes).Trim(Constants.IllegalCharacters);

                return version;
            }
        }
    }
}