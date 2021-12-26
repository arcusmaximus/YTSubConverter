using System;
using System.IO;
using System.Reflection;

namespace YTSubConverter.Tests
{
    public abstract class IntegrationTestsBase
    {
        protected static string DllFolderPath => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
    }
}
