using System;
using System.Collections.Generic;
using Twm.Core.Enums;
using Twm.Core.Market;

namespace Twm.Core.Classes
{
    public class AssetChangeEventArgs : EventArgs
    {
        public List<Asset> Assets { get; set; }

        public AssetChangeAction AssetChangeAction { get; set; }

        public AssetChangeEventArgs(List<Asset> assets, AssetChangeAction assetChangeAction)
        {
            Assets = assets;
            AssetChangeAction = assetChangeAction;
        }
    }
}