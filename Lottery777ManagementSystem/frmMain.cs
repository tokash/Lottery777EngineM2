using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lottery777EngineM3;
using SQLServerCommon;
using System.Diagnostics;

namespace Lottery777ManagementSystem
{
    public partial class frmMain : Form
    {

        //The engine filters the records from the DB to have no more than 4 similar numbers in a table
        MyLottery777Engine _LotteryEngine;// = new MyLottery777Engine("777.csv", false);
        bool _5HitsMarked = false;
        bool _6HitsMarked = false;
        bool _7HitsMarked = false;
        bool _GeneralHitsMarked = false;
        Stopwatch _Sw = new Stopwatch();
        frmPleaseWait frmPleaseWait = new frmPleaseWait();

        List<string> _ThreadMessages = new List<string>();

        internal static readonly string _DefaultConnectionString = "Server=TOKASHYO-PC\\SQLEXPRESS;User Id=sa;Password=tokash30;database=Lottery777DB";

        public frmMain()
        {
            frmPleaseWait.Show();
            frmPleaseWait.Refresh();
            InitializeComponent();
            _LotteryEngine = new MyLottery777Engine("777.csv", false);
            frmPleaseWait.Hide();

            lbxLog.Items.Add(string.Format("{0}: {1} Records loaded, Lowest hit count: {2}.", DateTime.Now, _LotteryEngine.ExistingWinningChosenTables.Count, 82));
            

            //TODO: Add thread to download new 777 results file and update DB with new records
            //TODO: Add thread to do the search - this should take pressure off the UI thread 
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //Check which checkboxes are marked, then call calculating function

            if (cb5Hits.Checked)
            {
                _5HitsMarked = true;
            }
            else
            {
                _5HitsMarked = false;
            }

            if (cb6Hits.Checked)
            {
                _6HitsMarked = true;
            }
            else
            {
                _6HitsMarked = false;
            }

            if (cb7Hits.Checked)
            {
                _7HitsMarked = true;
            }
            else
            {
                _7HitsMarked = false;
            }

            //if (lbxLog.Items.Count == 12)
            //{
            //    lbxLog.Items.Clear();
            //}

            lbxLog.Items.Add(string.Format("{0}:Started searching...", DateTime.Now));
            _Sw.Stop();
            _Sw.Reset();
            _Sw.Start();
            SearchDB();
            _Sw.Stop();
            lbxLog.Items.Add(string.Format("{0}:Finished searching, found {1} tables, search took: {2}.", DateTime.Now, _LotteryEngine.WinningChosenTables.Count, _Sw.Elapsed));
            lbxLog.SelectedIndex = lbxLog.Items.Count - 1;
            lbxLog.SelectedIndex = -1;
        }

        private void SearchDB()
        {
            _LotteryEngine.WinningChosenTables.Clear();
            try
            {
                if (tbSqlQuery.Text != string.Empty)
                {
                    dgvSearchResults.DataSource = SQLServerCommon.SQLServerCommon.ExecuteQuery(tbSqlQuery.Text, _DefaultConnectionString);
                }
                else
                {

                    if (nudNumHits.Value != 0 && nudLastNRaffles.Value != 0)
                    {
                        //dgvSearchResults.Rows.Clear();

                        //Need to go over all chosen tables,
                        //check if criterion is met and get them from the DB\convert their data into a datatable
                        int i = 1;
                        foreach (ChosenLottery777Table item in _LotteryEngine.ExistingWinningChosenTables)
                        {
                            //_Sw.Stop();
                            //_Sw.Reset();
                            //_Sw.Start();
                            if (item.WinningRafflesTracking == null && item.IsChecked == false)
                            {
                                item.WinningRafflesTracking = _LotteryEngine.GetWinChainForExistingChosenTable2(item);

                                //marking record as checked
                                item.IsChecked = true;
                            }
                            //_Sw.Stop();

                            //lbxLog.Items.Add(string.Format("Getting winchain for item {0} took {1}", i, _Sw.Elapsed));
                            i++;
                            if (item.WinningRafflesTracking != null)
                            {
                                if (_LotteryEngine.IsChoosingCriterionMet(item, Decimal.ToInt32(nudNumHits.Value), Decimal.ToInt32(nudLastNRaffles.Value)))
                                {
                                    _LotteryEngine.WinningChosenTables.Add(item);
                                } 
                            } 
                        }

                        //Convert found item to data table
                        DataTable dt = new DataTable();
                        dt.Clear();
                        dt.Columns.Add("Num1");
                        dt.Columns.Add("Num2");
                        dt.Columns.Add("Num3");
                        dt.Columns.Add("Num4");
                        dt.Columns.Add("Num5");
                        dt.Columns.Add("Num6");
                        dt.Columns.Add("Num7");
                        dt.Columns.Add("Hits5");
                        dt.Columns.Add("Hits6");
                        dt.Columns.Add("Hits7");
                        dt.Columns.Add("TotalHits");

                        for (int j = 0; j < _LotteryEngine.WinningChosenTables.Count; j++)
                        {
                            DataRow _ravi = dt.NewRow();
                            _ravi["Num1"] = _LotteryEngine.WinningChosenTables[j].Numbers[0];
                            _ravi["Num2"] = _LotteryEngine.WinningChosenTables[j].Numbers[1];
                            _ravi["Num3"] = _LotteryEngine.WinningChosenTables[j].Numbers[2];
                            _ravi["Num4"] = _LotteryEngine.WinningChosenTables[j].Numbers[3];
                            _ravi["Num5"] = _LotteryEngine.WinningChosenTables[j].Numbers[4];
                            _ravi["Num6"] = _LotteryEngine.WinningChosenTables[j].Numbers[5];
                            _ravi["Num7"] = _LotteryEngine.WinningChosenTables[j].Numbers[6];
                            _ravi["Hits5"] = _LotteryEngine.WinningChosenTables[j].HitCountArray[5];
                            _ravi["Hits6"] = _LotteryEngine.WinningChosenTables[j].HitCountArray[6];
                            _ravi["Hits7"] = _LotteryEngine.WinningChosenTables[j].HitCountArray[7];
                            _ravi["TotalHits"] = _LotteryEngine.WinningChosenTables[j].HitCountArray[5] +
                                                _LotteryEngine.WinningChosenTables[j].HitCountArray[6] +
                                                _LotteryEngine.WinningChosenTables[j].HitCountArray[7];
                            dt.Rows.Add(_ravi); 
                        }

                        dgvSearchResults.DataSource = dt;

                    }
                    else
                    {
                        dgvSearchResults.DataSource = SQLServerCommon.SQLServerCommon.ExecuteQuery("select * from LotteryChosenTables", _DefaultConnectionString);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(ex.ToString()));
            }
        }

        private void smiExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //this function should work in the background while updating the listboxview
        private void smiUpdate_Click(object sender, EventArgs e)
        {
            //Get latest raffle id from db
            //Get latest raffle id from latest downloaded results file
            //go over all raffles until the latest id and update db using a thread.

            int latestDBRaffleID = -1;
            int latestRealRaffleID = int.Parse(_LotteryEngine.WinningResults[0]._LotteryRaffleID);
            DataTable dt = SQLServerCommon.SQLServerCommon.ExecuteQuery("SELECT MAX(ProcessedWinningRaffleID) FROM [Lottery777DB].[dbo].ProcessedWinningTable", _DefaultConnectionString);
            if (dt.Rows.Count == 1)
            {
                latestDBRaffleID = int.Parse(dt.Rows[0][0].ToString());

                Action dbupdateEndedcallback = delegate()
                {
                    lbxLog.Invoke(new Action(() => lbxLog.Items.Add(string.Format("{0}:DB Update thread ended successfully.", DateTime.Now))));
                    lbxLog.Invoke(new Action(() => lbxLog.Items.Add(string.Format("{0}:Finished Updating.", DateTime.Now))));
                };

                //do a loop on all results until latest db raffle id
                Action generationEndedcallback = delegate()
                {
                    lbxLog.Invoke(new Action(() => lbxLog.Items.Add(string.Format("{0}:Generation thread ended successfully.", DateTime.Now))));
                    Task updateDBTask = Task.Factory.StartNew(() => _LotteryEngine.UpdateDBThread(dbupdateEndedcallback));
                    lbxLog.Invoke(new Action(() => lbxLog.Items.Add(string.Format("{0}:Started DB Update thread...", DateTime.Now))));
                };
                
                lbxLog.Items.Add(string.Format("{0}:Updating {1} raffles", DateTime.Now, latestRealRaffleID - latestDBRaffleID));
                Task tableGenerationTask = Task.Factory.StartNew(() => _LotteryEngine.GenerateLottery777TablesParallelThread(latestRealRaffleID - latestDBRaffleID, 0, generationEndedcallback));
                lbxLog.Items.Add(string.Format("{0}:Started generation thread...", DateTime.Now));
                
            }

            

        }
    }
}
