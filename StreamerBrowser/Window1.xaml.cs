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
using System.Windows.Shapes;

namespace StreamerBrowser
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class BrowserWindow : Window
    {
        public List<String> NGWords { get; set; } = new List<string>();

        public BrowserWindow()
        {
            InitializeComponent();
        }

        public Uri GoBack()
        {
            if (Browser.CanGoBack) Browser.GoBack();
            return Browser.Source; 
        }

        public Uri GoForward()
        {
            if (Browser.CanGoForward) Browser.GoForward();
            return Browser.Source;
        }

        public Uri Go(String uriString)
        {
            Browser.Visibility = Visibility.Hidden;
            if (Browser.CoreWebView2 == null)
            {
                Browser.Source = new Uri(uriString);
            }
            else
            {
                Browser.CoreWebView2.Navigate(uriString);
            }
            while (!Browser.IsLoaded)
            {
                System.Threading.Thread.Sleep(100);
            }
            return Browser.Source;
        }

        public Uri Reload()
        {
            Browser.Reload();
            return Browser.Source;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Browser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Console.WriteLine("NavigationCompleted!");
            if (Browser.CoreWebView2!=null)
            {
                var outerHTML = await Browser.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
                var hasNGWord = NGWords.Any(s => outerHTML.Contains(s));
                if (hasNGWord) 
                {
                    Browser.Visibility = Visibility.Hidden;
                    return; 
                }
            }
            Browser.Visibility=Visibility.Visible;
        }

        private void Browser_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            Browser.Visibility = Visibility.Hidden;
        }
    }
}
