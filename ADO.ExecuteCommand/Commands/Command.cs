using System.Collections.Generic;

namespace ADO.ExecuteCommand.Commands
{
    public abstract class Command
    {
        public string Expression { get; protected set; }
        public IDictionary<string, object> Parameters { get; protected set; }
    }
}