using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Digillect
{
	public static class XKeySerializer
	{
		private const byte Version = 1;

		private enum TypeCode
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
			String = 18
		}

		public static XKey Deserialize(string key)
		{
			if ( String.IsNullOrEmpty(key) )
			{
				throw new ArgumentNullException("key");
			}

			Contract.EndContractBlock();

			using ( MemoryStream stream = new MemoryStream(Convert.FromBase64String(key), false) )
			{
				XKey.Builder builder = XKey.Empty.ToBuilder();

				using ( BinaryReader reader = new BinaryReader(stream) )
				{
					byte version = reader.ReadByte();
					int typeCode;

					while ( (typeCode = reader.ReadInt32()) != 0 )
					{
						if ( !Enum.IsDefined(typeof(TypeCode), typeCode) )
						{
							throw new IOException("Invalid type code encountered.");
						}

						string name = reader.ReadString();

						switch ( (TypeCode) typeCode )
						{
							case TypeCode.Guid:
								builder.AddKey(name, new Guid(reader.ReadBytes(16)));
								break;
							case TypeCode.Boolean:
								builder.AddKey(name, reader.ReadBoolean());
								break;
							case TypeCode.Char:
								builder.AddKey(name, reader.ReadChar());
								break;
							case TypeCode.SByte:
								builder.AddKey(name, reader.ReadSByte());
								break;
							case TypeCode.Byte:
								builder.AddKey(name, reader.ReadByte());
								break;
							case TypeCode.Int16:
								builder.AddKey(name, reader.ReadInt16());
								break;
							case TypeCode.UInt16:
								builder.AddKey(name, reader.ReadUInt16());
								break;
							case TypeCode.Int32:
								builder.AddKey(name, reader.ReadInt32());
								break;
							case TypeCode.UInt32:
								builder.AddKey(name, reader.ReadUInt32());
								break;
							case TypeCode.Int64:
								builder.AddKey(name, reader.ReadInt64());
								break;
							case TypeCode.UInt64:
								builder.AddKey(name, reader.ReadUInt64());
								break;
							case TypeCode.Single:
								builder.AddKey(name, reader.ReadSingle());
								break;
							case TypeCode.Double:
								builder.AddKey(name, reader.ReadDouble());
								break;
							case TypeCode.Decimal:
								//builder.AddKey(name, reader.ReadDecimal());
								int[] bits = new int[4];
								bits[0] = reader.ReadInt32();
								bits[1] = reader.ReadInt32();
								bits[2] = reader.ReadInt32();
								bits[3] = reader.ReadInt32();
								builder.AddKey(name, new Decimal(bits));
								break;
							case TypeCode.DateTime:
								//builder.AddKey(name, DateTime.FromBinary(reader.ReadInt64()));
								builder.AddKey(name, DateTime.ParseExact(reader.ReadString(), "o", DateTimeFormatInfo.InvariantInfo));
								break;
							case TypeCode.TimeSpan:
								builder.AddKey(name, TimeSpan.FromTicks(reader.ReadInt64()));
								break;
							case TypeCode.String:
								builder.AddKey(name, reader.ReadString());
								break;
						}
					}
				}

				return builder.ToImmutable();
			}
		}

		public static string Serialize(XKey key)
		{
			if ( key == null )
			{
				throw new ArgumentNullException("key");
			}

			Contract.EndContractBlock();

			using ( MemoryStream stream = new MemoryStream() )
			{
				using ( BinaryWriter writer = new BinaryWriter(stream) )
				{
					writer.Write(Version);

					foreach ( var pair in key )
					{
						object value = pair.Value;
						TypeCode typeCode = GetTypeCode(value.GetType());

						writer.Write((int) typeCode);
						writer.Write(pair.Key);

						switch ( typeCode )
						{
							case TypeCode.Guid:
								writer.Write(((Guid) value).ToByteArray());
								break;
							case TypeCode.Boolean:
								writer.Write((bool) value);
								break;
							case TypeCode.Char:
								writer.Write((char) value);
								break;
							case TypeCode.SByte:
								writer.Write((sbyte) value);
								break;
							case TypeCode.Byte:
								writer.Write((byte) value);
								break;
							case TypeCode.Int16:
								writer.Write((short) value);
								break;
							case TypeCode.UInt16:
								writer.Write((ushort) value);
								break;
							case TypeCode.Int32:
								writer.Write((int) value);
								break;
							case TypeCode.UInt32:
								writer.Write((uint) value);
								break;
							case TypeCode.Int64:
								writer.Write((long) value);
								break;
							case TypeCode.UInt64:
								writer.Write((ulong) value);
								break;
							case TypeCode.Single:
								writer.Write((float) value);
								break;
							case TypeCode.Double:
								writer.Write((double) value);
								break;
							case TypeCode.Decimal:
								//writer.Write((decimal) value);
								int[] bits = Decimal.GetBits((decimal) value);
								writer.Write(bits[0]);
								writer.Write(bits[1]);
								writer.Write(bits[2]);
								writer.Write(bits[3]);
								break;
							case TypeCode.DateTime:
								//writer.Write(((DateTime) value).ToBinary());
								writer.Write(((DateTime) value).ToString("o", DateTimeFormatInfo.InvariantInfo));
								break;
							case TypeCode.TimeSpan:
								writer.Write(((TimeSpan) value).Ticks);
								break;
							case TypeCode.String:
								writer.Write((string) value);
								break;
						}
					}

					// End-Of-Stream marker (essentialy TypeCode.Empty)
					writer.Write(0);
				}

				return Convert.ToBase64String(stream.ToArray());
			}
		}

		private static TypeCode GetTypeCode(Type type)
		{
			if ( type == null )
			{
				return TypeCode.Empty;
			}

			if ( type == typeof(Guid) )
			{
				return TypeCode.Guid;
			}
			else if ( type == typeof(Boolean) )
			{
				return TypeCode.Boolean;
			}
			else if ( type == typeof(Char) )
			{
				return TypeCode.Char;
			}
			else if ( type == typeof(SByte) )
			{
				return TypeCode.SByte;
			}
			else if ( type == typeof(Byte) )
			{
				return TypeCode.Byte;
			}
			else if ( type == typeof(Int16) )
			{
				return TypeCode.Int16;
			}
			else if ( type == typeof(UInt16) )
			{
				return TypeCode.UInt16;
			}
			else if ( type == typeof(Int32) )
			{
				return TypeCode.Int32;
			}
			else if ( type == typeof(UInt32) )
			{
				return TypeCode.UInt32;
			}
			else if ( type == typeof(Int64) )
			{
				return TypeCode.Int64;
			}
			else if ( type == typeof(UInt64) )
			{
				return TypeCode.UInt64;
			}
			else if ( type == typeof(Single) )
			{
				return TypeCode.Single;
			}
			else if ( type == typeof(Double) )
			{
				return TypeCode.Double;
			}
			else if ( type == typeof(Decimal) )
			{
				return TypeCode.Decimal;
			}
			else if ( type == typeof(DateTime) )
			{
				return TypeCode.DateTime;
			}
			else if ( type == typeof(TimeSpan) )
			{
				return TypeCode.TimeSpan;
			}
			else if ( type == typeof(String) )
			{
				return TypeCode.String;
			}
#if WINDOWS8
			else if ( type.GetTypeInfo().IsEnum )
#else
			else if ( type.IsEnum )
#endif
			{
				return GetTypeCode(Enum.GetUnderlyingType(type));
			}
			else
			{
				return TypeCode.Object;
			}
		}
	}
}
