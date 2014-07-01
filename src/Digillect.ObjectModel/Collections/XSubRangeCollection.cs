#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Digillect.Collections
{
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class XSubRangeCollection<T> : XBasedCollection<T>
		where T : XObject
	{
		private readonly IXList<T> _baseCollection;
		private readonly int _startIndex;
		private readonly int _maxCount;

		private int _size;
#if CUSTOM_ENUMERATOR
		private int _version;
#endif

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

			Contract.EndContractBlock();

			_baseCollection = collection;
			_startIndex = startIndex;
			_maxCount = maxCount;

			_baseCollection.CollectionChanged += BaseCollection_CollectionChanged;

			_size = CalculateCollectionSize();
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				_baseCollection.CollectionChanged -= BaseCollection_CollectionChanged;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Properties
		/// <inheritdoc/>
		public override int Count
		{
			get { return _size; }
		}

		/// <inheritdoc/>
		public override T this[int index]
		{
			get
			{
				if ( index < 0 || index >= _size )
				{
					throw new ArgumentOutOfRangeException("index", Errors.ArgumentOutOfRange_Index);
				}

				return _baseCollection[_startIndex + index];
			}
		}

		public IXList<T> BaseCollection
		{
			get { return _baseCollection; }
		}
		#endregion

		#region Methods
		/// <inheritdoc/>
		public override XBasedCollection<T> Clone(bool deep)
		{
			return new XSubRangeCollection<T>(deep ? (IXList<T>) _baseCollection.Clone(true) : _baseCollection, _startIndex, _maxCount);
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool Contains(T item)
		{
			return _baseCollection.Skip(_startIndex).Take(_size).Any(x => x.Equals(item));
		}

		/// <inheritdoc/>
		[ContractVerification(false)]
		public override bool ContainsKey(XKey key)
		{
			return _baseCollection.Skip(_startIndex).Take(_size).Any(x => x.GetKey() == key);
		}

		/// <inheritdoc/>
		public override IEnumerator<T> GetEnumerator()
		{
#if CUSTOM_ENUMERATOR
			int version = _version;
			int endIndex = Math.Min(_startIndex + _size, _baseCollection.Count);

			for ( int i = _startIndex; i < endIndex; i++ )
			{
				if ( version != _version )
				{
					throw new InvalidOperationException(Errors.XCollectionEnumFailedVersionException);
				}

				yield return _baseCollection[i];
			}
#else
			return _baseCollection.Skip(_startIndex).Take(_size).GetEnumerator();
#endif
		}

		/// <inheritdoc/>
		public override int IndexOf(XKey key)
		{
			int index = 0;

			foreach ( T element in _baseCollection.Skip(_startIndex).Take(_size) )
			{
				if ( element.GetKey() == key )
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		/// <inheritdoc/>
		public override int IndexOf(T item)
		{
			int index = 0;

			foreach ( T element in _baseCollection.Skip(_startIndex).Take(_size) )
			{
				if ( element.Equals(item) )
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		[Pure]
		private int CalculateCollectionSize()
		{
			return Math.Min(_maxCount, Math.Max(_baseCollection.Count - _startIndex, 0));
		}
		#endregion

		#region Event Handlers
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void BaseCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Contract.Assume(_baseCollection != null);

#if CUSTOM_ENUMERATOR
			_version++;
#endif

			int prevSize = Interlocked.Exchange(ref _size, CalculateCollectionSize());

			if ( this.IsInUpdate )
			{
				return;
			}

			if ( prevSize != _size )
			{
				OnPropertyChanged(CountString);
			}

			switch ( e.Action )
			{
#if NET40 && SILVERLIGHT
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
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.NewStartingIndex - _startIndex));

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( e.OldStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.OldStartingIndex < _startIndex || (_size == _maxCount && _baseCollection.Count > _startIndex + _size) )
					{
						// Before the range or inside the fully filled collection with items remaining to the right in the source collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.OldItems[0], e.OldStartingIndex - _startIndex));

					break;

				case NotifyCollectionChangedAction.Replace:
					// e.NewStartingIndex == e.OldStartingIndex
					if ( e.NewStartingIndex < _startIndex || _startIndex + _size <= e.NewStartingIndex )
					{
						// Out of the range - no visible changes
						return;
					}

					OnPropertyChanged(IndexerName);
					OnCollectionChanged(() => new NotifyCollectionChangedEventArgs(e.Action, e.NewItems[0], e.OldItems[0], e.NewStartingIndex - _startIndex));

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

					OnCollectionChanged(() => {
						IList newItems;

						if ( e.NewStartingIndex + e.NewItems.Count <= _startIndex + _size )
						{
							newItems = e.NewItems;
						}
						else
						{
							int count = _startIndex + _size - e.NewStartingIndex;

							newItems = e.NewItems.Cast<T>().Take(count).ToList();
						}

						return new NotifyCollectionChangedEventArgs(e.Action, newItems, e.NewStartingIndex - _startIndex);
					});

					break;

				case NotifyCollectionChangedAction.Remove:
					if ( e.OldStartingIndex >= _startIndex + _size )
					{
						// After the range - no visible changes
						return;
					}
					else if ( e.OldStartingIndex < _startIndex || (_size == _maxCount && _baseCollection.Count > _startIndex + _size) )
					{
						// Before the range or inside the fully filled collection with items remaining to the right in the source collection - reset
						goto case NotifyCollectionChangedAction.Reset;
					}

					OnPropertyChanged(IndexerName);

					OnCollectionChanged(() => {
						IList oldItems;

						if ( e.OldStartingIndex + e.OldItems.Count <= _startIndex + _size )
						{
							oldItems = e.OldItems;
						}
						else
						{
							int count = _startIndex + _size - e.OldStartingIndex;

							oldItems = e.OldItems.Cast<T>().Take(count).ToList();
						}

						return new NotifyCollectionChangedEventArgs(e.Action, oldItems, e.OldStartingIndex - _startIndex);
					});

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

					OnCollectionChanged(() => {
						IList newItems;
						IList oldItems;
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

						return new NotifyCollectionChangedEventArgs(e.Action, newItems, oldItems, newStartingIndex);
					});

					break;
#endif
				case NotifyCollectionChangedAction.Reset:
					OnPropertyChanged(IndexerName);
					OnCollectionReset();

					break;

				default:
					throw new ArgumentException(e.Action.ToString(), "e");
			}
		}
		#endregion

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_size >= 0);
			Contract.Invariant(_size <= _maxCount);
		}
		#endregion
	}
}
