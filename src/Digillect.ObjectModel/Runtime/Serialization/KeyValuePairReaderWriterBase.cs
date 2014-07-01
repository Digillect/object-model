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
using System.Reflection;

namespace Digillect.Runtime.Serialization
{
	public class KeyValuePairReaderWriterBase : IDisposable
	{
		protected const byte Version = 1;

		#region Constructors/Disposer
		~KeyValuePairReaderWriterBase()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		protected virtual void Dispose( bool disposing )
		{
		}
		#endregion

		#region Protected methods
		protected static TypeCode GetTypeCode( Type type )
		{
			if( type == null )
			{
				return TypeCode.Empty;
			}

			if( type == typeof( Guid ) )
			{
				return TypeCode.Guid;
			}
			else if( type == typeof( Boolean ) )
			{
				return TypeCode.Boolean;
			}
			else if( type == typeof( Char ) )
			{
				return TypeCode.Char;
			}
			else if( type == typeof( SByte ) )
			{
				return TypeCode.SByte;
			}
			else if( type == typeof( Byte ) )
			{
				return TypeCode.Byte;
			}
			else if( type == typeof( Int16 ) )
			{
				return TypeCode.Int16;
			}
			else if( type == typeof( UInt16 ) )
			{
				return TypeCode.UInt16;
			}
			else if( type == typeof( Int32 ) )
			{
				return TypeCode.Int32;
			}
			else if( type == typeof( UInt32 ) )
			{
				return TypeCode.UInt32;
			}
			else if( type == typeof( Int64 ) )
			{
				return TypeCode.Int64;
			}
			else if( type == typeof( UInt64 ) )
			{
				return TypeCode.UInt64;
			}
			else if( type == typeof( Single ) )
			{
				return TypeCode.Single;
			}
			else if( type == typeof( Double ) )
			{
				return TypeCode.Double;
			}
			else if( type == typeof( Decimal ) )
			{
				return TypeCode.Decimal;
			}
			else if( type == typeof( DateTime ) )
			{
				return TypeCode.DateTime;
			}
			else if( type == typeof( TimeSpan ) )
			{
				return TypeCode.TimeSpan;
			}
			else if( type == typeof( String ) )
			{
				return TypeCode.String;
			}
#if WINDOWS8
			else if( type.GetTypeInfo().IsEnum )
#else
			else if( type.IsEnum )
#endif
			{
				return GetTypeCode( Enum.GetUnderlyingType( type ) );
			}
			else if( type == typeof( XKey ) )
			{
				return TypeCode.XKey;
			}
			else if( type == typeof( XParameters ) )
			{
				return TypeCode.XParameters;
			}
			else
			{
				return TypeCode.Object;
			}
		}
		#endregion

		#region Nested type: TypeCode
		protected enum TypeCode
		{
			Empty = 0,
			Object = 1,
			Guid = 2,
			Boolean = 3,
			Char = 4,
			SByte = 5,
			Byte = 6,
			Int16 = 7,
			UInt16 = 8,
			Int32 = 9,
			UInt32 = 10,
			Int64 = 11,
			UInt64 = 12,
			Single = 13,
			Double = 14,
			Decimal = 15,
			DateTime = 16,
			TimeSpan = 17,
			String = 18,
			XKey = 19,
			XParameters = 20,
		}
		#endregion
	}
}