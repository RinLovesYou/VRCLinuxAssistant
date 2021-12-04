using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using VRCLinuxAssistant.Classes;

namespace VRCLinuxAssistant
{
    public class App : Application
    {
        public static string VRChatInstallDirectory;
        public static string VRChatInstallType;
        public static App Instance { get; set; }
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Instance = this;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}