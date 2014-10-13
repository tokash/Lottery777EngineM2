using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lottery777EngineM2
{
    public class Lottery777Table
    {
        public int[] _Numbers = new int[17];
    }

    public class Lottery777WinningResult: Lottery777Table
    {
        public DateTime _LotteryDate { get; set; }
        public string _LotteryRaffleID { get; set; }
        public int _HitCount { get; set; }
        public int[] _Ranks { get; set; }
    }

    public class ShortLottery777Table
    {
        public int[] _Numbers = new int[17];
    }

    public class PartialLottery777Table
    {
        public int[] Numbers = new int[17];
        public int Commonness;
    }

    public class NumberCommoness
    {
        public int Number;
        public int Commoness;
        public int Rank = 1;
    }

    public class ChosenLottery777Table
    {
        public int[] Numbers = new int[7];
        //public int[] Ranks = new int[16];
        //public int Leading;        
        public int[] HitCountArray;
        public int HitCount;
        public List<int> WinningRafflesTracking;
        //public int[] HitDispersion;
        //public int StrongNumber;
    }

    public class ChosenLottery777MethodicalTable
    {
        public int[] Numbers = new int[9];
        //public int[] Ranks = new int[16];
        //public int Leading;        
        public int[] HitCountArray;
        //public int TotalHitcount;
        //public int[] HitDispersion;
        //public int StrongNumber;

        public int HitCount;
    }

    class DistinctItemComparer : IEqualityComparer<int[]>
    {

        public bool Equals(int[] x, int[] y)
        {
            bool isEqual = true;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Length == y.Length)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        isEqual = false;
                        break;
                    }
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(int[] obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class DistinctChosenLottery777TableComparer : IEqualityComparer<ChosenLottery777Table>
    {

        public bool Equals(ChosenLottery777Table x, ChosenLottery777Table y)
        {
            bool isEqual = true;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Numbers.Length == y.Numbers.Length)
            {
                int count = x.Numbers.Intersect(y.Numbers).ToList().Count;
                if (count < 4)
                {
                    isEqual = false;
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(ChosenLottery777Table obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class DistinctFullChosenLottery777TableComparer : IEqualityComparer<ChosenLottery777Table>
    {

        public bool Equals(ChosenLottery777Table x, ChosenLottery777Table y)
        {
            bool isEqual = false;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Numbers.Length == y.Numbers.Length)
            {
                int count = x.Numbers.Intersect(y.Numbers).ToList().Count;
                //if (count > 3)
                if (count == 7)
                {
                    isEqual = true;
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(ChosenLottery777Table obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class DistinctChosenLottery777MethodicalTableComparer : IEqualityComparer<ChosenLottery777MethodicalTable>
    {

        public bool Equals(ChosenLottery777MethodicalTable x, ChosenLottery777MethodicalTable y)
        {
            bool isEqual = true;
            

            if (x.Numbers.Length == y.Numbers.Length)
            {
                int count = x.Numbers.Intersect(y.Numbers).ToList().Count;
                if (count < 6)
                {
                    isEqual = false;
                }
            }
            else
            {
                isEqual = false;
            }

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            //if (x.Numbers.Length == y.Numbers.Length)
            //{
            //    for (int i = 0; i < x.Numbers.Length; i++)
            //    {
            //        if (x.Numbers[i] != y.Numbers[i])
            //        {
            //            isEqual = false;
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    isEqual = false;
            //}

            return isEqual;
        }

        public int GetHashCode(ChosenLottery777MethodicalTable obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class DistinctRanks777Comparer : IEqualityComparer<int[]>
    {

        public bool Equals(int[] x, int[] y)
        {
            bool isEqual = true;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Length == y.Length)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        isEqual = false;
                        break;
                    }
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(int[] obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class Distinct777TablePartialComparer : IEqualityComparer<int[]>
    {

        public bool Equals(int[] x, int[] y)
        {
            bool isEqual = false;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Length == y.Length)
            {
                int intersect = x.Intersect(y).Count();

                if (intersect > 1)
                {
                    isEqual = true;
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(int[] obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }

    class Distinct777TableComparer : IEqualityComparer<int[]>
    {

        public bool Equals(int[] x, int[] y)
        {
            bool isEqual = false;

            //Console.WriteLine(string.Format("{0} - {1}", x.Length, y.Length));
            if (x.Length == y.Length)
            {
                int intersect = x.Intersect(y).Count();

                if (intersect == 7)
                {
                    isEqual = true;
                }
            }
            else
            {
                isEqual = false;
            }

            return isEqual;
        }

        public int GetHashCode(int[] obj)
        {
            return base.GetHashCode();
            //return obj.Id.GetHashCode() ^
            //    obj.Name.GetHashCode() ^
            //    obj.Code.GetHashCode() ^
            //    obj.Price.GetHashCode();
        }
    }
}
