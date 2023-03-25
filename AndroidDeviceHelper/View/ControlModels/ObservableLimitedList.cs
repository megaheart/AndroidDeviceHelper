using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace AndroidDeviceHelper
{
    public class ObservableLimitedList<T> : LimitedList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        #region Private Methods
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Private Methods
        public ObservableLimitedList(int capacity):base(capacity){}
        public override void Insert(int index, T item)
        {
            var pre_count = Count;
            base.Insert(index, item);
            if(pre_count != Count)
            {
                OnPropertyChanged("Count");
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
            else
            {
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, _items[Count - 1], Count - 1);
            }
            OnPropertyChanged("Item[]");
        }
        public override void SetItem(int index, T value)
        {
            T originalItem = this[index];
            base.SetItem(index, value);
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, value, index);
        }
        public override void RemoveAt(int index)
        {
            T removedItem = _items[index];
            base.RemoveAt(index);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }
        public override void Clear()
        {
            base.Clear();
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionReset();
        }
    }
}
