using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// BookMarkSwitch.xaml の相互作用ロジック
    /// </summary>
    public partial class BookMarkSwitch : Window
    {
        public ObservableCollection<BookMarkItem> Items { get; set; }= new ObservableCollection<BookMarkItem>();
        public BrowserWindow wv2 { get; set; }
        
        public BookMarkSwitch(ObservableCollection<BookMarkItem> myBookMark, BrowserWindow myBrowserWindow)
        {
            InitializeComponent();
            Items = myBookMark;
            listBox.ItemsSource = Items;
            wv2 = myBrowserWindow;
        }


        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newUri = ((BookMarkItem)listBox.SelectedItem).Url;
            wv2.Go(newUri);
        }

        private void listBox_Drop(object sender, DragEventArgs e)
        {
            var d = e.Data;
            if (d.GetDataPresent(DataFormats.Text))
            {
                var targetString = ((string)(e.Data.GetData(DataFormats.Text)));
                if (!targetString.StartsWith("http")) targetString = "https://" + targetString;
                if (Uri.IsWellFormedUriString(targetString, UriKind.Absolute))
                {
                    var newBookmarkItem = new BookMarkItem()
                    { Url = targetString };
                    wv2_mini.Source = new Uri(newBookmarkItem.Url);
                    newBookmarkItem.FaviconUrl = "";
                    newBookmarkItem.PageTitle = "Loading..";
                    newBookmarkItem.isUpdating = true;
                    Items.Add(newBookmarkItem);
                }

            }
        }

        private void wv2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var wv2x = ((Microsoft.Web.WebView2.Wpf.WebView2)sender);
            var url = wv2x.Source.ToString();
            var target = Items.FirstOrDefault(b => b.isUpdating);
            if (target == null) return;
            target.isUpdating = false;
            target.Url = url;
            target.PageTitle = wv2x.CoreWebView2.DocumentTitle;
            target.FaviconUrl = wv2x.CoreWebView2.FaviconUri;
            listBox.ItemsSource = null;
            listBox.ItemsSource = Items;

        }
    }
}
