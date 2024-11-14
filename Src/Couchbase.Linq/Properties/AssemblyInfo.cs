using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("f9de7771-ec88-4eae-a039-2bdcfa8f7c3b")]

#if DEBUG
// For writing tests against internal classes
[assembly: InternalsVisibleTo("Couchbase.Linq.UnitTests")]
[assembly: InternalsVisibleTo("Couchbase.Linq.IntegrationTests")]

// For using Moq against internal classes
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif