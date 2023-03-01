using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StreamerBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String NGWord = "岸田";
        private BrowserWindow BrowserWindow;
        public MainWindow()
        {
            InitializeComponent();
            BrowserWindow = new BrowserWindow();
            BrowserWindow.Width = this.Width;
            BrowserWindow.Height = this.Width * 3 / 4;
            BrowserWindow.NGWords = NGWord.Split(' ').ToList();
            BrowserWindow.Show();
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            TextBoxUri.Text = BrowserWindow.GoBack().ToString();
        }

        private void ButtonGoForward_Click(object sender, RoutedEventArgs e)
        {
            TextBoxUri.Text = BrowserWindow.GoForward().ToString();
        }

        private void TextBoxUri_KeyDown(object sender, KeyEventArgs e)
        {
            var typedKey = e.Key;
            if (typedKey==Key.Enter||typedKey==Key.Return)
            {
                var newUri = GetUri(TextBoxUri.Text);
                TextBoxUri.Text = BrowserWindow.Go(newUri).ToString();
            }
        }

        private String GetUri(String uriString)
        {
            if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
            {
                    uriString = $"https://www.google.com/search?q={uriString}";
            }
            {
                return uriString;
            }
        }

        private void MenuWindow_LocationChanged(object sender, EventArgs e)
        {
            MoveBrowserWindowUnderMenuWindow();
        }

        private void MoveBrowserWindowUnderMenuWindow()
        {
            BrowserWindow.Left = this.Left;
            BrowserWindow.Top = this.Top + this.Height;
        }

        private void MenuWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BrowserWindow.Close();
        }

        private void MenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MenuWindow_LocationChanged(null, null);
            TextBoxUri.Text = "https://www.yahoo.co.jp/";
            BrowserWindow.Go(TextBoxUri.Text);
        }
    }
}
