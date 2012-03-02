using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Digillect
{
#if !SILVERLIGHT
	[Serializable]
#endif
	public abstract class XKey : IComparable<XKey>, IEquatable<XKey>
	{
		/// <summary>
		/// The default "null" key which isn't equal to any other key but itself.
		/// </summary>
		public static readonly XKey Null = new NullKey();

		#region Constructor
		protected XKey()
		{
		}

		protected XKey(XKey parent)
		{
			this.Parent = parent;
		}
		#endregion

		public XKey Parent
		{
			get;
			private set;
		}

		public abstract int CompareTo(XKey other);
		public abstract bool Equals(XKey other);
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();

		#region Compare Operators
		public static bool operator <(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) < 0;
		}

		public static bool operator >(XKey key1, XKey key2)
		{
			return Comparer<XKey>.Default.Compare(key1, key2) > 0;
		}
		#endregion

		#region Equality Operators
		public static bool operator ==(XKey key1, XKey key2)
		{
			return EqualityComparer<XKey>.Default.Equals(key1, key2);
		}

		public static bool operator !=(XKey key1, XKey key2)
		{
			return !EqualityComparer<XKey>.Default.Equals(key1, key2);
		}
		#endregion

		#region Cast Operators
		public static XKey From<T>(T key, XKey parentKey = null)
			where T : IComparable<T>, IEquatable<T>
		{
			if ( key == null )
			{
				return Null;
			}

			return new SimpleKey<T>(key, parentKey);
		}

		public static explicit operator XKey(Guid key)
		{
			return From(key);
		}

		public static explicit operator XKey(int key)
		{
			return From(key);
		}

		public static explicit operator XKey(string key)
		{
			return From(key);
		}
		#endregion

		#region class NullKey
#if !SILVERLIGHT
		[Serializable]
#endif
		private sealed class NullKey : XKey
		{
			public override int CompareTo(XKey other)
			{
				if ( other == null )
				{
					return 1;
				}
				else if ( other is NullKey )
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}

			public override bool Equals(XKey other)
			{
				return other is NullKey;
			}

			public override bool Equals(object obj)
			{
				return obj is NullKey;
			}

			public override int GetHashCode()
			{
				return -1;
			}

			public override string ToString()
			{
				return "Null";
			}
		}
		#endregion

		#region class SimpleKey`1
		[DebuggerDisplay("Key = {Key}")]
#if !SILVERLIGHT
		[Serializable]
#endif
		private sealed class SimpleKey<T> : XKey
			where T: IComparable<T>, IEquatable<T>
		{
			public T Key { get; private set; }

			public SimpleKey( T key, XKey parent )
				: base( parent )
			{
				if ( key == null )
				{
					throw new ArgumentNullException("key");
				}

				this.Key = key;
			}

			public override int CompareTo( XKey other )
			{
				if ( other == null || other is NullKey )
				{
					return 1;
				}

				SimpleKey<T> otherKey = other as SimpleKey<T>;

				if ( other == null )
				{
					throw new ArgumentException("Invalid key type.", "other");
				}

				int result = 0;

				if( this.Parent != null || otherKey.Parent != null )
				{
					if( this.Parent != null && otherKey.Parent == null )
						result = 1;

					if( this.Parent == null && otherKey.Parent != null )
						result = -1;

					if( this.Parent != null && otherKey.Parent != null )
						result = this.Parent.CompareTo( otherKey.Parent );
				}

				return result != 0 ? result : this.Key.CompareTo( otherKey.Key );
			}

			public override bool Equals( XKey other )
			{
				SimpleKey<T> otherKey = other as SimpleKey<T>;

				if ( otherKey == null )
				{
					return false;
				}

				return EqualsEx( otherKey );
			}

			public override bool Equals( object obj )
			{
				if ( obj == null || obj.GetType() != GetType() )
				{
					return false;
				}

				SimpleKey<T> otherKey = obj as SimpleKey<T>;

				return EqualsEx( otherKey );
			}

			private bool EqualsEx( SimpleKey<T> otherKey )
			{
				if( this.Parent != null || otherKey.Parent != null )
				{
					if( this.Parent != null && otherKey.Parent == null )
						return false;
					if( this.Parent == null && otherKey.Parent != null )
						return false;
					if( this.Parent != null && otherKey.Parent != null )
						if( !this.Parent.Equals( otherKey.Parent ) )
							return false;
				}

				return this.Key.Equals( otherKey.Key );
			}

			public override int GetHashCode()
			{
				if( this.Parent != null )
					return this.Parent.GetHashCode() ^ this.Key.GetHashCode();

				return this.Key.GetHashCode();
			}

			public override string ToString()
			{
				if( this.Parent != null )
					return this.Parent.ToString() + "+" + this.Key.ToString();

				return this.Key.ToString();
			}
		}
		#endregion
	}
}
