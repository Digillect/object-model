using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XSubRangeCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		[ContractPublicPropertyName("UnderlyingCollection")]
		private IXList<T> _underlyingCollection;
		private readonly int _startIndex;
		private readonly int _maxCount;

		private int _size;
		private uint _updateCount;
#if CUSTOM_ENUMERATOR
		private int _version;
#endif

		#region Constructor/Disposer
		public XSubRangeCollection(int startIndex, int maxCount)
		{
			if ( startIndex < 0 )
			{
				throw new ArgumentOutOfRangeException("startIndex", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			if ( maxCount < 0 )
			{
				throw new ArgumentOutOfRangeException("maxCount", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			_startIndex = startIndex;
			_maxCount = maxCount;
		}

		public XSubRangeCollection(IXList<T> collection, int startIndex, int maxCount)
			: this(startIndex, maxCount)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			_underlyingCollection = collection;

			_underlyingCollection.CollectionChanged += UnderlyingCollection_CollectionChanged;
			_underlyingCollection.Updated += UnderlyingCollection_Updated;

			_size = CalculateCollectionSize();
		}

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				if ( _underlyingCollection != null )
				{
					_underlyingCollection.CollectionChanged -= UnderlyingCollection_CollectionChanged;
					_underlyingCollection.Updated -= UnderlyingCollection_Updated;

					for ( uint i = _updateCount; i != 0; i-- )
					{
						_underlyingCollection.EndUpdate();
					}

					_underlyingCollection = null;
					_updateCount = 0;
				}
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Events
		public override event NotifyCollectionChangedEventHandler CollectionChanged;

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if ( _updateCount == 0 && CollectionChanged != null )
			{
				CollectionChanged(this, e);
			}
		}

		protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
		{
			if ( _updateCount == 0 )
			{
				base.OnPropertyChanged(e);
			}
		}

		protected override void OnUpdated(EventArgs e)
		{
			if ( _updateCount == 0 )
			{
				base.OnUpdated(e);
			}
		}
		#endregion

		#region Properties
		public override int Count
		{
			get { return _size; }
		}

		public override T this[int index]
		{
			get
			{
				if ( index < 0 || index >= _size )
				{
					throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_Index);
				}

				return _underlyingCollection[_startIndex + index];
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IXList<T> UnderlyingCollection
		{
			get { return _underlyingCollection; }
			set
			{
				if ( value == null )
				{
					throw new ArgumentNullException("value");
				}

				if ( _underlyingCollection != null )
				{
					_underlyingCollection.CollectionChanged -= UnderlyingCollection_CollectionChanged;
					_underlyingCollection.Updated -= UnderlyingCollection_Updated;

					uint updateCount = _updateCount;

					for ( uint i = updateCount; i != 0; i-- )
					{
						_underlyingCollection.EndUpdate();
					}
				}

				_underlyingCollection = value;

				uint updateCount2 = _updateCount;

				for ( uint i = 0 ; i < updateCount2; i++ )
				{
					_underlyingCollection.BeginUpdate();
				}

				_underlyingCollection.CollectionChanged += UnderlyingCollection_CollectionChanged;
				_underlyingCollection.Updated += UnderlyingCollection_Updated;

				_size = CalculateCollectionSize();

				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				OnUpdated(EventArgs.Empty);
			}
		}
		#endregion

		#region Methods
		public override void BeginUpdate()
		{
			_updateCount++;

			if ( _underlyingCollection != null )
			{
				_underlyingCollection.BeginUpdate();
			}
		}

		public override XBasedCollection<T> Clone(bool deep)
		{
			if ( _underlyingCollection == null )
			{
				return new XSubRangeCollection<T>(_startIndex, _maxCount);
			}

			return new XSubRangeCollection<T>(deep ? (IXList<T>) _underlyingCollection.Clone(true) : _underlyingCollection, _startIndex, _maxCount);
		}

		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			if ( _underlyingCollection == null )
			{
				return false;
			}

			return _underlyingCollection.Skip(_startIndex).Take(_size).Any(x => x.Equals(item));
		}

		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			if ( _underlyingCollection == null )
			{
				return false;
			}

			return _underlyingCollection.Skip(_startIndex).Take(_size).Any(x => x.GetKey() == key);
		}

		public override void EndUpdate()
		{
			if ( _updateCount == 0 )
			{
				return;
			}

			if ( _underlyingCollection != null )
			{
				_underlyingCollection.EndUpdate();
			}

			if ( --_updateCount == 0 )
			{
				OnUpdated(EventArgs.Empty);
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public override IEnumerator<T> GetEnumerator()
		{
#if CUSTOM_ENUMERATOR
			if ( _underlyingCollection == null )
			{
				yield break;
			}

			int version = _version;
			int endIndex = Math.Min(_startIndex + _size, _underlyingCollection.Count);

			for ( int i = _startIndex; i < endIndex; i++ )
			{
				if ( version != _version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				yield return _underlyingCollection[i];
			}
#else
			if ( _underlyingCollection == null )
			{
				return Enumerable.Empty<T>().GetEnumerator();
			}

			return _underlyingCollection.Skip(_startIndex).Take(_size).GetEnumerator();
#endif
		}

		public override int IndexOf(XKey key)
		{
			if ( _underlyingCollection != null )
			{
				int index = 0;

				foreach ( T element in _underlyingCollection.Skip(_startIndex).Take(_size) )
				{
					if ( element.GetKey() == key )
					{
						return index;
					}

					index++;
				}
			}

			return -1;
		}

		public override int IndexOf(T item)
		{
			if ( _underlyingCollection != null )
			{
				int index = 0;

				foreach ( T element in _underlyingCollection.Skip(_startIndex).Take(_size) )
				{
					if ( element.Equals(item) )
					{
						return index;
					}

					index++;
				}
			}

			return -1;
		}

		[Pure]
		protected int CalculateCollectionSize()
		{
			if ( _underlyingCollection == null )
			{
				throw new InvalidOperationException();
			}

			Contract.EndContractBlock();

			return Math.Min(_maxCount, Math.Max(_underlyingCollection.Count - _startIndex, 0));
		}
		#endregion

		#region Event Handlers
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void UnderlyingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Contract.Assume(_underlyingCollection != null);

#if CUSTOM_ENUMERATOR
			_version++;
#endif

			int prevSize = Interlocked.Exchange(ref _size, CalculateCollectionSize());

			if ( _updateCount != 0 )
			{
				return;
			}

			if ( prevSize != _size )
			{
				OnPropertyChanged(CountString);
			}

			switch ( e.Action )
			{
#if SILVERLIGHT
				case NotifyCollectionChangedAction.Add:
					if ( e.NewStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.NewStartingIndex < _startIndex || _size == _maxCount )
					{
						// Before the range or inside the fully filled collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.NewStartingIndex - _startIndex));
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( e.OldStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.OldStartingIndex < _startIndex || (_size == _maxCount && _underlyingCollection.Count > _startIndex + _size) )
					{
						// Before the range or inside the fully filled collection with items remaining to the right in the underlying collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], e.OldStartingIndex - _startIndex));
					}

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					if ( e.NewStartingIndex < _startIndex || _startIndex + _size <= e.NewStartingIndex )
					{
						// Out of the range - no visible changes
						return;
					}

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], e.NewStartingIndex - _startIndex));
					}

					break;
#else
				case NotifyCollectionChangedAction.Add:
					if ( e.NewStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.NewStartingIndex < _startIndex || _size == _maxCount )
					{
						// Before the range or inside the fully filled collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);

					IList newItems;

					if ( CollectionChanged != null )
					{
						if ( e.NewStartingIndex + e.NewItems.Count <= _startIndex + _size )
						{
							newItems = e.NewItems;
						}
						else
						{
							int count = _startIndex + _size - e.NewStartingIndex;

							newItems = e.NewItems.Cast<T>().Take(count).ToList();
						}

						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, newItems, e.NewStartingIndex - _startIndex));
					}

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( e.OldStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.OldStartingIndex < _startIndex || (_size == _maxCount && _underlyingCollection.Count > _startIndex + _size) )
					{
						// Before the range or inside the fully filled collection with items remaining to the right in the underlying collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);

					IList oldItems;

					if ( CollectionChanged != null )
					{
						if ( e.OldStartingIndex + e.OldItems.Count <= _startIndex + _size )
						{
							oldItems = e.OldItems;
						}
						else
						{
							int count = _startIndex + _size - e.OldStartingIndex;

							oldItems = e.OldItems.Cast<T>().Take(count).ToList();
						}

						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, oldItems, e.OldStartingIndex - _startIndex));
					}

					break;

				case NotifyCollectionChangedAction.Move:
					// e.NewItems and e.OldItems have the same content
					if ( (e.NewStartingIndex + e.NewItems.Count - 1 < _startIndex || _startIndex + _size <= e.NewStartingIndex)
						&& (e.OldStartingIndex + e.OldItems.Count - 1 < _startIndex || _startIndex + _size <= e.OldStartingIndex) )
					{
						// Out of the range - no visible changes
						return;
					}

					// All other cases - reset
					goto case NotifyCollectionChangedAction.Reset;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					if ( e.NewStartingIndex + e.NewItems.Count - 1 < _startIndex || _startIndex + _size <= e.NewStartingIndex )
					{
						// Out of the range - no visible changes
						return;
					}

					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						int newStartingIndex;

						if ( e.NewStartingIndex >= _startIndex )
						{
							// This case covers the most common scenario where e.NewItems.Count == 1
							if ( e.NewStartingIndex + e.NewItems.Count <= _startIndex + _size )
							{
								newItems = e.NewItems;
								oldItems = e.OldItems;
							}
							else
							{
								int count = _startIndex + _size - e.NewStartingIndex;

								newItems = e.NewItems.Cast<T>().Take(count).ToList();
								oldItems = e.OldItems.Cast<T>().Take(count).ToList();
							}

							newStartingIndex = e.NewStartingIndex - _startIndex;
						}
						else
						{
							// This case is unfeasible in almost all existing scenarios unless e.NewItems.Count > 1
							int count = Math.Min(e.NewStartingIndex + e.NewItems.Count - _startIndex, _size);

							newItems = e.NewItems.Cast<T>().Skip(_startIndex - e.NewStartingIndex).Take(count).ToList();
							oldItems = e.OldItems.Cast<T>().Skip(_startIndex - e.NewStartingIndex).Take(count).ToList();

							newStartingIndex = 0;
						}

						CollectionChanged(this, new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, newStartingIndex));
					}

					break;
#endif
				case NotifyCollectionChangedAction.Reset:
					OnPropertyChanged(IndexerName);

					if ( CollectionChanged != null )
					{
						CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					}

					break;

				default:
					throw new ArgumentException(e.Action.ToString(), "e");
			}
		}

		private void UnderlyingCollection_Updated(object sender, EventArgs e)
		{
			Contract.Assume(_underlyingCollection != null);

#if CUSTOM_ENUMERATOR
			_version++;
#endif

			Interlocked.Exchange(ref _size, CalculateCollectionSize());

			if ( _updateCount == 0 )
			{
				OnUpdated(EventArgs.Empty);
			}
		}
		#endregion

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_size >= 0);
			Contract.Invariant(_size <= _maxCount);
		}
		#endregion
#endif
	}
}
