using System;

using Windows.ApplicationModel.Resources;

namespace Digillect.Properties
{
	internal static class Resources
	{
		private static ResourceLoader resourceLoader;

		internal static ResourceLoader ResourceLoader
		{
			get
			{
				if( resourceLoader == null )
					resourceLoader = new ResourceLoader( "Resources" );

				return resourceLoader;
			}
		}

		internal static string XCollectionEnumFailedVersionException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionEnumFailedVersionException" );
			}
		}

		internal static string XCollectionItemDuplicateException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionItemDuplicateException" );
			}
		}

		internal static string XCollectionReadOnlyException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionReadOnlyException" );
			}
		}

		internal static string XObjectNullKeyException
		{
			get
			{
				return ResourceLoader.GetString("XObjectNullKeyException");
			}
		}

		internal static string XObjectSourceNotCompatibleException
		{
			get
			{
				return ResourceLoader.GetString( "XObjectSourceNotCompatibleException" );
			}
		}
	}
}
