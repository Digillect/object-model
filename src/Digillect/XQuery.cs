using System;
using System.ComponentModel;

namespace Digillect
{
	/// <summary>
	/// ������� ����� ��� ��������, ������������ ��������� ��������.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public class XQuery<T> : IEquatable<XQuery<T>>
#if !(SILVERLIGHT || WINDOWS8)
		, ICloneable
#endif
		where T : XObject
	{
		public const string None = null;
		public const string All = "all";

		private readonly string _method;
		private readonly XParameters _parameters;

		#region Constructors/Disposer
		public XQuery( string method = None, XParameters parameters = null )
		{
			_method = method;
			_parameters = parameters == null ? new XParameters() : XParameters.From( parameters );
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Gets query method.
		/// </summary>
		public string Method
		{
			get { return _method; }
		}

		/// <summary>
		/// Gets query parameters.
		/// </summary>
		public XParameters Parameters
		{
			get { return _parameters; }
		}

		/// <summary>
		/// ���������� <c>true</c>, ���� ������ ������ ������������ ����� <see cref="Match"/>.
		/// </summary>
		/// <value>
		/// <c>false</c>.
		/// </value>
		public virtual bool SupportsMatch
		{
			get { return false; }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// ������ ����� ��������� ������������ ���������� ������� ������� �������.
		/// </summary>
		/// <param name="value">������ ��� ��������.</param>
		/// <returns><see langword="true"/> � ������ ������������, ����� <see langword="false"/>.</returns>
		public virtual bool Match( T value )
		{
			return false;
		}

		/// <summary>
		/// ������ ����� ������ ������� <c>true</c>, ���� �� ����������� �������� �������
		/// ����� ���������� ������ <see cref="Match"/> ����� �������� ���������� ���������� �������.
		/// </summary>
		/// <param name="query">������ ��� ���������</param>
		/// <returns><see langword="true"/> ���� ���������� ������� ����� �������������, ����� <see langword="false"/>.</returns>
		/// <remarks>
		/// ������ ���������� ���������� ����� <see cref="Equals"/> ��� ����������� ����������� ��������������.
		/// </remarks>
		public virtual bool CanConvertTo( XQuery<T> query )
		{
			return Equals( query );
		}

		/// <summary>
		/// ������� ���������� (shallow) ����� �������.
		/// </summary>
		/// <returns>������������� ������</returns>
		public virtual XQuery<T> Clone()
		{
			return (XQuery<T>) MemberwiseClone();
		}
#if !(SILVERLIGHT || WINDOWS8)
		object ICloneable.Clone()
		{
			return Clone();
		}
#endif
		#endregion

		#region Object Overrides
		public bool Equals( XQuery<T> otherQuery )
		{
			return object.Equals( _method, otherQuery._method ) && _parameters.Equals( otherQuery._parameters );
		}

		public override bool Equals( object obj )
		{
			if( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			return Equals( (XQuery<T>) obj );
		}

		public override int GetHashCode()
		{
			int hashCode = 17;

			if( _method != null )
			{
				hashCode = hashCode * 37 + _method.GetHashCode();
			}

			hashCode = hashCode * 37 + _parameters.GetHashCode();

			return hashCode;
		}
		#endregion
	}
}
