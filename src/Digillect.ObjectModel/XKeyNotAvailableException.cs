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
using System.Runtime.Serialization;

namespace Digillect
{
	/// <summary>
	/// Exception that is thrown when <see cref="Digillect.XObject"/> can not provide valid key.
	/// </summary>
#if !(SILVERLIGHT || WINDOWS8)
	[Serializable]
#endif
	public sealed class XKeyNotAvailableException : Exception
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		public XKeyNotAvailableException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public XKeyNotAvailableException( string message )
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XKeyNotAvailableException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public XKeyNotAvailableException( string message, Exception innerException )
			: base(message, innerException)
		{
		}

#if !(SILVERLIGHT || WINDOWS8)
		private XKeyNotAvailableException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif
		#endregion
	}
}
