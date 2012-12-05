using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Digillect.Collections
{
	public class XSubRangeCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IXList<T> _underlyingCollection;
		private readonly int _startIndex;
		private readonly int _maxCount;

		private int _size = -1;
		private int _version;

		#region Constructor/Disposer
		public XSubRangeCollection(IXList<T> collection, int startIndex, int maxCount)
		{
			if ( collection == null )
			{
				throw new ArgumentNullException("collection");
			}

			if ( startIndex < 0 )
			{
				throw new ArgumentOutOfRangeException("startIndex", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			if ( maxCount < 0 )
			{
				throw new ArgumentOutOfRangeException("maxCount", Errors.ArgumentOutOfRange_NeedNonNegNum);
			}

			_underlyingCollection = collection;
			_startIndex = startIndex;
			_maxCount = maxCount;

			_underlyingCollection.CollectionChanged += UnderlyingCollection_CollectionChanged;
			_underlyingCollection.Updated += UnderlyingCollection_Updated;
		}

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				_underlyingCollection.CollectionChanged -= UnderlyingCollection_CollectionChanged;
				_underlyingCollection.Updated -= UnderlyingCollection_Updated;
			}

			base.Dispose(disposing);
		}
		#endregion

		public override event NotifyCollectionChangedEventHandler CollectionChanged;
		public override event EventHandler Updated;

		public override int Count
		{
			get
			{
				if ( _size == -1 )
				{
					_size = Math.Min(_maxCount, Math.Max(_underlyingCollection.Count - _startIndex, 0));
				}

				return _size;
			}
		}

		public override T this[int index]
		{
			get
			{
				if ( index < 0 || index >= this.Count )
				{
					throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_Index);
				}

				return _underlyingCollection[_startIndex + index];
			}
		}

		public override void BeginUpdate()
		{
			_underlyingCollection.BeginUpdate();
		}

		public override XBasedCollection<T> Clone(bool deep)
		{
			return new XSubRangeCollection<T>(deep ? (IXList<T>) _underlyingCollection.Clone(true) : _underlyingCollection, _startIndex, _maxCount);
		}

		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _underlyingCollection.Skip(_startIndex).Take(this.Count).Any(x => x.Equals(item));
		}

		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _underlyingCollection.Skip(_startIndex).Take(this.Count).Any(x => x.GetKey() == key);
		}

		public override void EndUpdate()
		{
			_underlyingCollection.EndUpdate();
		}

		public override IEnumerator<T> GetEnumerator()
		{
#if false
			int version = _version;
			int endIndex = Math.Min(_startIndex + 0, _underlyingCollection.Count);

			for ( int i = _startIndex; i < endIndex; i++ )
			{
				if ( _version != version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				yield return _underlyingCollection[i];
			}
#else
			return _underlyingCollection.Skip(_startIndex).Take(this.Count).GetEnumerator();
#endif
		}

		public override int IndexOf(XKey key)
		{
			int index = 0;

			foreach ( T element in _underlyingCollection.Skip(_startIndex).Take(this.Count) )
			{
				if ( element.GetKey() == key )
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		public override int IndexOf(T item)
		{
			int index = 0;

			foreach ( T element in _underlyingCollection.Skip(_startIndex).Take(this.Count) )
			{
				if ( element.Equals(item) )
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		#region Event Handlers
		private void UnderlyingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_version++;
			_size = -1;

			if ( CollectionChanged != null )
			{
				NotifyCollectionChangedEventArgs args;
#if !SILVERLIGHT
				IList newItems, oldItems;
				int newStartingIndex;
#endif

				switch ( e.Action )
				{
#if SILVERLIGHT
					case NotifyCollectionChangedAction.Add:
						if ( e.NewStartingIndex >= _startIndex + this.Count )
						{
							// After the range - no visible changes
							return;
						}
						else if ( e.NewStartingIndex < _startIndex || this.Count == _maxCount )
						{
							// Before the range or inside the fully filled collection - reset
							goto case NotifyCollectionChangedAction.Reset;
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems[0], e.NewStartingIndex - _startIndex);

						break;

					case NotifyCollectionChangedAction.Remove:
						if ( e.OldStartingIndex >= _startIndex + this.Count )
						{
							// After the range - no visible changes
							return;
						}
						else if ( e.OldStartingIndex < _startIndex || (this.Count == _maxCount && _underlyingCollection.Count > _startIndex + this.Count) )
						{
							// Before the range or inside the fully filled collection with items remaining to the right in the underlying collection - reset
							goto case NotifyCollectionChangedAction.Reset;
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems[0], e.OldStartingIndex - _startIndex);

						break;

					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						if ( e.NewStartingIndex < _startIndex || _startIndex + this.Count <= e.NewStartingIndex )
						{
							// Out of the range - no visible changes
							return;
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems[0], e.OldItems[0], e.NewStartingIndex - _startIndex);

						break;
#else
					case NotifyCollectionChangedAction.Add:
						if ( e.NewStartingIndex >= _startIndex + this.Count )
						{
							// After the range - no visible changes
							return;
						}
						else if ( e.NewStartingIndex < _startIndex || this.Count == _maxCount )
						{
							// Before the range or inside the fully filled collection - reset
							goto case NotifyCollectionChangedAction.Reset;
						}

						if ( e.NewStartingIndex + e.NewItems.Count <= _startIndex + this.Count )
						{
							newItems = e.NewItems;
						}
						else
						{
							int count = _startIndex + this.Count - e.NewStartingIndex;

							newItems = e.NewItems.Cast<T>().Take(count).ToList();
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex - _startIndex);

						break;

					case NotifyCollectionChangedAction.Remove:
						if ( e.OldStartingIndex >= _startIndex + this.Count )
						{
							// After the range - no visible changes
							return;
						}
						else if ( e.OldStartingIndex < _startIndex || (this.Count == _maxCount && _underlyingCollection.Count > _startIndex + this.Count) )
						{
							// Before the range or inside the fully filled collection with items remaining to the right in the underlying collection - reset
							goto case NotifyCollectionChangedAction.Reset;
						}

						if ( e.OldStartingIndex + e.OldItems.Count <= _startIndex + this.Count )
						{
							oldItems = e.OldItems;
						}
						else
						{
							int count = _startIndex + this.Count - e.OldStartingIndex;

							oldItems = e.OldItems.Cast<T>().Take(count).ToList();
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex - _startIndex);

						break;

					case NotifyCollectionChangedAction.Move:
						// e.NewItems and e.OldItems have the same content
						if ( (e.NewStartingIndex + e.NewItems.Count - 1 < _startIndex || _startIndex + this.Count <= e.NewStartingIndex)
							&& (e.OldStartingIndex + e.OldItems.Count - 1 < _startIndex || _startIndex + this.Count <= e.OldStartingIndex) )
						{
							// Out of the range - no visible changes
							return;
						}

						// All other cases - reset
						goto case NotifyCollectionChangedAction.Reset;

					case NotifyCollectionChangedAction.Replace:
						// e.NewStartingIndex == e.OldStartingIndex
						if ( e.NewStartingIndex + e.NewItems.Count - 1 < _startIndex || _startIndex + this.Count <= e.NewStartingIndex )
						{
							// Out of the range - no visible changes
							return;
						}

						if ( e.NewStartingIndex >= _startIndex )
						{
							// This case covers the most common scenario where e.NewItems.Count == 1
							if ( e.NewStartingIndex + e.NewItems.Count <= _startIndex + this.Count )
							{
								newItems = e.NewItems;
								oldItems = e.OldItems;
							}
							else
							{
								int count = _startIndex + this.Count - e.NewStartingIndex;

								newItems = e.NewItems.Cast<T>().Take(count).ToList();
								oldItems = e.OldItems.Cast<T>().Take(count).ToList();
							}

							newStartingIndex = e.NewStartingIndex - _startIndex;
						}
						else
						{
							// This case is unfeasible in almost all existing scenarios unless e.NewItems.Count > 1
							int count = Math.Min(e.NewStartingIndex + e.NewItems.Count - _startIndex, this.Count);

							newItems = e.NewItems.Cast<T>().Skip(_startIndex - e.NewStartingIndex).Take(count).ToList();
							oldItems = e.OldItems.Cast<T>().Skip(_startIndex - e.NewStartingIndex).Take(count).ToList();

							newStartingIndex = 0;
						}

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, newStartingIndex);

						break;
#endif
					case NotifyCollectionChangedAction.Reset:
						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

						break;

					default:
						throw new ArgumentException(e.Action.ToString(), "e");
				}

				CollectionChanged(this, args);
			}
		}

		private void UnderlyingCollection_Updated(object sender, EventArgs e)
		{
			_version++;
			_size = -1;

			if ( Updated != null )
			{
				Updated(this, EventArgs.Empty);
			}
		}
		#endregion

#if DEBUG || CONTRACTS_FULL
		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_size >= -1);
			Contract.Invariant(_size <= _maxCount);
		}
		#endregion
#endif
	}
}
