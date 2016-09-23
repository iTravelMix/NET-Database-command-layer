using System.Collections.Generic;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Test.Commands
{
    class CommandWithParameters : ICommand
    {
        public CommandWithParameters(int id, string name)
        {
            this.Expression = "insert id,name into table_in_database values (:id,:name)";

            this.Parameters = new Dictionary<string, object>
            {
                {"id", id},
                {"name", name}
            };
        }

        public string Expression { get; }

        public IDictionary<string, object> Parameters { get; }
    }
}
