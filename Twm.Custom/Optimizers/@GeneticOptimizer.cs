using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Twm.Core.DataCalc.Optimization;
using System.Runtime.Serialization;

namespace Twm.Custom.Optimizers
{
   
    public class GeneticOptimizer:Optimizer
    {

        private const string OptimizerName = "Genetic Optimizer";

        private List<OptimizerParameter> _checkedParameters;
        private double[] _parameterDeltas;

        private int _counter = 0;
        private double[] _history = null;

        [DataMember]
        [Display(Name = "Generation size", Description = "Population size - the number of configurations to optimazе in parallel", Order = 0)]
        public int GenerationSize { get; set; }

        [DataMember]
        [Display(Name = "Update block size", Description = "Configurations generated before resorting in one algorithm cycle", Order = 1)]
        public int USize { get; set; }

        private int _originalIterationsNum;
        [DataMember]
        [Display(Name = "Iterations", Description = "Minimal number of iterations", Order = 2)]
        public int IterationsNum { get; set; }

        [DataMember]
        [Display(Name = "Iterations max", Description = "Maximal number of iterations", Order = 3)]
        public int IterationsNumMax { get; set; }

        [DataMember]
        [Display(Name = "Custom optimization", Description = "Perform ordering including CustomValue", Order = 4)]
        public bool CustomO { get; set; }

        [DataMember]
        [Display(Name = "EMA threshold to stop", Description = "Limiting EMA value indicating result stability", Order = 5)]
        public double StopTestVLimit { get; set; }

        [DataMember]
        [Display(Name = "Island radius %", Description = "Relative size of island in %", Order = 6)]
        public double IslandR { get; set; }

        [DataMember]
        [Display(Name = "Island min value", Description = "Minimal acceptable value inside island", Order = 7)]
        public double IslandMin { get; set; }

        [DataMember]
        [Display(Name = "Island penalty P", Description = "Degree of fluctuation penalty for islands:    1 - mean abs value, 2 - least squares", Order = 8)]
        public double IslandP { get; set; }

        [DataMember]
        [Display(Name = "InSample min trades count", Description = "Minimal count of trades to consider", Order = 9)]
        public int InSampleMinTrades { get; set; }


        private long _combinationCount;
        [Browsable(false)]
        public override long CombinationCount
        {
            set
            {
                if (_combinationCount != value)
                {
                    _combinationCount = value;
                    OnPropertyChanged();
                }
            }

            get
            {
                return _combinationCount;
            }
        }

        private const bool Verbose = false;

        void DebugPrint(object message)
        {
            if (Verbose)
                Print(message);
        }

        public GeneticOptimizer()
        {
            SetupDefaultParamValues();
        }

        private void SetupDefaultParamValues()
        {
            GenerationSize = 30;
            USize = 10;
            IterationsNum = 120;
            IterationsNumMax = 360;
            CustomO = false;
            StopTestVLimit = 0.003;
            //
            IslandR = 20;
            IslandMin = 1.0;
            IslandP = 2;
        }

        // Genetic algorithm -- single configuration

        public class Fish : ICloneable
        {
            public OptimizerParameter[] x = null;

            public object optimizationFitnessValue = null;
            public object trades = 0;

            public int islandN = 0;
            public double islandPenalty = 0;
            public Fish parent = null;
            public double dx = 0;
            public double kNew = 0;
            public double W = 0;

            public Fish() { }

            public Fish(OptimizerParameter[] y)
            {
                x = y.Select(a => (OptimizerParameter)a.Clone()).ToArray();
            }

            public void RecalcWeight(bool customO, double islandR, double islandMin, double islandP, int inSampleMinTrades)
            {
                if (parent != null)
                {
                    if (dx > 0)
                    {
                        double u = 1 + islandR / 100.0;
                        if ((double)parent.optimizationFitnessValue != 0)
                        {
                            double dy = (double)optimizationFitnessValue / (double)parent.optimizationFitnessValue;
                            if (dy <= u && dy >= 1.0/u)
                            {
                                parent.islandN += 1;
                                islandN += 1;
                                if ((double)parent.optimizationFitnessValue < islandMin ||
                                    (double)optimizationFitnessValue < islandMin)
                                {                                    
                                    parent.islandPenalty += 1;
                                    islandPenalty += 1;
                                }
                                else
                                {
                                    var t = Math.Pow(
                                        Math.Abs((double)optimizationFitnessValue - (double)parent.optimizationFitnessValue) /
                                        Math.Abs((double)optimizationFitnessValue - islandMin)
                                        , 2);
                                    if (t > 1) t = 1;
                                    parent.islandPenalty += t;
                                    islandPenalty += t;
                                }
                            }
                        }
                    }
                    parent = null;
                }
                //
                W = (double)optimizationFitnessValue;
                //
                if (customO)
                {
                    W *= RelativeIslandPenalty(islandP);
                }
            }

            public double CustomQualityValue(double islandP, double inSampleMinTrades)
            {
                var V = RelativeIslandPenalty(islandP);
                if ((int)trades < inSampleMinTrades)
                {
                    V *= 0;
                }
                return V;
            }

            public double SortWeight(int inSampleMinTrades)
            {
                if (optimizationFitnessValue == null)
                    return W;
                //
                if ((int)trades < inSampleMinTrades)
                {
                    return 0;
                }
                //
                return kNew * (double)optimizationFitnessValue + (1 - kNew) * W;
            }

            public object Clone()
            {
                var fish2 = new Fish(x);
                fish2.optimizationFitnessValue = optimizationFitnessValue;
                fish2.trades = trades;
                fish2.dx = dx;
                fish2.islandN = islandN;
                fish2.islandPenalty = islandPenalty;
                fish2.W = W;
                return fish2;
            }

            public void ForgetIsland()
            {
                islandN = 0;
                islandPenalty = 0;
            }

            public void ShiftIsland(double y, double islandR)
            {
                if ((double)optimizationFitnessValue == 0 || islandR == 0)
                    return;
                double u = 1 + islandR / 100.0;
                double t = Math.Abs(Math.Log(y)) / Math.Log(u);
                if (t > 1)
                    t = 1;
                islandN = (int)Math.Round((1-t)*islandN);
                islandPenalty = (1 - t) * islandPenalty;
                kNew = t;
            }

            double RelativeIslandPenalty(double islandP)
            {
                double y = (0.25 * islandN) / (1 + 0.25 * islandN);
                //if (y < 0.2)
                //    y = 0.2;
                //
                double z = 1;
                if (islandN > 0)
                {
                    double k = islandPenalty / islandN;
                    z = 1 - Math.Pow(k, 1 / islandP);
                }
                //
                double v = y * z;
                if (islandN == 1)
                    v = 0.3 * z + 0.7 * v;
                if (islandN == 2)
                    v = 0.5 * z + 0.5 * v;
                v = 0.5 + 0.5 * v;
                if (v < 0.5)
                    v = 0.5;
                return v;
            }
        }

        int AlgorithmPhase, AlgI;
        Fish[] Population;

        Random R1 = new Random(DateTime.Now.Millisecond);


        private void AlgorithmInit()
        {
          //  SetupDefaultParamValues();
            //
            if (USize < 5)
                USize = 5;
            //
            if (GenerationSize < 2 * USize)
                GenerationSize = 2 * USize;
            //
            if (IterationsNumMax < 2 * GenerationSize)
                IterationsNumMax = 2 * GenerationSize;
            //
            if (IterationsNum > IterationsNumMax)
                IterationsNum = IterationsNumMax;
            //
            if (IterationsNum < GenerationSize)
                IterationsNum = GenerationSize;
            //
            _originalIterationsNum = IterationsNum;
            //
            if (2 * GenerationSize <= IterationsNumMax && IterationsNum + GenerationSize > IterationsNumMax)
                IterationsNum = IterationsNumMax - GenerationSize;
            //
            if (IterationsNumMax < IterationsNum + GenerationSize)
                IterationsNumMax = IterationsNum + GenerationSize;
            //
            Population = new Fish[GenerationSize];
            //
            AlgorithmPhase = 0;
            AlgI = 0;
            _history = new double[IterationsNumMax];
            _counter = 0;
        }

        public override async Task OnOptimize()
        {
            ResetParamValues();
            var parameters = _checkedParameters.ToArray();
            //
            OptimizerParameter[] Q = null;
            Fish fish = null;
            //
            AlgorithmInit();
            //
            do
            {
                //DebugPrint("Iteration: " + _counter.ToString());
                if (AlgorithmPhase == 0)
                {
                    Q = GenerateRandomInDomain(parameters);
                    fish = new Fish(Q);
                    Population[AlgI] = fish;
                    AlgI++;
                    if (AlgI >= GenerationSize)
                    {
                        AlgorithmPhase = 1;
                        AlgI = 0;
                    }
                }
                else if (AlgorithmPhase == 1)
                {
                    var r0 = R1.NextDouble();
                    //
                    var tau = 1 - Math.Exp(-_counter / 100.0);
                    var pCrossover = 0.025 * (1 + 2 * tau);
                    var pNew = 0.45 - 0.20 * tau;
                    var pMutate = 0.10 - 0.05 * tau;
                    //
                    if (r0 < pCrossover)
                    {
                        fish = ActionCrossover();
                    }
                    else if (r0 < pCrossover + pNew)
                    {
                        Q = GenerateRandomInDomain(parameters);
                        fish = ActionNew(Q);
                    }
                    else if (r0 < pCrossover + pNew + pMutate)
                    {
                        fish = ActionMutate();
                    }
                    else // pMove == 1 - pCrossover - pNew - pMutate
                    {
                        if (CustomO)
                        {
                            if (R1.NextDouble() < 0.5)
                                fish = ActionMoveHead();
                            else
                                fish = ActionMove();
                        }
                        else
                            fish = ActionMove();
                    }
                    Population[Population.Length - USize + AlgI] = fish;
                    //
                    if (Verbose)
                    {
                        var s1 = "";
                        for (int j = 0;  j < 10 && j < Population.Length; j++)
                            s1 += Population[j].optimizationFitnessValue.ToString() + "/" + Population[j].W.ToString() + " ";
                        DebugPrint(s1);
                    }
                    //
                    AlgI++;
                    if (AlgI >= USize)
                    {
                        ActionAnalyze();
                        AlgI = 0;
                    }
                }
                else if (AlgorithmPhase == 2)
                {
                    fish = Population[AlgI];
                    DebugPrint(AlgI.ToString());
                    AlgI++;
                    if (AlgI >= GenerationSize)
                    {
                        break;
                    }
                }
                //
                // Calling
                //
                Q = fish.x;
                CustomValue = fish.CustomQualityValue(IslandP, InSampleMinTrades);
                foreach (var p in Q)
                {
                    var cp = _checkedParameters.Find(x => x.Name == p.Name);
                    cp.CurrentValue = p.CurrentValue;
                }
                EnqueueParams();

                CombinationCount = AlgorithmCombinationEstimate();
                RunIteration();
                //
                await Task.WhenAll(LastTask);
                //
                fish.optimizationFitnessValue  = LastStrategy.OptimizationFitness.Value;

                fish.trades = LastStrategy.Trades;
                fish.trades = LastStrategy.SystemPerformance.Trades.Count;
                fish.RecalcWeight(CustomO, IslandR, IslandMin, IslandP, InSampleMinTrades);
                if (_counter >= IterationsNumMax)
                {
                }
                if (_counter < IterationsNumMax)
                    _history[_counter] = MaxPopW();
                //
                _counter++;
                if (AlgorithmPhase == 1)
                {
                    if (_counter >= IterationsNum)
                    {
                        bool readyToStop = (_counter >= IterationsNumMax - GenerationSize);
                        if (!readyToStop)
                        {
                            if (AlgorithmStopTest())
                                readyToStop = true;
                        }
                        if (readyToStop)
                        {
                            AlgI = 0;
                            AlgorithmPhase = 2;
                        }
                    }
                }
            } while (true);
        }

        // Algorithm actions

        private void ActionAnalyze()
        {
            SortPopulation();
            //
            for (int k = 0; k < Population.Length; k++)
            {
                Population[k].kNew *= 0.84;
            }
        }

        private void SortPopulation()
        {
            int n = Population.Length;
            for (int r = n-1; r >= 1; r--)
            {
                int k = 0;
                double w = Population[0].SortWeight(InSampleMinTrades);
                for (int j = 1; j <= r; j++)
                {
                    var w2 = Population[j].SortWeight(InSampleMinTrades);
                    if (w2 < w)
                    {
                        k = j;
                        w = w2;
                    }
                }
                if (k != r)
                {
                    var t = Population[k];
                    Population[k] = Population[r];
                    Population[r] = t;
                }
            }
        }

        private double MaxPopW()
        {
            double w = Population[0].W;
            for (int j = 1; j < Population.Length; j++)
            {
                if (Population[j] == null)
                    continue;
                if (Population[j].W > w)
                    w = Population[j].W;
            }
            return w;
        }

        private double AlgorithmStopTestV(int j)
        {
            double k = 1, q = 0.98, S = 0, F = 0, U = 0;
            while (j >= 1)
            {
                S += _history[j] * k;
                F += Math.Abs(_history[j] - _history[j - 1]) * k;
                U += k;
                k *= q;
                j -= 1;
            }
            double r = F / (S / U);
            return r;
        }

        private bool AlgorithmStopTest()
        {
            int j = _counter - 1;
            double r = AlgorithmStopTestV(j);
            if (r < (CustomO? 2 * StopTestVLimit : StopTestVLimit))
                return true;
            return false;
        }

        private int AlgorithmCombinationEstimate()
        {
            if (_counter >= IterationsNumMax - GenerationSize)
                return IterationsNumMax;
            //
            if (_counter <= IterationsNum || _counter <= 30)
            {
                int e0 = (int)Math.Sqrt(IterationsNum * IterationsNumMax);
                if (e0 <= _originalIterationsNum)
                    e0 = _originalIterationsNum;
                if (_counter + 5 >= e0)
                    return _counter + 5 <= IterationsNumMax? _counter + 5 : IterationsNumMax;
                else
                    return e0 <= IterationsNumMax ? e0 : IterationsNumMax;
            }
            const int M = 10;
            var R = new double[M];
            for (int i = 0; i < M; i++)
            {
                R[i] = AlgorithmStopTestV(_counter - 1 - 2 * i);
            }
            double S0 = 10, S1 = 0, S2 = 0, Y = 0, Z = 0; //
            for (int i = 0; i < M; i++)
            {
                S1 += i;
                S2 += i * i;
                Y += R[i];
                Z += R[i] * i;
            }
            double a = (S0 * Z - S1 * Y) / (S0 * S2 / S1 * S1);
            //
            if (a <= 0)
            {
                int e0 = (int)Math.Sqrt(IterationsNum * IterationsNumMax);
                if (e0 <= _originalIterationsNum)
                    e0 = _originalIterationsNum;
                if (_counter + 5 >= e0)
                    return _counter + 5 <= IterationsNumMax ? _counter + 5 : IterationsNumMax;
                else
                    return e0 <= IterationsNumMax ? e0 : IterationsNumMax;
            }
            //
            int N = (int)Math.Round((R[0] - StopTestVLimit) / a);
            int e1 = _counter + N + 1;
            if (e1 <= _originalIterationsNum)
                e1 = _originalIterationsNum;
            if (_counter + 5 >= e1)
                return _counter + 5 <= IterationsNumMax ? _counter + 5 : IterationsNumMax;
            else
            {
                if (e1 < IterationsNumMax)
                    return e1;
                else
                    return IterationsNumMax;
            }
        }

        private int RandomFishIndex(double l = -1)
        {
            if (l < -1)
                l = 0.25 * Population.Length;
            int n = Population.Length - USize - 1;
            int i = (int)(Math.Log(R1.NextDouble()) * l);
            if (i > n)
                i = n;
            return i;
        }

        private int UniformRandomFishIndex()
        {
            int n = Population.Length - USize - 1;
            return R1.Next(0, n);
        }

        private Fish ActionNew(OptimizerParameter[] Q)
        {
            var fish = new Fish(Q);
            fish.kNew = 1;
            return fish;
        }

        private int MaxPreformanceFishIndex()
        {
            int i = 0;
            double mxp = (double)Population[0].optimizationFitnessValue;
            for (int j = 1; j < Population.Length; j++)
            {
                Fish fish = Population[j];
                double v = (double)fish.optimizationFitnessValue;
                if (v > mxp)
                {
                    mxp = v;
                    i = j;
                }
            }
            return i;
        }

        private Fish ActionMoveHead()
        {
            int k = MaxPreformanceFishIndex();
            double p = (double)Population[k].optimizationFitnessValue;
            double w = Population[k].W;
            return ActionMove(k, 0.5);
        }

        private Fish ActionMove(int k = -1, double ff = 1)
        {
            if (k < 0)
                k = RandomFishIndex();
            var f0 = Population[k];
            var fish = (Fish)f0.Clone();
            int i = R1.Next(0, fish.x.Length);
            //
            double dx = 0;
            //
            var parameter = fish.x[i];
            var type = parameter.Type;
            if (type == typeof(int))
            {
                if (parameter is IntegerOptimizerParameter)
                {
                    var integerParameter = parameter as IntegerOptimizerParameter;
                    dx = ff * _parameterDeltas[i] * (-1 + 2 * R1.NextDouble());
                    int dxi = (int)Math.Round(dx);
                    if (dxi == 0)
                        dxi = -1 + 2 * R1.Next(0, 1);
                    while ((int)integerParameter.CurrentValue + dxi < 0)
                        dxi++;
                    dx = dxi;
                    fish.ShiftIsland(1 + dx / (int)integerParameter.CurrentValue, IslandR);
                    ChangeIntegerParamValueBy(integerParameter, dxi);
                }
            }
            else if (type == typeof(double))
            {
                if (parameter is DoubleOptimizerParameter)
                {
                    var doubleParameter = parameter as DoubleOptimizerParameter;
                    dx = ff * _parameterDeltas[i] * (-1 + 2 * R1.NextDouble());
                    if ((double)doubleParameter.CurrentValue + dx < 0)
                        dx = (double)doubleParameter.CurrentValue / 2;
                    fish.ShiftIsland(1 + dx / (double)doubleParameter.CurrentValue, IslandR);
                    ChangeDoubleParamValueBy(doubleParameter, dx);
                }
            }
            else if (type.IsEnum)
            {
                if (parameter is EnumOptimizerParameter)
                {
                    fish.ForgetIsland();
                    ChangeEnumParamValueToAny(parameter);
                }
            }
            else if (type == typeof(bool))
            {
                fish.ForgetIsland();
                AlterBoolParamValue(parameter);
            }
            //
            fish.dx = dx;
            fish.parent = f0;
            //
            return fish;
        }

        private Fish ActionCrossover()
        {
            int k1 = RandomFishIndex();
            int k2 = RandomFishIndex();
            var f1 = Population[k1];
            var f2 = Population[k2];
            var fish = new Fish(CombineParameters(f1.x, f2.x));
            //
            fish.kNew = 1;
            //
            return fish;
        }

        private Fish ActionMutate()
        {
            int k = RandomFishIndex();
            var f0 = Population[k];
            var fish = (Fish)f0.Clone();
            int i = R1.Next(0, fish.x.Length);
            //
            var parameter = fish.x[i];
            ChangeParameter2Random(parameter);
            //
            fish.kNew = 1;
            //
            return fish;
        }




        // Utilitary functions

        private void ResetParamValues()
        {
            _checkedParameters = OptimizerParameters.Where(x => x.IsChecked).ToList();
            _parameterDeltas = new double[_checkedParameters.Count];
            int j = 0;
            foreach (var parameter in _checkedParameters)
            {
                parameter.ResetValue();
                var type = parameter.Type;
                //
                if (type == typeof(int))
                {
                    if (parameter is IntegerOptimizerParameter)
                    {
                        var integerParameter = parameter as IntegerOptimizerParameter;
                        var dif = integerParameter.Max - integerParameter.Min;
                        if (dif < 0)
                            dif = -dif;
                        var delta = 0.36 * 2 * (Math.Sqrt(1 + .25 * dif) - 1) / .25;
                        _parameterDeltas[j] = delta;
                    }
                }
                else if (type == typeof(double))
                {
                    if (parameter is DoubleOptimizerParameter)
                    {
                        var doubleParameter = parameter as DoubleOptimizerParameter;
                        var dif = doubleParameter.Max - doubleParameter.Min;
                        if (dif < 0)
                            dif = -dif;
                        var delta = 0.36 * 2 * (Math.Sqrt(1 + .25 * dif) - 1) / .25;
                        _parameterDeltas[j] = delta;
                    }
                }
                j++;
            }
        }

        private OptimizerParameter[] CloneParameters(OptimizerParameter[] y)
        {
            return y.Select(a => (OptimizerParameter)a.Clone()).ToArray();
        }

        private OptimizerParameter[] GenerateRandomInDomain(OptimizerParameter[] y)
        {
            int m = y.Length;
            //
            var x = CloneParameters(y);
            for (int i = 0; i < m; i++)
            {
                var parameter = x[i];
                ChangeParameter2Random(parameter);
            }
            //
            return x;
        }

        private void ChangeParameter2Random(OptimizerParameter parameter)
        {
            var type = parameter.Type;
            //
            if (type == typeof(int))
            {
                if (parameter is IntegerOptimizerParameter)
                {
                    var integerParameter = parameter as IntegerOptimizerParameter;
                    var value = R1.Next(integerParameter.Min, Math.Max(integerParameter.Min, integerParameter.Max));
                    integerParameter.CurrentValue = value;
                }
            }
            else if (type == typeof(double))
            {
                if (parameter is DoubleOptimizerParameter)
                {
                    var doubleParameter = parameter as DoubleOptimizerParameter;
                    var value = R1.NextDouble();
                    var dif = doubleParameter.Max - doubleParameter.Min;
                    doubleParameter.CurrentValue = doubleParameter.Min + dif * value;
                }
            }
            else if (type.IsEnum)
            {
                ChangeEnumParamValueToAny(parameter);
                //if (parameter is EnumOptimizerParameter)
                //{
                //    var names = Enum.GetNames(type);
                //    var P = (EnumOptimizerParameter)parameter;
                //    var value = R1.Next(0, names.Length - 1);
                //    var vs = Enum.GetValues(type);
                //    var v = vs.GetValue(value);
                //    parameter.CurrentValue = v;
                //}
            }
            else if (type == typeof(bool))
            {
                var P = (BoolOptimizerParameter)parameter;
                if (P.IsOptimize)
                {
                    var value = R1.Next(0, 1);
                    parameter.CurrentValue = (value != 0);
                }
            }
        }

        private OptimizerParameter[] CombineParameters(OptimizerParameter[] y, OptimizerParameter[] z)
        {
            int m = y.Length;
            //
            var x = CloneParameters(y);
            for (int i = 0; i < m; i++)
            {
                var parameter = x[i];
                var type = parameter.Type;
                //
                if (type == typeof(int))
                {
                    if (parameter is IntegerOptimizerParameter)
                    {
                        var integerParameter = parameter as IntegerOptimizerParameter;
                        var t = 0.3 + 0.4 * R1.NextDouble();
                        var value = (int)(t * ((int)y[i].CurrentValue) + (1 - t) * ((int)z[i].CurrentValue));
                        integerParameter.CurrentValue = value;
                    }
                }
                else if (type == typeof(double))
                {
                    if (parameter is DoubleOptimizerParameter)
                    {
                        var doubleParameter = parameter as DoubleOptimizerParameter;
                        var t = 0.3 + 0.4 * R1.NextDouble();
                        var value = t * ((double)y[i].CurrentValue) + (1 - t) * ((double)z[i].CurrentValue);
                        doubleParameter.CurrentValue = value;
                    }
                }
                else if (type.IsEnum)
                {
                    if (parameter is EnumOptimizerParameter)
                    {
                        var u = R1.Next(0, 1);
                        parameter.CurrentValue = (u == 0)? y[i].CurrentValue : z[i].CurrentValue;
                    }
                }
                else if (type == typeof(bool))
                {
                    var u = R1.Next(0, 1);
                    parameter.CurrentValue = (u == 0) ? y[i].CurrentValue : z[i].CurrentValue;
                }
            }
            //
            return x;
        }

        private void ChangeIntegerParamValueBy(IntegerOptimizerParameter integerParameter, int dx)
        {
            var value = (int)integerParameter.CurrentValue + dx;
            if (value < integerParameter.Min)
                value = integerParameter.Min;
            if (value > integerParameter.Max)
                value = integerParameter.Max;
            integerParameter.CurrentValue = value;
        }

        private void ChangeDoubleParamValueBy(DoubleOptimizerParameter doubleParameter, double dx)
        {
            var value = (double)doubleParameter.CurrentValue + dx;
            if (value < doubleParameter.Min)
                value = doubleParameter.Min;
            if (value > doubleParameter.Max)
                value = doubleParameter.Max;
            doubleParameter.CurrentValue = value;
        }

        private int NameIndexByVal(string[] names, string name)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                    return i;
            }
            return -1;
        }
        private void ChangeEnumParamValueToAny(OptimizerParameter parameter)
        {
            var type = parameter.Type;
            var names = Enum.GetNames(type);
            //var value = R1.Next(0, names.Length - 1);
            //var name = Enum.GetValues(type).GetValue(value);
            //parameter.CurrentValue = name;
            //return;
            var P = (EnumOptimizerParameter)parameter;
            while (true)
            {
                int length = names.Length;
                int upperBound = names.Length - 1;

                //if only 1 enum is selected just select it
                if (P.Values.Count(x => x) == 1)
                {
                    var z = P.Values.FirstOrDefault(x => x);
                    parameter.CurrentValue = z;
                }

                ////if 2 enums are selected 
                //if (length == 2)
                //{
                //    if (P.Values[0] && !P.Values[1])
                //    {
                //        parameter.CurrentValue = Enum.GetValues(type).GetValue(0);
                //    }
                //    if (!P.Values[0] && P.Values[1])
                //    {
                //        parameter.CurrentValue = Enum.GetValues(type).GetValue(1);
                //    }

                //    break;
                //}

                //According to the documentation, Next returns an integer random number between the (inclusive) minimum and the (exclusive) maximum:

                var value1 = R1.Next(0, names.Length);
                var name1 = Enum.GetValues(type).GetValue(value1);
                if (!P.Values[value1])
                    continue;
                parameter.CurrentValue = name1;
                break;
            }
        }

        private void AlterBoolParamValue(OptimizerParameter parameter)
        {
            var type = parameter.Type;
            var P = (BoolOptimizerParameter)parameter;
            if (!P.IsOptimize)
                return;
            parameter.CurrentValue = !((bool)parameter.CurrentValue);
        }

    }
}