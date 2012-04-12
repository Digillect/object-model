using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Digillect.Collections;

namespace Digillect.Tests
{
	/// <summary>
	/// This is a test class for XCollectionsUtilTest and is intended to contain all XCollectionsUtilTest Unit Tests
	///</summary>
	[TestClass()]
	public class XCollectionsUtilTest
	{
		/// <summary>
		/// A test for Difference
		///</summary>
		[Ignore]
		[TestMethod]
		public void DifferenceTest()
		{
			Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
					"Please call DifferenceTestHelper<T>() with appropriate type parameters.");
		}

		/// <summary>
		/// A test for FilteredList
		///</summary>
		[Ignore]
		[TestMethod]
		public void FilteredListTest()
		{
			Assert.Inconclusive("No appropriate type parameter is found to satisfies the type constraint(s) of T. " +
					"Please call FilteredListTestHelper<T>() with appropriate type parameters.");
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1([])
		///</summary>
		[TestMethod]
		public void IsNullOrEmptyTest()
		{
			Assert.IsTrue(XCollectionsUtil.IsNullOrEmpty((object[]) null));
			Assert.IsTrue(XCollectionsUtil.IsNullOrEmpty(new object[] { }));
		}

		/// <summary>
		/// A test for IsNullOrEmpty`1(ICollection`1)
		///</summary>
		[TestMethod]
		public void IsNullOrEmptyTest1()
		{
			Assert.IsTrue(XCollectionsUtil.IsNullOrEmpty((ICollection<object>) null));
			Assert.IsTrue(XCollectionsUtil.IsNullOrEmpty(new List<object>()));
		}

		/// <summary>
		/// A test for Merge`1
		///</summary>
		[TestMethod]
		public void MergeTest()
		{
			var objs = XIntegerObject.CreateSeries(3);
			var data = new[] { XIntegerObject.Create(), objs[0], null, objs[1], null, objs[2], XIntegerObject.Create() };
			var expected = new[] { XIntegerObject.Create(), null, objs[2].Clone(), objs[1].Clone(), objs[0].Clone(), null, XIntegerObject.Create() };

			CollectionAssert.AreNotEqual(expected, data);

			List<XObject> actual = new List<XObject>(data);

			Assert.IsTrue(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.None).IsEmpty);
			CollectionAssert.AreEqual(data, actual);

			Assert.IsFalse(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.AddNew | CollectionMergeOptions.UpdateExisting).IsEmpty);
			Assert.IsTrue(expected.Length <= actual.Count);
			CollectionAssert.IsSubsetOf(expected, actual);
			Assert.IsTrue(actual.Count(NullSelector) == data.Count(NullSelector) + expected.Count(NullSelector));

			actual = new List<XObject>(data);
			Assert.IsFalse(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.RemoveOld | CollectionMergeOptions.UpdateExisting).IsEmpty);
			Assert.IsTrue(actual.Count <= expected.Length);
			CollectionAssert.IsSubsetOf(actual, expected);
			Assert.IsFalse(actual.Any(NullSelector));

			actual = new List<XObject>(data);
			Assert.IsFalse(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.AddNew | CollectionMergeOptions.RemoveOld | CollectionMergeOptions.UpdateExisting).IsEmpty);
			CollectionAssert.AreEquivalent(expected, actual);

			actual = new List<XObject>(data);
			Assert.IsFalse(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.Full).IsEmpty);
			CollectionAssert.AreEqual(expected, actual);

			//Assert.IsTrue(XCollectionsUtil.Merge(actual, expected, CollectionMergeOptions.Full).IsEmpty);
			//CollectionAssert.AreEqual(data, actual);
		}

		/// <summary>
		/// A test for RemoveAll`1(ICollection`1,IEnumerable`1)
		///</summary>
		[TestMethod]
		public void RemoveAllTest()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new List<XObject>(data);

			Assert.IsFalse(XCollectionsUtil.RemoveAll(source, XIntegerObject.CreateSeries(3)));
			CollectionAssert.AreEqual(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.IsTrue(XCollectionsUtil.RemoveAll(source, toRemove));
			Assert.IsTrue(source.Count < data.Length);
			CollectionAssert.IsNotSubsetOf(toRemove, source);
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,IEnumerable`1)
		///</summary>
		[TestMethod]
		public void RemoveAllTest1()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new XCollection<XObject>(data);

			Assert.IsFalse(XCollectionsUtil.RemoveAll(source, XIntegerObject.CreateSeries(3).Select(x => x.GetKey())));
			CollectionAssert.AreEqual(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.IsTrue(XCollectionsUtil.RemoveAll(source, toRemove.Select(x => x.GetKey())));
			Assert.IsTrue(source.Count < data.Length);
			CollectionAssert.IsNotSubsetOf(toRemove, source);
		}

		/// <summary>
		/// A test for RemoveAll`1(IXCollection`1,Func`2)
		///</summary>
		[TestMethod]
		public void RemoveAllTest2()
		{
			var data = XIntegerObject.CreateSeries(3);
			var source = new List<XObject>(data);

			Assert.IsFalse(XCollectionsUtil.RemoveAll(source, new Func<XObject, bool>(x => false)));
			CollectionAssert.AreEqual(data, source);

			var toRemove = new[] { source[0], source[2] };

			Assert.IsTrue(XCollectionsUtil.RemoveAll(source, new Func<XObject, bool>(x => toRemove.Select(y => y.GetKey()).Contains(x.GetKey()))));
			Assert.IsTrue(source.Count < data.Length);
			CollectionAssert.IsNotSubsetOf(toRemove, source);

			Assert.IsTrue(source.Count != 0);
			Assert.IsTrue(XCollectionsUtil.RemoveAll(source, new Func<XObject, bool>(x => true)));
			Assert.IsTrue(source.Count == 0);
		}

		/// <summary>
		///A test for UnmodifiableCollection
		///</summary>
		[TestMethod]
		public void UnmodifiableCollectionTest()
		{
			Assert.IsTrue(XCollectionsUtil.UnmodifiableCollection(new XCollection<XObject>()).IsReadOnly);
			Assert.Inconclusive("Though the collection seems to be read-only (by evaluating corresponding property) there must be additional testing of each affected method.");
		}

		/// <summary>
		///A test for UnmodifiableList
		///</summary>
		[TestMethod]
		public void UnmodifiableListTest()
		{
			Assert.IsTrue(XCollectionsUtil.UnmodifiableList(new XCollection<XObject>()).IsReadOnly);
			Assert.Inconclusive("Though the collection seems to be read-only (by evaluating corresponding property) there must be additional testing of each affected method.");
		}

		private static bool NullSelector<T>(T obj)
		{
			return obj == null;
		}
	}
}
