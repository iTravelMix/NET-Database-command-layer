namespace ADO.ExecuteCommand.Helper
{
    public class DataAccessSectionSettings
    {
        public DataAccessSectionSettings(string type, string connectionString)
        {
            Type = type;
            ConnectionString = connectionString;
        }

        public string Type { get; }
        public string ConnectionString { get; }
    }
}
