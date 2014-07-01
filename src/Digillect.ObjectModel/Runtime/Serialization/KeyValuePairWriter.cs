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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;

namespace Digillect.Runtime.Serialization
{
	public sealed class KeyValuePairWriter : KeyValuePairReaderWriterBase
	{
		private readonly BinaryWriter _binaryWriter;
		private readonly bool _ownWriter;
#if SILVERLIGHT || NET40
		private readonly bool _closeWriter;
#endif
		private bool _epilogueWritten;

		#region Constructors/Disposer
		public KeyValuePairWriter( Stream stream )
			: this( stream, false )
		{
			Contract.Requires( stream != null );
		}

		public KeyValuePairWriter( Stream stream, bool leaveOpen )
		{
			if( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}

			Contract.EndContractBlock();

#if SILVERLIGHT || NET40
			_binaryWriter = new BinaryWriter( stream );
			_closeWriter = !leaveOpen;
#else
			_binaryWriter = new BinaryWriter( stream, new UTF8Encoding( false, true ), leaveOpen );
#endif
			_ownWriter = true;

			WritePrologue();
		}

		public KeyValuePairWriter( BinaryWriter writer )
		{
			if( writer == null )
			{
				throw new ArgumentNullException( "writer" );
			}

			Contract.EndContractBlock();

			_binaryWriter = writer;

			WritePrologue();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				WriteEpilogue();

				if( _ownWriter )
				{
#if SILVERLIGHT || NET40
					if( _closeWriter )
					{
						_binaryWriter.Dispose();
					}
#else
					_binaryWriter.Dispose();
#endif
				}
			}

			base.Dispose( disposing );
		}
		#endregion

		#region Public methods
		public void Write( KeyValuePair<string, object> pair )
		{
			var value = pair.Value;
			var typeCode = GetTypeCode( value.GetType() );

			_binaryWriter.Write( (int) typeCode );
			_binaryWriter.Write( pair.Key );

			switch( typeCode )
			{
				case TypeCode.Guid:
					_binaryWriter.Write( ((Guid) value).ToByteArray() );
					break;
				case TypeCode.Boolean:
					_binaryWriter.Write( (bool) value );
					break;
				case TypeCode.Char:
					_binaryWriter.Write( (char) value );
					break;
				case TypeCode.SByte:
					_binaryWriter.Write( (sbyte) value );
					break;
				case TypeCode.Byte:
					_binaryWriter.Write( (byte) value );
					break;
				case TypeCode.Int16:
					_binaryWriter.Write( (short) value );
					break;
				case TypeCode.UInt16:
					_binaryWriter.Write( (ushort) value );
					break;
				case TypeCode.Int32:
					_binaryWriter.Write( (int) value );
					break;
				case TypeCode.UInt32:
					_binaryWriter.Write( (uint) value );
					break;
				case TypeCode.Int64:
					_binaryWriter.Write( (long) value );
					break;
				case TypeCode.UInt64:
					_binaryWriter.Write( (ulong) value );
					break;
				case TypeCode.Single:
					_binaryWriter.Write( (float) value );
					break;
				case TypeCode.Double:
					_binaryWriter.Write( (double) value );
					break;
				case TypeCode.Decimal:
					var bits = Decimal.GetBits( (decimal) value );
					_binaryWriter.Write( bits[0] );
					_binaryWriter.Write( bits[1] );
					_binaryWriter.Write( bits[2] );
					_binaryWriter.Write( bits[3] );
					break;
				case TypeCode.DateTime:
					_binaryWriter.Write( ((DateTime) value).ToString( "o", DateTimeFormatInfo.InvariantInfo ) );
					break;
				case TypeCode.TimeSpan:
					_binaryWriter.Write( ((TimeSpan) value).Ticks );
					break;
				case TypeCode.String:
					_binaryWriter.Write( (string) value );
					break;
				case TypeCode.XKey:
					XKeySerializer.Serialize( _binaryWriter, (XKey) value );
					break;
				case TypeCode.XParameters:
					XParametersSerializer.Serialize( _binaryWriter, (XParameters) value );
					break;
				default:
					throw new IOException( "Unable to serialize object of type " + value.GetType().FullName + "." );
			}
		}

		public void WriteEpilogue()
		{
			if( !_epilogueWritten )
			{
				_binaryWriter.Write( (int) TypeCode.Empty );
				_epilogueWritten = true;
			}
		}
		#endregion

		#region Miscellaneous
		private void WritePrologue()
		{
			_binaryWriter.Write( Version );
		}
		#endregion
	}
}