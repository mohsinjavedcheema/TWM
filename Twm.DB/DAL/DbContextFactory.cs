using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Twm.DB.DAL
{
    public class DbContextFactory 
    {
        private TwmContext DataContext { set; get; }

        private string ConnectionStringToDb { get; set; }

        public void Init(string appDataPath = "")
        {
            appDataPath = !string.IsNullOrEmpty(appDataPath)
                ? appDataPath
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm";
            var pathToDb = Path.Combine(appDataPath, "DB");
            var dataSource = Path.Combine(pathToDb, "Twm.db3");

            Directory.CreateDirectory(pathToDb);

            ConnectionStringToDb = new SqliteConnectionStringBuilder
            {
                DataSource = dataSource,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
        }

        public TwmContext GetContext()
        {
            DataContext = new TwmContext(ConnectionStringToDb);

            return DataContext;
        }


        


        
    }
}