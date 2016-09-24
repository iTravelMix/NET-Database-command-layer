using ADO.ExecuteCommand.Helper;
using ADO.ExecuteCommand.Test.AdoMocks;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.ExecuteCommand.Test
{
    [TestClass]
    public class FactoryTest
    {
        [TestMethod]
        public void TestCreateHelper()
        {
            var mockAdo = CommandHelper.CreateHelper("MockAdoHelper");
            Assert.IsInstanceOfType(mockAdo, typeof(MockCommandHelper));
        }

        [TestMethod]
        public void TestDependencyInjectionCreateHelper()
        {
            var container = new UnityContainer();
            container.RegisterType<CommandHelper>(new InjectionFactory(c => CommandHelper.CreateHelper("MockAdoHelper")));

            var mockAdo = container.Resolve<CommandHelper>();
            Assert.IsInstanceOfType(mockAdo, typeof(MockCommandHelper));
        }
    }
}
