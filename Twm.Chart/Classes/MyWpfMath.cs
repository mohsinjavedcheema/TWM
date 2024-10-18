﻿using System;
using System.Collections.Generic;

namespace Twm.Chart.Classes
{
    class MyWpfMath
    {
        //----------------------------------------------------------------------------------------------------------------------------------
        // Рассчитывает значения для меток на оси так, чтобы они имели достаточно круглые значения и располагались так, чтобы не мешать друг другу.
        // minValue, maxValue - минимальное и максимальное значение на оси (НЕ обязательно метки). 
        // chartAxisLength - длина отображаемой оси в единицах длины WPF. 
        // chartLabelLength - максимальная длина вдоль оси, отведенная для одной метки (метки не должны залазить друг на друга).
        // Предполагается, что продольный центр метки совпадает со значением на оси.
        public static List<double> CalcTicks(double minValue, double maxValue, double chartAxisLength, double chartLabelLength)
        {
            double lineHeight = (maxValue - minValue) / chartAxisLength * chartLabelLength;
            double lineHeightMaxDigit = MaxDigit(lineHeight);
            double step = Math.Ceiling(lineHeight / lineHeightMaxDigit) * lineHeightMaxDigit;
            double theMostRoundTick = TheMostRoundValueInsideRange(minValue, maxValue);
            List<double> ticks = new List<double>();
            ticks.Add(theMostRoundTick);

            int step_i = 1;
            double next_tick = theMostRoundTick + step_i * step;
            while (next_tick <= maxValue)
            {
                ticks.Add(next_tick);
                step_i++;
                next_tick = theMostRoundTick + step_i * step;
            }

            step_i = 1;
            next_tick = theMostRoundTick - step_i * step;
            while (next_tick >= minValue)
            {
                ticks.Insert(0, next_tick);
                step_i++;
                next_tick = theMostRoundTick - step_i * step;
            }

            return ticks;
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public static int MaxDecimalPow(double x)
        {
            if (x >= 1.0)
            {
                if (x >= 1000000.0)
                    return 6; // 1000000.0;
                else if (x >= 100000.0)
                    return 5; // 100000.0;
                else if (x >= 10000.0)
                    return 4; // 10000.0;
                else if (x >= 1000.0)
                    return 3; // 1000.0;
                else if (x >= 100.0)
                    return 2; // 100.0;
                else if (x >= 10.0)
                    return 1; // 10.0;
                else
                    return 0; // 1.0;
            }
            else
            {
                if (x >= 0.1)
                    return -1; // 0.1;
                else if (x >= 0.01)
                    return -2; // 0.01;
                else if (x >= 0.001)
                    return -3; // 0.001;
                else if (x >= 0.0001)
                    return -4; // 0.0001;
                else if (x >= 0.00001)
                    return -5; // 0.00001;
                else if (x >= 0.000001)
                    return -6; // 0.000001;
                else if (x >= 0.0000001)
                    return -7; // 0.0000001;
                else
                    return int.MinValue;
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public static double MaxDigit(double x)
        {
            if (x >= 1000000.0)
                return 1000000.0;
            else if (x >= 100000.0)
                return 100000.0;
            else if (x >= 10000.0)
                return 10000.0;
            else if (x >= 1000.0)
                return 1000.0;
            else if (x >= 100.0)
                return 100.0;
            else if (x >= 10.0)
                return 10.0;
            else if (x >= 1.0)
                return 1.0;
            else if (x >= 0.1)
                return 0.1;
            else if (x >= 0.01)
                return 0.01;
            else if (x >= 0.001)
                return 0.001;
            else if (x >= 0.0001)
                return 0.0001;
            else if (x >= 0.00001)
                return 0.00001;
            else if (x >= 0.000001)
                return 0.000001;
            else if (x >= 0.0000001)
                return 0.0000001;
            else
                return double.NegativeInfinity;
        }

        public static long MaxDigit(long x)
        {
            if (x >= 1000000000000)
                return 1000000000000;
            if (x >= 100000000000)
                return 100000000000;
            if (x >= 10000000000)
                return 10000000000;
            if (x >= 1000000000)
                return 1000000000;
            if (x >= 100000000)
                return 100000000;
            if (x >= 10000000)
                return 10000000;
            if (x >= 1000000)
                return 1000000;
            else if (x >= 100000)
                return 100000;
            else if (x >= 10000)
                return 10000;
            else if (x >= 1000)
                return 1000;
            else if (x >= 100)
                return 100;
            else if (x >= 10)
                return 10;
            else if (x >= 1)
                return 1;
            else return 0;
        }
        //----------------------------------------------------------------------------------------------------------------------------------
        public static double TheMostRoundValueInsideRange(double x0, double x1)
        {
            int max10Pow = MaxDecimalPow(Math.Abs(x0));
            double max10PowValue = Math.Pow(10.0, max10Pow);
            double mostR = x0 == 0 ? 0: Math.Ceiling(x0 / max10PowValue) * max10PowValue;
            while (mostR > x1)
            {
                max10Pow--;
                max10PowValue = Math.Pow(10.0, max10Pow);
                mostR = Math.Ceiling(x0 / max10PowValue) * max10PowValue;
            }
            return mostR;
        }
        //----------------------------------------------------------------------------------------------------------------------------------


        public static double RoundToTickSize(double value, double tickSize)
        {

            var diff = value % tickSize;

            if (diff == 0)
                return value;

            var val1 = value - diff;
            var val2 = val1 + tickSize;


            if (diff < val2 - value)
            {
                return val1;
            }


            return val2;
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------
    }
}
