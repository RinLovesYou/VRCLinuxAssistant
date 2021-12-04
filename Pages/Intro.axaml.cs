using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace VRCLinuxAssistant.Pages
{
    public class Intro : UserControl
    {
        public Intro()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private bool _hasClicked = false;
        private void Agree_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_hasClicked) return;
            _hasClicked = true;
            string text = "You can now use the Mods tab!";
            MainWindow.MainTextBlock.Text = text;
            MainWindow.ModsButton.IsEnabled = true;
        }

        private void Disagree_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_hasClicked) return;
            _hasClicked = true;
            string text = "Mods tab disabled. Please restart to try again.";
            MainWindow.MainTextBlock.Text = text;
        }
    }
}