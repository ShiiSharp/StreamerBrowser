using Microsoft.Web.WebView2.WinForms;
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
using Microsoft.Web.WebView2;
using System.Collections.ObjectModel;

namespace StreamerBrowser
{
    /// <summary>
    /// BookmarkEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkEditor : Window
    {
        public ObservableCollection<BookMarkItem> bookMarkItems = new ObservableCollection<BookMarkItem>();


        public BookmarkEditor(ObservableCollection<BookMarkItem> myBookMarkItems)
        {
            InitializeComponent();
            wv2.Source = new Uri("https://www.google.com/");
            foreach(var myBookMark in myBookMarkItems) 
            {
                bookMarkItems.Add(myBookMark);
            }
            ListArea.ItemsSource = bookMarkItems;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var BookmarkItemAddWindow = new BookmarkItemAdd();
            var dialogResult = BookmarkItemAddWindow.ShowDialog();
            if (dialogResult == true) 
            {
                var newBookmarkItem = new BookMarkItem()
                {
                    Url = BookmarkItemAddWindow.TextBoxUri.Text,
                };
                if (Uri.IsWellFormedUriString(newBookmarkItem.Url, UriKind.Absolute))
                {
                    wv2.Source = new Uri(newBookmarkItem.Url);
                    //wv2.EnsureCoreWebView2Async().Wait();
                    while (wv2.CoreWebView2 == null)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    newBookmarkItem.FaviconUrl = wv2.CoreWebView2.FaviconUri;
                    newBookmarkItem.PageTitle = wv2.CoreWebView2.DocumentTitle;
                }
                System.Diagnostics.Debug.Print(newBookmarkItem.Url);
                System.Diagnostics.Debug.Print(newBookmarkItem.PageTitle);
                System.Diagnostics.Debug.Print(newBookmarkItem.FaviconUrl);
                bookMarkItems.Add(newBookmarkItem);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1)
                return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Remove((BookMarkItem)ListArea.SelectedItem);
            if (bookMarkItems.Count == idx) idx--;
            ListArea.SelectedIndex = idx;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1) return;
            if (ListArea.SelectedIndex == 0) return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Move(idx, idx - 1);
            ListArea.SelectedIndex = idx- 1;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1) return;
            if (ListArea.SelectedIndex == bookMarkItems.Count-1) return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Move(idx, idx + 1);
            ListArea.SelectedIndex = idx + 1;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void CanceldButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            this.Close();
        }

        private void wv2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var wv2 = ((Microsoft.Web.WebView2.Wpf.WebView2) sender);
            var url = wv2.Source.ToString();
            var targets = bookMarkItems.Where(b => b.Url == url);
            foreach(var target in targets) 
            {
                target.PageTitle = wv2.CoreWebView2.DocumentTitle;
                target.FaviconUrl = wv2.CoreWebView2.FaviconUri;
            }
        }
    }
}
