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

		#region Delayed Collection Assignment Test
		[Fact(DisplayName = "SetUnderlyingCollection should raise CollectionChanged(Reset) and Updated events")]
		public void SetUnderlyingCollection_should_raise_CollectionChangedReset_and_Updated_events()
		{
			// Setup
			var sut = new XSubRangeCollection<XObject>(1, 1);

			bool expectedCollectionChanged = false;
			bool expectedUpdated = false;

			sut.CollectionChanged += (sender, e) => {
				expectedCollectionChanged.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			};

			sut.Updated += (sender, e) => {
				expectedUpdated.ShouldBe(false);
				expectedUpdated = true;
			};

			// Exercise
			sut.UnderlyingCollection = CreateSourceCollection(Many);

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedUpdated.ShouldBe(true);
		}
		#endregion

		#region Properties Tests
		[Fact]
		public void CountReturnsZeroWithTotalElementsBelowStartIndex()
		{
			// Setup
			const int expectedResult = 0;
			var sut = CreateTestCollection(Many, Many, 1);

			// Exercise

			// Verify
			sut.Count.ShouldBe(expectedResult);
		}

		[Fact]
		public void CountReturnsNonZeroBelowMaxCountWithNotEnoughElements()
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

		[Fact]
		public void CountReturnsMaxCountWithEnoughElements()
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

		[Fact]
		public void ItemIndexerReturnsProperValue()
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
		[Fact(DisplayName = "BeginUpdate should block events")]
		public void BeginUpdate_should_block_events()
		{
			// Setup
			bool eventsBlocked = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(eventsBlocked, "CollectionChanged with " + e.Action);
			}, (sender, e) => {
				Assert.False(eventsBlocked, "Updated");
			}, (sender, e) => {
				Assert.False(eventsBlocked, "PropertyChanged with " + e.PropertyName);
			});

			// Exercise
			sut.BeginUpdate();
			eventsBlocked = true;
			sut.UnderlyingCollection.Update(CreateSourceCollection(TooMany));
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());
			eventsBlocked = false;
			sut.EndUpdate();

			// Verify
		}

		[Fact]
		public void ContainsItemReturnsTrueForInnerItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[2];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(true);
		}

		[Fact]
		public void ContainsItemReturnsFalseForBelowOuterItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XObject item = sut.UnderlyingCollection[0];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact]
		public void ContainsItemReturnsFalseForBeyondOuterItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XObject item = sut.UnderlyingCollection[sut.Count];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact]
		public void ContainsKeyReturnsTrueForInnerItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[2].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(true);
		}

		[Fact]
		public void ContainsKeyReturnsFalseForBelowOuterItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, 2);
			XKey key = sut.UnderlyingCollection[0].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact]
		public void ContainsKeyReturnsFalseForBeyondOuterItem()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 0, TooMany - 1);
			XKey key = sut.UnderlyingCollection[sut.Count].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact(DisplayName = "CopyTo should copy all items")]
		public void CopyTo_should_copy_all_items()
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

		[Fact(DisplayName = "GetEnumerator enumerating should fail when underlying collection has changed")]
		public void GetEnumerator_enumerating_should_fail_when_underlying_collection_has_changed()
		{
			// Setup
			var sut = CreateTestCollection(TooMany, 1, Many);
			IEnumerator<XObject> result;

			// Exercise
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());

			// Verify
			Should.Throw<InvalidOperationException>(delegate { result.MoveNext(); });

			// Exercise
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Insert(2, XIntegerObject.Create());

			// Verify
			Should.Throw<InvalidOperationException>(delegate { result.MoveNext(); });

			// Exercise
			result = sut.GetEnumerator();
			result.MoveNext().ShouldBe(true);
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify
			Should.Throw<InvalidOperationException>(delegate { result.MoveNext(); });
		}

		[Fact]
		public void IndexOfItemReturnsProperIndexForInnerItem()
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

		[Fact]
		public void IndexOfItemReturnsMinusOneForBelowOuterItem()
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

		[Fact]
		public void IndexOfItemReturnsMinusOneForBeyondOuterItem()
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

		[Fact]
		public void IndexOfKeyReturnsProperIndexForInnerItem()
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

		[Fact]
		public void IndexOfKeyReturnsMinusOneForBelowOuterItem()
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

		[Fact]
		public void IndexOfKeyReturnsMinusOneForBeyondOuterItem()
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
		[Fact]
		public void CollectionChangedIssuesResetWhenItemAddedBelow()
		{
			// Setup
			bool eventRaised = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection.Insert(0, XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesResetWhentemAddedInsideFull()
		{
			// Setup
			bool eventRaised = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection.Insert(2, XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesAddWhenItemAddedInsideNonFull()
		{
			bool eventRaised = false;
			var sut = CreateTestCollection(Many, 1, TooMany, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Add);
				e.NewStartingIndex.ShouldBe(Many - 1);
				eventRaised = true;
			});
			int expectedCount = sut.Count + 1;

			// Exercise
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
			sut.Count.ShouldBe(expectedCount);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemAddedBeyond()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			});

			// Exercise
			sut.UnderlyingCollection.Add(XIntegerObject.Create());

			// Verify
		}

		[Fact]
		public void CollectionChangedIssuesResetWhenItemRemovedBelow()
		{
			// Setup
			bool eventRaised = false;
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(0);

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesResetWhenItemRemovedInsideFull()
		{
			// Setup
			bool eventRaised = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(2);

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesRemoveWhenItemRemovedInsideNonFull()
		{
			// Setup
			const int startIndex = 1;
			const int removeIndex = 1;
			bool eventRaised = false;
			var sut = CreateTestCollection(Many, startIndex, TooMany, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
				e.OldStartingIndex.ShouldBe(removeIndex - startIndex);
				eventRaised = true;
			});
			int expectedCount = sut.Count - 1;

			// Exercise
			sut.UnderlyingCollection.RemoveAt(removeIndex);

			// Verify
			eventRaised.ShouldBe(true);
			sut.Count.ShouldBe(expectedCount);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemRemovedBeyond()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex);
			});

			// Exercise
			sut.UnderlyingCollection.RemoveAt(2);

			// Verify
		}

		[Fact]
		public void CollectionChangedIssuesResetWhenItemMoveAffectsInside()
		{
			// Setup
			int eventRaisedCount = 0;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaisedCount++;
			});

			// Exercise
			((XCollection<XObject>) sut.UnderlyingCollection).Move(0, 1);
			((XCollection<XObject>) sut.UnderlyingCollection).Move(1, 3);
			((XCollection<XObject>) sut.UnderlyingCollection).Move(3, 4);

			// Verify
			eventRaisedCount.ShouldBe(3);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemMovedOutside()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex + " and " + e.NewStartingIndex);
			});

			// Exercise
			((XCollection<XObject>) sut.UnderlyingCollection).Move(0, 2);

			// Verify
		}

		[Fact]
		public void CollectionChangedIssuesReplaceWhenItemReplacedInside()
		{
			// Setup
			const int startIndex = 1;
			const int replaceIndex = 2;
			bool eventRaised = false;
			var sut = CreateTestCollection(TooMany, 1, Many, (sender, e) => {
				eventRaised.ShouldBe(false);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Replace);
				e.NewStartingIndex.ShouldBe(replaceIndex - startIndex);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection[replaceIndex] = XIntegerObject.Create();

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemReplacedOutside()
		{
			// Setup
			var sut = CreateTestCollection(Many, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			});

			// Exercise
			sut.UnderlyingCollection[0] = XIntegerObject.Create();
			sut.UnderlyingCollection[2] = XIntegerObject.Create();

			// Verify
		}

		[Fact(DisplayName = "Updated event should be raised when underlying collection gets updated")]
		public void Updated_event_should_be_raised_when_underlying_collection_gets_updated()
		{
			// Setup
			bool eventRaised = false;
			var sut = CreateTestCollection(Many, 1, 1, updatedHandler: (sender, e) => {
				eventRaised.ShouldBe(false);
				eventRaised = true;
			});

			// Exercise
			sut.UnderlyingCollection.Update(CreateSourceCollection(TooMany));

			// Verify
			eventRaised.ShouldBe(true);
		}
		#endregion

		#region Setup
		private static IXList<XObject> CreateSourceCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}

		private static XSubRangeCollection<XObject> CreateTestCollection(int sourceCount, int startIndex, int maxCount, NotifyCollectionChangedEventHandler changedHandler = null, EventHandler updatedHandler = null, PropertyChangedEventHandler propertyChangedHandler = null)
		{
			var result = new XSubRangeCollection<XObject>(CreateSourceCollection(sourceCount), startIndex, maxCount);

			if ( changedHandler != null )
			{
				result.CollectionChanged += changedHandler;
			}

			if ( updatedHandler != null )
			{
				result.Updated += updatedHandler;
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
