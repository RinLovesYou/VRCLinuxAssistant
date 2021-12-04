using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VRCLinuxAssistant.Pages
{
    public class About : UserControl
    {
        public About()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}