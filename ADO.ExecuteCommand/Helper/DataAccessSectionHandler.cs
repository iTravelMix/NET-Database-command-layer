
using System.Collections;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Xml;

namespace ADO.ExecuteCommand.Helper
{
    public sealed class DataAccessSectionHandler : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        /// <summary>
        /// Ver <see cref="IConfigurationSectionHandler.Create"/> 
        /// </summary>
        public object Create(object parent, object configContext, XmlNode section)
        {
            var ht = new Hashtable();
            var list = section.SelectNodes( "daCommandProvider" );

            if (list == null)
            {
                return ht;
            }

            foreach (XmlNode prov in list)
            {
                if (prov.Attributes == null) throw new ConfigurationMissingException();
                if (prov.Attributes["alias"] == null) throw new ConfigurationMissingException("alias");
                if (prov.Attributes["type"] == null) throw new ConfigurationMissingException("type");
                if (prov.Attributes["connectionStringName"] == null) throw new ConfigurationMissingException("connectionStringName");

                ht[prov.Attributes["alias"].Value] = new ProviderAlias(prov.Attributes["type"].Value, prov.Attributes["connectionStringName"].Value);
            }

            return ht;
        }

        #endregion
    }

    public sealed class ProviderAlias
    {
        public ProviderAlias( string typeName, string connectionString )
        {
            this.TypeName = typeName;
            this.ConnectionString = (CallContext.GetData("CONN_STRING") != null) ? (string)CallContext.GetData("CONN_STRING") : ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
        }

        public string TypeName { get; private set; }
        public string ConnectionString { get; private set; }
    }
}