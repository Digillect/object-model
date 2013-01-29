using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		protected override XObject CreateInstanceOfSameType()
		{
			return new XIntegerObject();
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

		public static int[] NewId(int count)
		{
			var ids = new int[count];

			for ( int i = 0; i < count; i++ )
			{
				ids[i] = NewId();
			}

			return ids;
		}

		public static XKey CreateKey( int id )
		{
			return XObject<int>.CreateKey( id, typeof( XIntegerObject ) );
		}
	}
}
