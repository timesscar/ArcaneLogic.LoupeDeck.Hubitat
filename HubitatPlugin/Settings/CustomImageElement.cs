
namespace Loupedeck.AudioDevicePlugin.Settings
{
    using System.Configuration;

    /// <summary>
    /// Configuration manager custom section for custom images
    /// </summary>
    public class CustomImageElement : ConfigurationSection
    {

        /// <summary>
        /// Gets the device name.
        /// </summary>
        [ConfigurationProperty(nameof(DeviceName), IsRequired = true)]
        public string DeviceName
        {
            get
            {
                return (string)this[nameof(this.DeviceName)];
            }
        }

        /// <summary>
        /// Gets the image name.
        /// </summary>
        [ConfigurationProperty(nameof(ImageName), IsRequired = true)]
        public string ImageName
        {
            get
            {
                return (string)this[nameof(this.ImageName)];
            }
        }
    }
}
