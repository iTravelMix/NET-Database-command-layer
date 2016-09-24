using System.Collections.Generic;

namespace ADO.ExecuteCommand.Commands
{
    public abstract class CommandBatch
    {
        public IEnumerable<Command> Commands { get; protected set; }

        public bool ThrowOnError { get; protected set; }
    }
}
