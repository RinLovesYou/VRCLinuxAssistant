using System;
using System.IO;
using Avalonia;
using Newtonsoft.Json;
using VRCLinuxAssistant.Classes;

namespace VRCLinuxAssistant
{
    class Program
    {
        public static Config VLAConfig { get; set; }
        public static string[] Arguments { get; set; }
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Arguments = args;
            if (!File.Exists("vrcla.config.json"))
            {
                VLAConfig = new Config();
                VLAConfig.VRCPath = "";
                VLAConfig.TOS = false;
                VLAConfig.LastPage = 0;
                var json = JsonConvert.SerializeObject(VLAConfig, Formatting.Indented);
                try
                {
                    File.WriteAllText("vrcla.config.json", json);   
                } 
                catch(Exception e) { /**It is perfectly fine to fail here.*/ }
            }
            else
            {
                try
                {
                    VLAConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText("vrcla.config.json"));   
                } 
                catch(Exception e) { /**It is perfectly fine to fail here.*/ }
            }
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
