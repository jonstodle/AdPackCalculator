using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.AddDate, v => v.AddDateTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AddDate, v => v.AddDateTextBox.Background, date => InvalidInputToBrush(DateTimeOffset.TryParse(date, out DateTimeOffset _))).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.AddAmount, v => v.AddAmountTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.AddAmount, v => v.AddAmountTextBox.Background, value => InvalidInputToBrush(int.TryParse(value, out int _))).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.AddAdPackInfoItem, v => v.AddButton).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.AdPackInfos, v => v.AdPackInfosListView.ItemsSource).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.CalculateDate, v => v.CalculateDateTextBox.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CalculateDate, v => v.CalculateDateTextBox.Background, date => InvalidInputToBrush(DateTimeOffset.TryParse(date, out DateTimeOffset _))).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.Calculate, v => v.CalculateButton).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CalculatedAmount, v => v.CalculateResultTextBlock.Text).DisposeWith(d);
            });
        }

        public MainViewModel ViewModel { get => (MainViewModel)GetValue(ViewModelProperty); set => SetValue(ViewModelProperty, value); }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));
        object IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as MainViewModel; }

        private SolidColorBrush InvalidInputToBrush(bool isValid) => new SolidColorBrush { Color = isValid ? Colors.Transparent : Colors.Red, Opacity = .5 };
    }
}
