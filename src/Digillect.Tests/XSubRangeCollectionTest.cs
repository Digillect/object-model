using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

		#region Methods Tests
		[Fact]
		public void CountReturnsZeroWithTotalElementsBelowStartIndex()
		{
			// Setup
			const int expectedResult = 0;
			var sut = new XSubRangeCollection<XObject>(CreateSourceCollection(Many), Many, 1);

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
			var sut = new XSubRangeCollection<XObject>(CreateSourceCollection(TooMany), startIndex, maxCount);

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
			var c = CreateSourceCollection(TooMany);
			XSubRangeCollection<XObject> sut;

			// Verify at left bound
			sut = new XSubRangeCollection<XObject>(c, 0, Many);
			sut.Count.ShouldBe(expectedResult);

			// Verify inside bounds
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many - 1, Many);
			sut.Count.ShouldBe(expectedResult);

			// Verify at right bound
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many, Many);
			sut.Count.ShouldBe(expectedResult);
		}

		[Fact]
		public void ItemIndexerReturnsProperValue()
		{
			// Setup
			const int startIndex = 1;
			const int testIndex = 1;
			var c = CreateSourceCollection(TooMany);
			XObject expectedResult = c[startIndex + testIndex];
			var sut = new XSubRangeCollection<XObject>(c, startIndex, Many);

			// Excercise
			XObject result = sut[testIndex];

			// Verify
			result.ShouldBeSameAs(expectedResult);
		}

		[Fact]
		public void ContainsItemReturnsTrueForInnerItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[2];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(true);
		}

		[Fact]
		public void ContainsItemReturnsFalseForBelowOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[0];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact]
		public void ContainsItemReturnsFalseForBeyondOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject item = c[sut.Count];

			// Excercise

			// Verify
			sut.Contains(item).ShouldBe(false);
		}

		[Fact]
		public void ContainsKeyReturnsTrueForInnerItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(true);
		}

		[Fact]
		public void ContainsKeyReturnsFalseForBelowOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact]
		public void ContainsKeyReturnsFalseForBeyondOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise

			// Verify
			sut.ContainsKey(key).ShouldBe(false);
		}

		[Fact]
		public void CopyToCopiesAllElements()
		{
			// Setup
			var sut = CreateRangeCollection(CreateSourceCollection(TooMany), 1, Many);

			// Exercise
			XObject[] result = new XObject[TooMany];
			sut.CopyTo(result, 1);

			// Verify
			result[0].ShouldBe(null);
			result[sut.Count + 1].ShouldBe(null);
			Assert.Equal(result.Skip(1).Take(sut.Count), sut);
		}

		[Fact]
		public void IndexOfItemReturnsProperIndexForInnerItem()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[2];

			// Excercise

			// Verify
			sut.IndexOf(item).ShouldBe(expectedResult);
		}

		[Fact]
		public void IndexOfItemReturnsMinusOneForBelowOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[0];

			// Excercise

			// Verify
			sut.IndexOf(item).ShouldBe(expectedResult);
		}

		[Fact]
		public void IndexOfItemReturnsMinusOneForBeyondOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject item = c[sut.Count];

			// Excercise

			// Verify
			sut.IndexOf(item).ShouldBe(expectedResult);
		}

		[Fact]
		public void IndexOfKeyReturnsProperIndexForInnerItem()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise

			// Verify
			sut.IndexOf(key).ShouldBe(expectedResult);
		}

		[Fact]
		public void IndexOfKeyReturnsMinusOneForBelowOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise

			// Verify
			sut.IndexOf(key).ShouldBe(expectedResult);
		}

		[Fact]
		public void IndexOfKeyReturnsMinusOneForBeyondOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise

			// Verify
			sut.IndexOf(key).ShouldBe(expectedResult);
		}
		#endregion

		#region Event Handling Tests
		[Fact]
		public void CollectionChangedIssuesResetWhenItemAddedBelow()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			c.Insert(0, XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesResetWhentemAddedInsideFull()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			c.Insert(2, XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesAddWhenItemAddedInsideNonFull()
		{
			bool eventRaised = false;
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, TooMany, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Add);
				e.NewStartingIndex.ShouldBe(Many - 1);
				eventRaised = true;
			});
			int expectedCount = sut.Count + 1;

			// Exercise
			c.Add(XIntegerObject.Create());

			// Verify
			eventRaised.ShouldBe(true);
			sut.Count.ShouldBe(expectedCount);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemAddedBeyond()
		{
			// Setup
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			});

			// Exercise
			c.Add(XIntegerObject.Create());

			// Verify
		}

		[Fact]
		public void CollectionChangedIssuesResetWhenItemRemovedBelow()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			c.RemoveAt(0);

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedIssuesResetWhenItemRemovedInsideFull()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaised = true;
			});

			// Exercise
			c.RemoveAt(2);

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
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, startIndex, TooMany, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
				e.OldStartingIndex.ShouldBe(removeIndex - startIndex);
				eventRaised = true;
			});
			int expectedCount = sut.Count - 1;

			// Exercise
			c.RemoveAt(removeIndex);

			// Verify
			eventRaised.ShouldBe(true);
			sut.Count.ShouldBe(expectedCount);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemRemovedBeyond()
		{
			// Setup
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex);
			});

			// Exercise
			c.RemoveAt(2);

			// Verify
		}

		[Fact(Skip = "MoveItem implementation required", DisplayName = "Move item -> CollectionChanged(Reset)")]
		public void CollectionChangedIssuesResetWhenItemMoveAffectsInside()
		{
			// Setup
			int eventRaisedCount = 0;
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				eventRaisedCount++;
			});

			// Exercise
			//c.MoveItem(0, 1);
			//c.MoveItem(1, 3);
			//c.MoveItem(3, 4);

			// Verify
			eventRaisedCount.ShouldBe(3);
		}

		[Fact(Skip = "MoveItem implementation required", DisplayName = "Move item -> no CollectionChanged")]
		public void CollectionChangedShouldNotBeRaisedWhenItemMovedOutside()
		{
			// Setup
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.OldStartingIndex + " and " + e.NewStartingIndex);
			});

			// Exercise
			//c.MoveItem(0, 2);

			// Verify
		}

		[Fact]
		public void CollectionChangedIssuesReplaceWhenItemReplacedInside()
		{
			// Setup
			const int startIndex = 1;
			const int replaceIndex = 2;
			bool eventRaised = false;
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				e.Action.ShouldBe(NotifyCollectionChangedAction.Replace);
				e.NewStartingIndex.ShouldBe(replaceIndex - startIndex);
				eventRaised = true;
			});

			// Exercise
			c[replaceIndex] = XIntegerObject.Create();

			// Verify
			eventRaised.ShouldBe(true);
		}

		[Fact]
		public void CollectionChangedShouldNotBeRaisedWhenItemReplacedOutside()
		{
			// Setup
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				Assert.False(true, "CollectionChanged at " + e.NewStartingIndex);
			});

			// Exercise
			c[0] = XIntegerObject.Create();
			c[2] = XIntegerObject.Create();

			// Verify
		}
		#endregion

		#region Setup
		private static IXList<XObject> CreateSourceCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}

		private static IXList<XObject> CreateRangeCollection(IXList<XObject> source, int startIndex, int maxCount, NotifyCollectionChangedEventHandler changedHandler = null)
		{
			var result = new XSubRangeCollection<XObject>(source, startIndex, maxCount);

			if ( changedHandler != null )
			{
				result.CollectionChanged += changedHandler;
			}

			return result;
		}
		#endregion
	}
}
