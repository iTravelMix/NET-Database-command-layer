using System.Collections.Generic;
using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Test.Commands
{
    class InsertCommandWithParameters : Command
    {
        public InsertCommandWithParameters(int id, string name)
        {
            this.Expression = "insert into user (id,name) values (:id,:name)";

            this.Parameters = new Dictionary<string, object>
            {
                {"id", id},
                {"name", name}
            };
        }
    }

    class UpdateCommandWithParameters : Command
    {
        public UpdateCommandWithParameters(int id, string name)
        {
            this.Expression = "update user set name=:name where id =:id";

            this.Parameters = new Dictionary<string, object>
            {
                {"id", id},
                {"name", name}
            };
        }
    }
}
