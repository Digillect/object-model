using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using Digillect;
using Digillect.Collections;

namespace Digillect.Tests
{
	public class XCacheTest
	{
		[Fact]
		public void cache_should_return_object_by_id()
		{
			var cache = new XCache<int, XIntegerObject>();
			var original = XIntegerObject.Create();
			
			cache.CacheObject( original );

			var retrieved = cache.Get( original.Id );

			Assert.Same( original, retrieved );
		}

		[Fact]
		public void cache_should_return_cached_object()
		{
			var cache = new XCache<int, XIntegerObject>();
			var original = XIntegerObject.Create();
			var second = original.Clone();

			cache.CacheObject( original );

			var cached = cache.CacheObject( second );

			Assert.Same( original, cached );
			Assert.NotSame( second, cached );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_same_query()
		{
			var cache = new XCache<int, XIntegerObject>();
			var query = new XQuery<XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 3 ) );

			cache.CacheQuery( query, original, this );

			var cached = cache.Get( query );

			Assert.Equal( original, cached );
		}

		[Fact]
		public void cache_should_return_equal_collection_for_the_equal_query()
		{
			var cache = new XCache<int, XIntegerObject>();
			var query = new XQuery<XIntegerObject>( "hello" );
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 3 ) );

			cache.CacheQuery( query, original, this );

			var cached = cache.Get( new XQuery<XIntegerObject>( "hello" ) );

			Assert.Equal( original, cached );
		}

		[Fact]
		public void cache_should_convert_query_results()
		{
			var cache = new XCache<int, XIntegerObject>();
			var original = new XCollection<XIntegerObject>( XIntegerObject.CreateSeries( 100 ) );
			var queryForAll = new IntegerQuery( "all" );
			var queryForOdd = new IntegerQuery( "odd" );
			var queryForEven = new IntegerQuery( "even" );

			queryForAll.CanConvertFunction = q => q is IntegerQuery;
			queryForOdd.MatchFunction = o => o.Id % 2 != 0;
			queryForEven.MatchFunction = o => o.Id % 2 == 0;

			cache.CacheQuery( queryForAll, original, this );

			var odd = cache.Get( queryForOdd, this );
			var even = cache.Get( queryForEven, this );

			Assert.NotNull( odd );
			Assert.NotNull( even );
			Assert.Equal( original.Count, odd.Count + even.Count );
			Assert.True( odd.All( o => o.Id % 2 != 0 ) );
			Assert.True( even.All( o => o.Id % 2 == 0 ) );
		}

		class IntegerQuery : XQuery<XIntegerObject>
		{
			public IntegerQuery( string method = None, XParameters parameters = null )
				: base( method, parameters )
			{
			}
			
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
		}
	}
}
