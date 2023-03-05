﻿using Microsoft.Web.WebView2.WinForms;
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
                    newBookmarkItem.FaviconUrl ="";
                    newBookmarkItem.PageTitle = "Loading..";
                    newBookmarkItem.isUpdating = true;
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
            System.Diagnostics.Debug.Print("Web Load Done.");
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

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
