using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
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
using System.Collections.Immutable;
using Microsoft.Web.WebView2.Core;
using System.Net;

namespace StreamerBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<BookMarkItem> bookMarkItems = new ObservableCollection<BookMarkItem>();
        String NGWord = "";
        private BrowserWindow BrowserWindow;
        private BookMarkSwitch bookMarkSwitch;
        private String bookmarkFileName = "bookmark.lst";
        private String NGWordFileName = "NGWords.lst";
        private String ResolutionFileName = "Resolution.lst";
        private HttpListener httpListener = new HttpListener();
        /// <summary>
        /// WebViewの起動環境を保存するためのプロパティ
        /// </summary>
        public CoreWebView2Environment coreWebView2Environment { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // ブックマーク読み込み
            if (File.Exists(bookmarkFileName))
            {
                var tmp = File.ReadLines(bookmarkFileName);
                foreach(var line in tmp)
                {
                    var splitted = line.Split('\t');
                    if (splitted.Count() == 3)
                    {
                        var newBookMarkItem = new BookMarkItem()
                        {
                            Url = splitted[0],
                            FaviconUrl = splitted[1],
                            isUpdating = false,
                            PageTitle = splitted[2]
                        }
                        ;
                        bookMarkItems.Add(newBookMarkItem);                       
                    }
                }
            }
            bookMarkItems.CollectionChanged += BookMarkItems_CollectionChanged;
            //NGワード読み込み
            if (File.Exists(NGWordFileName))
            {
                NGWord = File.ReadAllText(NGWordFileName);
            }
            //解像度読み込み
            if (File.Exists(ResolutionFileName))
            {
                var firstResolution = File.ReadAllText(ResolutionFileName);
            }
            //各ウインドウ生成・表示
            BrowserWindow = new BrowserWindow(coreWebView2Environment);
            BrowserWindow.NGWords = NGWord.Split(' ').ToList();
            BrowserWindow.Show();
            BrowserWindow.Browser.NavigationCompleted += Browser_NavigationCompleted; ;
            BrowserWindow.Browser.NavigationStarting += Browser_NavigationStarting;

            bookMarkSwitch = new BookMarkSwitch(bookMarkItems, BrowserWindow, coreWebView2Environment);
            bookMarkSwitch.Height = BrowserWindow.Height;
            bookMarkSwitch.Top = BrowserWindow.Top;
            bookMarkSwitch.Left = BrowserWindow.Left + BrowserWindow.Width;
            bookMarkSwitch.Show();
        }

        /// <summary>
        /// ブックマークコレクション変更イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookMarkItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveBookMarks();
        }

        /// <summary>
        /// ブラウザ遷移開始イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_NavigationStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            ButtonGoBack.IsEnabled = false;
            ButtonGoForward.IsEnabled = false;
        }

        /// <summary>
        /// ブラウザ遷移終了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            TextBoxUri.Text = BrowserWindow.Browser.Source.ToString();
            ButtonGoBack.IsEnabled = BrowserWindow.Browser.CanGoBack;
            ButtonGoForward.IsEnabled = BrowserWindow.Browser.CanGoForward;
        }

        /// <summary>
        /// 「戻る」ボタン押下イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            TextBoxUri.Text = BrowserWindow.GoBack().ToString();
        }

        /// <summary>
        /// 「進む」ボタン押下イベント処理 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGoForward_Click(object sender, RoutedEventArgs e)
        {
            TextBoxUri.Text = BrowserWindow.GoForward().ToString();
        }

        /// <summary>
        /// URL入力欄キーボード入力イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxUri_KeyDown(object sender, KeyEventArgs e)
        {
            var typedKey = e.Key;
            if (typedKey==Key.Enter||typedKey==Key.Return)
            {
                var newUri = GetUri(TextBoxUri.Text);
                TextBoxUri.Text = BrowserWindow.Go(newUri).ToString();
            }
        }

        /// <summary>
        /// URL文字列として適当かどうかを判断し、だめならGoogleの検索クエリURLに変更するメソッド
        /// </summary>
        /// <param name="uriString">URL文字列候補</param>
        /// <returns>URL文字列</returns>
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

        /// <summary>
        /// ウインドウ移動イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuWindow_LocationChanged(object sender, EventArgs e)
        {
            MoveBrowserWindowUnderMenuWindow();
        }

        /// <summary>
        /// ウインドウの整列
        /// </summary>
        private void MoveBrowserWindowUnderMenuWindow()
        {
            this.Width = BrowserWindow.Width;
            BrowserWindow.Left = this.Left;
            BrowserWindow.Top = this.Top + this.Height;
            bookMarkSwitch.Height = this.Height + BrowserWindow.Height;
            bookMarkSwitch.Top = this.Top;
            bookMarkSwitch.Left = BrowserWindow.Left + BrowserWindow.Width;
        }

        /// <summary>
        /// 終了イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveBookMarks();
            BrowserWindow.Close();
            bookMarkSwitch.Close();
        }

        /// <summary>
        /// ウインドウ表示イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ResolutionFileName))
            {
                var firstResolution = File.ReadAllText(ResolutionFileName);
                ChangeResolution(firstResolution);
            }
            var HomePageUrl = bookMarkItems.Count==0?"https://www.yahoo.co.jp/":bookMarkItems.First().Url; 
            TextBoxUri.Text = HomePageUrl;
            BrowserWindow.Go(TextBoxUri.Text);
            await HttpListenerLoop();
        }

        private async Task HttpListenerLoop()
        {
            httpListener.Prefixes.Add("http://localhost:9801/");
            httpListener.Start();
            while(true)
            {
                await LaunchServer();
            }
        }

        private async Task LaunchServer()
        {
            var context = await httpListener.GetContextAsync();
            var request = context.Request;
            var absolutePaths = request.Url.AbsolutePath.Split('/');
            if (absolutePaths.Count() == 0) { ReturnError(context); return; }
            var index = 0;
            if (absolutePaths[1] == "favicon.ico") { ReturnError(context); return; }
            if (absolutePaths[1] == "fx") { ToggleBlur(context);return; }
            Int32.TryParse(absolutePaths[1], out index);
            if (index < 0) { ReturnError(context); return; }
            if (index >= bookMarkItems.Count) { ReturnError(context); return; }
            var response = context.Response;
            var bookmarkItem = bookMarkItems[index];
            BrowserWindow.Go(bookmarkItem.Url);
            response.ContentType= "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = (int)HttpStatusCode.OK;
            var responseString = $"<html><head><meta charset=\"utf-8\"/></head><body><img src=\"{bookmarkItem.FaviconUrl}\"/>「{bookmarkItem.PageTitle}」に移動しました。</body></html>";
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = responseBytes.Length;
            await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            response.OutputStream.Close();

        }

        private void ToggleBlur(HttpListenerContext context)
        {
            BrowserWindow.ToggleBlur();
            var response = context.Response as HttpListenerResponse;
            response.StatusCode = (Int32)(HttpStatusCode.OK);
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html";
            var Message = "<html><body><h1>Browser Window Blur Toggled</h1></body></html>";
            var MessageBytes = System.Text.Encoding.UTF8.GetBytes(Message);
            response.ContentLength64 = MessageBytes.Length;
            response.OutputStream.Write(MessageBytes, 0, MessageBytes.Length);
        }

        /// <summary>
        /// ぼかしトグルボタン押下イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonToggleBlur_Click(object sender, RoutedEventArgs e)
        {
            BrowserWindow.ToggleBlur();
        }

        /// <summary>
        /// 「NGワード編集」ボタン押下処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void NGWordEdit_Click(object sender, RoutedEventArgs e)
        {
            var NGWordEdit = new NGWordEditor(NGWord.Split(' ').ToList());
            var dialogResult = NGWordEdit.ShowDialog();
            if (dialogResult == true)
            {
                NGWord = String.Concat(NGWordEdit.NGWordDB.Select(s => $"{s} "));
            }
            BrowserWindow.NGWords = NGWordEdit.NGWordDB;
            File.WriteAllText(NGWordFileName, NGWord);
        }

        /// <summary>
        /// 「ブックマーク編集」ボタン押下イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookMarkEdit_Click(object sender, RoutedEventArgs e)
        {
            var boorkmarkEditor = new BookmarkEditor(bookMarkItems, coreWebView2Environment);
            var dialogResult = boorkmarkEditor.ShowDialog();
            if (dialogResult == true)
            {
                bookMarkItems.Clear();
                foreach (var bookMarkItem in boorkmarkEditor.bookMarkItems)
                {
                    bookMarkItems.Add(bookMarkItem);
                }
            }
        }

        /// <summary>
        /// ブックマーク保存処理
        /// </summary>
        private void SaveBookMarks()
        {
            File.WriteAllLines(bookmarkFileName, bookMarkItems.Select(b => $"{b.Url}\t{b.FaviconUrl}\t{b.PageTitle}"));
        }

        /// <summary>
        /// 「ブックマーク追加」ボタン押下処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddToBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (BrowserWindow.Browser.CoreWebView2!=null)
            {
                bookMarkItems.Add(new BookMarkItem()
                {
                    Url = BrowserWindow.Browser.Source.ToString(),
                    FaviconUrl = BrowserWindow.Browser.CoreWebView2.FaviconUri,
                    PageTitle = BrowserWindow.Browser.CoreWebView2.DocumentTitle,
                    isUpdating = false
                }); 
                

            }
        }

        /// <summary>
        /// 解像度メニュー押下イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var theMenuItem = (MenuItem)sender;
            var theTitle = (String)(theMenuItem.Header);
            ChangeResolution(theTitle);
            File.WriteAllText(ResolutionFileName, theTitle);
        }

        /// <summary>
        /// 解像度変更処理
        /// </summary>
        /// <param name="theTitle"></param>
        private void ChangeResolution(string theTitle)
        {
            try
            {
                var xy = theTitle.Split('x').Select(s => Convert.ToInt32(s)).ToArray();
                BrowserWindow.Width = xy[0];
                BrowserWindow.Height = xy[1];
                MoveBrowserWindowUnderMenuWindow();
            }
            catch 
            {
            }   
        }
        private void ReturnError(HttpListenerContext context)
        {
            var response = context.Response as HttpListenerResponse;
            response.StatusCode = (Int32)(HttpStatusCode.NotFound);
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html";
            var ErrorMessage = "<html><body><h1>404 Not Found</h1></body></html>";
            var ErrorBytes = System.Text.Encoding.UTF8.GetBytes(ErrorMessage);
            response.ContentLength64 = ErrorBytes.Length;
            response.OutputStream.Write(ErrorBytes, 0, ErrorBytes.Length);
        }
    }


        /// <summary>
        /// ブックマーク１アイテム
        /// </summary>
        public class BookMarkItem
    {
        /// <summary>
        /// URL文字列
        /// </summary>
        public string Url { get; set; } = "";
        /// <summary>
        /// ウェブページのタイトル文字列
        /// </summary>
        public string PageTitle { get; set; } = "";
        /// <summary>
        /// ウェブページのアイコンURL文字列
        /// </summary>
        public string FaviconUrl { get; set; } = "";
        /// <summary>
        /// アップデート中フラグ
        /// </summary>
        public bool isUpdating { get; set; } = true;
    }
}
