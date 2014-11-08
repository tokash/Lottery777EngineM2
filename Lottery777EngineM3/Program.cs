using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lottery777EngineM3;

namespace Lottery777EngineM3
{
    class Program
    {
        static void Main(string[] args)
        {
            MyLottery777Engine lotteryEngine = new MyLottery777Engine("777.csv", false);

            try
            {
                lotteryEngine.GenerateLottery777TablesParallel(lotteryEngine.WinningResults.Count, 80);
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
