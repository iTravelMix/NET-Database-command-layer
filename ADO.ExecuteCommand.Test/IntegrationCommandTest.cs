using System;
using System.IO;
using ADO.ExecuteCommand.Helper;
using ADO.ExecuteCommand.Test.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.ExecuteCommand.Test
{
    [TestClass]
    public class IntegrationCommandTest
    {
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(@"..\..\"));
        }

        [TestMethod]
        public void TestInsertCommand()
        {
            var adoCommand = CommandHelper.CreateHelper("SqlAdoHelper");
            var rowsAffected = adoCommand.ExecuteCommand(new InsertCommandWithParameters(1, "Test 1"));

            Assert.AreEqual(1, rowsAffected);
        }
    }
}
