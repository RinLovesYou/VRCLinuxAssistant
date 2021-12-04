using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using VRCLinuxAssistant.Classes;
using VRCLinuxAssistant.Pages;

using static VRCLinuxAssistant.Program;

#pragma warning disable 8618
#pragma warning disable 8601

namespace VRCLinuxAssistant
{
    public partial class MainWindow : Window
    {
        public static string AppVersion = "0.0.1";

        public static MainWindow Instance { get; set; }
        public static UserControl Main { get; set; }
        public static TextBlock MainTextBlock { get; set; }
        public static Button ModsButton { get; set; }
        public static Button InstallButton { get; set; }
        public static Button InfoButton { get; set; }
        
        public static bool ModsOpened = false;
        public static bool ModsLoading = false;
        
        
        public static readonly Intro IntroPage = new Intro();
        public static readonly About AboutPage = new About();
        public static Mods ModsPage = new Mods();
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private async Task ShowModsPage()
        {
            void OpenModsPage()
            {
                Main.Content = ModsPage;
            }

            if (ModsOpened && !ModsPage.PendingChanges)
            {
                OpenModsPage();
                return;
            }

            //Main.Content = Loading.Instance;

            if (ModsLoading) return;
            ModsLoading = true;
            await ModsPage.LoadMods();
            ModsLoading = false;

            if (ModsOpened == false) ModsOpened = true;
            if (ModsPage.PendingChanges == true) ModsPage.PendingChanges = false;

            OpenModsPage();
        }

        private object PageFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return IntroPage;
                case 1:
                    return IntroPage;
                case 2:
                    return AboutPage;
                default:
                    return IntroPage;
            }
        }

        private void VersionText_OnInitialized(object? sender, EventArgs e)
        {
            var text = sender as TextBlock;
            text.Text = AppVersion;
        }

        private void Main_OnInitialized(object? sender, EventArgs e)
        {
            Main = sender as UserControl;
            var index = PageFromIndex(VLAConfig.LastPage);
            Main.Content = index;
        }

        private void AboutButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Main.Content = AboutPage;
            VLAConfig.LastPage = 2;
            VLAUtils.SaveConfig();
        }

        private void IntroButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Main.Content = IntroPage;
            VLAConfig.LastPage = 0;
            VLAUtils.SaveConfig();
        }

        private void MainTextBlock_OnInitialized(object? sender, EventArgs e)
        {
            MainTextBlock = sender as TextBlock;
        }

        private void ModsButton_OnInitialized(object? sender, EventArgs e)
        {
            ModsButton = sender as Button;
            if (VLAConfig.TOS)
            {
                ModsButton.IsEnabled = true;
            }
        }

        private void ModsButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ShowModsPage();
            VLAConfig.LastPage = 1;
            VLAUtils.SaveConfig();
        }

        private void InstallButton_OnInitialized(object? sender, EventArgs e)
        {
            InstallButton = sender as Button;
        }

        private void InfoButton_OnInitialized(object? sender, EventArgs e)
        {
            InfoButton = sender as Button;
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e)
        {
            Instance = sender as MainWindow;
            Task.Run(() =>
            {
                var res = VLAUtils.GetInstallDir();
                App.VRChatInstallDirectory = res;
                Program.VLAConfig.VRCPath = res;
                VLAUtils.SaveConfig();
            });
        }

        private void InstallButton_OnClick(object? sender, RoutedEventArgs e)
        {
            ModsPage.InstallMods();
        }
    }
}