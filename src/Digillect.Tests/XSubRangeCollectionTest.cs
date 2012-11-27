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
		[Fact]
		public void CountTest()
		{
			var c = CreateCollection(5);

			// Too few elements - count should be zero
			Assert.Equal(0, new XSubRangeCollection<XObject>(c, c.Count, 1).Count);

			// Not enough elements
			for ( int i = 0; i <= c.Count; i++ )
			{
				Assert.Equal(c.Count - i, new XSubRangeCollection<XObject>(c, i, c.Count + 1).Count);
			}

			// Enough elements
			for ( int i = 0; i <= c.Count; i++ )
			{
				Assert.Equal(c.Count - i, new XSubRangeCollection<XObject>(c, i, c.Count - i).Count);
			}

			for ( int i = 0; i < c.Count; i++ )
			{
				Assert.Equal(c.Count - i - 1, new XSubRangeCollection<XObject>(c, i + 1, c.Count - i - 1).Count);
			}
		}

		[Fact]
		public void IndexerTest()
		{
			var c = CreateCollection(5);
			var cr = new XSubRangeCollection<XObject>(c, 1, c.Count - 2);

			Assert.Equal(2, c.IndexOf(cr[1]));
		}

		[Fact]
		public void ContainsItemTest()
		{
			var c = CreateCollection(5);
			var cr = new XSubRangeCollection<XObject>(c, 1, c.Count - 2);

			Assert.False(cr.Contains(c[0]));
			Assert.True(cr.Contains(c[2]));
			Assert.False(cr.Contains(c[c.Count - 1]));
		}

		[Fact]
		public void ContainsKeyTest()
		{
			var c = CreateCollection(5);
			var cr = new XSubRangeCollection<XObject>(c, 1, c.Count - 2);

			Assert.False(cr.ContainsKey(c[0].GetKey()));
			Assert.True(cr.ContainsKey(c[2].GetKey()));
			Assert.False(cr.ContainsKey(c[c.Count - 1].GetKey()));
		}

		[Fact]
		public void IndexOfItemTest()
		{
			var c = CreateCollection(5);
			var cr = new XSubRangeCollection<XObject>(c, 1, c.Count - 2);

			Assert.Equal(-1, cr.IndexOf(c[0]));
			Assert.Equal(1, cr.IndexOf(c[2]));
			Assert.Equal(-1, cr.IndexOf(c[c.Count - 1]));
		}

		[Fact]
		public void IndexOfKeyTest()
		{
			var c = CreateCollection(5);
			var cr = new XSubRangeCollection<XObject>(c, 1, c.Count - 2);

			Assert.Equal(-1, cr.IndexOf(c[0].GetKey()));
			Assert.Equal(1, cr.IndexOf(c[2].GetKey()));
			Assert.Equal(-1, cr.IndexOf(c[c.Count - 1].GetKey()));
		}

		private static IXList<XObject> CreateCollection(int count)
		{
			return new XCollection<XObject>(XIntegerObject.CreateSeries(count));
		}
	}
}
