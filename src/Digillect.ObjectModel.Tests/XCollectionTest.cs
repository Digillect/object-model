#region Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman)
// Copyright (c) 2002-2013 Gregory Nickonov and Andrew Nefedkin (Actis® Wunderman).
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Digillect.Collections;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XCollectionTest
	{
		[Fact(DisplayName = "XC.ctor should not allow null members")]
		public void CtorNullMemberTest()
		{
			// Excercise & Verify
			Should.Throw<ArgumentException>(() => new XCollection<XObject>(new XObject[] { null }));
		}

		[Fact(DisplayName = "XC.Add should not allow null items")]
		public void AddNullItemTest()
		{
			// Setup
			var sut = new XCollection<XObject>();

			// Excercise & Verify
			Should.Throw<ArgumentNullException>(() => sut.Add(null));
		}

		[Fact(DisplayName = "XC.Add should not allow duplicate by reference items")]
		public void AddDupsByReferenceTest()
		{
			// Setup
			var obj = XIntegerObject.Create();
			var sut = new XCollection<XObject> { obj };

			// Excercise & Verify
			Should.Throw<ArgumentException>(() => sut.Add(obj));
		}

		[Fact(DisplayName = "XC.Add should allow differenet items with the same key")]
		public void AddDupsByKeyTest()
		{
			// Setup
			var obj = XIntegerObject.Create();
			var sut = new XCollection<XObject> { obj };

			// Excercise
			sut.Add(obj.Clone());

			// Verify
			sut.Count.ShouldBe(2);
		}

		[Fact(DisplayName = "XC.BeginUpdate should block events and raise them after the EndUpdate call")]
		public void BeginEndUpdateEventsTest()
		{
			// Setup
			bool eventsBlocked = false;
			var expectedCollectionChanged = false;
			var expectedPropertyChanged = false;
			var sut = new XCollection<XObject>();

			sut.CollectionChanged += (sender, e) => {
				Assert.False(eventsBlocked, "CollectionChanged with " + e.Action);
				expectedCollectionChanged.ShouldNotBe(true);
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
				expectedCollectionChanged = true;
			};
			((INotifyPropertyChanged) sut).PropertyChanged += (sender, e) => {
				Assert.False(eventsBlocked, "PropertyChanged with " + e.PropertyName);
				expectedPropertyChanged.ShouldNotBe(true);
				e.PropertyName.ShouldBe(null);
				expectedPropertyChanged = true;
			};

			// Exercise
			sut.BeginUpdate();
			eventsBlocked = true;
			sut.Update(XIntegerObject.CreateSeries(3));
			sut.Insert(0, XIntegerObject.Create());
			sut.RemoveAt(0);
			eventsBlocked = false;
			sut.EndUpdate();

			// Verify
			expectedCollectionChanged.ShouldBe(true);
			expectedPropertyChanged.ShouldBe(true);
		}

		[Fact(DisplayName = "XC.Clone should result in an equivalent collection")]
		public void CloneTest()
		{
			// Setup
			var sut = new XCollection<XObject>();
			sut.AddRange(XIntegerObject.CreateSeries(3));

			// Excercise
			var result = sut.Clone(true);

			// Verify
			result.ShouldBe(sut);
		}
	}
}
