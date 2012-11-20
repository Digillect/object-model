using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Digillect.Collections;

namespace Digillect.Tests
{
	/// <summary>
	/// This is a test class for XCollectionsUtilTest and is intended to contain all XCollectionsUtilTest Unit Tests
	///</summary>
	public class XCollectionsUtilTest
	{
		/// <summary>
		/// A test for Difference
		///</summary>
		[Fact( Skip="Test is not ready yet." )]
		public void DifferenceTest()
		{
#if false
			Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
					"Please call DifferenceTestHelper<T>() with appropriate type parameters.");
#endif
		}

		/// <summary>
		/// A test for FilteredList
		///</summary>
		[Fact( Skip="Test is not ready yet." )]
		public void FilteredListTest()
		{
#if false
			Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
					"Please call FilteredListTestHelper<T>() with appropriate type parameters.");
#endif
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1([])
		///</summary>
		[Fact]
		public void IsNullOrEmptyTest()
		{
			Assert.True(XCollectionsUtil.IsNullOrEmpty((object[]) null));
			Assert.True(XCollectionsUtil.IsNullOrEmpty(new object[] { }));
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1(ICollection`1)
		///</summary>
		[Fact]
		public void IsNullOrEmptyTest1()
		{
			Assert.True(XCollectionsUtil.IsNullOrEmpty((ICollection<object>) null));
			Assert.True(XCollectionsUtil.IsNullOrEmpty(new List<object>()));
		}

		/// <summary>
		/// A test for Merge`1
		///</summary>
		[Fact]
		public void MergeTest()
		{
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], null, objs[1], null, objs[2], XIntegerObject.Create() };
			var expected = new[] { XIntegerObject.Create(), null, objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), null, XIntegerObject.Create() };

			Assert.NotEqual(expected, data);

			List<XObject> actual = new List<XObject>(data);

			Assert.True(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.None).IsEmpty);
			Assert.Equal(data, actual);

			Assert.False(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.AddNew | CollectionMergeOptions.UpdateExisting).IsEmpty);
			Assert.True(expected.Length <= actual.Count);
			CollectionAssert.SubsetOf(expected, actual);
			Assert.True(actual.Count(NullSelector) == data.Count(NullSelector) + expected.Count(NullSelector));

			actual = new List<XObject>(data);
			Assert.False(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.RemoveOld | CollectionMergeOptions.UpdateExisting).IsEmpty);
			Assert.True(actual.Count <= expected.Length);
			CollectionAssert.SubsetOf(actual, expected);
			Assert.False( actual.Any( NullSelector ) );

			actual = new List<XObject>(data);
			Assert.False(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.AddNew | CollectionMergeOptions.RemoveOld | CollectionMergeOptions.UpdateExisting).IsEmpty);
			CollectionAssert.SubsetOf(actual, expected);

			actual = new List<XObject>(data);
			Assert.False(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.Full).IsEmpty);
			Assert.Equal(expected, actual);

			//Assert.IsTrue(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.Full).IsEmpty);
			//CollectionAssert.AreEqual(data, actual);
		}

		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,IEnumerable`1)
		///</summary>
		[Fact]
		public void RemoveAllTest()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new List<XObject>(data);

			Assert.False(XCollectionsUtil.RemoveAll(source, XIntegerObject.CreateSeries(3)));
			Assert.Equal(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.True(XCollectionsUtil.RemoveAll(source, toRemove));
			Assert.True(source.Count < data.Length);
			CollectionAssert.NotSubsetOf(toRemove, source);
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,IEnumerable`1)
		///</summary>
		[Fact]
		public void RemoveAllTest1()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new XCollection<XObject>(data);

			Assert.False(XCollectionsUtil.RemoveAll(source, XIntegerObject.CreateSeries(3).Select(x => x.GetKey())));
			Assert.Equal(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.True(XCollectionsUtil.RemoveAll(source, toRemove.Select(x => x.GetKey())));
			Assert.True(source.Count < data.Length);
			CollectionAssert.NotSubsetOf(toRemove, source);
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,Func`2)
		///</summary>
		[Fact]
		public void RemoveAllTest2()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new List<XObject>(data);

			Assert.False(XCollectionsUtil.RemoveAll(source, x => false));
			Assert.Equal(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.True(XCollectionsUtil.RemoveAll(source, x => toRemove.Select(y => y.GetKey()).Contains(x.GetKey())));
			Assert.True(source.Count < data.Length);
			CollectionAssert.NotSubsetOf(toRemove, source);

			Assert.True(source.Count != 0);
			Assert.True(XCollectionsUtil.RemoveAll(source, x => true));
			Assert.True(source.Count == 0);
		}

		/// <summary>
		///A test for UnmodifiableCollection
		///</summary>
		[Fact]
		public void UnmodifiableCollectionTest()
		{
			var mc = new XCollection<XIntegerObject>();
			var obj = XIntegerObject.Create();

			mc.Add(obj);

			var uc = XCollectionsUtil.UnmodifiableCollection(mc);

			Assert.True(uc.IsReadOnly);

			Assert.Throws<NotSupportedException>(() => uc.Add(obj));
			Assert.Throws<NotSupportedException>(() => uc.Remove(obj));
			Assert.Throws<NotSupportedException>(() => uc.Remove(obj.GetKey()));
			Assert.Throws<NotSupportedException>(() => uc.Clear());
			
			var mc2 = new XCollection<XIntegerObject>(XIntegerObject.CreateSeries(3));

			Assert.Throws<NotSupportedException>(() => uc.Update(mc2));
		}

		/// <summary>
		///A test for UnmodifiableList
		///</summary>
		[Fact]
		public void UnmodifiableListTest()
		{
			var mc = new XCollection<XIntegerObject>();
			var obj = XIntegerObject.Create();

			mc.Add(obj);

			var ul = XCollectionsUtil.UnmodifiableList(mc);

			Assert.True(ul.IsReadOnly);

			Assert.Throws<NotSupportedException>(() => ul.Add(obj));
			Assert.Throws<NotSupportedException>(() => ul.Remove(obj));
			Assert.Throws<NotSupportedException>(() => ul.Remove(obj.GetKey()));
			Assert.Throws<NotSupportedException>(() => ul.Clear());

			var mc2 = new XCollection<XIntegerObject>(XIntegerObject.CreateSeries(3));

			Assert.Throws<NotSupportedException>(() => ul.Update(mc2));
		}

		private static bool NullSelector<T>(T obj)
		{
			return obj == null;
		}
	}
}
