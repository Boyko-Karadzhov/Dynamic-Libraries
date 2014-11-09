using System.IO;

namespace Karadzhov.Interop.DynamicLibraries.Tests
{
    public static class Utilities
    {
        public static string TestProjectPath()
        {
            var location = typeof(Utilities).Assembly.Location;
            var path = Path.GetDirectoryName(location);
            return path + "\\..\\..\\";
        }
    }
}
