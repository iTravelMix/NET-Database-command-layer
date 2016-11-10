using System;

namespace ADO.ExecuteCommand.Helper
{
    public abstract partial class CommandHelper
    {
        #region - Factory -

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
