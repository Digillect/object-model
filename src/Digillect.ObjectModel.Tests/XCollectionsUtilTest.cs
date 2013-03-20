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
		[Fact(DisplayName = "XCollectionsUtil.IsNullOrEmpty should return true when ICollection is null or empty")]
		public void IsNullOrEmpty_should_return_true_when_ICollection_is_null_or_empty()
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
		[Fact(DisplayName = "XCollectionsUtil.IsNullOrEmpty should return true when ICollection`1 is null or empty")]
		public void IsNullOrEmpty_should_return_true_when_ICollection1_is_null_or_empty()
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
		[Fact(DisplayName = "XCollectionsUtil.IsNullOrEmpty should return true when IReadOnlyCollection`1 is null or empty")]
		public void IsNullOrEmpty_should_return_true_when_IReadOnlyCollection1_is_null_or_empty()
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
		[Fact]
		public void Merge_should_add_new_nulls_and_not_remove_existing()
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

		[Fact]
		public void Merge_should_not_add_new_nulls_and_remove_existing()
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

		[Fact]
		public void Merge_should_not_change_anything_when_options_are_None()
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

		[Fact]
		public void Merge_should_only_add_new_items()
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

		[Fact]
		public void Merge_should_only_remove_old_items()
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

		[Fact]
		public void Merge_should_only_update_existing_items()
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

		[Fact]
		public void Merge_should_add_remove_update()
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

		[Fact]
		public void Merge_should_do_full_merge()
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
		[Fact(DisplayName = "XCollectionsUtil.Merge should not perform excessive updates when the other collection contains duplicates")]
		public void Merge_should_not_perform_excessive_updates_when_other_collection_contains_duplicates()
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

		[Fact(DisplayName = "XCollectionsUtil.Merge should differentiate objects with the same id but of different type")]
		public void Merge_should_differentiate_objects_with_the_same_id_but_of_different_type()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(objs);
			var other = objs.Select(x => new XIntegerObject2(x.Id));

			// Exercise
			var result = sut.Merge(other, CollectionMergeOptions.Full);

			// Verify
			result.Added.ShouldBe(other.Count());
			result.Removed.ShouldBe(objs.Length);
			result.Updated.ShouldBe(0);
			sut.SequenceEqual(other).ShouldBe(true);
		}
		#endregion
		#endregion

		#region RemoveAll Tests
		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,Func`2)
		///</summary>
		[Fact]
		public void RemoveAllByPredicate_should_return_false_when_nothing_removed()
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
		[Fact]
		public void RemoveAllByPredicate_should_return_true_when_something_removed()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(3);
			ICollection<XObject> sut = new List<XObject>(data);
			var toRemove = new[] { data[0].Clone(), data[2].Clone() };

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
		[Fact]
		public void RemoveAllByObject_should_return_false_when_nothing_removed()
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
		[Fact]
		public void RemoveAllByObject_should_return_true_when_something_removed()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new List<XObject>(data);
			var toRemove = new[] { data[0].Clone(), data[Many - 1].Clone() };

			// Exercise
			bool result = sut.RemoveAll(toRemove);

			// Verify
			result.ShouldBe(true);
			sut.Count.ShouldBeLessThan(data.Length);
			sut.ShouldNotContain(x => toRemove.Contains(x));
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,IEnumerable`1)
		///</summary>
		[Fact]
		public void RemoveAllByKey_should_return_false_when_nothing_removed()
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
		[Fact]
		public void RemoveAllByKey_should_return_true_when_something_removed()
		{
			// Setup
			var data = XIntegerObject.CreateSeries(Many);
			var sut = new XCollection<XObject>(data);
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
		[Fact]
		public void UnmodifiableCollection_should_return_ReadOnly_collection()
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

		[Fact]
		public void UnmodifiableList_should_return_ReadOnly_list()
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

		[Fact]
		public void UnmodifiableList_should_equals_correctly()
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
