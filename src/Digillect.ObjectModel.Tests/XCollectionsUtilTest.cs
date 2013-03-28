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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Digillect.Collections;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	/// <summary>
	/// This is a test class for XCollectionsUtil and is intended to contain all XCollectionsUtil Unit Tests
	///</summary>
	public class XCollectionsUtilTest
	{
		private const int Many = 3;

		#region IsNullOrEmpty Tests
		/// <summary>
		/// A test for IsNullOrEmpty(ICollection)
		///</summary>
		[Fact(DisplayName = "XCU.IsNullOrEmpty should return true when ICollection is null or empty")]
		public void IsNullOrEmptyICollectionTest()
		{
			IList sut = null;

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			sut = new ArrayList();

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			sut.Add(null);

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(false);
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1(ICollection`1)
		///</summary>
		[Fact(DisplayName = "XCU.IsNullOrEmpty should return true when ICollection`1 is null or empty")]
		public void IsNullOrEmptyICollection1Test()
		{
			ICollection<object> sut = null;

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			sut = new List<object>();

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			sut.Add(null);

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(false);
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1(IReadOnlyCollection`1)
		///</summary>
		[Fact(DisplayName = "XCU.IsNullOrEmpty should return true when IReadOnlyCollection`1 is null or empty")]
		public void IsNullOrEmptyIReadOnlyCollection1Test()
		{
			IReadOnlyCollection<object> sut = null;

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			var list = new List<object>();
			sut = list.AsReadOnly();

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(true);

			list.Add(null);

			XCollectionsUtil.IsNullOrEmpty(sut).ShouldBe(false);
		}
		#endregion

		#region Merge Tests
		#region Standard Tests
		[Fact(DisplayName = "XCU.Merge should add new nulls but not remove existing")]
		public void MergeAddButNotRemoveNullsTest()
		{
			var rnd = new Random(Environment.TickCount);
			var data = new XObject[rnd.Next(1, 10)];
			var other = new XObject[rnd.Next(11, 20)];
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.AddNew);

			// Verify
			result.Added.ShouldBe(other.Length);
			result.Removed.ShouldBe(0);
			result.Updated.ShouldBe(0);
			sut.Count.ShouldBe(data.Length + other.Length);
		}

		[Fact(DisplayName = "XCU.Merge should not add new nulls but remove existing")]
		public void MergeNotAddButRemoveNullsTest()
		{
			var rnd = new Random(Environment.TickCount);
			var data = new XObject[rnd.Next(1, 10)];
			var other = new XObject[rnd.Next(11, 20)];
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.RemoveOld);

			// Verify
			result.Added.ShouldBe(0);
			result.Removed.ShouldBe(data.Length);
			result.Updated.ShouldBe(0);
			sut.Count.ShouldBe(0);
		}

		[Fact(DisplayName = "XCU.Merge should not change anything when options are None")]
		public void MergeNoChangeTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(XIntegerObject.CreateSeries(Many), CollectionMergeOptions.None);

			// Verify
			result.IsEmpty.ShouldBe(true);
			sut.SequenceEqual(data).ShouldBe(true);
		}

		[Fact(DisplayName = "XCU.Merge should only add new items")]
		public void MergeAddNewItemsTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], null, objs[1], null, objs[2], XIntegerObject.Create() };
			var other = new[] { XIntegerObject.Create(), null, objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), null, XIntegerObject.Create() };
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.AddNew);

			// Verify
			result.Added.ShouldBeGreaterThan(0);
			result.Removed.ShouldBe(0);
			result.Updated.ShouldBe(0);
			sut.Count.ShouldBeGreaterThan(other.Length);
			sut.ShouldContain(x => other.Contains(x));
		}

		[Fact(DisplayName = "XCU.Merge should only remove old items")]
		public void MergeRemoveOldItemsTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], objs[1], objs[2], XIntegerObject.Create() };
			var other = new[] { XIntegerObject.Create(), objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), XIntegerObject.Create() };
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.RemoveOld);

			// Verify
			result.Added.ShouldBe(0);
			result.Removed.ShouldBeGreaterThan(0);
			result.Updated.ShouldBe(0);
			sut.Count.ShouldBeLessThan(other.Length);
			sut.ShouldNotContain(x => data.Except(other).Contains(x));
			sut.ShouldNotContain((XObject) null);
		}

		[Fact(DisplayName = "XCU.Merge should only update existing items")]
		public void MergeUpdateExistingItemsTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], objs[1], objs[2], XIntegerObject.Create() };
			var other = new[] { XIntegerObject.Create(), objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), XIntegerObject.Create() };
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.UpdateExisting);

			// Verify
			result.Added.ShouldBe(0);
			result.Removed.ShouldBe(0);
			result.Updated.ShouldBeGreaterThan(0);
			sut.Count.ShouldBe(data.Length);
			sut.ShouldContain(x => other.Intersect(data).Contains(x));
		}

		[Fact(DisplayName = "XCU.Merge should add, remove and update")]
		public void MergeAddRemoveUpdateTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], objs[1], objs[2], XIntegerObject.Create() };
			var other = new[] { XIntegerObject.Create(), objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), XIntegerObject.Create() };
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.AddNew | CollectionMergeOptions.RemoveOld | CollectionMergeOptions.UpdateExisting);

			// Verify
			result.Added.ShouldBeGreaterThan(0);
			result.Removed.ShouldBeGreaterThan(0);
			result.Updated.ShouldBeGreaterThan(0);
			sut.ShouldContain(x => other.Contains(x));
			other.ShouldContain(x => sut.Contains(x));
		}

		[Fact(DisplayName = "XCU.Merge should do full merge")]
		public void MergeFullTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], null, objs[1], null, objs[2], XIntegerObject.Create() };
			var other = new[] { XIntegerObject.Create(), null, objs[2].Clone(), null, objs[1].Clone(), null, objs[0].Clone(), null, XIntegerObject.Create() };
			var sut = new List<XObject>(data);

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.Full);

			// Verify
			result.Added.ShouldBeGreaterThan(0);
			result.Removed.ShouldBeGreaterThan(0);
			result.Updated.ShouldBeGreaterThan(0);
			sut.SequenceEqual(other).ShouldBe(true);
		}
		#endregion

		#region Special Cases
		[Fact(DisplayName = "XCU.Merge should not perform excessive updates when the other collection contains duplicates")]
		public void MergeNoExcessiveUpdatesWithDuplicatesTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(objs);
			var other = objs.Select(x => x.Clone(true));

			// Exercise
			var result = sut.Merge(other.Concat(objs).Concat(other), CollectionMergeOptions.Full);

			// Verify
			result.Added.ShouldBe(Many * 2);
			result.Removed.ShouldBe(0);
			result.Updated.ShouldBe(Many);
		}

		[Fact(DisplayName = "XCU.Merge should differentiate objects with the same keys but of different type")]
		public void MergeXKeyWeaknessTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(objs);
			var other = ((IEnumerable<XObject>) objs.Select(x => new XIntegerObject2(x.Id)).Reverse()).Concat(Enumerable.Repeat(objs[0].Clone(), 1));

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.Full);

			// Verify
			result.Added.ShouldBe(Many);
			result.Removed.ShouldBe(Many - 1);
			result.Updated.ShouldBe(1);
			sut.SequenceEqual(other).ShouldBe(true);
		}
		#endregion
		#endregion

		#region RemoveAll Tests
		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,Func`2)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Predicate) should return false when nothing removed")]
		public void RemoveAllByPredicateNotModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			ICollection<XObject> sut = new List<XObject>(data);

			// Exercise
			bool result = sut.RemoveAll(x => false);

			// Verify
			result.ShouldBe(false);
			sut.SequenceEqual(data).ShouldBe(true);
		}

		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,Func`2)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Predicate) should return true when something removed")]
		public void RemoveAllByPredicateModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			ICollection<XObject> sut = new List<XObject>(data);
			var toRemove = new[] { data[0].Clone(), data[Many - 1].Clone() };

			// Exercise
			bool result = sut.RemoveAll(x => toRemove.Contains(x));

			// Verify
			result.ShouldBe(true);
			sut.Count.ShouldBeLessThan(data.Length);
			sut.ShouldNotContain(x => toRemove.Contains(x));
		}

		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,IEnumerable`1)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Object) should return false when nothing removed")]
		public void RemoveAllByObjectNotModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(data);

			// Exercise
			bool result = sut.RemoveAll(XIntegerObject.CreateSeries(Many));

			// Verify
			result.ShouldBe(false);
			sut.SequenceEqual(data).ShouldBe(true);
		}

		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,IEnumerable`1)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Object) should return true when something removed")]
		public void RemoveAllByObjectModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(data.Concat(data));
			var toRemove = new[] { data[0].Clone(), data[Many - 1].Clone() };

			// Exercise
			bool result = sut.RemoveAll(toRemove);

			// Verify
			result.ShouldBe(true);
			sut.Count.ShouldBe((data.Length - toRemove.Length) * 2);
			sut.ShouldNotContain(x => toRemove.Contains(x));
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,IEnumerable`1)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Key) should return false when nothing removed")]
		public void RemoveAllByKeyNotModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new XCollection<XObject>(data);

			// Exercise
			bool result = sut.RemoveAll(XIntegerObject.NewId(Many).Select(x => XKey.From(XKey.IdKeyName, x)));

			// Verify
			result.ShouldBe(false);
			sut.SequenceEqual(data).ShouldBe(true);
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,IEnumerable`1)
		///</summary>
		[Fact(DisplayName = "XCU.RemoveAll(Key) should return true when something removed")]
		public void RemoveAllByKeyModifiedTest()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new XCollection<XObject>(data.Concat(data.Select(x => x.Clone())));
			var toRemove = new[] { XKey.From(XKey.IdKeyName, data[0].Id), XKey.From(XKey.IdKeyName, data[Many - 1].Id) };

			// Exercise
			bool result = sut.RemoveAll(toRemove);

			// Verify
			result.ShouldBe(true);
			sut.Count.ShouldBeLessThan(data.Length);
			sut.ShouldNotContain(x => toRemove.Contains(x.GetKey()));
		}
		#endregion

		#region UnmodifiableXXX Tests
		[Fact(DisplayName = "XCU.UnmodifiableCollection should return ReadOnly collection")]
		public void UnmodifiableCollectionReadOnlyTest()
		{
			var sut = XCollectionsUtil.UnmodifiableCollection(new XCollection<XObject>());
			var obj = XIntegerObject.Create();

			sut.IsReadOnly.ShouldBe(true);

			Should.Throw<NotSupportedException>(() => sut.Add(obj));
			Should.Throw<NotSupportedException>(() => sut.Clear());
			Should.Throw<NotSupportedException>(() => sut.Remove(obj));
			Should.Throw<NotSupportedException>(() => sut.Remove(obj.GetKey()));
			Should.Throw<NotSupportedException>(() => sut.Update(new XCollection<XObject>()));
		}

		[Fact(DisplayName = "XCU.UnmodifiableList should return ReadOnly list")]
		public void UnmodifiableListReadOnlyTest()
		{
			var sut = XCollectionsUtil.UnmodifiableList(new XCollection<XObject>() { XIntegerObject.Create() });
			var obj = XIntegerObject.Create();

			sut.IsReadOnly.ShouldBe(true);

			Should.Throw<NotSupportedException>(() => sut.Add(obj));
			Should.Throw<NotSupportedException>(() => sut.Clear());
			Should.Throw<NotSupportedException>(() => sut.Insert(0, obj));
			Should.Throw<NotSupportedException>(() => sut.Remove(obj));
			Should.Throw<NotSupportedException>(() => sut.Remove(obj.GetKey()));
			Should.Throw<NotSupportedException>(() => sut.RemoveAt(0));
			Should.Throw<NotSupportedException>(() => sut.Update(new XCollection<XObject>()));
		}

		[Fact(DisplayName = "XCU.UnmodifiableList should equals correctly")]
		public void UnmodifiableListEqualsTest()
		{
			var series = new XCollection<XObject>(XIntegerObject.CreateSeries(Many));
			var seriesClone = series.Clone( true );
			var sut = XCollectionsUtil.UnmodifiableList( series );

			sut.ShouldBe(series);
			sut.ShouldBe(seriesClone);
			sut.ShouldBe(XCollectionsUtil.UnmodifiableList(seriesClone));
		}
		#endregion

		private class XIntegerObject2 : XImmutableIdentifiedObject<int>
		{
			public XIntegerObject2(int id)
				: base(id)
			{
			}

			public static XIntegerObject2 Create()
			{
				return new XIntegerObject2(XIntegerObject.NewId());
			}
		}
	}
}
