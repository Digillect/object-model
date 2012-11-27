using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

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
		public void CountWillReturnZeroWithTotalElementsBelowStartIndex()
		{
			// Setup
			const int expectedResult = 0;
			var sut = new XSubRangeCollection<XObject>(CreateSourceCollection(Many), Many, 1);

			// Exercise
			int result = sut.Count;

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void CountWillReturnNonZeroBelowMaxCountWithNotEnoughElements()
		{
			// Setup
			const int notEnough = TooMany;
			const int startIndex = TooMany - Many;
			const int maxCount = TooMany;
			const int expectedResult = notEnough - startIndex;
			var sut = new XSubRangeCollection<XObject>(CreateSourceCollection(TooMany), startIndex, maxCount);

			// Exercise
			int result = sut.Count;

			// Verify
			Assert.InRange(result, 1, maxCount);
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void CountWillReturnMaxCountWithEnoughElements()
		{
			// Setup
			const int expectedResult = Many;
			var c = CreateSourceCollection(TooMany);

			// Verify at left bound
			var sut = new XSubRangeCollection<XObject>(c, 0, Many);
			int result = sut.Count;
			Assert.Equal(expectedResult, result);

			// Verify inside bounds
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many - 1, Many);
			result = sut.Count;
			Assert.Equal(expectedResult, result);

			// Verify at right bound
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many, Many);
			result = sut.Count;
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void ItemIndexerWillReturnCorrectlyAdjustedValue()
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
			Assert.Same(expectedResult, result);
		}

		[Fact]
		public void ContainsItemWillReturnTrueForInnerItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[2];

			// Excercise
			bool result = sut.Contains(item);

			// Verify
			Assert.True(result);
		}

		[Fact]
		public void ContainsItemWillReturnFalseForBelowOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[0];

			// Excercise
			bool result = sut.Contains(item);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsItemWillReturnFalseForBeyondOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject item = c[sut.Count];

			// Excercise
			bool result = sut.Contains(item);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsKeyWillReturnTrueForInnerItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.True(result);
		}

		[Fact]
		public void ContainsKeyWillReturnFalseForBelowOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsKeyWillReturnFalseForBeyondOuterItem()
		{
			// Setup
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void IndexOfItemWillReturnCorrectValueForInnerItem()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[2];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void IndexOfItemWillReturnMinusOneForBelowOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject item = c[0];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void IndexOfItemWillReturnMinusOneForBeyondOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject item = c[sut.Count];

			// Excercise
			int result = sut.IndexOf(item);

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnCorrectValueForInnerItem()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnMinusOneForBelowOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnMinusOneForBeyondOuterItem()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateSourceCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal(expectedResult, result);
		}
		#endregion

		#region Event Handling Tests
		[Fact]
		public void AddItemBelowWillRaiseReset()
		{
			// Setup
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Reset;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			c.Insert(0, XIntegerObject.Create());

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void AddItemBeyondWillRaiseNothing()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				eventRaised = true;
			});

			// Exercise
			c.Add(XIntegerObject.Create());

			// Verify
			Assert.False(eventRaised);
		}

		[Fact]
		public void AddItemInsideFullWillRaiseReset()
		{
			// Setup
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Reset;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			c.Insert(2, XIntegerObject.Create());

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void AddItemInsideNonFullWillRaiseAdd()
		{
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Add;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, TooMany, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			int oldCount = sut.Count;
			c.Add(XIntegerObject.Create());

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
			Assert.Equal(1, sut.Count - oldCount);
		}

		[Fact]
		public void RemoveItemBelowWillRaiseReset()
		{
			// Setup
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Reset;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			c.RemoveAt(0);

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void RemoveItemBeyondWillRaiseNothing()
		{
			// Setup
			bool eventRaised = false;
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, 1, (sender, e) => {
				eventRaised = true;
			});

			// Exercise
			c.RemoveAt(c.Count - 1);

			// Verify
			Assert.False(eventRaised);
		}

		[Fact]
		public void RemoveItemInsideFullWillRaiseReset()
		{
			// Setup
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Reset;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(TooMany);
			var sut = CreateRangeCollection(c, 1, Many, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			c.RemoveAt(2);

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
		}

		[Fact]
		public void RemoveItemInsideNonFullWillRaiseRemove()
		{
			const NotifyCollectionChangedAction expectedResult = NotifyCollectionChangedAction.Remove;
			bool eventRaised = false;
			NotifyCollectionChangedAction result = (NotifyCollectionChangedAction) (-1);
			var c = CreateSourceCollection(Many);
			var sut = CreateRangeCollection(c, 1, TooMany, (sender, e) => {
				eventRaised = true;
				result = e.Action;
			});

			// Exercise
			int oldCount = sut.Count;
			c.RemoveAt(1);

			// Verify
			Assert.True(eventRaised);
			Assert.Equal(expectedResult, result);
			Assert.Equal(-1, sut.Count - oldCount);
		}
		#endregion

		#region Setup
		private static IXList<XObject> CreateSourceCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}

		private static IXList<XObject> CreateRangeCollection(IXList<XObject> source, int startIndex, int maxCount, NotifyCollectionChangedEventHandler changedHandler)
		{
			var result = new XSubRangeCollection<XObject>(source, startIndex, maxCount);

			result.CollectionChanged += changedHandler;

			return result;
		}
		#endregion
	}
}
