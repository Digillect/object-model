using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Resources;

namespace Digillect.Properties
{
	internal class Resources
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
		/// <summary>
		///   Looks up a localized string similar to Collection was modified; enumeration operation may not execute..
		/// </summary>
		internal static string XCollectionEnumFailedVersionException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionEnumFailedVersionException" );
			}
		}

		/// <summary>
		///   Looks up a localized string similar to This object is already a member of the collection..
		/// </summary>
		internal static string XCollectionItemDuplicateException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionItemDuplicateException" );
			}
		}

		/// <summary>
		///   Looks up a localized string similar to The target collection is read-only..
		/// </summary>
		internal static string XCollectionReadOnlyException
		{
			get
			{
				return ResourceLoader.GetString( "XCollectionReadOnlyException" );
			}
		}

		/// <summary>
		///   Looks up a localized string similar to Source object is not compatible with the current one..
		/// </summary>
		internal static string XObjectSourceNotCompatibleException
		{
			get
			{
				return ResourceLoader.GetString( "XObjectSourceNotCompatibleException" );
			}
		}
	}
}
