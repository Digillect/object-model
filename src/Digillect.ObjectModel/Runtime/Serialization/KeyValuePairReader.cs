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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;

namespace Digillect.Runtime.Serialization
{
	public sealed class KeyValuePairReader : KeyValuePairReaderWriterBase
	{
		private readonly BinaryReader _binaryReader;
		private readonly bool _ownReader;
#if SILVERLIGHT || NET40
		private readonly bool _closeReader;
#endif
		private int _version;

		#region Constructors/Disposer
		public KeyValuePairReader( Stream stream )
			: this( stream, false )
		{
			Contract.Requires( stream != null );
		}

		public KeyValuePairReader( Stream stream, bool leaveOpen )
		{
			if( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}

			Contract.EndContractBlock();

#if SILVERLIGHT || NET40
			_binaryReader = new BinaryReader( stream );
			_closeReader = !leaveOpen;
#else
			_binaryReader = new BinaryReader( stream, new UTF8Encoding( false, true ), leaveOpen );
#endif
			_ownReader = true;
		}

		public KeyValuePairReader( BinaryReader reader )
		{
			if( reader == null )
			{
				throw new ArgumentNullException( "reader" );
			}

			Contract.EndContractBlock();

			_binaryReader = reader;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( _ownReader )
				{
#if SILVERLIGHT || NET40
					if( _closeReader )
					{
						_binaryReader.Dispose();
					}
#else
					_binaryReader.Dispose();
#endif
				}
			}

			base.Dispose( disposing );
		}
		#endregion

		#region Public methods
		public KeyValuePair<string, object> Read()
		{
			Contract.Ensures( Contract.Result<KeyValuePair<string, object>>().Key == null || Contract.Result<KeyValuePair<string, object>>().Value != null );

			if( _version == 0 )
			{
				_version = _binaryReader.ReadByte();
			}

			var typeCode = _binaryReader.ReadInt32();

			if( typeCode == 0 )
			{
				return new KeyValuePair<string, object>();
			}

			if( !Enum.IsDefined( typeof( TypeCode ), typeCode ) )
			{
				throw new IOException( "Invalid type code encountered." );
			}

			var name = _binaryReader.ReadString();
			object value = null;

			switch( (TypeCode) typeCode )
			{
				case TypeCode.Guid:
					value = new Guid( _binaryReader.ReadBytes( 16 ) );
					break;
				case TypeCode.Boolean:
					value = _binaryReader.ReadBoolean();
					break;
				case TypeCode.Char:
					value = _binaryReader.ReadChar();
					break;
				case TypeCode.SByte:
					value = _binaryReader.ReadSByte();
					break;
				case TypeCode.Byte:
					value = _binaryReader.ReadByte();
					break;
				case TypeCode.Int16:
					value = _binaryReader.ReadInt16();
					break;
				case TypeCode.UInt16:
					value = _binaryReader.ReadUInt16();
					break;
				case TypeCode.Int32:
					value = _binaryReader.ReadInt32();
					break;
				case TypeCode.UInt32:
					value = _binaryReader.ReadUInt32();
					break;
				case TypeCode.Int64:
					value = _binaryReader.ReadInt64();
					break;
				case TypeCode.UInt64:
					value = _binaryReader.ReadUInt64();
					break;
				case TypeCode.Single:
					value = _binaryReader.ReadSingle();
					break;
				case TypeCode.Double:
					value = _binaryReader.ReadDouble();
					break;
				case TypeCode.Decimal:
					//builder.AddKey(name, reader.ReadDecimal());
					var bits = new int[4];

					bits[0] = _binaryReader.ReadInt32();
					bits[1] = _binaryReader.ReadInt32();
					bits[2] = _binaryReader.ReadInt32();
					bits[3] = _binaryReader.ReadInt32();

					value = new Decimal( bits );
					break;
				case TypeCode.DateTime:
					//builder.AddKey(name, DateTime.FromBinary(reader.ReadInt64()));
					value = DateTime.ParseExact( _binaryReader.ReadString(), "o", DateTimeFormatInfo.InvariantInfo );
					break;
				case TypeCode.TimeSpan:
					value = TimeSpan.FromTicks( _binaryReader.ReadInt64() );
					break;
				case TypeCode.String:
					value = _binaryReader.ReadString();
					break;
				case TypeCode.XKey:
					value = XKeySerializer.Deserialize( _binaryReader );
					break;
				case TypeCode.XParameters:
					value = XParametersSerializer.Deserialize( _binaryReader );
					break;
				default:
					throw new IOException( "Invalid type code encountered." );
			}

			return new KeyValuePair<string, object>( name, value );
		}
		#endregion
	}
}