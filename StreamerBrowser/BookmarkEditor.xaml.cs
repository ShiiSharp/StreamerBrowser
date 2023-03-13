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
using System.Windows.Threading;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;

namespace StreamerBrowser
{
    /// <summary>
    /// BookmarkEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkEditor : Window
    {
        /// <summary>
        /// ブックマークのコレクション
        /// </summary>
        public ObservableCollection<BookMarkItem> bookMarkItems = new ObservableCollection<BookMarkItem>();
        private CoreWebView2Environment environment;
        private HttpListener httpListener = new HttpListener();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="myBookMarkItems">ブックマークの初期コレクション</param>
        /// <param name="env">WebView2用の環境</param>
        public BookmarkEditor(ObservableCollection<BookMarkItem> myBookMarkItems, CoreWebView2Environment env)
        {
            InitializeComponent();
            this.environment = env;
            foreach(var myBookMark in myBookMarkItems) 
            {
                bookMarkItems.Add(myBookMark);
            }
            ListArea.ItemsSource = bookMarkItems;
        }

        /// <summary>
        /// 追加ボタンイベント処理
        /// </summary>
        /// <param name="sender">イベント発火オブジェクト</param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var BookmarkItemAddWindow = new BookmarkItemAdd();
            var dialogResult = BookmarkItemAddWindow.ShowDialog();
            if (dialogResult == true)
            {
                var UrlString = BookmarkItemAddWindow.TextBoxUri.Text;
                BookMarkItem newBookmarkItem = AddBookMarkItemCadidate(UrlString);
                bookMarkItems.Add(newBookmarkItem);
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

        /// <summary>
        /// 削除ボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1)
                return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Remove((BookMarkItem)ListArea.SelectedItem);
            if (bookMarkItems.Count == idx) idx--;
            ListArea.SelectedIndex = idx;
        }

        /// <summary>
        /// 上へボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1) return;
            if (ListArea.SelectedIndex == 0) return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Move(idx, idx - 1);
            ListArea.SelectedIndex = idx- 1;
        }

        /// <summary>
        /// 下へボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListArea.SelectedIndex == -1) return;
            if (ListArea.SelectedIndex == bookMarkItems.Count-1) return;
            var idx = ListArea.SelectedIndex;
            bookMarkItems.Move(idx, idx + 1);
            ListArea.SelectedIndex = idx + 1;
        }

        /// <summary>
        /// OKボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanceldButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            this.Close();
        }

        /// <summary>
        /// ブラウザ表示終了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wv2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var wv2 = ((Microsoft.Web.WebView2.Wpf.WebView2) sender);
            var url = wv2.Source.ToString();
            var target = bookMarkItems.FirstOrDefault(b => b.isUpdating);
            if (target == null) return;
            target.isUpdating = false;
            target.Url = url;
            target.PageTitle = wv2.CoreWebView2.DocumentTitle;
            target.FaviconUrl = wv2.CoreWebView2.FaviconUri;
            ListArea.ItemsSource = null;
            ListArea.ItemsSource = bookMarkItems;
            //DoEvents();
        }

        /// <summary>
        /// 一覧エリアドラッグドロップイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListArea_Drop(object sender, DragEventArgs e)
        {
            var d = e.Data;
            if (d.GetDataPresent(DataFormats.Text))
            {
                var targetString = ((string)(e.Data.GetData(DataFormats.Text)));
                if (!targetString.StartsWith("http")) targetString = "https://" + targetString;
                if (Uri.IsWellFormedUriString(targetString, UriKind.Absolute))
                {
                    var newBookmarkItem = AddBookMarkItemCadidate(targetString);
                    bookMarkItems.Add(newBookmarkItem);
                }
            }
        }
    }
}
