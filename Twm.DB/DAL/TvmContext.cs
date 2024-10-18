using Twm.DB.Seed;
using Twm.Model.Model;
using Microsoft.EntityFrameworkCore;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace Twm.DB.DAL
{
    public class TwmContext : DbContext
    {
        public string ConnectionString { get; }

        public TwmContext(string connectionString)
        {
            ConnectionString = connectionString;
        }


        public DbSet<SystemOption> SystemOptions { get; set; }

        public DbSet<Connection> Connections { get; set; }

        public DbSet<ConnectionOption> ConnectionOptions { get; set; }

        public DbSet<Instrument> Instruments { get; set; }

        public DbSet<InstrumentList> InstrumentLists { get; set; }

        public DbSet<InstrumentMap> InstrumentMaps { get; set; }

        public DbSet<InstrumentInstrumentList> InstrumentInstrumentLists { get; set; }

        public DbSet<HistoricalMetaData> HistoricalMetaDatas { get; set; }

        public DbSet<OptimizerResult> OptimizerResults { get; set; }

        public DbSet<Preset> Presets { get; set; }

        public DbSet<ViewOption> ViewOptions { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<DataProvider> DataProviders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlite(ConnectionString);
    ;

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
            modelBuilder.Seed();


        }

        public void Migrate()
        {
            this.Database.Migrate();
        }
    }

   
}