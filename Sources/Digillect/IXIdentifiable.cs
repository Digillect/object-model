using System;

namespace Digillect
{
	public interface IXIdentifiable<TId>
	{
		TId Id
		{
			get;
		}
	}
}
