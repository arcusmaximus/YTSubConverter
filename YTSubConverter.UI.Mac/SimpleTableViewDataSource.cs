using System;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace YTSubConverter.UI.Mac
{
    public class SimpleTableViewDataSource<T> : NSTableViewDataSource
    {
        private readonly Func<T, object> _valueSelector;

        public SimpleTableViewDataSource(List<T> items, Func<T, object> valueSelector)
        {
            InnerList = items;
            _valueSelector = valueSelector;
        }

        public List<T> InnerList
        {
            get;
        }

        public T this[int index]
        {
            get => InnerList[index];
        }

        public override nint GetRowCount(NSTableView tableView)
        {
            return InnerList.Count;
        }

        public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            T item = InnerList[(int)row];
            return FromObject(_valueSelector(item));
        }
    }
}
