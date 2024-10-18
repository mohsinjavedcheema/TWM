using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Design;

namespace Twm.DB.DAL
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TwmContext>
    {
        public TwmContext CreateDbContext(string[] args)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var path = Path.Combine(appDataPath, "Twm", "Data", "Twm.db3");

            var connectionStringToDb = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            var dbContext = new TwmContext(connectionStringToDb);

            return dbContext;
        }
    }
}