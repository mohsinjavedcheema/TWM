using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AlgoDesk.Core.Classes;
using AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.Classes;
using AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.MapHeaders;
using AlgoDesk.Core.DataProviders.DxFeed.Models;
using AlgoDesk.Model.Model;
using CsvHelper;
using CsvHelper.Configuration;

namespace AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed
{
    public class InstrumentsDataReader
    {
        public static DxFeedInstrument[] Read(string filePath)
        {

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                Delimiter = ",",
                MissingFieldFound = null
            };

           
            

            using (Stream stream = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    using (CsvReader csvReader = new CsvReader(reader, config))
                    {
                       
                        var instruments = new List<DxFeedInstrument>();

                        

                        while (csvReader.Read())
                        {
                            var field = csvReader.GetField(0);

                            if (field.Contains("#"))
                            {
                                //MetaData
                                var metaData = csvReader.Parser.RawRecord.Remove(0, csvReader.Parser.RawRecord.IndexOf('=')+1 );
                                var fields = metaData.Split(',');
                                var dictionary = new Dictionary<string, int>();
                                for (int i = 0; i < fields.Length; i++)
                                {
                                    dictionary.Add(fields[i], i);
                                }

                                ClassMap mapHeader = null;
                                
                                if (field.StartsWith("#FUTURE"))
                                {
                                    mapHeader = new InstrumentMapHeader<Future>(dictionary);
                                }
                                else if (field.StartsWith("#INDEX"))
                                {
                                    mapHeader = new InstrumentMapHeader<Index>(dictionary);
                                }
                                else if (field.StartsWith("#PRODUCT"))
                                {
                                    mapHeader = new InstrumentMapHeader<ProductIns>(dictionary);
                                }

                                if (mapHeader != null)
                                    csvReader.Context.RegisterClassMap(mapHeader);

                            }
                            else
                            {
                                DxFeedInstrument instrument = null;
                                //Data
                                switch (field)
                                {
                                    case "FUTURE":
                                        instrument = Session.Instance.Mapper.Map<DxFeedInstrument>(csvReader.GetRecord<Future>());
                                        break;
                                    case "INDEX":
                                        instrument = Session.Instance.Mapper.Map<DxFeedInstrument>(csvReader.GetRecord<Index>());
                                        break;
                                    case "PRODUCT":
                                        instrument = Session.Instance.Mapper.Map<DxFeedInstrument>(csvReader.GetRecord<ProductIns>());
                                        break;
                                }

                                if (instrument != null)
                                {
                                    instruments.Add(instrument);
                                }
                            }
                        }

                        return instruments.ToArray();


                    }
                }
            }
        }


    }
}
