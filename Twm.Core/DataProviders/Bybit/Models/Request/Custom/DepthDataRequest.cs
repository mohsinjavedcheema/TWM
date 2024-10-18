﻿using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Interfaces;
using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models.Request.Custom
{
    [DataContract]
    public class DepthDataRequest : IRequest
    {
        [DataMember(Order = 1, Name = "symbol")]
        public string Symbol { get; set; }

        [DataMember(Order = 2, Name = "market_type")]
        public MarketType MarketType { get; set; }


        [DataMember(Order = 3, Name = "levels")]
        public int Levels { get; set; }






    }
}
