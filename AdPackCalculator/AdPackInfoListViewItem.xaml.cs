﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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

namespace AdPackCalculator
{
    /// <summary>
    /// Interaction logic for AdPackInfoListViewItem.xaml
    /// </summary>
    public partial class AdPackInfoListViewItem : UserControl
    {
        public static string EditContract = nameof(EditContract);
        public static string RemoveContract = nameof(RemoveContract);



        public AdPackInfoListViewItem()
        {
            InitializeComponent();

            Observable.Merge(
                    Observable.FromEventPattern(this, nameof(MouseEnter)).Select(_ => true),
                    Observable.FromEventPattern(this, nameof(MouseLeave)).Select(_ => false))
                .Subscribe(showButtons =>
                {
                    EditButton.IsEnabled = RemoveButton.IsEnabled = showButtons;
                    EditButton.Opacity = RemoveButton.Opacity = showButtons ? 1 : 0;
                });

            MessageBus.Current.RegisterMessageSource(
                Observable.FromEventPattern(EditButton, nameof(Button.Click)).Select(_ => AdPackInfo).Where(api => api != null),
                EditContract);

            MessageBus.Current.RegisterMessageSource(
                Observable.FromEventPattern(RemoveButton, nameof(Button.Click)).Select(_ => AdPackInfo).Where(api => api != null),
                RemoveContract);
        }



        public AdPackInfo AdPackInfo => DataContext as AdPackInfo;
    }
}
