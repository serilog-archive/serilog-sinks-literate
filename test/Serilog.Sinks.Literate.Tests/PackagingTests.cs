using System.Reflection;
using Xunit;

namespace Serilog.Sinks.Literate.Tests
{
    public class PackagingTests
    {
        [Fact]
        public void AssemblyVersionIsSet()
        {
            var assembly = typeof(LoggerConfigurationLiterateExtensions).GetTypeInfo().Assembly;
            Assert.Equal("3", assembly.GetName().Version.ToString(1));
        }
    }
}
