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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;

namespace Digillect.Runtime.Serialization
{
	public static class XKeySerializer
	{
		#region Public methods
		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose MemoryStream multiple times" )]
		public static XKey Deserialize( string source )
		{
			if( string.IsNullOrEmpty( source ) )
			{
				throw new ArgumentNullException( "source" );
			}

			Contract.Ensures( Contract.Result<XKey>() != null );

			using( var stream = new MemoryStream( Convert.FromBase64String( source ), false ) )
			{
				var builder = XKey.Empty.ToBuilder();

				using( var reader = new KeyValuePairReader( stream ) )
				{
					while( true )
					{
						var pair = reader.Read();

						if( pair.Key == null )
						{
							break;
						}

						builder.AddKey( pair.Key, pair.Value );
					}
				}

				return builder.ToImmutable();
			}
		}

		public static XKey Deserialize( Stream source )
		{
			if( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			Contract.Ensures( Contract.Result<XKey>() != null );

			var builder = XKey.Empty.ToBuilder();

			using( var reader = new KeyValuePairReader( source, true ) )
			{
				while( true )
				{
					var pair = reader.Read();

					if( pair.Key == null )
					{
						break;
					}

					builder.AddKey( pair.Key, pair.Value );
				}
			}

			return builder.ToImmutable();
		}

		public static XKey Deserialize( BinaryReader source )
		{
			if( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			Contract.Ensures( Contract.Result<XKey>() != null );

			var builder = XKey.Empty.ToBuilder();

			using( var reader = new KeyValuePairReader( source ) )
			{
				while( true )
				{
					var pair = reader.Read();

					if( pair.Key == null )
					{
						break;
					}

					builder.AddKey( pair.Key, pair.Value );
				}
			}

			return builder.ToImmutable();
		}

		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose MemoryStream multiple times" )]
		public static string Serialize( XKey key )
		{
			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.Ensures( Contract.Result<string>() != null );

			using( var stream = new MemoryStream() )
			{
				using( var writer = new KeyValuePairWriter( stream ) )
				{
					foreach( var pair in key )
					{
						writer.Write( pair );
					}

					writer.WriteEpilogue();
				}

				return Convert.ToBase64String( stream.ToArray() );
			}
		}

		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose MemoryStream multiple times" )]
		public static void Serialize( Stream stream, XKey key )
		{
			if( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}

			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			using( var writer = new KeyValuePairWriter( stream, true ) )
			{
				foreach( var pair in key )
				{
					writer.Write( pair );
				}

				writer.WriteEpilogue();
			}
		}

		[SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is safe to dispose MemoryStream multiple times" )]
		public static void Serialize( BinaryWriter binaryWriter, XKey key )
		{
			if( binaryWriter == null )
			{
				throw new ArgumentNullException( "binaryWriter" );
			}

			if( key == null )
			{
				throw new ArgumentNullException( "key" );
			}

			Contract.EndContractBlock();

			using( var writer = new KeyValuePairWriter( binaryWriter ) )
			{
				foreach( var pair in key )
				{
					writer.Write( pair );
				}

				writer.WriteEpilogue();
			}
		}
		#endregion
	}
}