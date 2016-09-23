using System.Collections.Generic;

namespace ADO.ExecuteCommand.Commands
{
    public interface ICommand
    {
        string Expression { get; }
        IDictionary<string, object> Parameters { get; }
    }
}