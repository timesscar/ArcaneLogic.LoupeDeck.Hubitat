
namespace Loupedeck.HubitatPlugin.Settings
{
    using System;
    using System.Configuration;
    using System.Reflection;

    using Loupedeck.AudioDevicePlugin.Settings;

    /// <summary>
    /// Configuration manager custom section for domain controlers parameters.
    /// </summary>
    public class HubitatPluginSettingsConfigurationSection : ConfigurationSection
    {
        public static string DefaultSectionName = "hubitatPluginSettings";

        /// <summary>
        /// Gets the default config section.
        /// </summary>
        public static HubitatPluginSettingsConfigurationSection Current
        {
            get
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                try
                {
                    var section = ConfigurationManager.GetSection(DefaultSectionName);
                    return ConfigurationManager.GetSection(DefaultSectionName) as HubitatPluginSettingsConfigurationSection;
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
            }
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        [ConfigurationProperty(
            nameof(HubitatApiUrl),
            IsRequired = true)]
        public string HubitatApiUrl
        {
            get
            {
                return (string)this[nameof(this.HubitatApiUrl)];
            }
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        [ConfigurationProperty(
            nameof(DpapiEncryptedApiKey),
            IsRequired = true)]
        public string DpapiEncryptedApiKey
        {
            get
            {
                return (string)this[nameof(this.DpapiEncryptedApiKey)];
            }
        }

        /// <summary>
        /// Gets the jit groups.
        /// </summary>
        [ConfigurationProperty(nameof(CustomImages))]
        public CustomImageElementCollection CustomImages
        {
            get
            {
                return (CustomImageElementCollection)this[nameof(this.CustomImages)];
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(Object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("MattUniversalPlugin"))
            {
                return Assembly.GetExecutingAssembly();
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    return assembly;
                }
            }

            return null;
        }
    }
}
