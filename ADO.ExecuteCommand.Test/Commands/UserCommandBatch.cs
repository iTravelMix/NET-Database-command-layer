using System.Collections.Generic;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Test.Commands
{
    public class UserCommandBatch: CommandBatch
    {
        public UserCommandBatch()
        {
            this.Commands = new List<Command>
            {
                new InsertCommandWithParameters(1, "Test batch"),
                new UpdateCommandWithParameters(1, "update test batch")
            };
        }
    }
}
