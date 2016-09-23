using System.Collections.Generic;

namespace ADO.ExecuteCommand.Commands
{
    public interface ICommandBatch 
    {
        string[] Sql { get; }
        IDictionary<string, object>[] Parameters { get; }

        bool ThrowOnError { get; }
    }
}
