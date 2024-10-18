//#region Using declarations

//using System;
//using System.Windows.Data;
//using AlgoDesk.Core.DataCalc;
//using AlgoDesk.Core.DataCalc.Optimization;
//using AlgoDesk.Core.Enums;
//using AlgoDesk.Custom.Utils;

//#endregion

//namespace AlgoDesk.Custom.OptimizationFitnesses
//{
//    public class MaxGraalK : OptimizationFitness
//    {
//        private const string OptimizationFitnessName = "Max Graal";

//        public double[] Strategy2SamplePath(StrategyBase strategy)
//        {
//            var n = strategy.SystemPerformance.Trades.Count + 1;
//            //
//            double pS = 0, openPrice = 0, closePrice = 0;
//            var L = new double[n];
//            L[0] = 0;
//            double capEst = 0;
//            for (int j = 1; j < n; j++)
//            {
//                var trade = strategy.SystemPerformance.Trades[j-1];
//                double cp = trade.CumProfit;
//                L[j] = cp;
//                double cap = trade.EntryPrice * trade.EntryQuantity;
//                if (j >= 1)
//                {
//                    cap -= trade.CumProfit;
//                }
//                if (cap > capEst)
//                    capEst = cap;
//                //double p1 = 0.5 * (trade.ExitPrice + trade.EntryPrice);
//                //if (j == 1)
//                //    openPrice = p1;
//                //else if (j == n - 1)
//                //    closePrice = p1;
//                //pS += p1;
//            }
//            //double pE = Math.Pow(openPrice * closePrice * (pS / (n - 1)), 1.0 / 3);
//            //pE = strategy.SystemPerformance.Trades[0].EntryPrice;
//            for (int j = 0; j < n; j++)
//                L[j] /= capEst;
//            return L;
//        }

//        double[] ema(double[] A, double L)
//        {
//            var q = Math.Exp(-1.0 / L);
//            var B = new double[A.Length];
//            B[0] = A[0];
//            for (int j = 1; j < A.Length; j++)
//            {
//                B[j] = (1 - q) * A[j] + q * B[j - 1];
//            }
//            return B;
//        }

//        public override void OnCalculatePerformanceValue(StrategyBase strategy)
//        {
//            var path = Strategy2SamplePath(strategy);
//            if (path.Length == 1)
//            {
//                var path2 = new double[2];
//                path2[0] = path2[1] = path[0];
//                path = path2;
//            }
//            var emaL = (int)Math.Round(0.5 * Math.Sqrt(path.Length));
//            emaL = Math.Max(Math.Min(emaL, 5), 2);
//            path = ema(path, emaL);
//            var lr = new LinearRegression(0, path.Length-1, 1);
//            var res = lr.analyzePathX(path);

//            var alpha = res[0];
//            //var b = res[1];
//            var a0 = alpha * path.Length;
//            var a1 = path[path.Length - 1] - path[0];
//            var maxNetP = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.NetProfitSum);
//            double aa;
//            if (a0 < 0 || a1 < 0)
//                aa = Math.Min(a0, a1);
//            else
//                aa = Math.Sqrt(a0 * a1);
//            //double v0 = alpha;
//            double pen1 = 250 * Math.Pow(res[5], 3); // 'weightedF2'
//            double pen2 = 2250 * Math.Pow(res[6], 3); // 'D2'
//            //
//            var penalty = pen1 + pen2;
//            var pK = 0.2 * penalty;
//            //var graal = aa - pK;
//            var graal = - pK;
//            //
//            if (aa > 0.1)
//            {
//            }
//            if (graal > 0.2 && maxNetP < 0)
//            {
//            }

//            var equityHighs = strategy.SystemPerformance.Summary.GetValue<int>(AnalyticalFeature.EquityHighs); ;
            

//            double value = 0;

//            var smoothenes = graal * -1; //t
//            value = 1/(equityHighs / smoothenes);
//            value = (1 - value) * 100;


//            if (maxNetP <= 0)
//                value = 0;

            

//            Value = 2;


//        }
//    }
//}
