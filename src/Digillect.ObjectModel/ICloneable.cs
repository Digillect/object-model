#if NETSTANDARD
namespace System
{
	internal interface ICloneable
	{
		object Clone();
	}
}
#endif
