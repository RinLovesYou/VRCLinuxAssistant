using System;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VRCLinuxAssistant.Classes
{
    public class VLAUtils
    {
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
            string InstallDir = "/home/rin/.local/share/Steam/steamapps/common/VRChat/";
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

            if (!result.IsCompletedSuccessfully)
            {
                MainWindow.MainTextBlock.Text = "There was an error getting your VRChat Folder. Please try again.";
            }
            InstallDir = result.Result;
            if (!string.IsNullOrEmpty(InstallDir)
                && Directory.Exists(InstallDir)
                && Directory.Exists(Path.Combine(InstallDir, "VRChat_Data", "Plugins"))
                && File.Exists(Path.Combine(InstallDir, "VRChat.exe")))
            {
                return InstallDir;
            }
            return null;
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