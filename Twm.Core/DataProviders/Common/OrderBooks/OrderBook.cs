using System;
using System.Collections.Generic;
using System.Linq;
using Twm.Core.DataProviders.Enums;


namespace Twm.Core.DataProviders.Common.OrderBooks
{
    public class OrderBook
    {
        private Dictionary<double, double> _priceLevelsBuy;
        private Dictionary<double, double> _priceLevelsSell;


        public int Levels { get; set; }
        public bool IsInit { get; set; }
        public IEnumerable<PriceLevel> Asks
        {
            get {

                return _priceLevelsSell.Select(x => new PriceLevel(x.Key, x.Value, DateTime.UtcNow ));
            }
        }

        public IEnumerable<PriceLevel> Bids
        {
            get {

                return _priceLevelsBuy.Select(x => new PriceLevel(x.Key, x.Value, DateTime.UtcNow));
            }
        }

        public OrderBook()
        {
            _priceLevelsBuy = new Dictionary<double, double>();
            _priceLevelsSell = new Dictionary<double, double>();
        }



        public void Insert(MarketDataType marketDataType, double price, double quantity)
        {
            if (marketDataType == MarketDataType.Bid)
            {
                if (_priceLevelsBuy.ContainsKey(price))
                {
                    _priceLevelsBuy.Remove(price);
                }

                _priceLevelsBuy.Add(price, quantity);
            }
            else
            {
                if (_priceLevelsSell.ContainsKey(price))
                {
                    _priceLevelsSell.Remove(price);
                }

                _priceLevelsSell.Add(price, quantity);
            }
        }

        public void Update(MarketDataType marketDataType, double price, double quantity)
        {
            if (marketDataType == MarketDataType.Bid)
            {
                if (_priceLevelsBuy.ContainsKey(price))
                {
                    if (quantity == 0)
                    {
                        _priceLevelsBuy.Remove(price);                       
                    }
                    else
                    {
                        _priceLevelsBuy[price] = quantity;                        
                    }
                }
                else
                {
                    if (quantity > 0)
                        _priceLevelsBuy.Add(price, quantity);                        
                }

            }
            else
            {
                if (_priceLevelsSell.ContainsKey(price))
                {
                    if (quantity == 0)
                    {
                        _priceLevelsSell.Remove(price);                        
                    }
                    else
                    {
                        _priceLevelsSell[price] = quantity;                    
                    }
                }
                else
                {
                    if (quantity > 0)
                        _priceLevelsSell.Add(price, quantity);
                    
                }
            }
        }


        public void TruncateAsks()
        {
            var asks = _priceLevelsBuy.OrderByDescending(x => x.Key).Skip(Levels).Select(x=>x.Key).ToList();

            foreach (var ask in asks)
            {
                _priceLevelsBuy.Remove(ask);
            }
        }

        public void TruncateBids()
        {
            var bids = _priceLevelsSell.OrderBy(x => x.Key).Skip(Levels).Select(x => x.Key).ToList();

            foreach (var bid in bids)
            {
                _priceLevelsSell.Remove(bid);
            }
        }



        /*   public List<PriceLevel> GetPriceLevels()
           {
               var result = new List<PriceLevel>();

               result.AddRange(_priceLevelsSell.Select(x => new PriceLevel( x.Key, x.Value, Math.Round(x.Key * x.Value, 2))).OrderByDescending(x => x.Price).Take(50).ToList());
               result.AddRange(_priceLevelsBuy.Select(x => new PriceLevel( x.Key, x.Value, Math.Round(x.Key * x.Value, 2))).OrderByDescending(x => x.Price).Take(50).ToList());

               return result;
           }

           public double GetBestAsk()
           {

               var price = _priceLevelsSell.Select(x => new PriceLevel(MarketDataType.Ask, x.Key, x.Value, Math.Round(x.Key * x.Value, 2))).OrderByDescending(x => x.Price).Take(50).LastOrDefault();

               if (price != null)
               {
                   return price.Price;
               }

               return 0;


           }


           public double GetBestBid()
           {

               var price = _priceLevelsBuy.Select(x => new PriceLevel(MarketDataType.Bid, x.Key, x.Value, Math.Round(x.Key * x.Value, 2))).OrderByDescending(x => x.Price).Take(50).FirstOrDefault();

               if (price != null)
               {
                   return price.Price;
               }

               return 0;


           }*/

    }
}
