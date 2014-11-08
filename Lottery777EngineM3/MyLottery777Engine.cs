using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Data;
using System.Reflection;

namespace Lottery777EngineM3
{
    public class MyLottery777Engine
    {
        #region Members
        List<Lottery777WinningResult> _LotteryHistoricResults = new List<Lottery777WinningResult>();
        public List<Lottery777WinningResult> WinningResults
        {
            get
            {
                return _LotteryHistoricResults;
            }
        }

        List<ChosenLottery777Table> _ChosenFromWinningResults = new List<ChosenLottery777Table>();
        public List<ChosenLottery777Table> WinningChosenTables { get { return _ChosenFromWinningResults; } }

        Stopwatch _Stopwatch = new Stopwatch();
        Stopwatch _Stopwatch2 = new Stopwatch();

        HashSet<Lottery777WinningResult> _CurrentlyExistingWinningResults = new HashSet<Lottery777WinningResult>();
        HashSet<ChosenLottery777Table> _CurrentlyExistingChosenTables = new HashSet<ChosenLottery777Table>();
        HashSet<int> _CurrentlyExistingProcessedWinningTables = new HashSet<int>();
        public HashSet<ChosenLottery777Table> ExistingWinningChosenTables { get { return _CurrentlyExistingChosenTables; } }

        Dictionary<int[], ChosenLottery777Table> _CurrentlyExistingChosenTablesAsDict = new System.Collections.Generic.Dictionary<int[], ChosenLottery777Table>(new Distinct777TableComparer());
        DataTable _CurrentlyExistingChosenTablesWinChain = null;

        Queue<Lottery777DBOperation> _DBOperations = new System.Collections.Generic.Queue<Lottery777DBOperation>();
        bool _MoreDBOperationsExist = true;

        Object _Locker = new Object();        
        #endregion

        #region C'tor
        public MyLottery777Engine(string iLotteryResultsFilepath, bool iDownloadResultsFile)
        {
            string formerResultsFilename = string.Format("OldResults_{0}.csv", DateTime.Now.ToString("dd.MM.yyyy.HH.mm.ss.ffff"));

            if (iDownloadResultsFile)
            {
                if (File.Exists("777.csv"))
                {
                    //rename former file
                    RenameFile("777.csv", formerResultsFilename); 
                }

                _Stopwatch.Start();
                DownloadFile(@"http://www.pais.co.il/777/Pages/last_Results.aspx?download=1", "777.csv");
                _Stopwatch.Stop();

                Console.WriteLine(string.Format("{0}: New results file download time: {1} seconds.", DateTime.Now, _Stopwatch.ElapsedMilliseconds / 1000));
            }
            ReadLottery777OfficialResultFile(iLotteryResultsFilepath);

            CreateEmptyDB();

            GetProcessedWinningRafflesFromDB();
            GetCurrentWinningResultsFromDB();
            GetChosenTablesFromDB();

            //InjectLotteryResultsToDB();

            //Starting DB updates thread
            //Console.WriteLine(string.Format("{0}: Started DB Updating thread...", DateTime.Now));
            //Thread t = new Thread(new ThreadStart(UpdateDB));
            //t.Start();
        }
        #endregion

        private const double TaxThreshold = 50000;
        private const double MaxPartialTaxThreshold = 100000; 
        private const double TaxPercentage = 0.3;

        private static readonly string dbName = "Lottery777DB";

        internal static readonly string _MasterConnectionString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=master";
        internal static readonly string _DefaultConnectionString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=Lottery777DB";

        private static readonly string sqlCommandCreateDB = "CREATE DATABASE " + dbName + " ON PRIMARY " +
                "(NAME = " + dbName + ", " +
                "FILENAME = 'D:\\" + dbName + ".mdf', " +
                "SIZE = 3MB, MAXSIZE = 10000MB, FILEGROWTH = 10%) " +
                "LOG ON (NAME = " + dbName + "_LOG, " +
                "FILENAME = 'D:\\" + dbName + ".ldf', " +
                "SIZE = 3MB, " +
                "MAXSIZE = 10000MB, " +
                "FILEGROWTH = 10%)";

        private static readonly string sqlQueryGetChosenTables =
        @"
        SELECT ID, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Hits5, Hits6, Hits7 FROM [Lottery777DB].[dbo].LotteryChosenTables
        where Hits5 >= 66 or Hits6>=15
        order by Hits5 desc";


        private static readonly string sqlQueryGetAllChosenTables =        
        @"SELECT ID, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Hits5, Hits6, Hits7 FROM [Lottery777DB].[dbo].LotteryChosenTables
        order by Hits5 desc";


        internal static readonly string LotteryHistoricResultsTableSchema = "CREATE TABLE LotteryHistoricResults (ID int IDENTITY(1,1), RaffleID int NOT NULL, RaffleDate date, Num1 int NOT NULL , Num2 int NOT NULL, Num3 int NOT NULL, Num4 int NOT NULL, Num5 int NOT NULL, Num6 int NOT NULL, Num7 int NOT NULL, Num8 int NOT NULL, Num9 int NOT NULL, Num10 int NOT NULL, Num11 int NOT NULL, Num12 int NOT NULL, Num13 int NOT NULL, Num14 int NOT NULL, Num15 int NOT NULL, Num16 int NOT NULL, Num17 int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] LotteryHistoricResultsTableColumns = { "RaffleID", "RaffleDate", "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Num8", "Num9", "Num10", "Num11", "Num12", "Num13", "Num14", "Num15", "Num16", "Num17" };
        internal static readonly string LotteryHistoricResultsTableName = "LotteryHistoricResults";

        internal static readonly string LotteryChosenTablesTableSchema = "CREATE TABLE LotteryChosenTables (ID int IDENTITY(1,1), Num1 int NOT NULL, Num2 int NOT NULL, Num3 int NOT NULL, Num4 int NOT NULL, Num5 int NOT NULL, Num6 int NOT NULL, Num7 int NOT NULL, Hits5 int NOT NULL, Hits6 int NOT NULL, Hits7 int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] LotteryChosenTablesTableColumns = { "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Hits5", "Hits6", "Hits7" };
        internal static readonly string LotteryChosenTablesTableName = "LotteryChosenTables";

        internal static readonly string WinningTables_RaffleIDsTableSchema = "CREATE TABLE WinningTables_RaffleIDs (ID int IDENTITY(1,1), ChosenTableID int NOT NULL, WonRaffleID int NOT NULL, NumHits int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] WinningTables_RaffleIDsTableColumns = { "ChosenTableID", "WonRaffleID", "NumHits" };
        internal static readonly string WinningTables_RaffleIDsTableName = "WinningTables_RaffleIDs";

        internal static readonly string ProcessedWinningTablesTableSchema = "CREATE TABLE ProcessedWinningTable (ID int IDENTITY(1,1), ProcessedWinningRaffleID int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] ProcessedWinningTablesTableColumns = { "ProcessedWinningRaffleID" };
        internal static readonly string ProcessedWinningTablesTableName = "ProcessedWinningTable";

        #region Table generation & handling
        private void ReadLottery777OfficialResultFile(string iFilepath)
        {
            string path = string.Empty;

            if (Path.GetDirectoryName(iFilepath) == string.Empty)
            {
                path = Path.Combine(System.Environment.CurrentDirectory, iFilepath);
            }
            else
            {
                path = iFilepath;
            }

            if (Directory.Exists(Path.GetDirectoryName(path)))
            {
                using (StreamReader reader = new StreamReader(iFilepath, true))
                {
                    reader.ReadLine(); //skipping first line

                    int i = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] lineSplit = line.Split(',');
                        int[] numbers = new int[17];

                        string s = lineSplit[2].Replace("\"", "");

                        numbers[0] = int.Parse(lineSplit[2].Replace("\"", ""));
                        numbers[1] = int.Parse(lineSplit[3].Replace("\"", ""));
                        numbers[2] = int.Parse(lineSplit[4].Replace("\"", ""));
                        numbers[3] = int.Parse(lineSplit[5].Replace("\"", ""));
                        numbers[4] = int.Parse(lineSplit[6].Replace("\"", ""));
                        numbers[5] = int.Parse(lineSplit[7].Replace("\"", ""));
                        numbers[6] = int.Parse(lineSplit[8].Replace("\"", ""));
                        numbers[7] = int.Parse(lineSplit[9].Replace("\"", ""));
                        numbers[8] = int.Parse(lineSplit[10].Replace("\"", ""));
                        numbers[9] = int.Parse(lineSplit[11].Replace("\"", ""));
                        numbers[10] = int.Parse(lineSplit[12].Replace("\"", ""));
                        numbers[11] = int.Parse(lineSplit[13].Replace("\"", ""));
                        numbers[12] = int.Parse(lineSplit[14].Replace("\"", ""));
                        numbers[13] = int.Parse(lineSplit[15].Replace("\"", ""));
                        numbers[14] = int.Parse(lineSplit[16].Replace("\"", ""));
                        numbers[15] = int.Parse(lineSplit[17].Replace("\"", ""));
                        numbers[16] = int.Parse(lineSplit[18].Replace("\"", ""));

                        DateTime currDate;
                        bool isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "dd/MM/yy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                        if (!isParsed)
                        {
                            isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                            if (!isParsed)
                            {
                                isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "dd/M/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                if (!isParsed)
                                {
                                    isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "d/M/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                    if (!isParsed)
                                    {
                                        isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "d/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                        if (!isParsed)
                                        {
                                            isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "d/M/yy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                            if (!isParsed)
                                            {
                                                isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "dd/M/yy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                                if (!isParsed)
                                                {
                                                    isParsed = DateTime.TryParseExact(lineSplit[0].Replace("\"", ""), "d/MM/yy", new CultureInfo("en-US"), DateTimeStyles.None, out currDate);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        //0 - ID, 1 - Date, 2-7 numbers, 8 string number
                        _LotteryHistoricResults.Add(new Lottery777WinningResult()
                        {
                            _LotteryRaffleID = lineSplit[1].Replace("\"", ""),
                            _LotteryDate = currDate,
                            _Numbers = numbers
                        });

                        i++;
                    }

                    //foreach (Lottery777WinningResult winningResult in _LotteryHistoricResults)
                    //{
                    //    List<int> numbersHit = new List<int>();
                    //    winningResult._HitCount = GetHitCountForTable(winningResult._Numbers, GetNumberOfOfficialCombinationsSinceDate(new DateTime(2009, 2, 28)), ref numbersHit);
                    //}
                }
            }
            else
            {
                throw new Exception(string.Format("Path {0} doesn't exist", Path.GetDirectoryName(iFilepath)));
            }
        }

        private void DownloadFile(string iUri, string iFilename)
        {
            WebClient Client = new WebClient();

            try
            {
                Client.DownloadFile(iUri, iFilename);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void RenameFile(string iFilename, string iNewFilename)
        {
            if (File.Exists(iFilename))
            {
                try
                {
                    System.IO.File.Move(iFilename, iNewFilename);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                throw new Exception(string.Format("File: {0} doesn't exist...", iFilename));
            }
        }

        private int GetHitCountForLastNResults(int[] iGeneratedTable, int iTolerance, int iNumberOfResultsToConsider, ref int[] oHitCountArray, ref int oLastWonRaffle)
        {
            int hitCount = 0;

            for (int i = 0; i < iNumberOfResultsToConsider; i++)
            {
                int currHitCount = iGeneratedTable.Intersect(_LotteryHistoricResults[i]._Numbers).Count();
                if (currHitCount >= iTolerance)
                {
                    oHitCountArray[currHitCount]++;
                    hitCount++;

                    if (oLastWonRaffle == -1)
                    {
                        oLastWonRaffle = i + 1; //The last time these numbers appeared in a winning result (actually the number in the sequence of winning results since they are read from file)
                    }
                }
            }

            return hitCount;
        }

        private int GetHitCountForLastNResults(int[] iGeneratedTable, int iTolerance, int iNumberOfResultsToConsider, ref int[] oHitCountArray, ref List<WinOccurence> oWinningRaffleTracking)
        {
            int hitCount = 0;

            for (int i = 0; i < iNumberOfResultsToConsider; i++)
            {
                int currHitCount = iGeneratedTable.Intersect(_LotteryHistoricResults[i]._Numbers).Count();
                if (currHitCount >= iTolerance)
                {
                    oHitCountArray[currHitCount]++;
                    hitCount++;

                    //oWinningRaffleTracking.Add(i +1);
                    oWinningRaffleTracking.Add(new WinOccurence
                    {
                        RaffleID = int.Parse(_LotteryHistoricResults[i]._LotteryRaffleID),
                        HitCount = currHitCount
                    });
                }
            }

            return hitCount;
        }

        public void GenerateLottery777TablesParallel(int iNumResultsToConsider, int iMinThreshhold)
        {
            Stopwatch sw = new Stopwatch();

            List<ChosenLottery777Table> generatedPossibilites = new System.Collections.Generic.List<ChosenLottery777Table>();

            for (int i = 0; i < iNumResultsToConsider; i++)
            {
                if (!_CurrentlyExistingProcessedWinningTables.Contains(int.Parse(_LotteryHistoricResults[i]._LotteryRaffleID)))
                {
                    //sw.Start();
                    GenerateSubsets3(_LotteryHistoricResults[i]._Numbers, 7, ref generatedPossibilites);
                    //sw.Stop();

                    //Console.Write(string.Format("Generating all combinations of 7 from 17 numbers took: {0}", sw.Elapsed));
                    //Console.Write("\r");
                    //sw.Reset();

                    sw.Start();
                    Parallel.ForEach(generatedPossibilites, currItem =>
                    {
                        int[] hitCountArray = new int[8];
                        List<WinOccurence> winningRaffleTracking = new System.Collections.Generic.List<WinOccurence>();
                        int hitCountTotal = GetHitCountForLastNResults(currItem.Numbers, 5, iNumResultsToConsider, ref hitCountArray, ref winningRaffleTracking);

                        if (hitCountTotal >= 80 || hitCountArray[6] >= 10)// && !iChosen.Contains(item, new DistinctChosenLottery777TableComparer()))
                        {
                            currItem.HitCount = hitCountTotal;
                            currItem.HitCountArray = hitCountArray;
                            currItem.WinningRafflesTracking = winningRaffleTracking;

                            lock (_Locker)
                            {
                                if (!_CurrentlyExistingChosenTables.Contains(currItem, new DistinctFullChosenLottery777TableComparer()))
                                {
                                    //Adding chosen record to Currently existing chosen tables list
                                    _CurrentlyExistingChosenTables.Add(currItem);
                                    _CurrentlyExistingChosenTablesAsDict.Add(currItem.Numbers, currItem);

                                    _DBOperations.Enqueue(new Lottery777DBOperation { Operation = DBOperation.Add, Table = currItem });
                                }
                                else
                                {
                                    //The record exists in the DB, and we need to check if anything changed
                                    try
                                    {
                                        //_Stopwatch2.Reset();
                                        //_Stopwatch2.Start();
                                        ChosenLottery777Table existingRecord = _CurrentlyExistingChosenTablesAsDict[currItem.Numbers];
                                        //_Stopwatch2.Stop();
                                        //Console.WriteLine(string.Format("{0}: Lookup took: {1}", DateTime.Now, _Stopwatch2.Elapsed));


                                        if (existingRecord != null)
                                        {
                                            //Verifying that the chosen table record changed
                                            if (currItem.HitCountArray[5] > existingRecord.HitCountArray[5] ||
                                                currItem.HitCountArray[6] > existingRecord.HitCountArray[6] ||
                                                currItem.HitCountArray[7] > existingRecord.HitCountArray[7])
                                            {
                                                //Adding to update list
                                                _DBOperations.Enqueue(new Lottery777DBOperation{ Operation = DBOperation.Update, Table = currItem});
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }
                                } 
                            }
                        }
                    }
                    );

                    //Adding processed winning result to DB Updates list
                    AddProcessedWinningResultToDBWithoutCheckingExistence(_LotteryHistoricResults[i]);

                    sw.Stop();
                    Console.WriteLine(string.Format("{0}: {1} out of {2} took: {3}", DateTime.Now, i + 1, iNumResultsToConsider, sw.Elapsed));
                    Console.Write("\r");
                    sw.Reset();

                    generatedPossibilites.Clear(); 
                }
            }

            _MoreDBOperationsExist = false;

        }

        public void GenerateLottery777TablesParallelThread(int iNumResultsToConsider, int iStartingIndex, Action oCallback)
        {
            Stopwatch sw = new Stopwatch();

            //read all existing chosen tables from the db
            Dictionary<int[], ChosenLottery777Table> tmpCurrentlyExistingChosenTablesAsDict = new System.Collections.Generic.Dictionary<int[], ChosenLottery777Table>(new Distinct777TableComparer());
            HashSet<ChosenLottery777Table> tmpCurrentlyExistingChosenTables = GetAllChosenTablesFromDB(ref tmpCurrentlyExistingChosenTablesAsDict);

            List<ChosenLottery777Table> generatedPossibilites = new System.Collections.Generic.List<ChosenLottery777Table>();

            for (int i = iStartingIndex; i < iNumResultsToConsider; i++)
            {
                if (!_CurrentlyExistingProcessedWinningTables.Contains(int.Parse(_LotteryHistoricResults[i]._LotteryRaffleID)))
                {
                    //sw.Start();
                    GenerateSubsets3(_LotteryHistoricResults[i]._Numbers, 7, ref generatedPossibilites);
                    //sw.Stop();

                    //Console.Write(string.Format("Generating all combinations of 7 from 17 numbers took: {0}", sw.Elapsed));
                    //Console.Write("\r");
                    //sw.Reset();

                    sw.Start();
                    Parallel.ForEach(generatedPossibilites, currItem =>
                    {
                        int[] hitCountArray = new int[8];
                        List<WinOccurence> winningRaffleTracking = new System.Collections.Generic.List<WinOccurence>();
                        int hitCountTotal = GetHitCountForLastNResults(currItem.Numbers, 5, _LotteryHistoricResults.Count, ref hitCountArray, ref winningRaffleTracking);

                        if (hitCountTotal >= 80 || hitCountArray[6] >= 10)// && !iChosen.Contains(item, new DistinctChosenLottery777TableComparer()))
                        {
                            currItem.HitCount = hitCountTotal;
                            currItem.HitCountArray = hitCountArray;
                            currItem.WinningRafflesTracking = winningRaffleTracking;

                            lock (_Locker)
                            {
                                if (!_CurrentlyExistingChosenTables.Contains(currItem, new DistinctFullChosenLottery777TableComparer()))
                                {
                                    //Adding chosen record to Currently existing chosen tables list
                                    tmpCurrentlyExistingChosenTables.Add(currItem);


                                    try
                                    {
                                        tmpCurrentlyExistingChosenTablesAsDict.Add(currItem.Numbers, currItem);

                                        //adding only if didnt find a duplicate
                                        _DBOperations.Enqueue(new Lottery777DBOperation { Operation = DBOperation.Add, Table = currItem });
                                    }
                                    catch (Exception)
                                    {
                                        
                                        //throw;
                                    }

                                    
                                }
                                else
                                {
                                    //The record exists in the DB, and we need to check if anything changed
                                    try
                                    {
                                        //_Stopwatch2.Reset();
                                        //_Stopwatch2.Start();
                                        ChosenLottery777Table existingRecord = tmpCurrentlyExistingChosenTablesAsDict[currItem.Numbers];
                                        //_Stopwatch2.Stop();
                                        //Console.WriteLine(string.Format("{0}: Lookup took: {1}", DateTime.Now, _Stopwatch2.Elapsed));


                                        if (existingRecord != null)
                                        {
                                            //Verifying that the chosen table record changed
                                            if (currItem.HitCountArray[5] > existingRecord.HitCountArray[5] ||
                                                currItem.HitCountArray[6] > existingRecord.HitCountArray[6] ||
                                                currItem.HitCountArray[7] > existingRecord.HitCountArray[7])
                                            {
                                                //Adding to update list
                                                _DBOperations.Enqueue(new Lottery777DBOperation { Operation = DBOperation.Update, Table = currItem });
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }
                                }
                            }
                        }
                    }
                    );

                    //Adding processed winning result to DB Updates list
                    AddProcessedWinningResultToDBWithoutCheckingExistence(_LotteryHistoricResults[i]);

                    //sw.Stop();
                    //Console.WriteLine(string.Format("{0}: {1} out of {2} took: {3}", DateTime.Now, i + 1, iNumResultsToConsider, sw.Elapsed));
                    //Console.Write("\r");
                    //sw.Reset();

                    generatedPossibilites.Clear();
                    
                }
            }

            _MoreDBOperationsExist = false;
            oCallback();

        }

        private Dictionary<int[], int> GetTablesCount()
        {
            Dictionary<int[], int> tableCount = new System.Collections.Generic.Dictionary<int[], int>();

            foreach (Lottery777WinningResult table in _LotteryHistoricResults)
            {
                if (tableCount.ContainsKey(table._Numbers))
                {
                    tableCount[table._Numbers]++;
                }
                else
                {
                    tableCount.Add(table._Numbers, 1);
                }
            }

            return tableCount;
        }

        public void GenerateSubsets3(int[] set, int k, ref List<ChosenLottery777Table> iChosen)
        {
            int[] subset = new int[k];
            ProcessLargerSubsets3(set, subset, 0, 0, ref iChosen);

            return;
        }

        void ProcessLargerSubsets3(int[] set, int[] subset, int subsetSize, int nextIndex, ref List<ChosenLottery777Table> iChosen)
        {
            if (subsetSize == subset.Length)
            {
                ChosenLottery777Table curr = new ChosenLottery777Table() { Numbers = (int[])subset.Clone(), HitCount = 0 };
                iChosen.Add(curr);
            }
            else
            {

                for (int j = nextIndex; j < set.Length; j++)
                {
                    subset[subsetSize] = set[j];
                    ProcessLargerSubsets3(set, subset, subsetSize + 1, j + 1, ref iChosen);
                }
            }
        }

        public double CalculateWinnings(int[] iWinningResult, int iRaffleNo, List<int[]> iChosenTables, ref int[] oHitCount)
        {
            double winnings = 0;

            foreach (int[] chosenTable in iChosenTables)
            {
                int hitCount = iWinningResult.Intersect(chosenTable).Count();

                oHitCount[hitCount]++;

                switch (hitCount)
                {
                    case 0:
                        winnings += 5;
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        winnings += 5;
                        break;
                    case 4:
                        winnings += 20;
                        break;
                    case 5:
                        winnings += 50;
                        break;
                    case 6:
                        winnings += 500;
                        //Console.WriteLine(string.Format("Won 500 NIS with the following numbers: {0},{1},{2},{3},{4},{5},{6}, Raffle No.{7}",
                        //                                chosenTable[0],
                        //                                chosenTable[1],
                        //                                chosenTable[2],
                        //                                chosenTable[3],
                        //                                chosenTable[4],
                        //                                chosenTable[5],
                        //                                chosenTable[6],
                        //                                iRaffleNo)
                        //                                );

                        //ChosenLottery777Table currChosenTable = new ChosenLottery777Table();
                        //currChosenTable.Numbers = chosenTable;

                        //if (currChosenTable.WinningRafflesTracking == null)
                        //{
                        //    currChosenTable.WinningRafflesTracking = new System.Collections.Generic.List<int>(); 
                        //}
                        //currChosenTable.WinningRafflesTracking.Add(iRaffleNo);

                        //_ChosenFromWinningResults.Add(currChosenTable);

                        //_ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctFullChosenLottery777TableComparer()).ToList();



                        break;
                    case 7:
                        winnings += 70000;
                        Console.WriteLine(string.Format("Won the BIG PRIZE with the following numbers: {0},{1},{2},{3},{4},{5},{6}, Raffle No.{7}",
                                                        chosenTable[0],
                                                        chosenTable[1],
                                                        chosenTable[2],
                                                        chosenTable[3],
                                                        chosenTable[4],
                                                        chosenTable[5],
                                                        chosenTable[6],
                                                        iRaffleNo)
                                                        );

                        ChosenLottery777Table currChosenTable = new ChosenLottery777Table();
                        currChosenTable = new ChosenLottery777Table();
                        currChosenTable.Numbers = chosenTable;
                        if (currChosenTable.WinningRafflesTracking == null)
                        {
                            currChosenTable.WinningRafflesTracking = new System.Collections.Generic.List<WinOccurence>();
                        }
                        currChosenTable.WinningRafflesTracking.Add(new WinOccurence { RaffleID = iRaffleNo, HitCount = hitCount });

                        _ChosenFromWinningResults.Add(currChosenTable);

                        //_ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctFullChosenLottery777TableComparer()).ToList();
                        _ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctChosenLottery777TableComparer()).ToList();
                        break;
                }
            }

            //return winnings;
            return CalculateTax(winnings);

        }

        public double CalculateWinnings(int[] iWinningResult, int iRaffleNo, List<ChosenLottery777Table> iChosenTables, ref int[] oHitCount)
        {
            double winnings = 0;

            foreach (ChosenLottery777Table chosenTable in iChosenTables)
            {
                int hitCount = iWinningResult.Intersect(chosenTable.Numbers).Count();

                oHitCount[hitCount]++;

                switch (hitCount)
                {
                    case 0:
                        winnings += 5;
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        winnings += 5;
                        break;
                    case 4:
                        winnings += 20;
                        break;
                    case 5:
                        winnings += 50;
                        break;
                    case 6:
                        winnings += 500;
                        //Console.WriteLine(string.Format("Won 500 NIS with the following numbers: {0},{1},{2},{3},{4},{5},{6}, Raffle No.{7}",
                        //                                chosenTable[0],
                        //                                chosenTable[1],
                        //                                chosenTable[2],
                        //                                chosenTable[3],
                        //                                chosenTable[4],
                        //                                chosenTable[5],
                        //                                chosenTable[6],
                        //                                iRaffleNo)
                        //                                );

                        //ChosenLottery777Table currChosenTable = new ChosenLottery777Table();
                        //currChosenTable.Numbers = chosenTable;

                        //if (currChosenTable.WinningRafflesTracking == null)
                        //{
                        //    currChosenTable.WinningRafflesTracking = new System.Collections.Generic.List<int>(); 
                        //}
                        //currChosenTable.WinningRafflesTracking.Add(iRaffleNo);

                        //_ChosenFromWinningResults.Add(currChosenTable);

                        //_ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctFullChosenLottery777TableComparer()).ToList();



                        break;
                    case 7:
                        winnings += 70000;
                        Console.WriteLine(string.Format("Won the BIG PRIZE with the following numbers: {0},{1},{2},{3},{4},{5},{6}, Raffle No.{7}",
                                                        chosenTable.Numbers[0],
                                                        chosenTable.Numbers[1],
                                                        chosenTable.Numbers[2],
                                                        chosenTable.Numbers[3],
                                                        chosenTable.Numbers[4],
                                                        chosenTable.Numbers[5],
                                                        chosenTable.Numbers[6],
                                                        iRaffleNo)
                                                        );

                        _ChosenFromWinningResults.Add(chosenTable);

                        //_ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctFullChosenLottery777TableComparer()).ToList();
                        _ChosenFromWinningResults = _ChosenFromWinningResults.Distinct(new DistinctChosenLottery777TableComparer()).ToList();
                        break;
                }
            }

            //return winnings;
            return CalculateTax(winnings);

        }

        private double CalculateTax(double iWinnings)
        {
            double winningsAfterTax = 0;


            //if winnings under 50k - no tax
            if (iWinnings < TaxThreshold)
            {
                winningsAfterTax = iWinnings;
            }
            else if (iWinnings > TaxThreshold && iWinnings <= MaxPartialTaxThreshold)
            {//if winning > 50k && < 100k, pay partial tax according to formula

                double sumDifference = iWinnings - TaxThreshold;
                winningsAfterTax = iWinnings - (sumDifference * 2 * TaxPercentage);
            }
            else
            {//if winning > 100k, pay 30% tax
                winningsAfterTax = iWinnings * (1.0 - TaxPercentage);
            }

            return winningsAfterTax;
        }

        public bool IsChoosingCriterionMet(ChosenLottery777Table iLotteryTable, int iNumWinnings, int iRaffleTollerance)
        {
            bool retVal = false;
            //int lastWin = iLotteryTable.WinningRafflesTracking[0].RaffleID;
            int lastWin = int.Parse(_LotteryHistoricResults[0]._LotteryRaffleID.ToString());

            //Need to calc difference between 2 adjacent raffle numbers
            List<int> differences = new System.Collections.Generic.List<int>();

            for (int i = 1; i < iLotteryTable.WinningRafflesTracking.Count; i++)
            {

                if (iLotteryTable.WinningRafflesTracking[i].RaffleID > lastWin - iRaffleTollerance && iLotteryTable.WinningRafflesTracking[i].HitCount != 7 && i + 1 < iLotteryTable.WinningRafflesTracking.Count)
                {
                    differences.Add(iLotteryTable.WinningRafflesTracking[i - 1].RaffleID - iLotteryTable.WinningRafflesTracking[i].RaffleID);
                }
                else
                {
                    break;
                }
            }

            //Check the criterion
            if (differences.Count >= iNumWinnings)
            {
                retVal = true;
            }

            return retVal;
        }

        private bool BigPrizeHitInLastNRaffles(ChosenLottery777Table iLotteryTable, int iRaffleTolerance)
        {
            bool isHit = false;

            foreach (WinOccurence winningRaffle in iLotteryTable.WinningRafflesTracking)
            {
                //Check if won the big prize
                int count = _LotteryHistoricResults.Find((x) => int.Parse(x._LotteryRaffleID) == winningRaffle.RaffleID)._Numbers.Intersect(iLotteryTable.Numbers).ToList().Count;

                if (count == 7)
                {
                    if (int.Parse(_LotteryHistoricResults[0]._LotteryRaffleID) - winningRaffle.RaffleID <= iRaffleTolerance)
                    {
                        isHit = true;
                        break;
                    }
                }
            }

            return isHit;
        }

        public void UpdateDB()
        {
            int i = 1;

            while (_MoreDBOperationsExist || _DBOperations.Count > 0)
            {
                if (_DBOperations.Count > 0)
                {
                    Lottery777DBOperation op = _DBOperations.Dequeue();

                    if (op.Operation == DBOperation.Add)
                    {
                        try
                        {
                            //Adding chosen record to DB
                            AddChosenLotteryTableRecordToDBWithoutCheckingExistence(op.Table);

                            //Adding win chain to DB
                            for (int j = 0; j < op.Table.WinningRafflesTracking.Count; j++)
                            {
                                int rafflePlace = _LotteryHistoricResults.FindIndex(x => int.Parse(x._LotteryRaffleID) == op.Table.WinningRafflesTracking[j].RaffleID);
                                int hitCount = op.Table.Numbers.Intersect(_LotteryHistoricResults[rafflePlace]._Numbers).Count();

                                AddChosenLotteryTableWinChainToDB(op.Table, j, hitCount);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else if (op.Operation == DBOperation.Update)
                    {
                        try
                        {
                            UpdateChosenTableInDB(op.Table);

                            //Need to populate win chain before updating an existing record
                            //currItem.WinningRafflesTracking = GetWinChainForExistingChosenTable(currItem);

                            //Now we need to update the win chain as well, because if one of the hit counts changed
                            //The win chain has changed as well
                            UpdateChosenLotteryTableWinChain(op.Table);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    Console.WriteLine(string.Format("{0}: Completed {1}/{2} modifications to the DB", DateTime.Now, i, _DBOperations.Count));
                    i++; 
                }
            }
        }

        public void UpdateDBThread(Action oCallback)
        {
            int i = 1;

            while (_MoreDBOperationsExist || _DBOperations.Count > 0)
            {
                if (_DBOperations.Count > 0)
                {
                    Lottery777DBOperation op = _DBOperations.Dequeue();

                    if (op.Operation == DBOperation.Add)
                    {
                        try
                        {
                            //Adding chosen record to DB
                            AddChosenLotteryTableRecordToDBWithoutCheckingExistence(op.Table);

                            //Adding win chain to DB
                            for (int j = 0; j < op.Table.WinningRafflesTracking.Count; j++)
                            {
                                int rafflePlace = _LotteryHistoricResults.FindIndex(x => int.Parse(x._LotteryRaffleID) == op.Table.WinningRafflesTracking[j].RaffleID);
                                int hitCount = op.Table.Numbers.Intersect(_LotteryHistoricResults[rafflePlace]._Numbers).Count();

                                AddChosenLotteryTableWinChainToDB(op.Table, j, hitCount);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else if (op.Operation == DBOperation.Update)
                    {
                        try
                        {
                            UpdateChosenTableInDB(op.Table);

                            //Need to populate win chain before updating an existing record
                            //currItem.WinningRafflesTracking = GetWinChainForExistingChosenTable(currItem);

                            //Now we need to update the win chain as well, because if one of the hit counts changed
                            //The win chain has changed as well
                            UpdateChosenLotteryTableWinChain(op.Table);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    i++;
                }
            }

            oCallback();
        }
        #endregion

        #region DB Generation
        void CreateEmptyDB()
        {
            try
            {
                if (!SQLServerCommon.SQLServerCommon.IsDatabaseExists(_MasterConnectionString, dbName))//connStringInitial, dbName))
                {
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(sqlCommandCreateDB, _MasterConnectionString);

                    //Create tables upon DB creation
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(LotteryHistoricResultsTableSchema, _DefaultConnectionString);
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(LotteryChosenTablesTableSchema, _DefaultConnectionString);
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(WinningTables_RaffleIDsTableSchema, _DefaultConnectionString);
                    SQLServerCommon.SQLServerCommon.ExecuteNonQuery(ProcessedWinningTablesTableSchema, _DefaultConnectionString);
                }
                else
                {
                    if (!SQLServerCommon.SQLServerCommon.IsTableExists(_DefaultConnectionString, dbName, LotteryHistoricResultsTableName))
                    {
                        SQLServerCommon.SQLServerCommon.ExecuteNonQuery(LotteryHistoricResultsTableSchema, _DefaultConnectionString);
                    }

                    if (!SQLServerCommon.SQLServerCommon.IsTableExists(_DefaultConnectionString, dbName, LotteryChosenTablesTableName))
                    {
                        SQLServerCommon.SQLServerCommon.ExecuteNonQuery(LotteryChosenTablesTableSchema, _DefaultConnectionString);
                    }

                    if (!SQLServerCommon.SQLServerCommon.IsTableExists(_DefaultConnectionString, dbName, WinningTables_RaffleIDsTableName))
                    {
                        SQLServerCommon.SQLServerCommon.ExecuteNonQuery(WinningTables_RaffleIDsTableSchema, _DefaultConnectionString);
                    }

                    if (!SQLServerCommon.SQLServerCommon.IsTableExists(_DefaultConnectionString, dbName, ProcessedWinningTablesTableName))
                    {
                        SQLServerCommon.SQLServerCommon.ExecuteNonQuery(ProcessedWinningTablesTableSchema, _DefaultConnectionString);
                    }
                }

                
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void AddLotteryHistoricResultRecordToDB(Lottery777WinningResult iLotteryHistoricRecord)
        {
            //Write record to DB
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[0]), iLotteryHistoricRecord._LotteryRaffleID);
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[1]), iLotteryHistoricRecord._LotteryDate.ToString("yyyy/MM/dd"));
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[2]), iLotteryHistoricRecord._Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[3]), iLotteryHistoricRecord._Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[4]), iLotteryHistoricRecord._Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[5]), iLotteryHistoricRecord._Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[6]), iLotteryHistoricRecord._Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[7]), iLotteryHistoricRecord._Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[8]), iLotteryHistoricRecord._Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[9]), iLotteryHistoricRecord._Numbers[7].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[10]), iLotteryHistoricRecord._Numbers[8].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[11]), iLotteryHistoricRecord._Numbers[9].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[12]), iLotteryHistoricRecord._Numbers[10].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[13]), iLotteryHistoricRecord._Numbers[11].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[14]), iLotteryHistoricRecord._Numbers[12].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[15]), iLotteryHistoricRecord._Numbers[13].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[16]), iLotteryHistoricRecord._Numbers[14].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[17]), iLotteryHistoricRecord._Numbers[15].ToString());
            parameters.Add(String.Format("@{0}", LotteryHistoricResultsTableColumns[18]), iLotteryHistoricRecord._Numbers[16].ToString());


            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2};", LotteryHistoricResultsTableName, LotteryHistoricResultsTableColumns[0], "'" + iLotteryHistoricRecord._LotteryRaffleID + "'"), _DefaultConnectionString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert(LotteryHistoricResultsTableName, _DefaultConnectionString, LotteryHistoricResultsTableColumns, parameters);
                }
                //else - DO NOTHING
                //{
                //    //this means that the record for the user and page already exists - DO NOTHING
                //    //need to update current record
                //    if (dt.Rows.Count == 1)
                //    {
                //        //we need to update the value
                //        SQLServerCommon.SQLServerCommon.Update("LotteryHistoricResults", _DefaultConnectionString, LotteryHistoricResultsTableColumns, parameters);
                //    }
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }


        //This function checks if the record exists in the DB before writing it - takes more time
        public void AddChosenLotteryTableRecordToDB(ChosenLottery777Table iLotteryChosenTable)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());


            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6} and {7} = {8} and {9} = {10} and {11} = {12} and {13} = {14};",
                                                                                          LotteryChosenTablesTableName,
                                                                                          LotteryChosenTablesTableColumns[0], "'" + iLotteryChosenTable.Numbers[0] + "'",
                                                                                          LotteryChosenTablesTableColumns[1], "'" + iLotteryChosenTable.Numbers[1] + "'",
                                                                                          LotteryChosenTablesTableColumns[2], "'" + iLotteryChosenTable.Numbers[2] + "'",
                                                                                          LotteryChosenTablesTableColumns[3], "'" + iLotteryChosenTable.Numbers[3] + "'",
                                                                                          LotteryChosenTablesTableColumns[4], "'" + iLotteryChosenTable.Numbers[4] + "'",
                                                                                          LotteryChosenTablesTableColumns[5], "'" + iLotteryChosenTable.Numbers[5] + "'",
                                                                                          LotteryChosenTablesTableColumns[6], "'" + iLotteryChosenTable.Numbers[6] + "'"),
                                                                                          _DefaultConnectionString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert(LotteryChosenTablesTableName, _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters);
                }
                else //UPDATE 5,6 & 7 hits
                {
                    //this means that the record for the user and page already exists
                    //need to update current record
                    if (dt.Rows.Count == 1)
                    {
                        if (int.Parse(dt.Rows[0][8].ToString()) > int.Parse(parameters["@Hits5"]) ||
                            int.Parse(dt.Rows[0][9].ToString()) > int.Parse(parameters["@Hits6"]) ||
                            int.Parse(dt.Rows[0][10].ToString()) > int.Parse(parameters["@Hits7"]))
                        {
                            string whereClause = string.Format(" Where Num1={0} and Num2={1} and Num3={2} and Num4={3} and Num5={4} and Num6={5} and Num7={6}",
                                                    iLotteryChosenTable.Numbers[0].ToString(),
                                                    iLotteryChosenTable.Numbers[1].ToString(),
                                                    iLotteryChosenTable.Numbers[2].ToString(),
                                                    iLotteryChosenTable.Numbers[3].ToString(),
                                                    iLotteryChosenTable.Numbers[4].ToString(),
                                                    iLotteryChosenTable.Numbers[5].ToString(),
                                                    iLotteryChosenTable.Numbers[6].ToString());
                            SQLServerCommon.SQLServerCommon.Update(LotteryChosenTablesTableName, _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters, whereClause); 
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        //This function doesn't check if the record exists in the DB (faster) - the programmer needs to check first or there will
        // be duplicate records in the DB
        public void AddChosenLotteryTableRecordToDBWithoutCheckingExistence(ChosenLottery777Table iLotteryChosenTable)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());


            try
            {

                SQLServerCommon.SQLServerCommon.Insert(LotteryChosenTablesTableName, _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AddChosenLotteryTableWinChainToDB(ChosenLottery777Table iLotteryChosenTable, int iRaffleID, int iHitCount)
        {
            //1.Make sure the winning table exists and get its ID (auto ID)
            //2.Add the win chain

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6} and {7} = {8} and {9} = {10} and {11} = {12} and {13} = {14};",
                                                                                          LotteryChosenTablesTableName,
                                                                                          LotteryChosenTablesTableColumns[0], "'" + iLotteryChosenTable.Numbers[0] + "'",
                                                                                          LotteryChosenTablesTableColumns[1], "'" + iLotteryChosenTable.Numbers[1] + "'",
                                                                                          LotteryChosenTablesTableColumns[2], "'" + iLotteryChosenTable.Numbers[2] + "'",
                                                                                          LotteryChosenTablesTableColumns[3], "'" + iLotteryChosenTable.Numbers[3] + "'",
                                                                                          LotteryChosenTablesTableColumns[4], "'" + iLotteryChosenTable.Numbers[4] + "'",
                                                                                          LotteryChosenTablesTableColumns[5], "'" + iLotteryChosenTable.Numbers[5] + "'",
                                                                                          LotteryChosenTablesTableColumns[6], "'" + iLotteryChosenTable.Numbers[6] + "'"),
                                                                                          _DefaultConnectionString);

                //Located the table, now add win chain to DB
                if (dt.Rows.Count == 1)
                {
                    Dictionary<string, string> parameters2 = new Dictionary<string, string>();

                    
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[0]), dt.Rows[0][0].ToString());
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[1]), iLotteryChosenTable.WinningRafflesTracking[iRaffleID].RaffleID.ToString());
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[2]), iHitCount.ToString());

                    DataTable dt2 = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4};",
                                                                                            WinningTables_RaffleIDsTableName,
                                                                                            WinningTables_RaffleIDsTableColumns[0], "'" + dt.Rows[0][0].ToString() + "'",
                                                                                            WinningTables_RaffleIDsTableColumns[1], "'" + iLotteryChosenTable.WinningRafflesTracking[iRaffleID].RaffleID + "'"),
                                                                                            _DefaultConnectionString);

                    if (dt2.Rows.Count == 0)
                    {
                        SQLServerCommon.SQLServerCommon.Insert(WinningTables_RaffleIDsTableName, _DefaultConnectionString, WinningTables_RaffleIDsTableColumns, parameters2);
                    }
                    //else - DO NOTHING, already in the DB!!
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void AddProcessedWinningResultToDBWithoutCheckingExistence(Lottery777WinningResult iWinningResult)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", ProcessedWinningTablesTableColumns[0]), iWinningResult._LotteryRaffleID.ToString());
            

            try
            {

                SQLServerCommon.SQLServerCommon.Insert(ProcessedWinningTablesTableName, _DefaultConnectionString, ProcessedWinningTablesTableColumns, parameters);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void InjectLotteryResultsToDB()
        {
            foreach (Lottery777WinningResult item in _LotteryHistoricResults)
	        {
                // x => x._LotteryRaffleID == item._LotteryRaffleID
                if (!_CurrentlyExistingWinningResults.Contains(item))
                {
                    AddLotteryHistoricResultRecordToDB(item); 
                }
	        }
        }

        private void GetCurrentWinningResultsFromDB()
        {
            Console.WriteLine("{0}: Populating Existing Winning results list from DB...", DateTime.Now);

            //get records from DB
            DataTable queryResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("Select * from {0}", LotteryHistoricResultsTableName), _DefaultConnectionString);

            //convert DB record to a Lottery777WinningResult
            for (int i = 0; i < queryResults.Rows.Count; i++)
            {

                Lottery777WinningResult res = new Lottery777WinningResult();
                //"RaffleID", "RaffleDate", "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Num8", "Num9", "Num10", "Num11", "Num12", "Num13", "Num14", "Num15", "Num16", "Num17"
                res._LotteryRaffleID = queryResults.Rows[i]["RaffleID"].ToString();
                res._LotteryDate = DateTime.Parse(queryResults.Rows[i]["RaffleDate"].ToString());

                res._Numbers = new int[17];
                res._Numbers[0] = int.Parse(queryResults.Rows[i]["Num1"].ToString());
                res._Numbers[1] = int.Parse(queryResults.Rows[i]["Num2"].ToString());
                res._Numbers[2] = int.Parse(queryResults.Rows[i]["Num3"].ToString());
                res._Numbers[3] = int.Parse(queryResults.Rows[i]["Num4"].ToString());
                res._Numbers[4] = int.Parse(queryResults.Rows[i]["Num5"].ToString());
                res._Numbers[5] = int.Parse(queryResults.Rows[i]["Num6"].ToString());
                res._Numbers[6] = int.Parse(queryResults.Rows[i]["Num7"].ToString());
                res._Numbers[7] = int.Parse(queryResults.Rows[i]["Num8"].ToString());
                res._Numbers[8] = int.Parse(queryResults.Rows[i]["Num9"].ToString());
                res._Numbers[9] = int.Parse(queryResults.Rows[i]["Num10"].ToString());
                res._Numbers[10] = int.Parse(queryResults.Rows[i]["Num11"].ToString());
                res._Numbers[11] = int.Parse(queryResults.Rows[i]["Num12"].ToString());
                res._Numbers[12] = int.Parse(queryResults.Rows[i]["Num13"].ToString());
                res._Numbers[13] = int.Parse(queryResults.Rows[i]["Num14"].ToString());
                res._Numbers[14] = int.Parse(queryResults.Rows[i]["Num15"].ToString());
                res._Numbers[15] = int.Parse(queryResults.Rows[i]["Num16"].ToString());
                res._Numbers[16] = int.Parse(queryResults.Rows[i]["Num17"].ToString());

                _CurrentlyExistingWinningResults.Add(res);
            }

            Console.WriteLine("{0}: Done Populating Existing Winning results list.", DateTime.Now);
        }

        private void GetChosenTablesFromDB()
        {
            Console.WriteLine("{0}: Populating Existing Chosen results list from DB...", DateTime.Now);

            //get records from DB
            DataTable queryResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format(sqlQueryGetChosenTables, LotteryChosenTablesTableName), _DefaultConnectionString);

            //convert DB record to a Lottery777WinningResult
            for (int i = 0; i < queryResults.Rows.Count; i++)
            {
                ChosenLottery777Table res = new ChosenLottery777Table();

                res.DB_ID = int.Parse(queryResults.Rows[i]["ID"].ToString());

                //"Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Hits5", "Hits6", "Hits7"
                res.Numbers = new int[7];
                res.Numbers[0] = int.Parse(queryResults.Rows[i]["Num1"].ToString());
                res.Numbers[1] = int.Parse(queryResults.Rows[i]["Num2"].ToString());
                res.Numbers[2] = int.Parse(queryResults.Rows[i]["Num3"].ToString());
                res.Numbers[3] = int.Parse(queryResults.Rows[i]["Num4"].ToString());
                res.Numbers[4] = int.Parse(queryResults.Rows[i]["Num5"].ToString());
                res.Numbers[5] = int.Parse(queryResults.Rows[i]["Num6"].ToString());
                res.Numbers[6] = int.Parse(queryResults.Rows[i]["Num7"].ToString());

                res.HitCountArray = new int[8];
                res.HitCountArray[5] = int.Parse(queryResults.Rows[i]["Hits5"].ToString());
                res.HitCountArray[6] = int.Parse(queryResults.Rows[i]["Hits6"].ToString());
                res.HitCountArray[7] = int.Parse(queryResults.Rows[i]["Hits7"].ToString());

                res.HitCount = res.HitCountArray[5] + res.HitCountArray[6] + res.HitCountArray[7];
                
                //get win chain for current chosen table
                //int tableID = int.Parse(queryResults.Rows[i]["ID"].ToString());
                //res.WinningRafflesTracking = new System.Collections.Generic.List<WinOccurence>();
                //DataTable queryWinChainResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("Select * from {0} where ChosenTableID={1}", WinningTables_RaffleIDsTableName, tableID), _DefaultConnectionString);

                //int rowCount = queryWinChainResults.Rows.Count;
                //for (int j = 0; j < rowCount; j++)
                //{
                //    res.WinningRafflesTracking.Add(new WinOccurence
                //    {
                //        RaffleID = int.Parse(queryWinChainResults.Rows[j]["WonRaffleID"].ToString()),
                //        HitCount = int.Parse(queryWinChainResults.Rows[j]["NumHits"].ToString())
                //    });
                //}

                _CurrentlyExistingChosenTables.Add(res);
            }

            _CurrentlyExistingChosenTables = _CurrentlyExistingChosenTables.Distinct(new DistinctChosenLottery777TableComparer()).ToHashSet();

            foreach (ChosenLottery777Table item in _CurrentlyExistingChosenTables)
	        {
                _CurrentlyExistingChosenTablesAsDict.Add(item.Numbers, item);
	        }

            //_CurrentlyExistingChosenTablesAsDict = _CurrentlyExistingChosenTables.ToDictionary(x => x.Numbers, x => x, new Distinct777TableComparer());

            Console.WriteLine("{0}: Done Populating Chosen results list.", DateTime.Now);
        }

        private HashSet<ChosenLottery777Table> GetAllChosenTablesFromDB(ref Dictionary<int[], ChosenLottery777Table> oChosenTablesAsDictionary)
        {
            Console.WriteLine("{0}: Populating Existing Chosen results list from DB...", DateTime.Now);
            HashSet<ChosenLottery777Table> dbChosenTables = new System.Collections.Generic.HashSet<ChosenLottery777Table>();

            //get records from DB
            DataTable queryResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format(sqlQueryGetAllChosenTables, LotteryChosenTablesTableName), _DefaultConnectionString);

            //convert DB record to a Lottery777WinningResult
            for (int i = 0; i < queryResults.Rows.Count; i++)
            {
                ChosenLottery777Table res = new ChosenLottery777Table();
                res.DB_ID = int.Parse(queryResults.Rows[i]["ID"].ToString());

                //"Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Hits5", "Hits6", "Hits7"
                res.Numbers = new int[7];
                res.Numbers[0] = int.Parse(queryResults.Rows[i]["Num1"].ToString());
                res.Numbers[1] = int.Parse(queryResults.Rows[i]["Num2"].ToString());
                res.Numbers[2] = int.Parse(queryResults.Rows[i]["Num3"].ToString());
                res.Numbers[3] = int.Parse(queryResults.Rows[i]["Num4"].ToString());
                res.Numbers[4] = int.Parse(queryResults.Rows[i]["Num5"].ToString());
                res.Numbers[5] = int.Parse(queryResults.Rows[i]["Num6"].ToString());
                res.Numbers[6] = int.Parse(queryResults.Rows[i]["Num7"].ToString());

                res.HitCountArray = new int[8];
                res.HitCountArray[5] = int.Parse(queryResults.Rows[i]["Hits5"].ToString());
                res.HitCountArray[6] = int.Parse(queryResults.Rows[i]["Hits6"].ToString());
                res.HitCountArray[7] = int.Parse(queryResults.Rows[i]["Hits7"].ToString());

                res.HitCount = res.HitCountArray[5] + res.HitCountArray[6] + res.HitCountArray[7];

                //get win chain for current chosen table
                //int tableID = int.Parse(queryResults.Rows[i]["ID"].ToString());
                //res.WinningRafflesTracking = new System.Collections.Generic.List<WinOccurence>();
                //DataTable queryWinChainResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("Select * from {0} where ChosenTableID={1}", WinningTables_RaffleIDsTableName, tableID), _DefaultConnectionString);

                //int rowCount = queryWinChainResults.Rows.Count;
                //for (int j = 0; j < rowCount; j++)
                //{
                //    res.WinningRafflesTracking.Add(new WinOccurence
                //    {
                //        RaffleID = int.Parse(queryWinChainResults.Rows[j]["WonRaffleID"].ToString()),
                //        HitCount = int.Parse(queryWinChainResults.Rows[j]["NumHits"].ToString())
                //    });
                //}

                dbChosenTables.Add(res);
            }

            foreach (ChosenLottery777Table item in dbChosenTables)
            {
                try
                {
                    oChosenTablesAsDictionary.Add(item.Numbers, item);
                }
                catch (Exception)
                {
                    
                    //throw;
                }
            }

            return dbChosenTables;
        }

        private void GetProcessedWinningRafflesFromDB()
        {
            Console.WriteLine("{0}: Populating Existing processed winning raffles list from DB...", DateTime.Now);

            //get records from DB
            DataTable queryResults = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("Select * from {0}", ProcessedWinningTablesTableName), _DefaultConnectionString);

            //convert DB record to a Lottery777WinningResult
            for (int i = 0; i < queryResults.Rows.Count; i++)
            {
                _CurrentlyExistingProcessedWinningTables.Add(int.Parse(queryResults.Rows[i]["ProcessedWinningRaffleID"].ToString()));
            }

            Console.WriteLine("{0}: Done Populating Chosen results list.", DateTime.Now);
        }

        //This function doesn't check if the record exists before updating, the programmer needs to check for himself - might raise an exception
        private void UpdateChosenTableInDB(ChosenLottery777Table iLotteryChosenTable)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());

            try
            {
                string whereClause = string.Format(" Where Num1={0} and Num2={1} and Num3={2} and Num4={3} and Num5={4} and Num6={5} and Num7={6}",
                                                    iLotteryChosenTable.Numbers[0].ToString(),
                                                    iLotteryChosenTable.Numbers[1].ToString(),
                                                    iLotteryChosenTable.Numbers[2].ToString(),
                                                    iLotteryChosenTable.Numbers[3].ToString(),
                                                    iLotteryChosenTable.Numbers[4].ToString(),
                                                    iLotteryChosenTable.Numbers[5].ToString(),
                                                    iLotteryChosenTable.Numbers[6].ToString());
                SQLServerCommon.SQLServerCommon.Update(LotteryChosenTablesTableName, _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters, whereClause);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void UpdateChosenLotteryTableWinChain(ChosenLottery777Table iLotteryChosenTable)
        {
            //To update the win chain, we first need to get the records that already exist,
            //then discard all the records that already exist
            //And then, add whats left to the DB

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6} and {7} = {8} and {9} = {10} and {11} = {12} and {13} = {14};",
                                                                                          LotteryChosenTablesTableName,
                                                                                          LotteryChosenTablesTableColumns[0], "'" + iLotteryChosenTable.Numbers[0] + "'",
                                                                                          LotteryChosenTablesTableColumns[1], "'" + iLotteryChosenTable.Numbers[1] + "'",
                                                                                          LotteryChosenTablesTableColumns[2], "'" + iLotteryChosenTable.Numbers[2] + "'",
                                                                                          LotteryChosenTablesTableColumns[3], "'" + iLotteryChosenTable.Numbers[3] + "'",
                                                                                          LotteryChosenTablesTableColumns[4], "'" + iLotteryChosenTable.Numbers[4] + "'",
                                                                                          LotteryChosenTablesTableColumns[5], "'" + iLotteryChosenTable.Numbers[5] + "'",
                                                                                          LotteryChosenTablesTableColumns[6], "'" + iLotteryChosenTable.Numbers[6] + "'"),
                                                                                          _DefaultConnectionString);

                //Located the table, now get the existing win chain from the DB
                if (dt.Rows.Count == 1)
                {
                    Dictionary<string, string> parameters2 = new Dictionary<string, string>();

                    //Getting the Chosen table ID from the query result
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[0]), dt.Rows[0][0].ToString());

                    //Getting all records connected with the wanted Chosen table ID
                    DataTable dt2 = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2}",
                                                                                            WinningTables_RaffleIDsTableName,
                                                                                            WinningTables_RaffleIDsTableColumns[0], "'" + dt.Rows[0][0].ToString() + "'"),
                                                                                            _DefaultConnectionString);

                    //Convert the query results into an actual win chain
                    List<WinOccurence> oldWinChain = ConvertDBDataToWinChain(dt2);

                    //subtruct new record win chain from the old one, then add the difference to the DB
                    List<WinOccurence> difference = iLotteryChosenTable.WinningRafflesTracking.Except(oldWinChain, new WinOccurenceComparer()).ToList();
                    int chosenTableID = int.Parse(dt.Rows[0][0].ToString());
                    for (int i = 0; i < difference.Count; i++)
                    {
                        Dictionary<string, string> parameters3 = new Dictionary<string, string>();

                        parameters3.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[0]), chosenTableID.ToString());
                        parameters3.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[1]), difference[i].RaffleID.ToString());
                        parameters3.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[2]), difference[i].HitCount.ToString());

                        SQLServerCommon.SQLServerCommon.Insert(WinningTables_RaffleIDsTableName, _DefaultConnectionString, WinningTables_RaffleIDsTableColumns, parameters3);
                    }                   
                }
            }
            catch (Exception)
            {

                throw;
            }

            
        }

        public List<WinOccurence> GetWinChainForExistingChosenTable(ChosenLottery777Table iLotteryChosenTable)
        {
            //To update the win chain, we first need to get the records that already exist,
            //then discard all the records that already exist
            //And then, add whats left to the DB

            List<WinOccurence> winChain = new System.Collections.Generic.List<WinOccurence>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[0]), iLotteryChosenTable.Numbers[0].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[1]), iLotteryChosenTable.Numbers[1].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[2]), iLotteryChosenTable.Numbers[2].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[3]), iLotteryChosenTable.Numbers[3].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[4]), iLotteryChosenTable.Numbers[4].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[5]), iLotteryChosenTable.Numbers[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[6]), iLotteryChosenTable.Numbers[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[7]), iLotteryChosenTable.HitCountArray[5].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[8]), iLotteryChosenTable.HitCountArray[6].ToString());
            parameters.Add(String.Format("@{0}", LotteryChosenTablesTableColumns[9]), iLotteryChosenTable.HitCountArray[7].ToString());

            try
            {
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4} and {5} = {6} and {7} = {8} and {9} = {10} and {11} = {12} and {13} = {14};",
                                                                                          LotteryChosenTablesTableName,
                                                                                          LotteryChosenTablesTableColumns[0], "'" + iLotteryChosenTable.Numbers[0] + "'",
                                                                                          LotteryChosenTablesTableColumns[1], "'" + iLotteryChosenTable.Numbers[1] + "'",
                                                                                          LotteryChosenTablesTableColumns[2], "'" + iLotteryChosenTable.Numbers[2] + "'",
                                                                                          LotteryChosenTablesTableColumns[3], "'" + iLotteryChosenTable.Numbers[3] + "'",
                                                                                          LotteryChosenTablesTableColumns[4], "'" + iLotteryChosenTable.Numbers[4] + "'",
                                                                                          LotteryChosenTablesTableColumns[5], "'" + iLotteryChosenTable.Numbers[5] + "'",
                                                                                          LotteryChosenTablesTableColumns[6], "'" + iLotteryChosenTable.Numbers[6] + "'"),
                                                                                          _DefaultConnectionString);

                //Located the table, now get the existing win chain from the DB
                if (dt.Rows.Count == 1)
                {
                    Dictionary<string, string> parameters2 = new Dictionary<string, string>();

                    //Getting the Chosen table ID from the query result
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[0]), dt.Rows[0][0].ToString());

                    //Getting all records connected with the wanted Chosen table ID
                    DataTable dt2 = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2}",
                                                                                            WinningTables_RaffleIDsTableName,
                                                                                            WinningTables_RaffleIDsTableColumns[0], "'" + dt.Rows[0][0].ToString() + "'"),
                                                                                            _DefaultConnectionString);

                    //Convert the query results into an actual win chain
                    List<WinOccurence> oldWinChain = ConvertDBDataToWinChain(dt2);
                    winChain = oldWinChain;
                }
            }
            catch(Exception)
            {
                throw;
            }


            return winChain;
        }

        private bool IsWinChainIdentical(ChosenLottery777Table iLotteryChosenTable1, ChosenLottery777Table iLotteryChosenTable2)
        {
            bool isIdentical = true;

            if (iLotteryChosenTable1.WinningRafflesTracking.Count != iLotteryChosenTable2.WinningRafflesTracking.Count)
            {
                isIdentical = false;
            }
            else
            {
                for (int i = 0; i < iLotteryChosenTable1.WinningRafflesTracking.Count; i++)
                {
                    if (iLotteryChosenTable1.WinningRafflesTracking[i] != iLotteryChosenTable2.WinningRafflesTracking[i])
                    {
                        isIdentical = false;
                        break;
                    }
                }
            }

            return isIdentical;
        }

        private List<WinOccurence> ConvertDBDataToWinChain(DataTable iWinChain)
        {
            List<WinOccurence> res = new System.Collections.Generic.List<WinOccurence>();

            for (int j = 0; j < iWinChain.Rows.Count; j++)
            {
                res.Add(new WinOccurence
                {
                    RaffleID = int.Parse(iWinChain.Rows[j]["WonRaffleID"].ToString()),
                    HitCount = int.Parse(iWinChain.Rows[j]["NumHits"].ToString())
                });
            }

            return res;
        }

        private DataTable GetRawTableData(string iSqlQuery, string iConnectionString)
        {
            DataTable tmpDataTable = null;

            try
            {
                tmpDataTable = SQLServerCommon.SQLServerCommon.ExecuteQuery(iSqlQuery, iConnectionString);
            }
            catch (Exception)
            {
                
                throw;
            }

            return tmpDataTable;
        }

        public List<WinOccurence> GetWinChainForExistingChosenTable2(ChosenLottery777Table iLotteryChosenTable)
        {
            DataTable rawWinchain = null;

            try
            {
                if (_CurrentlyExistingChosenTablesWinChain != null)
                {
                    rawWinchain = _CurrentlyExistingChosenTablesWinChain.Select("ChosenTableID=" + iLotteryChosenTable.DB_ID.ToString()).CopyToDataTable();
                }
                else
                {
                    _CurrentlyExistingChosenTablesWinChain = GetRawTableData("Select * from WinningTables_RaffleIDs", _DefaultConnectionString);

                    rawWinchain = _CurrentlyExistingChosenTablesWinChain.Select("ChosenTableID=" + iLotteryChosenTable.DB_ID.ToString()).CopyToDataTable();
                }
            }
            catch (Exception)
            {
            }

            List<WinOccurence> winchain = null;

            if (rawWinchain !=null)
            {
                winchain = ConvertDBDataToWinChain(rawWinchain); 
            }

            return winchain;
        }

        #endregion
    }

    public static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}
