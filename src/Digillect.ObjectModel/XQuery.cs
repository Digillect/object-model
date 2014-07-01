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
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Базовый класс для запросов, возвращающих коллекции объектов.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	[DataContract]
	public class XQuery<T> : IEquatable<XQuery<T>>
	{
		private string _method;
		private XParameters _parameters;

		#region Constructors/Disposer
		public XQuery()
			: this( null, null )
		{
		}

		public XQuery( string method )
			: this( method, null )
		{
		}

		public XQuery( XParameters parameters )
			: this( null, parameters )
		{
		}

		public XQuery( string method, XParameters parameters )
		{
			_method = method;
			_parameters = parameters ?? XParameters.Empty;
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
		/// Возвращает <c>true</c>, если данный запрос поддерживает вызов <see cref="Match"/>.
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
		/// Данный метод проверяет соответствие указанного объекта данному запросу.
		/// </summary>
		/// <param name="value">Объект для проверки.</param>
		/// <returns><see langword="true"/> в случае соответствия, иначе <see langword="false"/>.</returns>
		[Pure]
		public virtual bool Match( T value )
		{
			return false;
		}

		/// <summary>
		/// Данный метод должен вернуть <c>true</c>, если из результатов текущего запроса
		/// путем применения метода <see cref="Match"/> можно получить результаты указанного запроса.
		/// </summary>
		/// <param name="query">Запрос для сравнения</param>
		/// <returns><see langword="true"/> если результаты запроса можно преобразовать, иначе <see langword="false"/>.</returns>
		/// <remarks>
		/// Данная реализация использует метод <see cref="Equals"/> для определения возможности преобразования.
		/// </remarks>
		[Pure]
		public virtual bool CanConvertTo( XQuery<T> query )
		{
			return Equals( query );
		}
		#endregion

		#region Clone
		[Pure]
		public virtual XQuery<T> Clone()
		{
			Contract.Ensures( Contract.Result<XQuery<T>>() != null );

			var clone = CreateInstanceOfSameType();

			clone.ProcessClone( this );

			return clone;
		}

		[Pure]
		protected virtual XQuery<T> CreateInstanceOfSameType()
		{
			Contract.Ensures(Contract.Result<XQuery<T>>() != null);

#if WINDOWS8
			return (XQuery<T>) Activator.CreateInstance(GetType());
#else
			return (XQuery<T>) Activator.CreateInstance( GetType(), true );
#endif
		}

		protected virtual void ProcessClone( XQuery<T> source )
		{
			if( source == null )
			{
				throw new ArgumentNullException( "source" );
			}

			Contract.EndContractBlock();
			Contract.Assume(source._parameters != null);

			_method = source._method;
			_parameters = source._parameters;
		}
		#endregion

		#region Object Overrides
		bool IEquatable<XQuery<T>>.Equals( XQuery<T> otherQuery )
		{
			if( otherQuery == null )
			{
				return false;
			}

			return object.Equals( _method, otherQuery._method ) && _parameters.Equals( otherQuery._parameters );
		}

		public override bool Equals( object obj )
		{
			if( obj == null || GetType() != obj.GetType() )
			{
				return false;
			}

			return ((IEquatable<XQuery<T>>) this).Equals( (XQuery<T>) obj );
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

		#region ObjectInvariant
		[ContractInvariantMethod]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
		private void ObjectInvariant()
		{
			Contract.Invariant(_parameters != null);
		}
		#endregion
	}
}
