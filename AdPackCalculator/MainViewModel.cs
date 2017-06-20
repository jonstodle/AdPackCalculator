using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
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

            Calculate = ReactiveCommand.CreateFromObservable<Unit, int>(
                _ => CalculateImpl(AdPackInfos, DateTimeOffset.Parse(CalculateDate)),
                Observable.CombineLatest(
                        this.WhenAnyValue(vm => vm.CalculateDate, date => DateTimeOffset.TryParse(date, out DateTimeOffset _)),
                        this.WhenAnyObservable(vm => vm.AdPackInfos.CountChanged).Select(count => count > 0),
                        (isValidDate, hasItems) => isValidDate && hasItems));

            _addAdPackInfo = this.WhenAnyValue(
                    vm => vm.AddDate,
                    vm => vm.AddAmount,
                    (date, amount) => DateTimeOffset.TryParse(date, out DateTimeOffset dto) && int.TryParse(amount, out int amnt) && amnt > 0
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

            MessageBus.Current.Listen<AdPackInfo>(AdPackInfoListViewItem.EditContract)
                .Subscribe(adPackInfo =>
                {
                    AddDate = adPackInfo.BuyDate.ToString("d");
                    AddAmount = adPackInfo.Amount.ToString();
                    AdPackInfos.Remove(adPackInfo);
                });

            MessageBus.Current.Listen<AdPackInfo>(AdPackInfoListViewItem.RemoveContract)
                .Subscribe(adPackInfo => AdPackInfos.Remove(adPackInfo));

            Observable.CombineLatest(
                    this.WhenAnyValue(
                        vm => vm.AddDate,
                        vm => vm.AddAmount,
                        vm => vm.CalculateDate),
                    this.WhenAnyObservable(vm => vm.AdPackInfos.CountChanged).Select(_ => AdPackInfos).StartWith(Enumerable.Empty<AdPackInfo>()),
                    (props, apis) => new StateObject
                    {
                        AddDate = props.Item1,
                        AddAmount = props.Item2,
                        CalculateDate = props.Item3,
                        AdPackInfos = apis
                    })
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe(stateObject => File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(stateObject)));


            if (File.Exists(_stateFilePath))
            {
                var stateObject = JsonConvert.DeserializeObject<StateObject>(File.ReadAllText(_stateFilePath));
                AddDate = stateObject.AddDate;
                AddAmount = stateObject.AddAmount;
                AdPackInfos.AddRange(stateObject.AdPackInfos);
                CalculateDate = stateObject.CalculateDate;
            }
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



        private IObservable<int> CalculateImpl(IEnumerable<AdPackInfo> adPackInfos, DateTimeOffset endDate) => Observable.Start(() =>
        {
            var adPacks = adPackInfos
                .SelectMany(adPackInfo => Enumerable.Range(1, adPackInfo.Amount).Select(_ => new AdPack(adPackInfo)))
                .OrderBy(adPack => adPack.BuyDate)
                .ToList();
            var currentDate = adPacks[0].BuyDate.Date.AddDays(1);
            var money = 0d;

            while (currentDate < endDate.AddDays(1))
            {
                var adPacksToRemove = new List<AdPack>();
                var adPacksToEvaluate = adPacks.Where(adPack => adPack.BuyDate.Date < currentDate.Date).ToList();

                for (int i = adPacksToEvaluate.Count - 1; i >= 0; i--)
                {
                    var adPack = adPacksToEvaluate[i];
                    adPack.TakeTicket();
                    adPacksToEvaluate.Remove(adPack);
                    money += .5;
                }

                while(money > 50)
                {
                    adPacks.Add(new AdPack { BuyDate = currentDate });
                    money -= 50;
                }

                foreach (var adPack in adPacksToRemove) adPacks.Remove(adPack);

                currentDate = currentDate.Date.AddDays(1);
            }

            return adPacks.Count;
        }, RxApp.TaskpoolScheduler);



        private readonly ObservableAsPropertyHelper<AdPackInfo> _addAdPackInfo;
        private readonly ObservableAsPropertyHelper<int> _calculatedAmount;
        private string _stateFilePath => Path.Combine(App.AppDataFolderPath, "data.json");
        private string _addDate;
        private string _addAmount;
        private string _calculateDate;
    }
}
