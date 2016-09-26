using ADO.ExecuteCommand.Commands;

namespace ADO.ExecuteCommand.Test.Commands
{
    class CommandSimple : Command
    {
        public CommandSimple()
        {
            this.Expression = "Insert ....";
        }
    }
}
