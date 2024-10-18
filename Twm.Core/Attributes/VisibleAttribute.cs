using System;
using Twm.Core.Enums;

namespace Twm.Core.Attributes
{
    public class VisibleAttribute : Attribute
    {
        private readonly PropertyVisibility[] _visibilities;
        private readonly bool _visible;

        public VisibleAttribute(bool visible, params PropertyVisibility[] visibilities)
        {
            _visible = visible;
            _visibilities = visibilities;
        }

        public PropertyVisibility[] Visibilities()
        {
            return this._visibilities;
        }

        public bool Visible()
        {
            return _visible;
        }
    }
}