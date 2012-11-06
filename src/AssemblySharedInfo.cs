using System;
using System.Reflection;
using System.Resources;

[assembly: AssemblyDescription("Digillect Common Libraries: Core")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("Actis Systems")]
[assembly: AssemblyProduct("Digillect® Common Libraries")]
[assembly: AssemblyCopyright("© 2002-2012 Actis Systems. All rights reserved.")]
[assembly: AssemblyTrademark("Digillect is a registered trademark of Actis Systems.")]

[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]
