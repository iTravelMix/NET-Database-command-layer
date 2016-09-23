using System.Configuration;

namespace ADO.ExecuteCommand.Helper
{
    public class ConfigurationMissingException : ConfigurationErrorsException
    {
        public ConfigurationMissingException() : base("The 'daProvider' node must contain required attributes.")
        {
        }

        public ConfigurationMissingException(string property) : base($"The 'daProvider' node must contain an attribute named '{property}'.") 
        {
        }
    }
}
