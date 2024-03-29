﻿using System;
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
using System.Diagnostics;
using System.Windows.Resources;

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

            //現在のプロセスがすでに実行されているかどうかを確認する
            Process current = Process.GetCurrentProcess();

            //現在のプロセスと同じ名前のすべてのプロセスを取得します
            Process[] processes = Process.GetProcessesByName(current.ProcessName);

            //同じ名前のプロセスが複数ある場合は、メッセージを表示して終了します
            if (processes.Length > 1)
            {
                MessageBox.Show("すでに起動しています。");
                Environment.Exit(0);
            }
            InitializeComponent();
            // ブックマーク読み込み
            if (File.Exists(bookmarkFileName))
            {
                var tmp = File.ReadLines(bookmarkFileName);
                foreach (var line in tmp)
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
            if (typedKey == Key.Enter || typedKey == Key.Return)
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
        //このコードは、メニュー ウィンドウが閉じるときに実行されます。ブックマークを保存し、ブラウザ ウィンドウを閉じ、ブック マーク スイッチを閉じます。 
        private void MenuWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Save the bookmarks
            SaveBookMarks();
            //Close the BrowserWindow
            BrowserWindow.Close();
            //Close the bookMarkSwitch
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
            var HomePageUrl = bookMarkItems.Count == 0 ? "https://www.yahoo.co.jp/" : bookMarkItems.First().Url;
            TextBoxUri.Text = HomePageUrl;
            BrowserWindow.Go(TextBoxUri.Text);
            httpListener.Stop();
            await HttpListenerLoop();
            BrowserWindow.Owner = this;
            bookMarkSwitch.Owner = this;

        }


        /// <summary>
        /// This method starts an HTTP listener loop that listens for requests on port 9801.
        /// </summary>
        /// <returns>
        /// An asynchronous task that launches the server.
        /// </returns>

        //This code creates an HTTP listener on port 9801 and starts a loop that will launch the server each time it is called.
        private async Task HttpListenerLoop()
        {
            //Add the prefix for the HTTP listener
            httpListener.Prefixes.Add("http://localhost:9801/");
            //Start the HTTP listener
            httpListener.Start();
            //Create an infinite loop
            while (true)
            {
                //Call the LaunchServer method each time the loop is called
                await LaunchServer();
            }
        }

        /// <summary>
        /// Launches the server and navigates to the bookmark item URL.
        /// </summary>
        /// <returns>
        /// Task representing the asynchronous operation.
        /// </returns>


        //Rewritten code with comments

        private async Task LaunchServer()
        {
            //リクエストのコンテキストを取得する
            var context = await httpListener.GetContextAsync();
            var request = context.Request;
            //リクエストの絶対パスを分割する
            var absolutePaths = request.Url.AbsolutePath.Split('/');
            //絶対パスが空かどうかを確認する
            if (absolutePaths.Count() == 0) { ReturnError(context); return; }
            var index = 0;
            //Check if the request is for favicon.ico
            if (absolutePaths[1] == "favicon.ico") { ReturnError(context); return; }
            //Check if the request is for toggle blur
            if (absolutePaths[1] == "fx") { ToggleBlur(context); return; }
            //Try to parse the absolute path to an integer
            Int32.TryParse(absolutePaths[1], out index);
            //Check if the index is out of bounds
            if (index == bookMarkItems.Count) { ReturnError(context); return; }
            var response = context.Response;
            //Get the bookmark item from the list
            var bookmarkItem = bookMarkItems[index];
            //Go to the URL of the bookmark item
            BrowserWindow.Go(bookmarkItem.Url);
            //Set the response content type
            response.ContentType = "text/html";
            //Set the response encoding
            response.ContentEncoding = Encoding.UTF8;
            //Set the response status code
            response.StatusCode = (int)HttpStatusCode.OK;
            //Create the response string
            var responseString = $"      「{bookmarkItem.PageTitle}」に移動しました。  ";
            //Get the response bytes
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
            //Set the response content length
            response.ContentLength64 = responseBytes.Length;
            //Write the response bytes to the output stream
            await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            //Close the output stream
            response.OutputStream.Close();
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
            if (BrowserWindow.Browser.CoreWebView2 != null)
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

        /// <summary>
        /// Sends a 404 Not Found response to the client.
        /// </summary>
        /// <param name="context">The HttpListenerContext to send the response to.</param>
        private void ReturnError(HttpListenerContext context)
        {

            //応答ステータス コードを 404 Not Found に設定します。
            var response = context.Response as HttpListenerResponse;
            response.StatusCode = (Int32)(HttpStatusCode.NotFound);

            //コンテンツのエンコーディングを UTF8 に設定する
            response.ContentEncoding = Encoding.UTF8;

            //コンテンツ タイプを text/html に設定します
            response.ContentType = "text/html";

            //エラー メッセージを含む文字列を作成する
            var ErrorMessage = "<html><body>404 Not Found</body></html>";

            //エラー メッセージ文字列をバイトに変換する
            var ErrorBytes = System.Text.Encoding.UTF8.GetBytes(ErrorMessage);

            //コンテンツの長さをエラー メッセージのバイトの長さに設定します。
            response.ContentLength64 = ErrorBytes.Length;

            //エラーメッセージバイトを応答出力ストリームに書き込みます
            response.OutputStream.Write(ErrorBytes, 0, ErrorBytes.Length);

            //応答出力ストリームを閉じる
            response.OutputStream.Close();
        }


        /// <summary>
        /// リクエストに基づいてブラウザ ウィンドウのぼかしを切り替えます。
        /// </summary>

        //このコードは URL リクエストを解析し、値を使用してぼかしフラグを切り替えます。
        //次に、ぼかしフラグのステータスを示すメッセージを含む応答を送信します。
        private void ToggleBlur(HttpListenerContext context)
        {
            //Get the absolute path of the request
            var request = context.Request.Url.AbsolutePath;
            //Split the request into chunks
            var requestChunk = request.Split('/');
            //Declare a boolean flag to store the blur status
            bool? BlurFlag = null;
            //If the request has more than 2 chunks
            if (requestChunk.Count() > 2)
            {
                //Declare a variable to store the parsed value
                var num = 0;
                //Try to parse the value from the request
                if (Int32.TryParse(requestChunk[2], out num))
                {
                    //If the value is 0, set the blur flag to true
                    if (num == 0) BlurFlag = true;
                    //Otherwise, set the blur flag to false
                    else BlurFlag = false;
                }
            }
            //ぼかしフラグを切り替えます
            BrowserWindow.ToggleBlur(BlurFlag);
            //応答オブジェクトを取得する
            var response = context.Response as HttpListenerResponse;
            //応答ステータス コードを OK に設定します
            response.StatusCode = (Int32)(HttpStatusCode.OK);
            //応答エンコーディングを UTF8 に設定します
            response.ContentEncoding = Encoding.UTF8;
            //応答のコンテンツ タイプを text/html に設定します
            response.ContentType = "text/html";
            //ぼかしフラグのステータスを示すメッセージを作成する
            var ErrorMessage = $"<html><body><H1>Blur Changed {BlurFlag}</H1></body></html>";
            //Convert the message to bytes
            var ErrorBytes = System.Text.Encoding.UTF8.GetBytes(ErrorMessage);
            //Set the response content length to the length of the message
            response.ContentLength64 = ErrorBytes.Length;
            //メッセージを応答出力ストリームに書き込みます
            response.OutputStream.Write(ErrorBytes, 0, ErrorBytes.Length);
        }

        private void MenuWindow_Activated(object sender, EventArgs e)
        {
            if (BrowserWindow != null)
            {
                BrowserWindow.Topmost = true;
            }
            if (bookMarkSwitch != null)
            {
                bookMarkSwitch.Topmost = true;
            }
            if (BrowserWindow != null)
            {
                BrowserWindow.Topmost = false;
            }
            if (bookMarkSwitch != null)
            {
                bookMarkSwitch.Topmost = false;
            }
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
