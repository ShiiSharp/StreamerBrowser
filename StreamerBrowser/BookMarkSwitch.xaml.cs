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
    }
}
