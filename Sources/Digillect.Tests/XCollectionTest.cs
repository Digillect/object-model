using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

using Digillect.Collections;

namespace Digillect.Tests
{
	public class XCollectionTest
	{
		[Fact]
		public void CtorNullMemberTest()
		{
			Assert.Throws<ArgumentException>(() => new XCollection<XObject>(new XIntegerObject[] { null }));
		}

		[Fact]
		public void AddNullItemTest()
		{
			Assert.Throws<ArgumentNullException>(() => new XCollection<XObject>().Add(null));
		}

		[Fact]
		public void AddDupsByReferenceTest()
		{
			var coll = new XCollection<XObject>();
			var obj = XIntegerObject.Create();

			coll.Add(obj);
			Assert.Throws<ArgumentException>(() => coll.Add(obj));
		}

		[Fact]
		public void AddDupsByKeyTest()
		{
			var coll = new XCollection<XObject>();
			var obj = XIntegerObject.Create();

			coll.Add(obj);
			coll.Add(obj.Clone());

			Assert.True(coll.Count == 2);
		}

		[Fact]
		public void CloneTest()
		{
			var coll = new XCollection<XObject>();

			coll.AddRange(XIntegerObject.CreateSeries(3));

			Assert.Equal(coll, coll.Clone(true));
		}
	}
}
