using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// <summary>
        /// NGワードコレクション
        /// </summary>
        public List<String> NGWords { get; set; } = new List<string>();
        
        /// <summary>
        /// ぼかしサイズ
        /// </summary>
        public Int32 BlurSize { get; set; } = 8;

        /// <summary>
        /// ぼかし中フラグ
        /// </summary>
        public bool isBlured { get; set; } = true;

        private CoreWebView2Environment CoreWebView2Environment;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BrowserWindow(CoreWebView2Environment env)
        {
            InitializeComponent();
            CoreWebView2Environment = env;
        }

        /// <summary>
        /// ブラウザ「戻る」処理
        /// </summary>
        /// <returns></returns>
        public Uri GoBack()
        {
            if (Browser.CanGoBack) Browser.GoBack();
            return Browser.Source; 
        }

        /// <summary>
        /// ブラウザ進む処理
        /// </summary>
        /// <returns></returns>
        public Uri GoForward()
        {
            if (Browser.CanGoForward) Browser.GoForward();
            return Browser.Source;
        }

        /// <summary>
        /// ブラウザ「URL移動」処理
        /// </summary>
        /// <param name="uriString">URL文字列</param>
        /// <returns>Uriオブジェクト</returns>
        public Uri Go(String uriString)
        {
            ChangeBrowserBlur(16);
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

        /// <summary>
        /// ブラウザ「再読み込み」処理
        /// </summary>
        /// <returns></returns>
        public Uri Reload()
        {
            Browser.Reload();
            return Browser.Source;
        }

        /// <summary>
        /// ウインドウ読み込み後初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 音声リダイレクト無効化
            var options = new CoreWebView2EnvironmentOptions()
            {
                AdditionalBrowserArguments = "--disable-features=AudioServiceOutOfProcess"
            };
            CoreWebView2Environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await Browser.EnsureCoreWebView2Async(CoreWebView2Environment);
        }

        /// <summary>
        /// ブラウザ遷移終了イベント処理(NGワードチェック・ぼかし解除)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Browser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            Console.WriteLine("NavigationCompleted!");
            if (Browser.CoreWebView2!=null)
            {
                var outerHTML = await Browser.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");
                var hasNGWord = NGWords.Any(s => outerHTML.Contains(s));
                if (hasNGWord) 
                {
                    ChangeBrowserBlur(BlurSize);
                    return; 
                }
            }
            ChangeBrowserBlur(0);
        }
        
        /// <summary>
        /// ブラウザ遷移開始イベント処理(ぼかし有効化)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            ChangeBrowserBlur(BlurSize, .25); 
        }

        /// <summary>
        /// ぼかし切り替え
        /// </summary>
        public void ToggleBlur(bool? ToggleMode = null)
        {
            if (ToggleMode == true) 
            {
                isBlured = false;
            }
            if (ToggleMode ==false)
            {
                isBlured = true;
            }
            if (isBlured)
            {
                ChangeBrowserBlur(0);
                isBlured = false;
            } 
            else
            {
                ChangeBrowserBlur(BlurSize);
                isBlured = true;
            }
        }

        /// <summary>
        /// ぼかし処理
        /// </summary>
        /// <param name="radius">ぼかし半径</param>
        /// <param name="zoomFactor">拡大倍率</param>
        public void ChangeBrowserBlur(Int32 radius, double zoomFactor = 1.0)
        {
            isBlured = true;
            if (radius == 0) isBlured = false;
            if (Browser.CoreWebView2!!=null)
            {
                var script = $"document.body.style.filter='blur({radius}px)';";
                Browser.CoreWebView2.ExecuteScriptAsync(script);
            }
            Browser.ZoomFactor = zoomFactor;
        }
    }
}
