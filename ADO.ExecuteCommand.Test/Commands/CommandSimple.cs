using System.Collections.Generic;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Test.Commands
{
    class CommandSimple : Command
    {
        public CommandSimple()
        {
            this.Expression = "Insert ....";
        }

        public string Expression { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
    }
}
