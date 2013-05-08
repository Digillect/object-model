#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Shouldly;

using Xunit;

using Digillect.Collections;

namespace Digillect.Tests
{
	public class XSubRangeCollectionTest
	{
		private const int Many = 3;
		private const int TooMany = 6;
		private const string IndexerName = "Item[]";

		#region Delayed Collection Assignment Test
		[Fact(DisplayName = "XSRC.UC.set should raise CollectionChanged(Reset) and PropertyChanged(null) events")]
		public void ChangeUnderlyingCollectionEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = new XSubRangeCollection<XObject>(1, 1);

			sut.CollectionChanged += (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			};

			sut.PropertyChanged += (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			};

			// Exercise
			sut.UnderlyingCollection = CreateSourceCollection(Many);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}
		#endregion

		#region Properties Tests
		[Fact(DisplayName = "XSRC.Count should return zero with total elements below start index")]
		public void CountWithTotalElementsBelowStartIndexTest()
		{
			// Setup
			const int expectedResult = 0;
			var sut = CreateTestCollection(Many, Many, 1);

			// Exercise

			// Verify
			sut.Count.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.Count should return non-zero below max count with not enough elements")]
		public void CountWithNotEnoughElementsTest()
		{
			// Setup
			const int notEnough = TooMany;
			const int startIndex = TooMany - Many;
			const int maxCount = TooMany;
			const int expectedResult = notEnough - startIndex;
			var sut = CreateTestCollection(TooMany, startIndex, maxCount);

			// Exercise

			// Verify
			sut.Count.ShouldBeGreaterThan(0);
			sut.Count.ShouldBeLessThan(maxCount + 1);
			sut.Count.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.Count should return max count with enough elements")]
		public void CountWithEnoughElementstest()
		{
			// Setup
			const int expectedResult = Many;
			XSubRangeCollection<XObject> sut;

			// Verify at left bound
			sut = CreateTestCollection(TooMany, 0, Many);
			sut.Count.ShouldBe(expectedResult);

			// Verify inside bounds
			sut = CreateTestCollection(TooMany, TooMany - Many - 1, Many);
			sut.Count.ShouldBe(expectedResult);

			// Verify at right bound
			sut = CreateTestCollection(TooMany, TooMany - Many, Many);
			sut.Count.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.Item[] should return proper value")]
		public void ItemIndexerTest()
		{
			// Setup
			const int startIndex = 1;
			const int testIndex = 1;
			var sut = CreateTestCollection(TooMany, startIndex, Many);
			XObject expectedResult = sut.UnderlyingCollection[startIndex + testIndex];

			// Excercise
			XObject result = sut[testIndex];

			// Verify
			result.ShouldBeSameAs(expectedResult);
		}
		#endregion

		#region Methods Tests
		[Fact(DisplayName = "XSRC.BeginUpdate should block events and raise them after the EndUpdate call")]
		public void BeginEndUpdateEventsTest()
		{
			// Setup
			bool eventsBlocked = false;
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(eventsBlocked, "CollectionChanged with " + e.Action);
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				Assert.False(eventsBlocked, "PropertyChanged with " + e.PropertyName);
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.BeginUpdate();
			eventsBlocked = true;
			sut.UnderlyingCollection.Update(CreateSourceCollection(TooMany));
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());
			sut.UnderlyingCollection.RemoveAt(0);
			eventsBlocked = false;
			sut.EndUpdate();

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.Contains should return true for inner item")]
		public void ContainsItemInnerItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[2];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.Contains should return false for below outer item")]
		public void ContainsItemBelowOuterItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[0];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact(DisplayName = "XSRC.Contains should return false for beyond outer item")]
		public void ContainsItemBeyondOuterItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XObject item = sut.UnderlyingCollection[sut.Count];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact(DisplayName = "XSRC.ContainsKey should return true for inner item")]
		public void ContainsKeyInnerItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[2].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.ContainsKey should return false for below outer item")]
		public void ContainsKeyBelowOuterItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[0].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact(DisplayName = "XSRC.ContainsKey should return false for beyond outer item")]
		public void ContainsKeyBeyondOuterItemTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XKey key = sut.UnderlyingCollection[sut.Count].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact(DisplayName = "XSRC.CopyTo should copy all items")]
		public void CopyToTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, Many);

			// Exercise
			XObject[] result = new XObject[TooMany];
			sut.CopyTo(result, 1);

			// Verify
			result[0].ShouldBe(null);
			result[sut.Count + 1].ShouldBe(null);
			Assert.Equal(result.Skip(1).Take(sut.Count), sut);
		}

		[Fact(DisplayName = "XSRC.GetEnumerator should fail enumerating when underlying collection has changed")]
		public void GetEnumeratorEnumeratingWhileChangeTest()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, Many);
			IEnumerator<XObject> result;

			// Exercise
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());

			// Verify
			Should.Throw<InvalidOperationException>(() => result.MoveNext());

			// Exercise 2
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Insert(2, XIntegerObject.Create());

			// Verify 2
			Should.Throw<InvalidOperationException>(() => result.MoveNext());

			// Exercise 3
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify 3
			Should.Throw<InvalidOperationException>(() => result.MoveNext());

			// Exercise 4
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.RemoveAt(0);

			// Verify 4
			Should.Throw<InvalidOperationException>(() => result.MoveNext());
		}

		[Fact(DisplayName = "XSRC.IndexOf(T) should return proper index for inner item")]
		public void IndexOfItemInnerItemTest()
		{
			// Setup
			const int expectedResult = 1;
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[2];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			result.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.IndexOf(T) should return -1 for below outer item")]
		public void IndexOfItemBelowOuterItemTest()
		{
			// Setup
			const int expectedResult = -1;
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[0];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			result.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.IndexOf(T) should return -1 for beyond outer item")]
		public void IndexOfItemBeyondOuterItemTest()
		{
			// Setup
			const int expectedResult = -1;
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XObject item = sut.UnderlyingCollection[sut.Count];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			result.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.IndexOf(XKey) should return proper index for inner item")]
		public void IndexOfKeyInnerItemTest()
		{
			// Setup
			const int expectedResult = 1;
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[2].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			result.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.IndexOf(XKey) should return -1 for below outer item")]
		public void IndexOfKeyBelowOuterItemTest()
		{
			// Setup
			const int expectedResult = -1;
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[0].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			result.ShouldBe(expectedResult);
		}

		[Fact(DisplayName = "XSRC.IndexOf(XKey) should return -1 for beyond outer item")]
		public void IndexOfKeyBeyondOuterItemTest()
		{
			// Setup
			const int expectedResult = -1;
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XKey key = sut.UnderlyingCollection[sut.Count].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			result.ShouldBe(expectedResult);
		}
		#endregion

		#region Event Handling Tests
		[Fact(DisplayName = "XSRC.UC.Insert should raise CollectionChanged(Reset) when item added below")]
		public void InsertItemBelowEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.UC.Insert should raise CollectionChanged(Reset) when item added inside full")]
		public void InsertItemInsideFullEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.UnderlyingCollection.Insert(2, XIntegerObject.Create());

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.UC.Add should raise CollectionChanged(Add) when item added inside non-full")]
		public void AddItemInsideNonFullEventsTest()
		{
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many, 1, TooMany, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Add);
				e.NewStartingIndex.ShouldBe(Many - 1);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				e.PropertyName.ShouldNotBe(null);
				expectedPropertyChanged++;
			});
			int expectedCount = sut.Count + 1;

			// Exercise
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify
			sut.Count.ShouldBe(expectedCount);
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(2);
		}

		[Fact(DisplayName = "XSRC.UC.Add should not raise events when item added beyond")]
		public void AddItemBeyondEventsTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged with " + e.PropertyName);
			});

			// Exercise
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify
		}

		[Fact(DisplayName = "XSRC.UC.Remove should raise CollectionChanged(Reset) when item removed below")]
		public void RemoveItemBelowEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(0);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.UC.Remove should raise CollectionChanged(Reset) when item removed inside full with spare elements")]
		public void RemoveItemInsideFullWithSpareEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(2);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.UC.Remove should raise CollectionChanged(Reset) when item removed inside full without spare elements")]
		public void RemoveItemInsideFullWithoutSpareEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many + 1, 1, Many, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				e.PropertyName.ShouldNotBe(null);
				expectedPropertyChanged++;
			});
			int expectedCount = sut.Count - 1;

			// Exercise
			sut.UnderlyingCollection.RemoveAt(2);

			// Verify
			sut.Count.ShouldBe(expectedCount);
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(2);
		}

		[Fact(DisplayName = "XSRC.UC.Remove should raise CollectionChanged(Remove) when item removed inside non-full")]
		public void RemoveItemInsideNonFullEventsTest()
		{
			// Setup
			const int startIndex = 1;
			const int removeIndex = 1;
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many, startIndex, TooMany, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
				e.OldStartingIndex.ShouldBe(removeIndex - startIndex);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				e.PropertyName.ShouldNotBe(null);
				expectedPropertyChanged++;
			});
			int expectedCount = sut.Count - 1;

			// Exercise
			sut.UnderlyingCollection.RemoveAt(removeIndex);

			// Verify
			sut.Count.ShouldBe(expectedCount);
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(2);
		}

		[Fact(DisplayName = "XSRC.UC.Remove should not raise events when item removed beyond")]
		public void RemoveItemBeyondEventsTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged with " + e.PropertyName);
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(2);

			// Verify
		}

		[Fact(DisplayName = "XSRC.UC.Move should raise CollectionChanged(Reset) when item move affects inside")]
		public void MoveItemAffectsInsideEventsTest()
		{
			// Setup
			var expectedCollectionChanged = 0;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged++;
			}, (sender, e) => {
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged++;
			});

			// Exercise
			((XCollection<XObject>) sut.UnderlyingCollection).Move(0, 1);
			((XCollection<XObject>) sut.UnderlyingCollection).Move(1, 3);
			((XCollection<XObject>) sut.UnderlyingCollection).Move(3, 4);

			// Verify
			expectedCollectionChanged.ShouldBe(3);
			expectedPropertyChanged.ShouldBe(3);
		}

		[Fact(DisplayName = "XSRC.UC.Move should not raise events when item moved outside")]
		public void MoveItemOutsideEventsTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex + " and " + e.NewStartingIndex);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged with " + e.PropertyName);
			});

			// Exercise
			((XCollection<XObject>) sut.UnderlyingCollection).Move(0, 2);

			// Verify
		}

		[Fact(DisplayName = "XSRC.UC.Item[].set should raise CollectionChanged(Replace) when item replaced inside")]
		public void ReplaceItemInsideEventsTest()
		{
			// Setup
			const int startIndex = 1;
			const int replaceIndex = 2;
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Replace);
				e.NewStartingIndex.ShouldBe(replaceIndex - startIndex);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(IndexerName);
				expectedPropertyChanged = true;
			});

			// Exercise
			sut.UnderlyingCollection[replaceIndex] = XIntegerObject.Create();

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XSRC.UC.Item[].set should not raise events when item replaced outside")]
		public void ReplaceItemOutsideEventsTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged with " + e.PropertyName);
			});

			// Exercise
			sut.UnderlyingCollection[0] = XIntegerObject.Create();
			sut.UnderlyingCollection[2] = XIntegerObject.Create();

			// Verify
		}

		[Fact(DisplayName = "XSRC.UC.Update should raise events")]
		public void UnderlyingCollectionUpdateEventsTest()
		{
			// Setup
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = 0;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			}, (sender, e) => {
				e.PropertyName.ShouldNotBe(null);
				expectedPropertyChanged++;
			});

			// Exercise
			sut.UnderlyingCollection.Update(CreateSourceCollection(TooMany));

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBeGreaterThanOrEqualTo(1);
		}
		#endregion

		#region Setup
		private static IXList<XObject> CreateSourceCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}

		private static XSubRangeCollection<XObject> CreateTestCollection(int sourceCount, int startIndex, int maxCount, NotifyCollectionChangedEventHandler changedHandler = null, PropertyChangedEventHandler propertyChangedHandler = null)
		{
			var result = new XSubRangeCollection<XObject>(CreateSourceCollection(sourceCount), startIndex, maxCount);

			if ( changedHandler != null )
			{
				result.CollectionChanged += changedHandler;
			}

			if ( propertyChangedHandler != null )
			{
				result.PropertyChanged += propertyChangedHandler;
			}

			return result;
		}
		#endregion
	}
}
