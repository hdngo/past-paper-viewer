using System.Windows;

namespace PastPaperViewer
{
    /// <summary>
    /// Interaction logic for Information.xaml
    /// </summary>
    public partial class Information : Window
    {
        public Information()
        {
            InitializeComponent();

            if (Properties.Settings.Default.DarkMode)
                InfoDarkImage.Visibility = Visibility.Visible;
            else
                InfoDarkImage.Visibility = Visibility.Collapsed;
        }
    }
}
