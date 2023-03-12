using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        /// <summary>
        /// ブックマークのコレクション
        /// </summary>
        public ObservableCollection<BookMarkItem> Items { get; set; }= new ObservableCollection<BookMarkItem>();
        /// <summary>
        /// ブラウザウインドウ（ブックマーク押下イベント処理用）
        /// </summary>
        public BrowserWindow wv2 { get; set; }
        private CoreWebView2Environment wv2Environment;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="myBookMark">ブックマーク一覧</param>
        /// <param name="myBrowserWindow">ブラウザウインドウ</param>
        /// <param name="env">WebView2起動用環境</param>
        public BookMarkSwitch(ObservableCollection<BookMarkItem> myBookMark, BrowserWindow myBrowserWindow, CoreWebView2Environment env)
        {
            InitializeComponent();
            Items = myBookMark;
            listBox.ItemsSource = Items;
            wv2 = myBrowserWindow;
        }

        /// <summary>
        /// ブックマーク押下イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newUri = ((BookMarkItem)listBox.SelectedItem).Url;
            wv2.Go(newUri);
        }

        /// <summary>
        /// ブックマークドラッグドロップイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_Drop(object sender, DragEventArgs e)
        {
            var d = e.Data;
            if (d.GetDataPresent(DataFormats.Text))
            {
                var targetString = ((string)(e.Data.GetData(DataFormats.Text)));
                if (!targetString.StartsWith("http")) targetString = "https://" + targetString;
                if (Uri.IsWellFormedUriString(targetString, UriKind.Absolute))
                {
                    var newBookmarkItem = AddBookMarkItemCadidate(targetString); 
                    Items.Add(newBookmarkItem);
                }
            }
        }

        private BookMarkItem AddBookMarkItemCadidate(string UrlString)
        {
            var newBookmarkItem = new BookMarkItem()
            {
                Url = UrlString,
            };
            if (Uri.IsWellFormedUriString(newBookmarkItem.Url, UriKind.Absolute))
            {
                var ps = new System.Diagnostics.Process();
                var psi = ps.StartInfo;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.CreateNoWindow = true;
                psi.FileName = "Curl.exe";
                psi.Arguments = $"-L {UrlString}";
                ps.Start();
                var HtmlDocument = ps.StandardOutput.ReadToEnd();
                var TitleLine = Regex.Match(HtmlDocument, "<title>.*</title>").Value;
                newBookmarkItem.FaviconUrl = String.Concat(newBookmarkItem.Url.Split('/').Take(3).Select(s => $"{s}/")) + "favicon.ico";
                newBookmarkItem.PageTitle = TitleLine.Replace("<title>", "").Replace("</title>", "");
                newBookmarkItem.isUpdating = true;
            }
            return newBookmarkItem;
        }
    }
}
