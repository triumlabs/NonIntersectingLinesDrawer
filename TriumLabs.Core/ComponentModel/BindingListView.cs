using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using TriumLabs.Core.Collections;
using TriumLabs.Core.Threading;

namespace TriumLabs.Core.ComponentModel
{
    /// <summary>
    /// Represents a bindable, sortable and filterable view of list of objects.
    /// </summary>
    public class BindingListView<T> : IBindingListView, IList<T>
    {
        #region Fields

        private readonly List<T> listOriginal;
        private readonly List<T> listView;
        private readonly Lazy<PropertyDescriptorCollection> itemProperties = new Lazy<PropertyDescriptorCollection>(() => TypeDescriptor.GetProperties(typeof(T)));
        private bool isSorted;
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;
        private string filter;
        private ListSortDescriptionCollection sortDescriptions;
        private bool dispatchEvents;
        private Dispatcher dispatcher;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether event-handler calls are dispatched if required.
        /// </summary>
        /// <returns>true if event-handler calls are dispatched if required; otherwise false.</returns>
        public bool DispatchEvents
        {
            get { lock (SyncRoot) { return dispatchEvents; } }
            set { lock (SyncRoot) { dispatchEvents = value; } }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BindingListView`1"/> class.
        /// </summary>
        public BindingListView()
        {
            listOriginal = new List<T>();
            listView = new List<T>(listOriginal);
            
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BindingListView`1"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity" /> is less than zero.</exception>
        public BindingListView(int capacity)
        {
            listOriginal = new List<T>(capacity);
            listView = new List<T>(listOriginal);

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BindingListView`1"/> class.
        /// </summary>
        /// <param name="collection">The <see cref="T:System.Collections.ICollection" /> whose elements are copied to the new list.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection" /> is null.</exception>
        public BindingListView(ICollection<T> collection)
        {
            listOriginal = new List<T>(collection);
            listView = new List<T>(listOriginal);
            
            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1" />.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="collection" /> is null.</exception>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            lock (SyncRoot)
            {
                collection.ForEach(item =>
                    {
                        listOriginal.Add(item);
                        SubscribeItemPropertyChangedEvent(item);
                    });
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
            }

            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// Raises the <see cref="ListChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ListChangedEventArgs" /> that contains the event data.</param>
        protected virtual void RaiseEventListChanged(ListChangedEventArgs e)
        {
            IsSorted = IsSorted &&
                !(e.ListChangedType == ListChangedType.ItemAdded ||
                e.ListChangedType == ListChangedType.ItemChanged ||
                e.ListChangedType == ListChangedType.ItemMoved ||
                e.ListChangedType == ListChangedType.Reset);

            if (DispatchEvents)
                dispatcher.Invoke(() => ListChanged(this, e));
            else ListChanged(this, e);
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        private void Initialize()
        {
            AllowNew = typeof(T).HasDefaultConstructor();
            IsSorted = false;
            DispatchEvents = false;
            dispatcher = new Dispatcher();
        }

        /// <summary>
        /// Replaces the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to replace.</param>
        /// <param name="item">The item to replace in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        private void Replace(int index, T item)
        {
            var indexFiltered = -1;
            lock (SyncRoot)
            {
                var indexOriginal = listOriginal.IndexOf(listView[index]);
                var itemOriginal = listOriginal[indexOriginal];
                listOriginal[indexOriginal] = item;

                UnsubscribeItemPropertyChangedEvent(itemOriginal);
                SubscribeItemPropertyChangedEvent(item);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);

                indexFiltered = listView.IndexOf(item);
            }
            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));

            if (indexFiltered >= 0)
            {
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, indexFiltered));
            }
        }

        /// <summary>
        /// Subscribes to item's PropertyChanged event if it is supported.
        /// </summary>
        /// <param name="value">The object to subscribe to.</param>
        private void SubscribeItemPropertyChangedEvent(object value)
        {
            var notifyObject = value as INotifyPropertyChanged;
            if (notifyObject == null) return;

            notifyObject.PropertyChanged += HandleEventItemPropertyChanged;
        }

        /// <summary>
        /// Unsubscribes from item's PropertyChanged event if it is supported.
        /// </summary>
        /// <param name="value">The object to unsubscribe from.</param>
        private void UnsubscribeItemPropertyChangedEvent(object value)
        {
            var notifyObject = value as INotifyPropertyChanged;
            if (notifyObject == null) return;

            notifyObject.PropertyChanged -= HandleEventItemPropertyChanged;
        }

        /// <summary>
        /// Called when item's PropertyChanged event is raised.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The PropertyChanged event argument.</param>
        private void HandleEventItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var indexFiltered = listView.IndexOf((T)sender);
            if (indexFiltered >= 0)
            {
                var itemProperty = itemProperties.Value.Find(e.PropertyName, true);
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, indexFiltered, itemProperty));
            }
        }

        /// <summary>
        /// Updates the filtered list.
        /// </summary>
        /// <param name="listSrc">The list to filter and sort.</param>
        /// <param name="listDst">The list filtered and sorted.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <param name="sorts">The <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> containing the sorts to apply to the list.</param>
        private static void FilterAndSortList(List<T> listSrc, List<T> listDst, string filter, ListSortDescriptionCollection sorts)
        {
            var regexFilter = new Regex(@"(\w+) *= *'([^']*)'", RegexOptions.Singleline);

            listDst.Clear();
            listDst.AddRange(listSrc);

            var matches = regexFilter.Matches(filter ?? String.Empty);
            if (matches.Count > 0)
            {
                var filterOperands = matches.Cast<Match>()
                    .Select(match => new 
                    { 
                        PropertyDescriptor = TypeDescriptor.GetProperties(typeof(T))[match.Groups[1].Value], 
                        Text = match.Groups[2].Value 
                    })
                    .ToArray();

                filterOperands.ForEach(filterOperand =>
                    {
                        var value = TypeDescriptor
                            .GetConverter(filterOperand.PropertyDescriptor.PropertyType)
                            .ConvertFromString(filterOperand.Text);
                        var itemsToRemove = listDst
                            .Where(item => CompareValues(filterOperand.PropertyDescriptor.GetValue(item), value) != 0)
                            .ToArray();
                        itemsToRemove.ForEach(item => listDst.Remove(item));
                    });
            }

            SortList(listDst, sorts);
        }

        /// <summary>
        /// Sorts a list.
        /// </summary>
        /// <param name="list">The list to sort.</param>
        /// <param name="sorts">The <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> containing the sorts to apply to the list.</param>
        private static void SortList(List<T> list, ListSortDescriptionCollection sorts)
        {
            if (sorts != null && sorts.Count > 0)
            {
                list.Sort((x, y) =>
                {
                    var result = 0;
                    for (var idxSort = 0; idxSort < sorts.Count && result == 0; idxSort++)
                    {
                        result = CompareItems(x, y, sorts[idxSort].PropertyDescriptor, sorts[idxSort].SortDirection);
                    }
                    return result;
                });
            }
        }

        /// <summary>
        /// Compares two items and returns an integer that 
        /// indicates whether the item X precedes, follows, or occurs in the same position in the sort order as item Y.
        /// </summary>
        /// <param name="x">The first item to compare.</param>
        /// <param name="y">The second item to compare.</param>
        /// <param name="property">The property descriptor.</param>
        /// <param name="direction">The sort direction.</param>
        /// <returns>Less then zero, if X preceeds Y, Greater than zero, if Y preceeds X, Zero if both are in the same position in the sort order.</returns>
        private static int CompareItems(T x, T y, PropertyDescriptor property, ListSortDirection direction)
        {
            var valueX = x != null ? property.GetValue(x) : null;
            var valueY = y != null ? property.GetValue(y) : null;

            var result = CompareValues(valueX, valueY);
            result = direction == ListSortDirection.Ascending ? result : -result;
            return result;
        }

        /// <summary>
        /// Compares two values and returns an integer that 
        /// indicates whether the value X precedes, follows, or occurs in the same position in the sort order as value Y.
        /// </summary>
        /// <param name="valueX">The first value to compare.</param>
        /// <param name="valueY">The second value to compare.</param>
        /// <returns>Less then zero, if X preceeds Y, Greater than zero, if Y preceeds X, Zero if both are in the same position in the sort order.</returns>
        private static int CompareValues(object valueX, object valueY)
        {
            var result = 0;
            if (ReferenceEquals(valueX, valueY))
                result = 0;
            else if (valueX == null)
                result = -1;
            else if (valueY == null)
                result = 1;
            else
            {
                var comparableX = valueX as IComparable;
                if (comparableX != null)
                    result = comparableX.CompareTo(valueY);
                else if (valueX.Equals(valueY))
                    result = 0;
                else result = String.Compare(valueX.ToString(), valueY.ToString(), StringComparison.CurrentCulture);
            }

            return result;
        }

        #endregion

        #region IBindingListView

        /// <summary>
        /// Gets or sets the filter to be used to exclude items from the collection of items returned by the data source.
        /// </summary>
        /// <returns>The string used to filter items out in the item collection returned by the data source.</returns>
        public string Filter 
        {
            get
            {
                lock (SyncRoot)
                { 
                    return filter; 
                }
            }
            set
            {
                lock (SyncRoot)
                {
                    filter = value;

                    FilterAndSortList(listOriginal, listView, filter, SortDescriptions);
                }
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }


        /// <summary>
        /// Gets the collection of sort descriptions currently applied to the data source.
        /// </summary>
        /// <returns>The <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> currently applied to the data source.</returns>
        public ListSortDescriptionCollection SortDescriptions 
        {
            get
            {
                lock (SyncRoot)
                {
                    return sortDescriptions;
                }
            }
            private set
            {
                lock (SyncRoot)
                {
                    sortDescriptions = value;
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the data source supports advanced sorting.
        /// </summary>
        /// <returns>true if the data source supports advanced sorting; otherwise, false.</returns>
        public bool SupportsAdvancedSorting { get { return SupportsSorting; } }
        
        /// <summary>
        /// Gets a value indicating whether the data source supports filtering.
        /// </summary>
        /// <returns>true if the data source supports filtering; otherwise, false.</returns>
        public bool SupportsFiltering { get { return true; } }
        
        /// <summary>
        /// Sorts the data source based on the given <see cref="T:System.ComponentModel.ListSortDescriptionCollection" />.
        /// </summary>
        /// <param name="sorts">The <see cref="T:System.ComponentModel.ListSortDescriptionCollection" /> containing the sorts to apply to the data source.</param>
        public void ApplySort(ListSortDescriptionCollection sorts)
        {
            lock (SyncRoot)
            {
                if (!SupportsAdvancedSorting) throw new NotSupportedException();

                SortDescriptions = sorts;

                if (sorts != null && sorts.Count > 0)
                {
                    SortProperty = sorts[0].PropertyDescriptor;
                    SortDirection = sorts[0].SortDirection;

                    SortList(listView, sorts);

                    IsSorted = true;
                }
                else
                {
                    SortProperty = null;
                    SortDirection = ListSortDirection.Ascending;

                    FilterAndSortList(listOriginal, listView, Filter, sorts);

                    IsSorted = false;
                }
            }
            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// Removes the current filter applied to the data source.
        /// </summary>
        public void RemoveFilter()
        {
            lock (SyncRoot)
            {
                Filter = null;
            }
        }

        #endregion

        #region IBindingList Members
        /// <summary>
        /// Occurs when the list changes or an item in the list changes.
        /// </summary>
        public event ListChangedEventHandler ListChanged = delegate { };

        /// <summary>
        /// Gets whether you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew" />.
        /// </summary>
        /// <returns>true if you can add items to the list using <see cref="M:System.ComponentModel.IBindingList.AddNew" />; otherwise, false.</returns>
        public bool AllowNew { get; private set; }
        
        /// <summary>
        /// Gets whether you can update items in the list.
        /// </summary>
        /// <returns>true if you can update the items in the list; otherwise, false.</returns>
        public bool AllowEdit { get { return true; } }

        /// <summary>
        /// Gets whether you can remove items from the list, using <see cref="M:System.Collections.IList.Remove(System.Object)" /> or 
        /// <see cref="M:System.Collections.IList.RemoveAt(System.Int32)" />.
        /// </summary>
        /// <returns>true if you can remove items from the list; otherwise, false.</returns>
        public bool AllowRemove { get { return true; } }
        
        /// <summary>
        /// Gets whether a <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event is raised when the list changes or an item in the list changes.
        /// </summary>
        /// <returns>true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event is raised when the list changes or when an item changes; otherwise, false.</returns>
        public bool SupportsChangeNotification { get { return true; } }

        /// <summary>
        /// Gets whether the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" /> method.
        /// </summary>
        /// <returns>true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" /> method; otherwise, false.</returns>
        public bool SupportsSearching { get { return true; } }

        /// <summary>
        /// Gets whether the list supports sorting.
        /// </summary>
        /// <returns>true if the list supports sorting; otherwise, false.</returns>
        public bool SupportsSorting { get { return true; } }

        /// <summary>
        /// Gets whether the items in the list are sorted.
        /// </summary>
        /// <returns>true if <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" /> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort" /> has not been called; otherwise, false.</returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is false.</exception>
        public bool IsSorted 
        { 
            get 
            {
                lock (SyncRoot)
                {
                    if (!SupportsSorting) throw new NotSupportedException();
                    return isSorted;
                }
            }
            private set
            {
                lock (SyncRoot)
                {
                    isSorted = value;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> that is being used for sorting.
        /// </summary>
        /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> that is being used for sorting.</returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is false.</exception>
        public PropertyDescriptor SortProperty
        {
            get
            {
                lock (SyncRoot)
                {
                    if (!SupportsSorting) throw new NotSupportedException();
                    return sortProperty;
                }
            }
            private set
            {
                lock (SyncRoot)
                {
                    sortProperty = value;
                }
            }
        }
        
        /// <summary>
        /// Gets the direction of the sort.
        /// </summary>
        /// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values.</returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is false.</exception>
        public ListSortDirection SortDirection
        {
            get
            {
                lock (SyncRoot)
                {
                    if (!SupportsSorting) throw new NotSupportedException();
                    return sortDirection;
                }
            }
            private set
            {
                lock (SyncRoot)
                {
                    sortDirection = value;
                }
            }
        }

        
        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        /// <returns>The item added to the list.</returns>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.AllowNew" /> is false.</exception>
        public object AddNew()
        {
            lock (SyncRoot)
            {
                if (!AllowNew) throw new NotSupportedException();
                return Activator.CreateInstance<T>();
            }
        }

        /// <summary>
        /// Adds the <see cref="T:System.ComponentModel.PropertyDescriptor" /> to the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to add to the indexes used for searching.</param>
        public void AddIndex(PropertyDescriptor property)
        {
            lock (SyncRoot)
            {
                if (!SupportsSearching) throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor" /> and 
        /// a <see cref="T:System.ComponentModel.ListSortDirection" />.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to sort by.</param>
        /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values.</param>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is false.</exception>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            lock (SyncRoot)
            {
                if (!SupportsSorting) throw new NotSupportedException();

                ApplySort(new ListSortDescriptionCollection(
                    new[] { new ListSortDescription(property, direction) }));
            }
        }

        /// <summary>
        /// Returns the index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.
        /// </summary>
        /// <returns>The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor" />.</returns>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to search on.</param>
        /// <param name="key">The value of the <paramref name="property" /> parameter to search for.</param>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSearching" /> is false.</exception>
        public int Find(PropertyDescriptor property, object key)
        {
            lock (SyncRoot)
            {
                if (!SupportsSearching) throw new NotSupportedException();

                listView.FindIndex(item =>
                    {
                        if (item == null) return false;
                        var value = property.GetValue(item);

                        if (ReferenceEquals(value, key)) return true;
                        if (value == null || key == null) return false;
                        return value.Equals(key);
                    });

                return 0;
            }
        }

        /// <summary>
        /// Removes the <see cref="T:System.ComponentModel.PropertyDescriptor" /> from the indexes used for searching.
        /// </summary>
        /// <param name="property">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to remove from the indexes used for searching.</param>
        public void RemoveIndex(PropertyDescriptor property)
        {
            lock (SyncRoot)
            {
                if (!SupportsSearching) throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Removes any sort applied using <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException"><see cref="P:System.ComponentModel.IBindingList.SupportsSorting" /> is false.</exception>
        public void RemoveSort()
        {
            lock (SyncRoot)
            {
                if (!SupportsSorting) throw new NotSupportedException();

                ApplySort(null);
            }
        }

        #endregion

        #region IList Members

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public T this[int index]
        {
            get
            {
                lock (SyncRoot)
                {
                    return listView[index];
                }
            }
            set
            {
                Replace(index, value);
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.IList" /> is read-only.</exception>
        object IList.this[int index]
        {
            get 
            {
                lock (SyncRoot)
                {
                    return listView[index];
                }
            }
            set { Replace(index, (T)value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList" /> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.IList" /> has a fixed size; otherwise, false.</returns>
        public bool IsFixedSize { 
            get 
            {
                lock (SyncRoot)
                {
                    return ((IList)listOriginal).IsFixedSize;
                }
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public int IndexOf(T item)
        {
            lock (SyncRoot)
            {
                return listView.IndexOf(item);
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <returns>The index of <paramref name="value" /> if found in the list; otherwise, -1.</returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
        int IList.IndexOf(object value)
        {
            return ((IList)listView).IndexOf(value);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void Insert(int index, T item)
        {
            lock (SyncRoot)
            {
                var indexOriginal = index < listView.Count ? listOriginal.IndexOf(listView[index]) : index;
                listOriginal.Insert(indexOriginal, item);

                SubscribeItemPropertyChangedEvent(item);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
            }

            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value" /> should be inserted.</param>
        /// <param name="value">The object to insert into the <see cref="T:System.Collections.IList" />.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.IList" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
        /// <exception cref="T:System.NullReferenceException"><paramref name="value" /> is null reference in the <see cref="T:System.Collections.IList" />.</exception>
        void IList.Insert(int index, object value)
        {
            lock (SyncRoot)
            {
                var indexOriginal = index < listView.Count ? listOriginal.IndexOf(listView[index]) : index;
                ((IList)listOriginal).Insert(indexOriginal, value);

                SubscribeItemPropertyChangedEvent(value);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
            }
            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public void RemoveAt(int index)
        {
            lock (SyncRoot)
            {
                var item = listView[index];
                var indexOriginal = listOriginal.IndexOf(item);
                listOriginal.RemoveAt(indexOriginal);

                UnsubscribeItemPropertyChangedEvent(item);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
            }
            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get
            {
                lock (SyncRoot) { return listView.Count; }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get
            {
                lock (SyncRoot)
                {
                    return ((IList)listOriginal).IsReadOnly;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
        /// </summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized { get { return true; } }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
        public object SyncRoot { get { return ((ICollection)listOriginal).SyncRoot; } }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(T item)
        {
            var indexFiltered = -1;
            lock (SyncRoot)
            {
                listOriginal.Add(item);

                SubscribeItemPropertyChangedEvent(item);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);

                indexFiltered = listView.IndexOf(item);
            }
            if (indexFiltered >= 0)
            {
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, indexFiltered));
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <returns>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection,</returns>
        /// <param name="value">The object to add to the <see cref="T:System.Collections.IList" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
        int IList.Add(object value)
        {
            var indexFiltered = -1;
            lock (SyncRoot)
            {
                ((IList)listOriginal).Add(value);

                SubscribeItemPropertyChangedEvent(value);
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
                indexFiltered = ((IList)listView).IndexOf(value);
            }
            if (indexFiltered >= 0)
            {
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, indexFiltered));
            }
            return indexFiltered;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear()
        {
            lock (SyncRoot)
            {
                listOriginal.ForEach(item => UnsubscribeItemPropertyChangedEvent(item));
                listOriginal.Clear();
                FilterAndSortList(listOriginal, listView, Filter, SortDescriptions);
            }
            RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item)
        {
            lock (SyncRoot)
            {
                return listView.Contains(item);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList" /> contains a specific value.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Object" /> is found in the <see cref="T:System.Collections.IList" />; otherwise, false.</returns>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList" />.</param>
        public bool Contains(object value)
        {
            lock (SyncRoot)
            {
                return ((IList)listView).Contains(value);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (SyncRoot)
            {
                listView.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array" /> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.-or-The type of the source <see cref="T:System.Collections.ICollection" /> cannot be cast automatically to the type of the destination <paramref name="array" />.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)listView).CopyTo(array, index);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(T item)
        {
            bool retVal = false;
            var indexFiltered = -1;
            lock (SyncRoot)
            {
                if (listOriginal.Remove(item))
                {
                    UnsubscribeItemPropertyChangedEvent(item);

                    indexFiltered = listView.IndexOf(item);
                    if (indexFiltered >= 0)
                    {
                        listView.RemoveAt(indexFiltered);
                    }
                    retVal = true;
                }
            }
            if (indexFiltered >= 0)
            {
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, indexFiltered));
            }
            return retVal;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList" />.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="T:System.Collections.IList" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList" /> is read-only.-or- The <see cref="T:System.Collections.IList" /> has a fixed size.</exception>
        public void Remove(object value)
        {
            var indexFiltered = -1;
            lock (SyncRoot)
            {
                var indexOriginal = ((IList)listOriginal).IndexOf(value);
                if (indexOriginal >= 0)
                {
                    ((IList)listOriginal).Remove(value);

                    UnsubscribeItemPropertyChangedEvent(value);

                    indexFiltered = ((IList)listView).IndexOf(value);
                    if (indexFiltered >= 0)
                    {
                        ((IList)listView).RemoveAt(indexFiltered);
                    }
                }
            }
            if (indexFiltered >= 0)
            {
                RaiseEventListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, indexFiltered));
            }
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            //return listView.GetEnumerator();
            //List<T> listClone;
            lock (SyncRoot)
            {
                //listClone = new List<T>(listView);
                for (var idx = 0; idx < Count; idx++)
                {
                    yield return this[idx];
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            //return ((IEnumerable)listView).GetEnumerator();
            //List<T> listClone;
            lock (SyncRoot)
            {
                //listClone = new List<T>(listView);
                for (var idx = 0; idx < Count; idx++)
                {
                    yield return this[idx];
                }
            }
        }

        #endregion
    }
}
 