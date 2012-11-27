using System;
using System.Collections.Generic;
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

		[Fact]
		public void CountWillReturnZeroWithTotalElementsBelowStartIndex()
		{
			// Setup
			const int expectedResult = 0;
			var sut = new XSubRangeCollection<XObject>(CreateCollection(Many), Many, 1);

			// Exercise
			int result = sut.Count;

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void CountWillReturnNonZeroBelowMaxCountWithNotEnoughElements()
		{
			// Setup
			const int notEnough = TooMany;
			const int startIndex = TooMany - Many;
			const int maxCount = TooMany;
			const int expectedResult = notEnough - startIndex;
			var sut = new XSubRangeCollection<XObject>(CreateCollection(TooMany), startIndex, maxCount);

			// Exercise
			int result = sut.Count;

			// Verify
			Assert.InRange(result, 1, maxCount);
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void CountWillReturnMaxCountWithEnoughElements()
		{
			// Setup
			const int expectedResult = Many;
			var c = CreateCollection(TooMany);

			// Verify at left bound
			var sut = new XSubRangeCollection<XObject>(c, 0, Many);
			int result = sut.Count;
			Assert.Equal<int>(expectedResult, result);

			// Verify inside bounds
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many - 1, Many);
			result = sut.Count;
			Assert.Equal<int>(expectedResult, result);

			// Verify at right bound
			sut = new XSubRangeCollection<XObject>(c, TooMany - Many, Many);
			result = sut.Count;
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void ItemIndexerWillReturnCorrectlyAdjustedValue()
		{
			// Setup
			const int startIndex = 1;
			const int testIndex = 1;
			var c = CreateCollection(TooMany);
			XObject expectedResult = c[startIndex + testIndex];
			var sut = new XSubRangeCollection<XObject>(c, startIndex, Many);

			// Excercise
			XObject result = sut[testIndex];

			// Verify
			Assert.Same(expectedResult, result);
		}

		[Fact]
		public void ContainsItemWillReturnTrueForInnerElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject obj = c[2];

			// Excercise
			bool result = sut.Contains(obj);

			// Verify
			Assert.True(result);
		}

		[Fact]
		public void ContainsItemWillReturnFalseForBelowOuterElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject obj = c[0];

			// Excercise
			bool result = sut.Contains(obj);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsItemWillReturnFalseForBeyondOuterElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject obj = c[sut.Count];

			// Excercise
			bool result = sut.Contains(obj);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsKeyWillReturnTrueForInnerElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.True(result);
		}

		[Fact]
		public void ContainsKeyWillReturnFalseForBelowOuterElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void ContainsKeyWillReturnFalseForBeyondOuterElements()
		{
			// Setup
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise
			bool result = sut.ContainsKey(key);

			// Verify
			Assert.False(result);
		}

		[Fact]
		public void IndexOfItemWillReturnCorrectValueForInnerElements()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject obj = c[2];

			// Excercise
			int result = sut.IndexOf(obj);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void IndexOfItemWillReturnMinusOneForBelowOuterElements()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XObject obj = c[0];

			// Excercise
			int result = sut.IndexOf(obj);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void IndexOfItemWillReturnMinusOneForBeyondOuterElements()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XObject obj = c[sut.Count];

			// Excercise
			int result = sut.IndexOf(obj);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnCorrectValueForInnerElementsest()
		{
			// Setup
			const int expectedResult = 1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[2].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnMinusOneForBelowOuterElements()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 1, 2);
			XKey key = c[0].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		[Fact]
		public void IndexOfKeyWillReturnMinusOneForBeyondOuterElements()
		{
			// Setup
			const int expectedResult = -1;
			var c = CreateCollection(TooMany);
			var sut = new XSubRangeCollection<XObject>(c, 0, TooMany - 1);
			XKey key = c[sut.Count].GetKey();

			// Excercise
			int result = sut.IndexOf(key);

			// Verify
			Assert.Equal<int>(expectedResult, result);
		}

		private static IXList<XObject> CreateCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}
	}
}
