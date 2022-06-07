using Monobank.Core;
using NUnit.Framework;

namespace Monobank.Tests
{
    public class TestsBase
    {
        protected MonoClient Instance;

        [SetUp]
        public void Setup()
        {
            Instance = new MonoClient();
        }
    }
}