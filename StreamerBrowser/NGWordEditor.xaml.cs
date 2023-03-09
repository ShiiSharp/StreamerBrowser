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
    /// NGWordEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class NGWordEditor : Window
    {
        /// <summary>
        /// NGWordのデータベース
        /// </summary>
        public List<String> NGWordDB { get; set; } =new List<String>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="NGWordsParam">NGWordのリスト</param>
        public NGWordEditor(List<String> NGWordsParam)
        {
            InitializeComponent();

            TextBoxNGWord.Text = String.Concat(NGWordsParam.Select(s => $"{s}\r\n"));

        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            NGWordDB.Clear();
            NGWordDB.AddRange(TextBoxNGWord.Text.Replace("\r","").Split('\n'));
            DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
