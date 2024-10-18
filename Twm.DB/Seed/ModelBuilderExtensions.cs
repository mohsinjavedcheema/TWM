using Twm.Model.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Twm.DB.Seed
{
    public static class ModelBuilderExtensions
    {
        public static async void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemOption>().HasData(
                new SystemOption() { Id = 1, Name = "Custom project path", Code = "CstPrjPath", Category = "General", Group = "Project" },
                new SystemOption() { Id = 2, Name = "Custom project dll path", Code = "CstPrjDllPath", Category = "General", Group = "Project" },

                new SystemOption() { Id = 3, Name = "Compile in debug", Code = "CompileDebug", Category = "General", Group = "Project", ValueType = "bool" },

                new SystemOption() { Id = 4, Name = "Trade buy color", Code = "TradeBuyColor", Category = "General", Group = "Display", Value = "Blue" },
                new SystemOption() { Id = 5, Name = "Trade sell color", Code = "TradeSellColor", Category = "General", Group = "Display", Value = "Magenta" },

                new SystemOption() { Id = 6, Name = "Color for down bars", Code = "DownBarColor", Category = "General", Group = "Display", Value = "Red" },
                new SystemOption() { Id = 7, Name = "Color for up bars", Code = "UpBarColor", Category = "General", Group = "Display", Value = "LimeGreen" },
                new SystemOption() { Id = 8, Name = "Candle body outline color", Code = "CandleOutlineColor", Category = "General", Group = "Display", Value = "Black" },
                new SystemOption() { Id = 9, Name = "Candle wick color", Code = "CandleWickColor", Category = "General", Group = "Display", Value = "Black" },
                new SystemOption() { Id = 10, Name = "Chart background color", Code = "ChartBackgroundColor", Category = "General", Group = "Display", Value = "White" },
                new SystemOption() { Id = 11, Name = "Chart vertical grid color", Code = "ChartVGridColor", Category = "General", Group = "Display", Value = "LightGray" },
                new SystemOption() { Id = 12, Name = "Chart horizontal grid color", Code = "ChartHGridColor", Category = "General", Group = "Display", Value = "LightGray" },
                new SystemOption() { Id = 13, Name = "Chart axis text color", Code = "TextColor", Category = "General", Group = "Display", Value = "Black" },
                new SystemOption() { Id = 14, Name = "Indicator separator color", Code = "IndicatorSeparatorColor", Category = "General", Group = "Display", Value = "LightGray" },
                new SystemOption() { Id = 15, Name = "Indicator separator width", Code = "IndicatorSeparatorWidth", Category = "General", Group = "Display", Value = "1" },
                new SystemOption() { Id = 16, Name = "Plot executions", Code = "PlotExecutions", Category = "General", Group = "Display", Value = "1" },
                new SystemOption() { Id = 17, Name = "Marker text color", Code = "MarkerTextColor", Category = "General", Group = "Display", Value = "Black" },
                new SystemOption() { Id = 18, Name = "Time zone", Code = "TimeZone", Category = "General", Group = "Preferences", Value = "" },
                new SystemOption() { Id = 20, Name = "Calculate simulation", Code = "CalculateSimulation", Category = "Calculation", Group = "Optimizer", ValueType = "bool" },
                new SystemOption() { Id = 21, Name = "Email host", Code = "EmailHost", Category = "General", Group = "Email" },
                new SystemOption() { Id = 22, Name = "Email port", Code = "EmailPort", Category = "General", Group = "Email", ValueType = "int" },
                new SystemOption() { Id = 23, Name = "Email username", Code = "EmailUsername", Category = "General", Group = "Email" },
                new SystemOption() { Id = 24, Name = "Email password", Code = "EmailPassword", Category = "General", Group = "Email" },
                new SystemOption() { Id = 25, Name = "Log in file", Code = "LogInFile", Category = "General", Group = "Preferences", ValueType = "bool", ValueBool = true },

            

                new SystemOption() { Id = 29, Name = "Reload script on recompile", Code = "ReloadOnRecompile", Category = "General", Group = "Project", ValueType = "bool", ValueBool = true }

            );

            modelBuilder.Entity<DataProvider>().HasData(
               new SystemOption() { Id = 3, Name = "Bybit", Code = "Bybit" },
               new SystemOption() { Id = 4, Name = "Binance", Code = "Binance" }


           );

        }
    }
}