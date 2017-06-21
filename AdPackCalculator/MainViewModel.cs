using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace AdPackCalculator
{
    public class MainViewModel : ReactiveObject, ISupportsActivation
    {
        public MainViewModel()
        {
            if (File.Exists(_stateFilePath)) _state = JsonConvert.DeserializeObject<StateObject>(File.ReadAllText(_stateFilePath));
            else _state = new StateObject();

            AddAdPackInfoItem = ReactiveCommand.Create<Unit, AdPackInfo>(
                _ => AddAdPackInfo,
                this.WhenAnyValue(vm => vm.AddAdPackInfo).Select(adPackInfo => adPackInfo != null));

            Calculate = ReactiveCommand.CreateFromObservable<Unit, int>(
                _ => CalculateImpl(AdPackInfos, DateTimeOffset.Parse(CalculateDate), SettingsToSave),
                Observable.CombineLatest(
                        this.WhenAnyValue(vm => vm.CalculateDate, date => DateTimeOffset.TryParse(date, out DateTimeOffset _)),
                        this.WhenAnyObservable(vm => vm.AdPackInfos.CountChanged).Select(count => count > 0),
                        (isValidDate, hasItems) => isValidDate && hasItems));

            SaveSettings = ReactiveCommand.Create<Unit, SettingsObject>(
                _ => new SettingsObject
                {
                    AdPackCost = double.Parse(AdPackCost),
                    AdPackDuration = int.Parse(AdPackDuration),
                    AdPackIncomePerDay = double.Parse(AdPackIncomePerDay),
                    ReservePercentage = double.Parse(ReservePercentage)
                },
                this.WhenAnyValue(
                    vm => vm.AdPackCost, vm => vm.AdPackDuration,
                    vm => vm.AdPackIncomePerDay, vm => vm.ReservePercentage,
                    (cost, duration, income, percentage) =>
                        double.TryParse(cost, out double _)
                        && int.TryParse(duration, out int _)
                        && double.TryParse(income, out double _)
                        && double.TryParse(percentage, out double _)));

            _addAdPackInfo = this.WhenAnyValue(
                    vm => vm.AddDate,
                    vm => vm.AddAmount,
                    (date, amount) => DateTimeOffset.TryParse(date, out DateTimeOffset dto) && int.TryParse(amount, out int amnt) && amnt > 0
                        ? new AdPackInfo { BuyDate = dto, Amount = amnt }
                        : null)
                .ToProperty(this, vm => vm.AddAdPackInfo);

            var adPackInfosChanges = this.WhenAnyObservable(vm => vm.AdPackInfos.CountChanged)
                .Select(_ => AdPackInfos)
                .StartWith(Enumerable.Empty<AdPackInfo>())
                .Publish()
                .RefCount();

            _adPacksCount = adPackInfosChanges
                .Select(adPackInfos => adPackInfos.Sum(adPackInfo => adPackInfo.Amount))
                .ToProperty(this, vm => vm.AdPacksCount);

            _calculatedAmount = Calculate
                .ToProperty(this, vm => vm.CalculatedAmount);

            _settingsToSave = Observable.Merge(
                    SaveSettings,
                    Observable.Return(_state.Settings))
                .ToProperty(this, vm => vm.SettingsToSave);

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
                        vm => vm.CalculateDate,
                        vm => vm.SettingsToSave),
                    adPackInfosChanges,
                    (props, apis) => new StateObject
                    {
                        AddDate = props.Item1,
                        AddAmount = props.Item2,
                        CalculateDate = props.Item3,
                        Settings = props.Item4,
                        AdPackInfos = apis
                    })
                .Throttle(TimeSpan.FromSeconds(2))
                .Subscribe(stateObject => File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(stateObject)));

            Observable.Merge(
                    AddAdPackInfoItem.ThrownExceptions,
                    Calculate.ThrownExceptions,
                    SaveSettings.ThrownExceptions)
                .Select(ex => new AlertInfo(AlertType.Error, "An error occured", string.Join(Environment.NewLine, "The following error message was generated:", "", ex.Message)))
                .Subscribe(_alerts);

            
            AddDate = _state.AddDate;
            AddAmount = _state.AddAmount;
            AdPackInfos.AddRange(_state.AdPackInfos);
            CalculateDate = _state.CalculateDate;
            AdPackCost = _state.Settings.AdPackCost.ToString();
            AdPackDuration = _state.Settings.AdPackDuration.ToString();
            AdPackIncomePerDay = _state.Settings.AdPackIncomePerDay.ToString();
            ReservePercentage = _state.Settings.ReservePercentage.ToString();
        }



        public ReactiveCommand<Unit, AdPackInfo> AddAdPackInfoItem { get; }
        public ReactiveCommand<Unit, int> Calculate { get; }
        public ReactiveCommand<Unit, SettingsObject> SaveSettings { get; }
        public AdPackInfo AddAdPackInfo => _addAdPackInfo?.Value;
        public int AdPacksCount => _adPacksCount.Value;
        public int CalculatedAmount => _calculatedAmount.Value;
        public SettingsObject SettingsToSave => _settingsToSave.Value;
        public IObservable<AlertInfo> Alerts => _alerts;
        public ReactiveList<AdPackInfo> AdPackInfos { get; } = new ReactiveList<AdPackInfo>();
        public string AddDate { get => _addDate; set => this.RaiseAndSetIfChanged(ref _addDate, value); }
        public string AddAmount { get => _addAmount; set => this.RaiseAndSetIfChanged(ref _addAmount, value); }
        public string CalculateDate { get => _calculateDate; set => this.RaiseAndSetIfChanged(ref _calculateDate, value); }
        public string AdPackCost { get => _adPackCost; set => this.RaiseAndSetIfChanged(ref _adPackCost, value); }
        public string AdPackDuration { get => _adPackDuration; set => this.RaiseAndSetIfChanged(ref _adPackDuration, value); }
        public string AdPackIncomePerDay { get => _adPackIncomePerDay; set => this.RaiseAndSetIfChanged(ref _adPackIncomePerDay, value); }
        public string ReservePercentage { get => _reservePercentage; set => this.RaiseAndSetIfChanged(ref _reservePercentage, value); }
        public ViewModelActivator Activator => new ViewModelActivator();



        private IObservable<int> CalculateImpl(IEnumerable<AdPackInfo> adPackInfos, DateTimeOffset endDate, SettingsObject settings) => Observable.Start(() =>
        {
            var adPacks = adPackInfos
                .SelectMany(adPackInfo => Enumerable.Range(1, adPackInfo.Amount).Select(_ => new AdPack(adPackInfo, settings.AdPackDuration)))
                .OrderBy(adPack => adPack.BuyDate.Date)
                .ToList();
            var currentDate = (DateTimeOffset)adPacks[0].BuyDate.Date;
            var money = 0d;
            var reserveAmount = settings.AdPackIncomePerDay * (settings.ReservePercentage / 100);

            while (currentDate < endDate.AddDays(1))
            {
                var adPacksToRemove = new List<AdPack>();
                var adPacksToEvaluate = adPacks.Where(adPack => adPack.BuyDate.Date < currentDate.Date).ToList();

                for (int i = adPacksToEvaluate.Count - 1; i >= 0; i--)
                {
                    var adPack = adPacksToEvaluate[i];
                    adPack.TakeTicket();
                    if(adPack.Tickets == 0) adPacksToEvaluate.Remove(adPack);
                    money += settings.AdPackIncomePerDay - reserveAmount;
                }

                while (money > settings.AdPackCost)
                {
                    adPacks.Add(new AdPack { BuyDate = currentDate });
                    money -= settings.AdPackCost;
                }

                foreach (var adPack in adPacksToRemove) adPacks.Remove(adPack);

                currentDate = currentDate.Date.AddDays(1);
            }

            return adPacks.Count;
        }, RxApp.TaskpoolScheduler);



        private readonly StateObject _state;
        private readonly ObservableAsPropertyHelper<AdPackInfo> _addAdPackInfo;
        private readonly ObservableAsPropertyHelper<int> _adPacksCount;
        private readonly ObservableAsPropertyHelper<int> _calculatedAmount;
        private readonly ObservableAsPropertyHelper<SettingsObject> _settingsToSave;
        private readonly Subject<AlertInfo> _alerts = new Subject<AlertInfo>();
        private string _stateFilePath => Path.Combine(App.AppDataFolderPath, "data.json");
        private string _addDate;
        private string _addAmount;
        private string _calculateDate;
        private string _adPackCost;
        private string _adPackDuration;
        private string _adPackIncomePerDay;
        private string _reservePercentage;
    }

    public enum AlertType
    {
        Info, Success, Warning, Error
    }
}
