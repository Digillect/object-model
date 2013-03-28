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
using System.Diagnostics.Contracts;

namespace Digillect.Reflection
{
	public static class ReflectionHelpers
	{
		#region IsAssignableFrom
		public static bool IsAssignableFrom( Type targetType, Type sourceType, params Type[] sourceTypeArguments )
		{
			Contract.Requires(sourceType != null, "sourceType");
			Contract.Requires(targetType != null, "targetType");

			if ( sourceType == null || targetType == null )
			{
				throw new ArgumentNullException(sourceType == null ? "sourceType" : "targetType");
			}

			bool assignable = false;

			if( targetType == sourceType )
			{
				assignable = true;
			}
			else if( !sourceType.IsGenericType )
			{
				assignable = sourceType.IsAssignableFrom( targetType );
			}
			else if( sourceType.IsInterface && !targetType.IsInterface )
			{
				Type[] targetTypeInterfaces = targetType.GetInterfaces();

				foreach( Type interfaceType in targetTypeInterfaces )
					if( (assignable = IsAssignableFrom( interfaceType, sourceType, sourceTypeArguments )) )
						break;
			}
			else
			{
				Type sourceTypeDefinition = sourceType.GetGenericTypeDefinition();

				if( sourceTypeArguments == null || sourceTypeArguments.Length == 0 )
					sourceTypeArguments = sourceType.GetGenericArguments();

				do
				{
					if( targetType.IsGenericType )
					{
						Type targetTypeDefinition = targetType.GetGenericTypeDefinition();

						if( targetTypeDefinition == sourceTypeDefinition )
						{
							Type[] targetTypeArguments = targetType.GetGenericArguments();

							assignable = true;

							for( int i = 0; i < targetTypeArguments.Length; ++i )
							{
								if( sourceTypeArguments[i] != null && !sourceTypeArguments[i].IsGenericParameter && !targetTypeArguments[i].IsGenericParameter )
								{
									if( !IsAssignableFrom( targetTypeArguments[i], sourceTypeArguments[i] ) )
									{
										assignable = false;
										break;
									}
								}
							}

							break;
						}
					}

					if( targetType.IsInterface )
						break;

					targetType = targetType.BaseType;
				}
				while( targetType != typeof( object ) );
			}

			return assignable;
		}
		#endregion
	}
}
