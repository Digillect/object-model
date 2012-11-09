using System.Reflection;
using System.Resources;

[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.ProductVersion)]
[assembly: SatelliteContractVersion(AssemblyInfo.SatelliteContractVersion)]

internal static class AssemblyInfo
{
	public const string Major = "4";
	public const string Minor = "0";
	public const string Revision = "0";
	public const string BuildNumber = "0";

	public const string Version = Major + "." + Minor + "." + Revision + "." + BuildNumber;
	public const string FileVersion = Major + "." + Minor + "." + Revision + "." + BuildNumber;
	public const string ProductVersion = Major + "." + Minor + "." + Revision;
	public const string SatelliteContractVersion = Major + "." + Minor + ".0.0";
}
