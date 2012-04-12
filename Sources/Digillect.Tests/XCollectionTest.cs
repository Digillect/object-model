using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Digillect.Collections;

namespace Digillect.Tests
{
	[TestClass]
	public class XCollectionTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorNullMemberTest()
		{
			new XCollection<XObject>(new XIntegerObject[] { null });
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AddNullItemTest()
		{
			new XCollection<XObject>().Add(null);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddDupsByReferenceTest()
		{
			var coll = new XCollection<XObject>();
			var obj = XIntegerObject.Create();

			coll.Add(obj);
			coll.Add(obj);
		}

		[TestMethod]
		public void AddDupsByKeyTest()
		{
			var coll = new XCollection<XObject>();
			var obj = XIntegerObject.Create();

			coll.Add(obj);
			coll.Add(obj.Clone());

			Assert.IsTrue(coll.Count == 2);
		}

		[TestMethod]
		public void CloneTest()
		{
			var coll = new XCollection<XObject>();

			coll.AddRange(XIntegerObject.CreateSeries(3));

			Assert.AreEqual(coll, coll.Clone(true));
		}
	}
}
