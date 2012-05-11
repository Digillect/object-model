using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Digillect")]

[assembly: ComVisible(false)]

#if CONTRACTS_FULL
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Suppress all those annoying warnings about excess validation")]
#endif
