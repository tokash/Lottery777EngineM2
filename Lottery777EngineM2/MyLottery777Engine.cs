using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Data;

namespace Lottery777EngineM2
{
    class MyLottery777Engine
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

            InjectLotteryResultsToDB();
        }
        #endregion

        private const double TaxThreshold = 50000;
        private const double MaxPartialTaxThreshold = 100000; 
        private const double TaxPercentage = 0.3;

        private static readonly string dbName = "Lottery777DB";

        internal static readonly string _MasterConnectionString = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=master";
        internal static readonly string _DefaultConnectionString = "Server=TOKASHYOS-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=Lottery777DB";

        private static readonly string sqlCommandCreateDB = "CREATE DATABASE " + dbName + " ON PRIMARY " +
                "(NAME = " + dbName + ", " +
                "FILENAME = 'D:\\" + dbName + ".mdf', " +
                "SIZE = 3MB, MAXSIZE = 100000MB, FILEGROWTH = 10%) " +
                "LOG ON (NAME = " + dbName + "_LOG, " +
                "FILENAME = 'D:\\" + dbName + ".ldf', " +
                "SIZE = 3MB, " +
                "MAXSIZE = 100000MB, " +
                "FILEGROWTH = 10%)";

        internal static readonly string LotteryHistoricResultsTableSchema = "CREATE TABLE LotteryHistoricResults (ID int IDENTITY(1,1), RaffleID int NOT NULL, RaffleDate date, Num1 int NOT NULL , Num2 int NOT NULL, Num3 int NOT NULL, Num4 int NOT NULL, Num5 int NOT NULL, Num6 int NOT NULL, Num7 int NOT NULL, Num8 int NOT NULL, Num9 int NOT NULL, Num10 int NOT NULL, Num11 int NOT NULL, Num12 int NOT NULL, Num13 int NOT NULL, Num14 int NOT NULL, Num15 int NOT NULL, Num16 int NOT NULL, Num17 int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] LotteryHistoricResultsTableColumns = { "RaffleID", "RaffleDate", "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Num8", "Num9", "Num10", "Num11", "Num12", "Num13", "Num14", "Num15", "Num16", "Num17" };

        internal static readonly string LotteryChosenTablesTableSchema = "CREATE TABLE LotteryChosenTables (ID int IDENTITY(1,1), Num1 int NOT NULL, Num2 int NOT NULL, Num3 int NOT NULL, Num4 int NOT NULL, Num5 int NOT NULL, Num6 int NOT NULL, Num7 int NOT NULL, Hits5 int NOT NULL, Hits6 int NOT NULL, Hits7 int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] LotteryChosenTablesTableColumns = { "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Hits5", "Hits6", "Hits7" };

        internal static readonly string WinningTables_RaffleIDsTableSchema = "CREATE TABLE WinningTables_RaffleIDs (ID int IDENTITY(1,1), ChosenTableID int NOT NULL, WonRaffleID int NOT NULL, NumHits int NOT NULL, PRIMARY KEY (ID))";
        internal static readonly string[] WinningTables_RaffleIDsTableColumns = { "ChosenTableID", "WonRaffleID", "NumHits" };

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

        private int GetHitCountForLastNResults(int[] iGeneratedTable, int iTolerance, int iNumberOfResultsToConsider, ref int[] oHitCountArray, ref List<int> oWinningRaffleTracking)
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
                    oWinningRaffleTracking.Add(int.Parse(_LotteryHistoricResults[i]._LotteryRaffleID));
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
                    List<int> winningRaffleTracking = new System.Collections.Generic.List<int>();
                    int hitCountTotal = GetHitCountForLastNResults(currItem.Numbers, 5, iNumResultsToConsider, ref hitCountArray, ref winningRaffleTracking);
                    
                    if (hitCountTotal >= 80 || hitCountArray[6] >= 10)// && !iChosen.Contains(item, new DistinctChosenLottery777TableComparer()))
                    {
                        currItem.HitCount = hitCountTotal;
                        currItem.HitCountArray = hitCountArray;
                        currItem.WinningRafflesTracking = winningRaffleTracking;

                        //INSTEAD: Writing to DB
                        lock (this)
                        {
                            AddChosenLotteryTableRecordToDB(currItem);
                            for (int j = 0; j < currItem.WinningRafflesTracking.Count; j++ )
                            {
                                int rafflePlace = _LotteryHistoricResults.FindIndex(x => int.Parse(x._LotteryRaffleID) == currItem.WinningRafflesTracking[j]);
                                int hitCount = currItem.Numbers.Intersect(_LotteryHistoricResults[rafflePlace]._Numbers).Count();

                                try
                                {
                                    AddChosenLotteryTableWinChainToDB(currItem, j, hitCount);
                                }
                                catch (Exception)
                                {
                                    
                                    throw;
                                }
                            }
                        }

                        //lock (this)
                        //{
                        //    WriteChosenCombinationToFile(iFilename, currItem);
                        //}
                    }
                }
                );

                sw.Stop();
                Console.WriteLine(string.Format("{0} out of {1}: Going over all combinations took: {2}", i + 1, iNumResultsToConsider, sw.Elapsed));
                Console.Write("\r");
                sw.Reset();

                generatedPossibilites.Clear();
            }

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
                            currChosenTable.WinningRafflesTracking = new System.Collections.Generic.List<int>();
                        }
                        currChosenTable.WinningRafflesTracking.Add(iRaffleNo);

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

        private bool IsChoosingCriterionMet(ChosenLottery777Table iLotteryTable, int iNumWinnings, int iRaffleTollerance)
        {
            bool retVal = false;
            int lastWin = iLotteryTable.WinningRafflesTracking[0];

            //Need to calc difference between 2 adjacent raffle numbers
            List<int> differences = new System.Collections.Generic.List<int>();

            for (int i = 1; i < iLotteryTable.WinningRafflesTracking.Count; i++)
            {

                if (iLotteryTable.WinningRafflesTracking[i] > lastWin - iRaffleTollerance && i + 1 < iLotteryTable.WinningRafflesTracking.Count)
                {
                    differences.Add(iLotteryTable.WinningRafflesTracking[i - 1] - iLotteryTable.WinningRafflesTracking[i]);
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

            foreach (int winningRaffle in iLotteryTable.WinningRafflesTracking)
            {
                //Check if won the big prize
                int count = _LotteryHistoricResults.Find((x) => int.Parse(x._LotteryRaffleID) == winningRaffle)._Numbers.Intersect(iLotteryTable.Numbers).ToList().Count;

                if (count == 7)
                {
                    if (int.Parse(_LotteryHistoricResults[0]._LotteryRaffleID) - winningRaffle <= iRaffleTolerance)
                    {
                        isHit = true;
                        break;
                    }
                }
            }

            return isHit;
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
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void AddLotteryHistoricResultRecordToDB(Lottery777WinningResult iLotteryHistoricRecord)
        {
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
                DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2};", "LotteryHistoricResults", LotteryHistoricResultsTableColumns[0], "'" + iLotteryHistoricRecord._LotteryRaffleID + "'"), _DefaultConnectionString);
                if (dt.Rows.Count == 0)
                {
                    SQLServerCommon.SQLServerCommon.Insert("LotteryHistoricResults", _DefaultConnectionString, LotteryHistoricResultsTableColumns, parameters);
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
                                                                                          "LotteryChosenTables",
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
                    SQLServerCommon.SQLServerCommon.Insert("LotteryChosenTables", _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters);
                }
                else //UPDATE 5,6 & 7 hits
                {
                    //this means that the record for the user and page already exists
                    //need to update current record
                    if (dt.Rows.Count == 1)
                    {
                        SQLServerCommon.SQLServerCommon.Update("LotteryChosenTables", _DefaultConnectionString, LotteryChosenTablesTableColumns, parameters);
                    }
                }
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
                                                                                          "LotteryChosenTables",
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
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[1]), iLotteryChosenTable.WinningRafflesTracking[iRaffleID].ToString());
                    parameters2.Add(String.Format("@{0}", WinningTables_RaffleIDsTableColumns[2]), iHitCount.ToString());

                    DataTable dt2 = SQLServerCommon.SQLServerCommon.ExecuteQuery(String.Format("select * from {0} where {1} = {2} and {3} = {4};",
                                                                                            "WinningTables_RaffleIDs",
                                                                                            WinningTables_RaffleIDsTableColumns[0], "'" + dt.Rows[0][0].ToString() + "'",
                                                                                            WinningTables_RaffleIDsTableColumns[1], "'" + iLotteryChosenTable.WinningRafflesTracking[iRaffleID] + "'"),
                                                                                            _DefaultConnectionString);

                    if (dt2.Rows.Count == 0)
                    {
                        SQLServerCommon.SQLServerCommon.Insert("WinningTables_RaffleIDs", _DefaultConnectionString, WinningTables_RaffleIDsTableColumns, parameters2);
                    }
                    //else - DO NOTHING, already in the DB!!
                }
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
                AddLotteryHistoricResultRecordToDB(item);
	        }
        }

        #endregion
    }
}
