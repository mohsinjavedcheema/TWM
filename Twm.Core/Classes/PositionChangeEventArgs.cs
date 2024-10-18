using System;
using System.Collections.Generic;
using Twm.Core.Enums;
using Twm.Core.Market;

namespace Twm.Core.Classes
{
    public class PositionChangeEventArgs : EventArgs
    {
        public List<Position> Positions { get; set; }

        public PositionChangeAction PositionChangeAction { get; set; }

        public PositionChangeEventArgs(List<Position> positions, PositionChangeAction positionChangeAction)
        {
            Positions = positions;
            PositionChangeAction = positionChangeAction;
        }
    }
}