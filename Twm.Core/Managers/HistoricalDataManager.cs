using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.DataReaders.Csv;
using Twm.Core.DataProviders.DataReaders.Csv.Map.Classes;
using Twm.Core.DataProviders.DataReaders.Csv.Map.MapHeaders;
using Twm.Core.DataProviders.DataWriters.Csv.Map.DxFeed.MapHeaders;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Messages;
using Twm.DB.DAL.Repositories.MetaDatas;
using Twm.Model.Model;


namespace Twm.Core.Managers
{
    public class HistoricalDataManager
    {
        private const string HistoricalDataPath = "\\Twm\\DB\\HistoricalData";

        private const int MaxYearData = 1980;

        private readonly string _pathToHistoricalData;

        private readonly Dictionary<DataSeriesType, int> _singleRequestSize = new Dictionary<DataSeriesType, int>()
        {
            {DataSeriesType.Tick, 5},
            {DataSeriesType.Second, 5},
            {DataSeriesType.Minute, 60},
            {DataSeriesType.Hour, 365},
            {DataSeriesType.Day, 1000},
            {DataSeriesType.Week, 1000},
            {DataSeriesType.Month, 9900}
        };

        private string _contextName;

        public HistoricalDataManager(DataCalcContext dataCalcContext = null, string pathToHistoricalData = null)
        {
            if (string.IsNullOrEmpty(pathToHistoricalData))
                _pathToHistoricalData =
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + HistoricalDataPath;
            else
                _pathToHistoricalData = pathToHistoricalData;
            Directory.CreateDirectory(_pathToHistoricalData);
            SetContextName(dataCalcContext);
        }

        public event EventHandler<MessageEventArgs> RaiseMessageEvent;

        public DataSeriesParams DataSeriesParams { get; set; }

        public object SenderObject { get; set; }

        private static readonly object SyncRoot = new object();


        private void RaiseMessage(string message, string subMessage)
        {
            RaiseMessageEvent?.Invoke(this, new MessageEventArgs(_contextName + message, subMessage));
        }

        private void SetContextName(DataCalcContext dataCalcContext)
        {
            if (dataCalcContext != null)
            {
                switch (dataCalcContext.DataCalcPeriodType)
                {
                    case DataCalcPeriodType.InSample:
                        _contextName = "IS: ";
                        break;
                    case DataCalcPeriodType.OutSample:
                        _contextName = "OS: ";
                        break;
                    case DataCalcPeriodType.Simulation:
                        _contextName = "Sim: ";
                        break;
                }
            }
        }

        public async Task<IEnumerable<IHistoricalCandle>> GetData(DataSeriesParams dataSeriesParams,
            CancellationToken token, bool returnResult = true)
        {
            DataSeriesParams = dataSeriesParams;

            if (dataSeriesParams.DataSeriesType != DataSeriesType.Tick)
            {
                await GetCandleData(dataSeriesParams, token);
            }
            else
            {
                await GetTickData(dataSeriesParams, token);
            }

            if (token.IsCancellationRequested)
                return Enumerable.Empty<IHistoricalCandle>();

            if (returnResult)
            {
                var historicalCandles = await GetDataFromFiles(dataSeriesParams);
                return historicalCandles;
            }
            else
            {
                return Enumerable.Empty<IHistoricalCandle>();
            }
        }

        private async Task GetCandleData(DataSeriesParams dataSeriesParams,
            CancellationToken token)
        {
            HistoricalMetaData historicalMetaData;
            await Session.DbSemaphoreSlim.WaitAsync(token);
            try
            {
                var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    historicalMetaData = context.HistoricalMetaDatas.FirstOrDefault(
                        x => x.Symbol == dataSeriesParams.Instrument.Symbol &&
                             x.DataType == "Candle" &&
                             x.InstrumentType == dataSeriesParams.Instrument.Type &&
                             x.IsTest == connection.Client.IsTestMode &&
                             x.DataProviderId == connection.DataProviderId &&
                             x.DataSeriesValue == dataSeriesParams.DataSeriesValue &&
                             x.DataSeriesType == dataSeriesParams.DataSeriesType.ToString());
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }

            var dateTimeEndUnspecified = DateTime.SpecifyKind(dataSeriesParams.PeriodEnd, DateTimeKind.Unspecified);
            var periodEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspecified, SystemOptions.Instance.TimeZone);

            //if (periodEnd.Date == DateTime.UtcNow.Date)
            //{
            //    periodEnd = DateTime.UtcNow;
            //}

            if (historicalMetaData == null || historicalMetaData.PeriodEnd.AddMinutes(dataSeriesParams.GetLengthInMinutes()) < periodEnd.ToUniversalTime())
            {
                var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
                //Need download from service
                if (connection is IDataProvider dataProvider && ((connection.IsConnected)))
                {
                    RaiseMessage("Download historical data from service...", "");
                    DateTime periodStart = DateTime.MinValue;
                    if (historicalMetaData != null)
                    {
                        switch (dataSeriesParams.DataSeriesType)
                        {
                            /*case DataSeriesType.Tick:*/
                            case DataSeriesType.Second:
                            case DataSeriesType.Minute:
                            case DataSeriesType.Hour:
                                periodStart = historicalMetaData.PeriodEnd;
                                break;
                            case DataSeriesType.Day:
                            case DataSeriesType.Week:
                            case DataSeriesType.Month:
                                periodStart = new DateTime(historicalMetaData.PeriodEnd.Year, 1, 1);
                                break;
                        }
                    }
                    else
                    {
                        if (dataSeriesParams.DataSeriesType == DataSeriesType.Second)
                        {
                            var dateTimeStartUnspecified =
                                DateTime.SpecifyKind(dataSeriesParams.PeriodStart, DateTimeKind.Unspecified);
                            periodStart = TimeZoneInfo.ConvertTimeToUtc(dateTimeStartUnspecified,
                                SystemOptions.Instance.TimeZone);
                        }
                    }

                    var candles =
                        (await GetDataFromService(dataSeriesParams, periodStart, periodEnd, dataProvider, token))
                        .ToList();

                    if (candles.Any())
                    {
                        //Save historical candle files
                        RaiseMessage("Save historical data...", "");
                        await SaveDataToFiles(historicalMetaData, dataSeriesParams, candles, token);
                    }
                }
            }
        }


        private async Task GetTickData(DataSeriesParams dataSeriesParams,
            CancellationToken token)
        {
            IEnumerable<HistoricalMetaData> historicalMetaDatas;
            await Session.DbSemaphoreSlim.WaitAsync(token);
            try
            {
                var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    historicalMetaDatas = context.HistoricalMetaDatas.Where(
                        x => x.Symbol == dataSeriesParams.Instrument.Symbol &&
                             x.DataType == "Candle" &&
                             x.DataProviderId == connection.DataProviderId &&
                             x.DataSeriesValue == dataSeriesParams.DataSeriesValue &&
                             x.DataSeriesType == dataSeriesParams.DataSeriesType.ToString()).ToList();
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }


            var dateTimeStartUnspecified = DateTime.SpecifyKind(dataSeriesParams.PeriodStart, DateTimeKind.Unspecified);
            var periodStart = TimeZoneInfo.ConvertTimeToUtc(dateTimeStartUnspecified, SystemOptions.Instance.TimeZone);

            var dateTimeEndUnspecified = DateTime.SpecifyKind(dataSeriesParams.PeriodEnd, DateTimeKind.Unspecified);
            var periodEnd = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspecified, SystemOptions.Instance.TimeZone);


            if (historicalMetaDatas.FirstOrDefault(x => x.PeriodStart == dataSeriesParams.PeriodStart) == null)
            {
                var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
                //Need download from service
                if (connection is IDataProvider dataProvider && !Session.Instance.IsPlayback)
                {
                    RaiseMessage("Download historical data from service...", "");

                    var candles =
                        (await GetDataFromService(dataSeriesParams, periodStart, periodEnd, dataProvider, token))
                        .ToList();

                    if (candles.Any())
                    {
                        //Save historical candle files
                        RaiseMessage("Save historical data...", "");
                        await SaveDataToFiles(null, dataSeriesParams, candles, token);
                    }
                }
            }
        }

        public async void ClearAll()
        {
            var pathToHistoricalData =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + HistoricalDataPath;
            await Session.DbSemaphoreSlim.WaitAsync();
            try
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    context.HistoricalMetaDatas.RemoveRange(context.HistoricalMetaDatas.ToList());
                    context.SaveChanges();
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }

            if (Directory.Exists(pathToHistoricalData))
            {
                Directory.Delete(pathToHistoricalData, true);
                Directory.CreateDirectory(pathToHistoricalData);
            }
        }

        public async void DeleteHistoricalData(HistoricalData data)
        {
            var pathToHistoricalData =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + HistoricalDataPath;
            string fullPath;

            var dataProvider = Session.Instance.GetDataProvider(data.DataProviderId);
            string folderToSymbol;


            if (data.IsProvider)
            {
                fullPath = Path.Combine(pathToHistoricalData, dataProvider.Code);
            }
            else if (data.IsNet)
            {
                fullPath = Path.Combine(pathToHistoricalData, dataProvider.Code, data.NetName);
            }
            else if (data.IsType)
            {
                fullPath = Path.Combine(pathToHistoricalData, dataProvider.Code, data.NetName, data.TypeName);
            }
            else
            {
                folderToSymbol = Path.Combine(pathToHistoricalData, dataProvider.Code, data.NetName, data.TypeName, ReplaceSpecialSymbols(data.Symbol));

                if (data.IsSymbol || data.Parent.Children.Count == 1)
                {
                    fullPath = folderToSymbol;
                }
                else
                {
                    var value = (data.DataSeriesValue == 1 ? "" : data.DataSeriesValue.ToString()) +
                                (data.DataSeriesType.ToAbbr());
                    fullPath = Path.Combine(folderToSymbol, value);
                }
            }


            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }

            await Session.DbSemaphoreSlim.WaitAsync();
            try
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new HistoricalMetaDataRepository(context);

                    List<HistoricalMetaData> historicalsMetaDataList;

                    if (data.IsProvider)
                    {
                        historicalsMetaDataList = (await repository.GetEntities()).Where(x => x.DataProviderId == data.DataProviderId).ToList();                        
                    }
                    else if (data.IsNet)
                    {
                        historicalsMetaDataList = (await repository.GetEntities()).Where(x => x.DataProviderId == data.DataProviderId
                        && x.IsTest == (data.NetName == "Testnet") ).ToList();
                    }
                    else if (data.IsType)
                    {
                        historicalsMetaDataList = (await repository.GetEntities()).Where(x => x.DataProviderId == data.DataProviderId
                        && x.IsTest == (data.NetName == "Testnet")
                        && x.InstrumentType == data.TypeName.ToUpper()).ToList();
                    }
                    else if (data.IsSymbol)
                    {
                        historicalsMetaDataList = (await repository.GetEntities()).Where(x => x.DataProviderId == data.DataProviderId
                        && x.IsTest == (data.NetName == "Testnet")
                        && x.InstrumentType == data.TypeName.ToUpper()
                        && x.Symbol == data.Symbol).ToList();
                    }
                    else
                    {
                        historicalsMetaDataList = data.Data.ToList();
                    }

                    repository.Remove(historicalsMetaDataList.ToArray());

                    context.SaveChanges();
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }
        }

        private async Task<IEnumerable<IHistoricalCandle>> GetDataFromService(DataSeriesParams dataSeriesParams,
            DateTime periodStart, DateTime periodEnd, IDataProvider dataProvider, CancellationToken token)
        {
            var historicalCandles = new List<IHistoricalCandle>();

            int requestSize = _singleRequestSize[dataSeriesParams.DataSeriesType];

            DateTime reqStartDate = periodStart.Year > 1980 ? periodStart : new DateTime(MaxYearData, 1, 1);
            DateTime reqEndDate = periodEnd;

            var daysProcessed = 0;

            while (true)
            {
                if (token.IsCancellationRequested)
                    return Enumerable.Empty<IHistoricalCandle>();

                if ((reqEndDate - reqStartDate).TotalDays > requestSize)
                {
                    reqStartDate = reqEndDate.AddDays(-(requestSize - 1));
                }

                var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
                var request = connection.Client.GetHistoricalDataRequest(dataSeriesParams, reqStartDate, reqEndDate);
                var candles = (await dataProvider.GetHistoricalData(request)).ToList();

                historicalCandles.AddRange(candles);


                if (reqStartDate <= periodStart || !candles.Any() || reqStartDate.Year <= MaxYearData)
                {
                    return historicalCandles;
                }
                else
                {
                    reqEndDate = reqStartDate.Date.AddMilliseconds(-1);
                    reqStartDate = reqEndDate.AddDays(-(requestSize - 1));
                    reqStartDate = reqStartDate.Year > MaxYearData ? reqStartDate : new DateTime(MaxYearData, 1, 1);
                }

                daysProcessed += requestSize;
                RaiseMessage("Download historical data from service...", $"Processed {daysProcessed} days");
            }
        }

        public Task<IEnumerable<IHistoricalCandle>> GetDataFromFiles(DataSeriesParams dataSeriesParams)
        {
            List<IHistoricalCandle> candles = new List<IHistoricalCandle>();

            var dateTimeStartUnspecified =
                DateTime.SpecifyKind(dataSeriesParams.PeriodStart.Date, DateTimeKind.Unspecified);
            var currentDate = TimeZoneInfo.ConvertTimeToUtc(dateTimeStartUnspecified, SystemOptions.Instance.TimeZone);

            var dateTimeEndUnspecified = DateTime.SpecifyKind(dataSeriesParams.PeriodEnd, DateTimeKind.Unspecified);
            var endDate = TimeZoneInfo.ConvertTimeToUtc(dateTimeEndUnspecified, SystemOptions.Instance.TimeZone);

            var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);

            var symbol = ReplaceSpecialSymbols(dataSeriesParams.Instrument.Symbol);
            var pathToFiles = "";
            if (connection != null)
            {
                var netDirectory = !connection.Client.IsTestMode ? "Mainnet" : "Testnet"; ;
                var typeDirectory = dataSeriesParams.Instrument.Type.ToUpper(); ;                
                pathToFiles = Path.Combine(_pathToHistoricalData,
                    connection == null ? "" : connection.DataProvider,
                    netDirectory,
                    typeDirectory,
                    symbol,
                    dataSeriesParams.DataSeries);
            }
            else
            {
                pathToFiles = Path.Combine(_pathToHistoricalData,                    
                    symbol,
                    dataSeriesParams.DataSeries);
            }

            switch (dataSeriesParams.DataSeriesType)
            {
                case DataSeriesType.Tick:
                case DataSeriesType.Second:
                case DataSeriesType.Minute:
                case DataSeriesType.Hour:
                    candles = GetDataFromDaysFiles(symbol, dataSeriesParams.DataSeries, pathToFiles, currentDate, endDate);
                    break;
                case DataSeriesType.Day:
                case DataSeriesType.Week:
                case DataSeriesType.Month:
                    candles = GetDataFromYearFiles(symbol, dataSeriesParams.DataSeries, pathToFiles, currentDate, endDate);
                    break;
            }

            RaiseMessage("Getting data from db completed", "");
            foreach (var candle in candles)
            {
                candle.Time = TimeZoneInfo.ConvertTimeFromUtc(candle.Time, SystemOptions.Instance.TimeZone);
                candle.CloseTime = CalcCloseTime(dataSeriesParams, candle.Time);
                candle.IsClosed = true;
            }


            var localStartDate = TimeZoneInfo.ConvertTimeFromUtc(currentDate, SystemOptions.Instance.TimeZone);
            var localEndDate = TimeZoneInfo.ConvertTimeFromUtc(endDate, SystemOptions.Instance.TimeZone);

            candles.RemoveAll(x => x.Time >= localEndDate || x.Time < localStartDate);

            var lastCandle = candles.LastOrDefault();

            if (lastCandle != null && lastCandle.CloseTime > TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SystemOptions.Instance.TimeZone))
            {
                lastCandle.IsClosed = false;
            }

            return Task.FromResult(candles.AsEnumerable());
        }


        public static DateTime CalcCloseTime(DataSeriesParams dataSeriesParams, DateTime datetime)
        {
            switch (dataSeriesParams.DataSeriesType)
            {
                case DataSeriesType.Second:
                    return datetime.AddSeconds(dataSeriesParams.DataSeriesValue);

                case DataSeriesType.Minute:
                    return datetime.AddMinutes(dataSeriesParams.DataSeriesValue);

                case DataSeriesType.Hour:
                    return datetime.AddHours(dataSeriesParams.DataSeriesValue);

                case DataSeriesType.Day:
                    return datetime.AddDays(dataSeriesParams.DataSeriesValue);

                case DataSeriesType.Week:
                    return datetime;

                case DataSeriesType.Month:
                    return datetime;

                default:
                    return datetime;
            }
        }

        private List<IHistoricalCandle> GetDataFromDaysFiles(string symbol, string dataSeries, string pathToFiles, DateTime startDate, DateTime endDate)
        {
            List<IHistoricalCandle> candles = new List<IHistoricalCandle>();
            var requestCount = (endDate.Date - startDate.Date).TotalDays + 1;
            var requestNo = 1;
            while (startDate.Date <= endDate.Date)
            {
                var downloadingPercent = (int)((requestNo / requestCount) * 100);

                var pathToFile = Path.Combine(pathToFiles, startDate.ToString("yyyyMMdd") + ".csv");
                if (File.Exists(pathToFile))
                {
                    Candle[] result;
                    bool createdNew;
                    var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                                                  , null)
                          , MutexRights.FullControl
                          , AccessControlType.Allow
                          );
                    var securitySettings = new MutexSecurity();
                    securitySettings.AddAccessRule(allowEveryoneRule);
                    var mutexName = $"{symbol}-{dataSeries}-{startDate.ToString("yyyyMMdd")}";
                    using (var mutex = new Mutex(false, mutexName, out createdNew, securitySettings))
                    {

                        var hasHandle = false;
                        try
                        {
                            // Wait for the muted to be available
                            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                            //Debug.WriteLine($"Read Занял {mutexName} {Task.CurrentId}");
                            // Do the file read
                            result = CsvDataReader<Candle, HistoricalCandleReadMapHeader>.Read(pathToFile, true, ",");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            // Very important! Release the mutex
                            // Or the code will be locked forever
                            if (hasHandle)
                            {
                                mutex.ReleaseMutex();
                                // Debug.WriteLine($"Read Освободил {mutexName}  {Task.CurrentId}");
                            }
                        }
                    }


                    candles.AddRange(result);
                }
                RaiseMessage("Get data from db...", $"Downloading {downloadingPercent}%");
                startDate = startDate.AddDays(1);
                requestNo++;
            }


            return candles;
        }


        private List<IHistoricalCandle> GetDataFromYearFiles(string symbol, string dataSeries, string pathToFiles, DateTime startDate, DateTime endDate)
        {
            List<IHistoricalCandle> candles = new List<IHistoricalCandle>();
            var requestCount = (endDate.Year - startDate.Year) + 1;
            double requestNo = 1;
            var currentYear = startDate.Year;
            while (currentYear <= endDate.Year)
            {
                var downloadingPercent = (int)((requestNo / requestCount) * 100);
                var pathToFile = Path.Combine(pathToFiles, $"{currentYear}0101.csv");
                if (File.Exists(pathToFile))
                {
                    Candle[] result;

                    bool createdNew;
                    var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid
                                                  , null)
                          , MutexRights.FullControl
                          , AccessControlType.Allow
                          );
                    var securitySettings = new MutexSecurity();
                    securitySettings.AddAccessRule(allowEveryoneRule);
                    var mutexName = $"{symbol}-{dataSeries}-{startDate.ToString("yyyyMMdd")}";

                    using (var mutex = new Mutex(false, mutexName, out createdNew, securitySettings))
                    {

                        var hasHandle = false;
                        try
                        {
                            // Wait for the muted to be available
                            hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                            // Do the file read
                            result = CsvDataReader<Candle, HistoricalCandleReadMapHeader>.Read(pathToFile, true, ",");

                        }
                        catch (Exception)
                        {
                            throw;
                        }
                        finally
                        {
                            // Very important! Release the mutex
                            // Or the code will be locked forever
                            if (hasHandle)
                                mutex.ReleaseMutex();
                        }
                    }


                    candles.AddRange(result.Where(x => x.Time >= startDate && x.Time <= endDate));
                }
                RaiseMessage("Get data from db...", $"Downloading {downloadingPercent}%");

                currentYear = currentYear + 1;
                requestNo++;
            }

            return candles;
        }


        private static string ReplaceSpecialSymbols(string value)
        {
            return value.Replace('/', '@').Replace(':', '#');
        }


        private async Task SaveDataToFiles(HistoricalMetaData historicalMetaData,
            DataSeriesParams dataSeriesParams, IEnumerable<IHistoricalCandle> historicalCandles,
            CancellationToken token)
        {
            var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);
            var netDirectory = !connection.Client.IsTestMode ? "Mainnet" : "Testnet";
            var typeDirectory = dataSeriesParams.Instrument.Type.ToUpper();


            var symbolDirectoryPath =
                Path.Combine(_pathToHistoricalData, connection.DataProvider, netDirectory, typeDirectory, ReplaceSpecialSymbols(dataSeriesParams.Instrument.Symbol));
            Directory.CreateDirectory(symbolDirectoryPath);
            DateTime periodStart = DateTime.MinValue;
            DateTime periodEnd = DateTime.MaxValue;

            var symbolSeriesTypeDirectory = Path.Combine(symbolDirectoryPath, dataSeriesParams.DataSeries);
            switch (dataSeriesParams.DataSeriesType)
            {
                case DataSeriesType.Tick:
                case DataSeriesType.Second:
                case DataSeriesType.Minute:
                case DataSeriesType.Hour:
                    periodStart = SaveDataToDayFiles(symbolSeriesTypeDirectory, dataSeriesParams, historicalCandles, token);
                    periodEnd = historicalCandles.Max(x => x.Time).ToUniversalTime();
                    break;
                case DataSeriesType.Day:
                case DataSeriesType.Week:
                case DataSeriesType.Month:
                    var historicalCandleArray = historicalCandles.ToArray();
                    periodStart = historicalCandleArray.Min(x => x.Time.ToUniversalTime());
                    periodEnd = historicalCandleArray.Max(x => x.Time.ToUniversalTime());
                    SaveDataToYearFiles(symbolSeriesTypeDirectory, historicalCandleArray, token);
                    break;
            }

            RaiseMessage("Historical data saved", "");
            await SaveMetaData(historicalMetaData, dataSeriesParams, periodStart, periodEnd, token, connection.Client.IsTestMode);
        }


        private DateTime SaveDataToDayFiles(string symbolSeriesTypeDirectory, DataSeriesParams dataSeriesParams,
             IEnumerable<IHistoricalCandle> historicalCandles, CancellationToken token)
        {
            Directory.CreateDirectory(symbolSeriesTypeDirectory);
            var historicalCandlesGroups = historicalCandles.GroupBy(x => x.Time.ToUniversalTime().Date)
                .OrderBy(x => x.Key).ToList();
            var daysCount = historicalCandlesGroups.Count;
            double dayNo = 1;
            DateTime periodStart = DateTime.MinValue;
            foreach (var historicalCandlesGroup in historicalCandlesGroups)
            {
                if (token.IsCancellationRequested)
                    return periodStart;

                var pathToFile = Path.Combine(symbolSeriesTypeDirectory,
                    historicalCandlesGroup.Key.ToString("yyyyMMdd") + ".csv");

                var mutexName = $"{dataSeriesParams.Instrument.Symbol}-{dataSeriesParams.DataSeries}-{historicalCandlesGroup.Key.ToString("yyyyMMdd")}";

                var data = historicalCandlesGroup.OrderBy(x => x.Time);

                using (var mutex = new Mutex(false, mutexName))
                //await Session.DbSemaphoreSlim.WaitAsync(token);
                {

                    var hasHandle = false;
                    try
                    {
                        // Wait for the muted to be available
                        hasHandle = mutex.WaitOne(Timeout.Infinite, false);
                        Debug.WriteLine($"Write Занял {mutexName} {Task.CurrentId}");
                        CsvDataWriter<IHistoricalCandle, HistoricalCandleWriteMapHeader>.Write(pathToFile,
                        data, true, ",");


                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        //Session.DbSemaphoreSlim.Release(1);
                        // Very important! Release the mutex
                        // Or the code will be locked forever
                        //  if (hasHandle)
                        {
                            mutex.ReleaseMutex();
                            Debug.WriteLine($"Write Освободил {mutexName}  {Task.CurrentId}");
                        }
                    }
                }







                if (periodStart == DateTime.MinValue)
                    periodStart = historicalCandlesGroup.Key;
                var savePercent = (int)((dayNo / daysCount) * 100);
                RaiseMessage("Save historical data...", $"Saving {savePercent}%");
                dayNo++;
            }
            return periodStart;
        }


        private void SaveDataToYearFiles(string symbolSeriesTypeDirectory,
            IEnumerable<IHistoricalCandle> historicalCandles, CancellationToken token)
        {
            Directory.CreateDirectory(symbolSeriesTypeDirectory);
            var historicalCandlesGroups = historicalCandles.GroupBy(x => x.Time.ToUniversalTime().Year)
                .OrderBy(x => x.Key).ToList();
            var yearsCount = historicalCandlesGroups.Count;
            double yearNo = 1;
            foreach (var historicalCandlesGroup in historicalCandlesGroups)
            {
                if (token.IsCancellationRequested)
                    return;

                var pathToFile = Path.Combine(symbolSeriesTypeDirectory, $"{historicalCandlesGroup.Key}0101.csv");

                var data = historicalCandlesGroup.OrderBy(x => x.Time);

                CsvDataWriter<IHistoricalCandle, HistoricalCandleWriteMapHeader>.Write(pathToFile,
                    historicalCandlesGroup, true, ",");
                var savePercent = (int)((yearNo / yearsCount) * 100);
                RaiseMessage("Save historical data...", $"Saving {savePercent}%");
                yearNo++;
            }
        }


        private async Task SaveMetaData(HistoricalMetaData historicalMetaData, DataSeriesParams dataSeriesParams,
            DateTime periodStart, DateTime periodEnd, CancellationToken token, bool isTest)
        {
            RaiseMessage("Save Metadata to db...", "");

            await Session.DbSemaphoreSlim.WaitAsync(token);
            try
            {
                using (var context = Session.Instance.DbContextFactory.GetContext())
                {
                    var repository = new HistoricalMetaDataRepository(context);
                    var connection = Session.Instance.GetConnection(dataSeriesParams.Instrument.ConnectionId);

                    if (dataSeriesParams.DataSeriesType != DataSeriesType.Tick)
                    {
                        if (historicalMetaData == null)
                        {

                            historicalMetaData = new HistoricalMetaData()
                            {
                                Symbol = dataSeriesParams.Instrument.Symbol,
                                DataSeriesType = dataSeriesParams.DataSeriesType.ToString(),
                                DataSeriesValue = dataSeriesParams.DataSeriesValue,
                                InstrumentType = dataSeriesParams.Instrument.Type.ToUpper(),
                                DataType = "Candle",
                                PeriodStart = periodStart,
                                PeriodEnd = periodEnd,
                                IsTest = isTest,
                                DataProviderId = connection.DataProviderId
                            };

                            await repository.Add(historicalMetaData);
                            await repository.CompleteAsync();
                        }
                        else
                        {
                            historicalMetaData.PeriodEnd = periodEnd;
                            await repository.Update(historicalMetaData);
                        }
                    }
                    else
                    {
                        var historicalMetaDatas = context.HistoricalMetaDatas.Where(
                            x => x.Symbol == dataSeriesParams.Instrument.Symbol &&
                                 x.DataType == "Candle" &&
                                 x.DataProviderId == connection.DataProviderId &&
                                 x.DataSeriesValue == dataSeriesParams.DataSeriesValue &&
                                 x.DataSeriesType == dataSeriesParams.DataSeriesType.ToString()).ToList();

                        while (true)
                        {
                            historicalMetaData = historicalMetaDatas.FirstOrDefault(x => x.PeriodStart == periodStart);
                            if (historicalMetaData == null)
                            {
                                historicalMetaData = new HistoricalMetaData()
                                {
                                    Symbol = dataSeriesParams.Instrument.Symbol,
                                    DataSeriesType = dataSeriesParams.DataSeriesType.ToString(),
                                    DataSeriesValue = dataSeriesParams.DataSeriesValue,
                                    InstrumentType = dataSeriesParams.Instrument.Type.ToUpper(),
                                    DataType = "Candle",
                                    IsTest = isTest,
                                    PeriodStart = periodStart,
                                    DataProviderId = connection.DataProviderId,
                                    PeriodEnd = periodStart.AddDays(1).AddMilliseconds(-1)
                                };

                                await repository.Add(historicalMetaData);
                                await repository.CompleteAsync();
                            }

                            periodStart = periodStart.AddDays(1);

                            if (periodStart > periodEnd)
                                break;
                        }
                    }
                }
            }
            finally
            {
                Session.DbSemaphoreSlim.Release(1);
            }
        }

        public List<ICandle> GetTicksByHistoricalCandles(IEnumerable<IHistoricalCandle> ticks, CancellationToken token)
        {
            var tickSource = new List<ICandle>();

            foreach (var tick in ticks)
            {
                if (token.IsCancellationRequested)
                    return Enumerable.Empty<ICandle>().ToList();
                tickSource.Add(Session.Instance.Mapper.Map<IHistoricalCandle, ICandle>(tick));
            }

            var tickGrouped = new List<ICandle>();
            var groupedTicks = tickSource.GroupBy(x => new { x.t, x.O });
            foreach (var groupedTick in groupedTicks)
            {
                tickGrouped.Add(new Twm.Chart.Classes.Candle(groupedTick.Key.t, groupedTick.Key.O, groupedTick.Key.O, groupedTick.Key.O, groupedTick.Key.O, groupedTick.Sum(x => x.V)) { ct = groupedTick.FirstOrDefault().ct });
            }

            return tickGrouped;
        }
    }
}