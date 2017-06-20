using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class MainViewModel : ReactiveObject, ISupportsActivation
    {
        public MainViewModel()
        {
            AddAdPackInfoItem = ReactiveCommand.Create<Unit, AdPackInfo>(
                _ => AddAdPackInfo,
                this.WhenAnyValue(vm => vm.AddAdPackInfo).Select(adPackInfo => adPackInfo != null));

            Calculate = ReactiveCommand.Create<Unit, int>(
                _ => 0,
                Observable.CombineLatest(
                        this.WhenAnyValue(vm => vm.CalculateDate, date => DateTimeOffset.TryParse(date, out DateTimeOffset _)),
                        this.WhenAnyObservable(vm => vm.AdPackInfos.CountChanged).Select(count => count > 0),
                        (isValidDate, hasItems) => isValidDate && hasItems));

            _addAdPackInfo = this.WhenAnyValue(
                    vm => vm.AddDate,
                    vm => vm.AddAmount,
                    (date, amount) => DateTimeOffset.TryParse(date, out DateTimeOffset dto) && int.TryParse(amount, out int amnt)
                        ? new AdPackInfo { BuyDate = dto, Amount = amnt }
                        : null)
                .ToProperty(this, vm => vm.AddAdPackInfo);

            _calculatedAmount = Calculate
                .ToProperty(this, vm => vm.CalculatedAmount);

            AddAdPackInfoItem
                .Subscribe(adPackInfo =>
                {
                    AdPackInfos.Add(adPackInfo);
                    AddDate = "";
                    AddAmount = "";
                });

            MessageBus.Current.Listen<AdPackInfo>(AdPackInfoListViewItem.RemoveContract)
                .Subscribe(adPackInfo => AdPackInfos.Remove(adPackInfo));
        }



        public ReactiveCommand<Unit, AdPackInfo> AddAdPackInfoItem { get; }
        public ReactiveCommand<AdPackInfo, AdPackInfo> RemoveAdPack { get; }
        public ReactiveCommand<Unit, int> Calculate { get; }
        public AdPackInfo AddAdPackInfo => _addAdPackInfo?.Value;
        public int CalculatedAmount => _calculatedAmount.Value;
        public ReactiveList<AdPackInfo> AdPackInfos { get; } = new ReactiveList<AdPackInfo>();
        public string AddDate { get => _addDate; set => this.RaiseAndSetIfChanged(ref _addDate, value); }
        public string AddAmount { get => _addAmount; set => this.RaiseAndSetIfChanged(ref _addAmount, value); }
        public string CalculateDate { get => _calculateDate; set => this.RaiseAndSetIfChanged(ref _calculateDate, value); }
        public ViewModelActivator Activator => new ViewModelActivator();



        private readonly ObservableAsPropertyHelper<AdPackInfo> _addAdPackInfo;
        private readonly ObservableAsPropertyHelper<int> _calculatedAmount;
        private string _addDate;
        private string _addAmount;
        private string _calculateDate;
    }
}
