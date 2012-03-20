/* $Id: ReflectionHelpers.cs,v 1.2 2006/06/01 14:32:00 Andrew Nefedkin Exp $ */

using System;
using System.Diagnostics.Contracts;
//using System.Reflection;

namespace Digillect.Reflection
{
	public static class ReflectionHelpers
	{
		#region IsAssignableFrom
		public static bool IsAssignableFrom( Type targetType, Type sourceType, params Type[] sourceTypeArguments )
		{
			Contract.Requires( sourceType != null );
			Contract.Requires( targetType != null );

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
