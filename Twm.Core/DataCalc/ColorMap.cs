using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Twm.Core.DataCalc
{
    public class ColorMap : PositionBasedMap<Brush>
    {
        public ColorMap(DataCalcContext dataCalcContext) : base(dataCalcContext)
        {
        }
    }

    public class PositionBasedMap<V> : IDictionary<int, V>
    {
        private DataCalcContext _dataCalcContext;
        protected Dictionary<int, V> _valuesUnderFacade = new Dictionary<int, V>();

        public PositionBasedMap(DataCalcContext dataCalcContext)
        {
            _dataCalcContext = dataCalcContext;
        }

        public IEnumerator<KeyValuePair<int, V>> GetEnumerator()
        {
            return _valuesUnderFacade.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<int, V> item)
        { 
            _valuesUnderFacade.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _valuesUnderFacade.Clear();
        }

        public bool Contains(KeyValuePair<int, V> item)
        {
            return _valuesUnderFacade.Contains(item);
        }

        public void CopyTo(KeyValuePair<int, V>[] array, int arrayIndex)
        {
            
        }

        public bool Remove(KeyValuePair<int, V> item)
        {
            return _valuesUnderFacade.Remove(item.Key);
        }

        public int Count { get => _valuesUnderFacade.Count; }
        public bool IsReadOnly { get => false; }

        public bool ContainsKey(int key)
        {
            return _valuesUnderFacade.ContainsKey(key);
        }

        public void Add(int key, V value)
        {
            _valuesUnderFacade.Add(key, value);
        }

        public bool Remove(int key)
        {
            return _valuesUnderFacade.Remove(key);
        }

        public bool TryGetValue(int key, out V value)
        {
            return _valuesUnderFacade.TryGetValue(key, out value);
        }

        public virtual V this[int key]
        {
            get =>  _valuesUnderFacade[_dataCalcContext.CurrentBar - 1 - key];
            set => _valuesUnderFacade[_dataCalcContext.CurrentBar - 1 - key] = value;
        }

        public ICollection<int> Keys { get => _valuesUnderFacade.Keys; }
        public ICollection<V> Values { get=> _valuesUnderFacade.Values; }
    }
}