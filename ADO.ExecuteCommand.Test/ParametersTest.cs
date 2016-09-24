using System;
using System.Collections.Generic;
using System.Linq;
using ADO.ExecuteCommand.Test.AdoMocks;
using ADO.ExecuteCommand.Test.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.ExecuteCommand.Test
{
    [TestClass]
    public class ParametersTest
    {
        [TestMethod]
        public void TestParametersInQuery()
        {
            var commandHelper = new MockCommandHelper
            {
                ReturnValues = new List<IDictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {"Id", 1},
                        { "Name", "Test"}                   
                    }                
                }
            };

            commandHelper.ExecuteCommand(new InsertCommandWithParameters(1, "Test"));

            Assert.IsNotNull(commandHelper.Parameters.SingleOrDefault(p => p.ParameterName == "id"));
            Assert.IsNotNull(commandHelper.Parameters.SingleOrDefault(p => p.ParameterName == "name"));


            // ReSharper disable PossibleNullReferenceException
            var id = Convert.ToInt32(commandHelper.Parameters.SingleOrDefault(p => p.ParameterName == "id").Value);
            var name = Convert.ToString(commandHelper.Parameters.SingleOrDefault(p => p.ParameterName == "name").Value);
            // ReSharper restore PossibleNullReferenceException

            Assert.AreEqual(1, id);
            Assert.AreEqual("Test", name);
        }

        [TestMethod]
        public void TestNullParametersInQuery()
        {
            var queryRunner = new MockCommandHelper
            {
                ReturnValues = new List<IDictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {"Id", 1},
                        { "Name", "Test"}                   
                    }                
                }
            };

            queryRunner.ExecuteCommand(new InsertCommandWithParameters(1, null));

            Assert.IsNotNull(queryRunner.Parameters.SingleOrDefault(p => p.ParameterName == "name"));

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(DBNull.Value, queryRunner.Parameters.SingleOrDefault(p => p.ParameterName == "name").Value);
            // ReSharper restore PossibleNullReferenceException
        }
    }
}
