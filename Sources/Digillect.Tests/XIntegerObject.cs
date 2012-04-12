using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digillect.Tests
{
	class XIntegerObject : XObject<int>
	{
		private static readonly Random _rnd = new Random(Environment.TickCount);

		private XIntegerObject()
		{
		}

		private XIntegerObject(int id)
			: base(id)
		{
		}

		public XIntegerObject Clone()
		{
			return (XIntegerObject) Clone(true);
		}

		public static XIntegerObject Create()
		{
			return new XIntegerObject(NewId());
		}

		public static XIntegerObject[] CreateSeries(int count)
		{
			var objs = new XIntegerObject[count];

			for ( int i = 0; i < count; i++ )
			{
				objs[i] = Create();
			}

			return objs;
		}

		public static int NewId()
		{
			return _rnd.Next();
		}
	}
}
