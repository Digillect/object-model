using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digillect.Tests
{
	[TestClass]
	public class XObjectTest
	{
		[TestMethod]
		public void IdTest()
		{
			var id = XIntegerObject.NewId();
			var obj = XIntegerObject.Create();

			Assert.AreNotEqual(obj.Id, id);

			obj.Id = id;

			Assert.AreEqual(obj.Id, id);
		}

		[TestMethod]
		public void CloneTest()
		{
			var obj = XIntegerObject.Create();
			var actual = obj.Clone();

			Assert.AreEqual(obj, actual);
			Assert.AreEqual(obj.GetKey(), actual.GetKey());
		}

		[TestMethod]
		public void UpdateTest()
		{
			var obj = XIntegerObject.Create();
			var actual = XIntegerObject.Create();

			Assert.AreNotEqual(obj, actual);

			actual.Update(obj);

			Assert.AreEqual(obj, actual);
		}
	}
}
