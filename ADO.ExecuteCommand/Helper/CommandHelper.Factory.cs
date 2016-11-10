using System;
using System.Collections;
using System.Configuration;

namespace ADO.ExecuteCommand.Helper
{
    public abstract partial class CommandHelper
    {
        #region - Factory -

        public static CommandHelper CreateHelper(string providerAlias)
        {
            try
            {
                var dict = ConfigurationManager.GetSection("daProviders") as IDictionary;
                if (dict == null) throw new Exception("Null Reference in DataAccess Provider configuration Session.");

                var providerConfig = dict[providerAlias] as ProviderAlias;
                if (providerConfig == null) throw new Exception("Null Reference in Provider Alias configuration Session.");

                return CreateHelper(new DataAccessSectionSettings(providerConfig.TypeName, providerConfig.ConnectionString));
            }
            catch (Exception e)
            {
                throw new Exception("If the section is not defined on the configuration file this method can't be used to create an AdoHelper instance.", e);
            }
        }

        public static CommandHelper CreateHelper(DataAccessSectionSettings settings)
        {
            try
            {
                var providerType = settings.Type;

                var daType = Type.GetType(providerType);
                if (daType == null) throw new NullReferenceException("Null Reference in Provider type configuration Session.");

                var provider = Activator.CreateInstance(daType, settings.ConnectionString);
                if (provider is CommandHelper)
                {
                    return provider as CommandHelper;
                }

                throw new Exception("The provider specified does not extends the QueryRunner abstract class.");
            }
            catch (Exception e)
            {
                throw new Exception("If the section is not defined on the configuration file this method can't be used to create an QueryRunner instance.", e);
            }
        }

        #endregion
    }
}
