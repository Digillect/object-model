using System;
using System.Diagnostics.Contracts;

namespace Digillect
{
#if DEBUG || CONTRACTS_FULL
	[ContractClassFor(typeof(IXUpdatable<>))]
	abstract class IXUpdatableContract<T> : IXUpdatable<T>
		where T : IXUpdatable<T>
	{
		protected IXUpdatableContract()
		{
		}

		public abstract event EventHandler Updated;

		public abstract void BeginUpdate();
		public abstract void EndUpdate();

#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
		public bool IsUpdateRequired(T source)
		{
			Contract.Requires<ArgumentNullException>(source != null, "source");

			return false;
		}

#if WINDOWS_PHONE && CODE_ANALYSIS
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule")]
#endif
		public void Update(T source)
		{
			Contract.Requires<ArgumentNullException>(source != null, "source");
		}
	}
#endif
}
