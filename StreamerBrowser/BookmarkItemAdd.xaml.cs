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
    /// BookmarkItemAdd.xaml の相互作用ロジック
    /// </summary>
    public partial class BookmarkItemAdd : Window
    {
        /// <summary>
        /// ブックマーク追加ウインドウ　コンストラクタ
        /// </summary>
        public BookmarkItemAdd()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OKボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
