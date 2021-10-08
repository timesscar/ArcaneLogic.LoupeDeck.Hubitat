namespace Loupedeck.AudioDevicePlugin
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Timers;

    using AudioSwitcher.AudioApi;
    using AudioSwitcher.AudioApi.CoreAudio;

    using Loupedeck.AudioDevicePlugin.Settings;

    /// <summary>
    /// Dynamic adjustment command that updates the image associated with the plugin based on mute state.
    /// </summary>
    public class AdjustMicVolumeCommand : PluginDynamicAdjustment
    {
        private readonly CoreAudioController controller;

        private readonly Timer volumeCooldown;

        private readonly Dictionary<bool, string> imageCache;

        private readonly WindowsAudioPluginSettingsConfigurationSection config;

        /// <summary>
        /// Initializes a new instance of <see cref="AdjustMicVolumeCommand"/>
        /// </summary>
        public AdjustMicVolumeCommand()
            : base("Adjust mic volume", "Adjusts the default communcation device volume.", "Audio", false)
        {
            this.controller = new CoreAudioController();

            MuteDefaultMicCommand.MicMuteStateChangeRequested += this.MuteDefaultMicCommand_MicMuteStateChangeRequested;
            
            var executingAssembly = Assembly.GetExecutingAssembly();
            var configPath = Path.Combine(executingAssembly.GetFilePath(), executingAssembly.GetFilePathName() + ".config");
            using (AppConfigRemapper.Change(configPath))
            {
                this.config = WindowsAudioPluginSettingsConfigurationSection.Current;
            }

            this.volumeCooldown = new Timer(this.config.VolumeChangeCooldown);
            this.volumeCooldown.Elapsed += this.VolumeCooldown_Elapsed;

            var currLoc = Assembly.GetExecutingAssembly().GetFilePath();
            this.imageCache = new Dictionary<bool, string>
            {
                { true, BitmapImage.FromFile(Path.Combine(currLoc, Constants.ImagesFolderName, this.config.MuteIcon)).ToBase64String() },
                { false, BitmapImage.FromFile(Path.Combine(currLoc, Constants.ImagesFolderName, this.config.UnmuteIcon)).ToBase64String() }
            };
        }

        /// <summary>
        /// Triggers when the cooldown elapses for volume changes.
        /// </summary>
        /// <param name="sender">the sender.</param>
        /// <param name="e">event args relating to the action.</param>
        private void VolumeCooldown_Elapsed(Object sender, ElapsedEventArgs e)
        {
            this.volumeCooldown.Enabled = false;
            this.ActionImageChanged();
        }

        /// <summary>
        /// Triggers when the device mute state is changed by <see cref="MuteDefaultMicCommand"/>
        /// </summary>
        /// <param name="sender">the sender.</param>
        /// <param name="e">event args relating to the action.</param>
        private void MuteDefaultMicCommand_MicMuteStateChangeRequested(Object sender, EventArgs e)
        {
            this.ActionImageChanged();
        }

        /// <inheritdoc />
        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var defaultMic = this.controller.GetDefaultDevice(DeviceType.Capture, Role.Communications);

            var newVolume = defaultMic.Volume + diff;

            if (newVolume > 100)
                newVolume = 100;

            if (newVolume < 0)
                newVolume = 0;

            defaultMic.Volume = newVolume;

            this.volumeCooldown.Stop();
            this.volumeCooldown.Start();
            this.AdjustmentValueChanged(actionParameter);
        }

        /// <inheritdoc />
        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var defaultMic = this.controller.GetDefaultDevice(DeviceType.Capture, Role.Communications);

            return defaultMic.Volume.ToString();
        }

        /// <inheritdoc />
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this.volumeCooldown.Enabled)
            {
                var defaultMic = this.controller.GetDefaultDevice(DeviceType.Capture, Role.Communications);

                var volumeString = defaultMic.Volume.ToString();
                var textImage = this.ConvertTextToImage(volumeString, "Consolas", Color.Black, Color.White, imageSize);

                byte[] result = null;

                if (textImage != null)
                {
                    var stream = new MemoryStream();

                    textImage.Save(stream, ImageFormat.Png);
                    result = stream.ToArray();
                }

                return new BitmapImage(result);
            }

            var muteState = this.GetDefaultCommDeviceMuteState();
            var desiredImage = BitmapImage.FromBase64String(this.imageCache[muteState]);

            int dimensions = imageSize.GetSize();
            desiredImage.Resize(dimensions, dimensions);
            return desiredImage;
        }

        /// <summary>
        /// Gets the mute state of the current default communications device.
        /// </summary>
        /// <returns>A value indicating whether or not the device is muted.</returns>
        private bool GetDefaultCommDeviceMuteState()
        {
            var defaultMic = this.controller.GetDefaultDevice(DeviceType.Capture, Role.Communications);

            return defaultMic.IsMuted;
        }

        /// <summary>
        /// Creates a bitmap image of text from a string
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="fontName">The font.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="textColor">The text color.</param>
        /// <param name="size">The image size.</param>
        /// <returns>A bitmap containing the supplied text.</returns>
        public Bitmap ConvertTextToImage(string text, string fontName, Color backgroundColor, Color textColor, PluginImageSize size)
        {
            int dimensions = size.GetSize();

            var bitmap = new Bitmap(dimensions, dimensions);

            var fontSize = dimensions / 3;
            using (var graphics = Graphics.FromImage(bitmap))
            {
                using(var font = new Font(fontName, fontSize))
                {
                    var horizontalSpacer = (dimensions - text.Length * fontSize) / 2;
                    var verticalSpacer = (dimensions - fontSize) / 2;

                    graphics.FillRectangle(new SolidBrush(backgroundColor), 0, 0, dimensions, dimensions);
                    graphics.DrawString(text, font, new SolidBrush(textColor), horizontalSpacer, verticalSpacer);
                    graphics.Flush();
                }
            }

            return bitmap;
        }
    }
}
