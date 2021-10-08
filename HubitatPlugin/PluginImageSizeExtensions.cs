namespace Loupedeck.AudioDevicePlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class PluginImageSizeExtensions
    {
        public static int GetSize(this PluginImageSize size)
        {
            switch (size)
            {
                case PluginImageSize.Width90:
                    return 90;
                case PluginImageSize.Width60:
                default:
                    return 60;
            }
        }
    }
}
