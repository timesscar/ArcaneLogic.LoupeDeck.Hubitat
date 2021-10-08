// -------------------------------------------------------------------------------------------------
// <copyright file="AppConfigRemapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace Loupedeck.AudioDevicePlugin
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A class to remap app.config files at runtime.
    /// </summary>
    /// <remarks>Adapted from <see href="https://stackoverflow.com/a/6151688"/>.</remarks>
    public abstract class AppConfigRemapper : IDisposable
    {
        /// <summary>
        /// The data field name for the app.config content.
        /// </summary>
        private const string AppConfigDataKey = "APP_CONFIG_FILE";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigRemapper"/> class.
        /// </summary>
        internal AppConfigRemapper()
        {
        }

        /// <summary>
        /// Remaps the app.config to a given path.
        /// </summary>
        /// <param name="path">The path to the new app.config.</param>
        /// <returns>The remapper.</returns>
        public static AppConfigRemapper Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        /// <summary>
        /// Releases resources used by the class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the class.
        /// </summary>
        /// <param name="disposing">Whether or not the object is being disposed rather than finalized.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Private class for use as a singleton.
        /// </summary>
        private class ChangeAppConfig : AppConfigRemapper
        {
            /// <summary>
            /// The default configuration.
            /// </summary>
            private readonly string oldConfig =
                AppDomain.CurrentDomain.GetData(AppConfigDataKey).ToString();

            /// <summary>
            /// Whether or not the object has been disposed.
            /// </summary>
            private bool isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="ChangeAppConfig"/> class.
            /// </summary>
            /// <param name="path">The path to the new app.config.</param>
            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData(AppConfigDataKey, path);
                ResetConfigMechanism();
            }

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
                if (!this.isDisposed && disposing)
                {
                    AppDomain.CurrentDomain.SetData(AppConfigDataKey, this.oldConfig);
                    ResetConfigMechanism();

                    this.isDisposed = true;
                }
            }

            /// <summary>
            /// Resets the configuration mechanism.
            /// </summary>
            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager)
                    .GetField(
                    "s_initState",
                    BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, 0);

                typeof(ConfigurationManager)
                    .GetField(
                    "s_configSystem",
                    BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetTypes()
                    .Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField(
                    "s_current",
                    BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}
