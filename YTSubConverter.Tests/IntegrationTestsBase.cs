using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace YTSubConverter.Tests
{
    public abstract class IntegrationTestsBase
    {
        protected static string DllFolderPath => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);

        [SetUp]
        public void Setup()
        {
            // Verify that time and number separators are culture-independent
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fi");
        }
    }
}
