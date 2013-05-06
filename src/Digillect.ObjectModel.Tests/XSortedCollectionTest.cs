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

using Digillect.Collections;

using Shouldly;

using Xunit;

namespace Digillect.Tests
{
	public class XSortedCollectionTest
	{
		private const int Many = 3;

		[Fact(DisplayName = "XSC: All items should be sorted according to a comparator function")]
		public void BasicSortTest()
		{
			// Setup
			var sut = CreateTestCollection(Many);

			// Exercise

			// Verify
			XIntegerObject value = sut[0];

			foreach ( var item in sut.Skip(1) )
			{
				IdComparison(item, value).ShouldBeGreaterThanOrEqualTo(0);

				value = item;
			}
		}

		[Fact(DisplayName = "XSC.OC.Add should raise appropriate events")]
		public void InsertItemTest()
		{
			// Setup
			bool ncchEventRaised = false;
			int pchEventRaised = 0;
			var sut = CreateTestCollection(Many, (sender, e) => {
				ncchEventRaised = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Add);
			}, (sender, e) => {
				pchEventRaised++;
			});

			// Exercise
			var item = sut[0].Clone();
			item.Id -= 1;
			sut.OriginalCollection.Add(item);

			// Verify
			ncchEventRaised.ShouldBe(true);
			pchEventRaised.ShouldBe(2);
			sut.IndexOf(item).ShouldBe(0);
		}

		[Fact(DisplayName = "XSC.OC.Remove should raise appropriate events")]
		public void RemoveItemTest()
		{
			// Setup
			bool ncchEventRaised = false;
			int pchEventRaised = 0;
			var sut = CreateTestCollection(Many, (sender, e) => {
				ncchEventRaised = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Remove);
			}, (sender, e) => {
				pchEventRaised++;
			});

			// Exercise
			sut.OriginalCollection.RemoveAt(0);

			// Verify
			ncchEventRaised.ShouldBe(true);
			pchEventRaised.ShouldBe(2);
		}

		[Fact(DisplayName = "XSC.OC.Move should not produce any visible changes")]
		public void MoveItemTest()
		{
			// Setup
			var sut = CreateTestCollection(Many, (sender, e) => {
				Assert.False(true, "CollectionChanged: " + e.Action);
			}, (sender, e) => {
				Assert.False(true, "PropertyChanged: " + e.PropertyName);
			});
			var snaphot = sut.ToArray();

			// Exercise
			((XCollection<XIntegerObject>) sut.OriginalCollection).Move(0, Many - 1);
			((XCollection<XIntegerObject>) sut.OriginalCollection).Move(0, Many - 1);

			// Verify
			sut.SequenceEqual(snaphot).ShouldBe(true);
		}

		[Fact(DisplayName = "XSC: Collection should raise events when an item's property changes which affects comparator")]
		public void ItemPropertyChangeTest()
		{
			// Setup
			bool ncchEventRaised = false;
			bool pchEventRaised = false;
			var sut = CreateTestCollection(Many, (sender, e) => {
				ncchEventRaised = true;
				e.Action.ShouldBe(NotifyCollectionChangedAction.Reset);
			}, (sender, e) => {
				pchEventRaised = true;
				e.PropertyName.ShouldBe("Item[]");
			});

			// Exercise
			var item0 = sut[0];
			item0.Id = sut[Many - 1].Id + 1;

			// Verify
			ncchEventRaised.ShouldBe(true);
			pchEventRaised.ShouldBe(true);
			sut.IndexOf(item0).ShouldBe(Many - 1);

			// Setup 2
			ncchEventRaised = false;
			pchEventRaised = false;

			// Exercise 2
			item0.Id = sut[0].Id - 1;

			// Verify 2
			ncchEventRaised.ShouldBe(true);
			pchEventRaised.ShouldBe(true);
			sut.IndexOf(item0).ShouldBe(0);
		}

		[Fact(DisplayName = "XSC.Clone should produce an equivalent collection")]
		public void CloneTest()
		{
			// Setup
			var sut = CreateTestCollection(Many);

			// Exercise
			var result = sut.Clone(false);

			// Verify
			result.SequenceEqual(sut).ShouldBe(true);

			// Exercise 2
			result = sut.Clone(true);

			// Verify 2
			result.SequenceEqual(sut).ShouldBe(true);
		}

		#region Setup
		private static XCollection<XIntegerObject> CreateSourceCollection(int count)
		{
			return new XCollection<XIntegerObject>(XIntegerObject.CreateSeries(count).OrderByDescending(x => x.Id));
		}

		private static XSortedCollection<XIntegerObject> CreateTestCollection(int sourceCount, NotifyCollectionChangedEventHandler changedHandler = null, PropertyChangedEventHandler propertyChangedHandler = null)
		{
			var result = new XSortedCollection<XIntegerObject>(CreateSourceCollection(sourceCount), IdComparison);

			if ( changedHandler != null )
			{
				result.CollectionChanged += changedHandler;
			}

			if ( propertyChangedHandler != null )
			{
				result.PropertyChanged += propertyChangedHandler;
			}

			return result;
		}

		private static int IdComparison<TId>(IXIdentified<TId> obj1, IXIdentified<TId> obj2) where TId : IComparable<TId>
		{
			/*
			if ( obj1 == null || obj2 == null )
			{
				if ( obj1 == null )
				{
					return obj2 == null ? 0 : -1;
				}
				else if ( obj2 == null )
				{
					return 1;
				}
			}
			*/

			return obj1.Id.CompareTo(obj2.Id);
		}
		#endregion
	}
}
