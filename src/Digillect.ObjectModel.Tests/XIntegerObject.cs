#region Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2014 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
	}
}
