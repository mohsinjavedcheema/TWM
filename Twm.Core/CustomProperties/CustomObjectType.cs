using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace Twm.Core.CustomProperties
{
    public class BusinessObject : DynamicObject, ICustomTypeDescriptor, INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> _dynamicProperties =
            new Dictionary<string, object>();

        private readonly IDictionary<string, Type> _dynamicPropertyTypes =
            new Dictionary<string, Type>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var memberName = binder.Name;
            
            return _dynamicProperties.TryGetValue(memberName, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var memberName = binder.Name;
            _dynamicProperties[memberName] = value;
            _dynamicPropertyTypes[memberName] = binder.GetType();
            NotifyToRefreshAllProperties();
            return true;
        }

        public bool TryGetMemberValue(string name, out object  value)
        {
            value = null;
            if (_dynamicProperties.ContainsKey(name))
            {
                value = _dynamicProperties[name];
                return true;
            }

            return false;
        }

        public void AddMember(string name, Type type, object val = null)
        {
            if (!_dynamicProperties.ContainsKey(name))
            {
                _dynamicProperties.Add(name, val);
                _dynamicPropertyTypes.Add(name,type);
            }
            else
            {
                _dynamicProperties[name] = val;
                _dynamicPropertyTypes[name] = type;
            }
        }


        #region Implementation of ICustomTypeDescriptor

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            // of course, here must be the attributes associated
            // with each of the dynamic properties
            var attributes = new Attribute[0];
            var properties = _dynamicProperties
                .Select(pair => new DynamicPropertyDescriptor(this,
                    pair.Key, _dynamicPropertyTypes[pair.Key], attributes));
            return new PropertyDescriptorCollection(properties.ToArray());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public AttributeCollection GetAttributes()
        {
            return null;
        }

        public string GetClassName()
        {
            return GetType().Name;
        }

        public string GetComponentName()
        {
            return "";
        }

        public TypeConverter GetConverter()
        {
            return null;
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Hide not implemented members



        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            var eventArgs = new PropertyChangedEventArgs(propertyName);
            PropertyChanged(this, eventArgs);
        }

        private void NotifyToRefreshAllProperties()
        {
            OnPropertyChanged(String.Empty);
        }

        #endregion


        private class DynamicPropertyDescriptor : PropertyDescriptor
        {
            private readonly BusinessObject businessObject;
            private readonly Type propertyType;

            public DynamicPropertyDescriptor(BusinessObject businessObject,
                string propertyName, Type propertyType, Attribute[] propertyAttributes)
                : base(propertyName, propertyAttributes)
            {
                this.businessObject = businessObject;
                this.propertyType = propertyType;
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override object GetValue(object component)
            {
                return businessObject._dynamicProperties[Name];
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
                businessObject._dynamicProperties[Name] = value;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(BusinessObject); }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            

            public override Type PropertyType
            {
                get { return propertyType; }
            }
        }

        public override string ToString()
        {
            return _dynamicProperties.Count + " fields";
        }
    }
}
