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
using System.Linq;

using Xunit;
using Shouldly;

using Digillect.Collections;

namespace Digillect.Tests
{
	public class XCacheTest
	{
		private const int Many = 3;

		[Fact]
		public void cache_should_return_object_by_id()
		{
			var cache = new XCache<XIntegerObject>();
			var original = XIntegerObject.Create();
			
			cache.Cache( original );

			var retrieved = cache.Get( original.GetKey() );

			retrieved.ShouldBe( original );
		}

		[Fact]
		public void cache_should_return_cached_object()
		{
			var cache = new XCache<XIntegerObject>();
			var original = XIntegerObject.Create();
			var second = original.Clone();

			cache.Cache( original );

			var cached = cache.Cache( second );

			cached.ShouldBeSameAs( original );
			cached.ShouldNotBeSameAs( second );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_same_query()
		{
			var cache = new XCache<XIntegerObject>();
			var query = new XQuery<XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( Many ) );

			cache.Cache(original, query, this);

			var cached = cache.Get( query );

			cached.ShouldBe( original );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_equal_query()
		{
			var cache = new XCache<XIntegerObject>();
			var query = new XQuery<XIntegerObject>( "hello" );
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( Many ) );

			var response = cache.Cache(original, query, this);

			var cached = cache.Get( new XQuery<XIntegerObject>( "hello" ) );

			cached.ShouldBe( response );
			cached.ShouldBe( original );
		}

		[Fact]
		public void cache_should_convert_query_results()
		{
			var cache = new XCache<XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 100 ) );
			var queryForAll = new IntegerQuery( "all" );
			var queryForOdd = new IntegerQuery( "odd" );
			var queryForEven = new IntegerQuery( "even" );

			queryForAll.CanConvertFunction = q => q is IntegerQuery;
			queryForOdd.MatchFunction = o => o.Id % 2 != 0;
			queryForEven.MatchFunction = o => o.Id % 2 == 0;

			cache.Cache(original, queryForAll, this);

			var odd = cache.Get( queryForOdd, this );
			var even = cache.Get( queryForEven, this );

			odd.ShouldNotBe( null );
			even.ShouldNotBe( null );

			original.Count.ShouldBe( odd.Count + even.Count );

			odd.All( o => o.Id % 2 != 0 ).ShouldBe( true );
			even.All( o => o.Id % 2 == 0 ).ShouldBe( true );
		}

		[Fact(DisplayName = "XCache.Cleanup should remove all GC'd objects")]
		public void CleanupTest()
		{
			// Setup
			var objs = XIntegerObject.CreateSeries(Many);
			var sut = new XCacheTestStubSubclass<XObject>();
			sut.Cache(objs);
			sut.Cache(XIntegerObject.CreateSeries(Many));
			sut.Count.ShouldBe(Many * 2);

			// Exercise
			GC.Collect();
			sut.Cleanup();

			// Verify
			sut.Count.ShouldBe(Many);
			sut.ShouldContain(x => objs.Contains(x));
		}

		private class XCacheTestStubSubclass<T> : XCache<T>
			where T : XObject
		{
			public int Count
			{
				get { return this.ObjectCache.Count; }
			}
		}

		private class IntegerQuery : XQuery<XIntegerObject>
		{
			#region Constructors/Disposer
			public IntegerQuery()
			{
			}

			public IntegerQuery( string method )
				: base( method )
			{
			}

			public IntegerQuery( XParameters parameters )
				: base( parameters )
			{
			}

			public IntegerQuery( string method, XParameters parameters )
				: base( method, parameters )
			{
			}
			#endregion

			public Func<XQuery<XIntegerObject>, bool> CanConvertFunction { get; set; }
			public Func<XIntegerObject, bool> MatchFunction { get; set; }

			public override bool CanConvertTo( XQuery<XIntegerObject> query )
			{
				if( CanConvertFunction != null )
				{
					return CanConvertFunction( query );
				}
				else
				{
					return false;
				}
			}

			public override bool SupportsMatch
			{
				get { return MatchFunction != null; }
			}

			public override bool Match( XIntegerObject value )
			{
				if( MatchFunction != null )
				{
					return MatchFunction( value );
				}
				else
				{
					return false;
				}
			}

			protected override void ProcessClone( XQuery<XIntegerObject> source )
			{
				base.ProcessClone( source );

				var other = (IntegerQuery) source;

				CanConvertFunction = other.CanConvertFunction;
				MatchFunction = other.MatchFunction;
			}
		}
	}
}
