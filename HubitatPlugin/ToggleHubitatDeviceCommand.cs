namespace Loupedeck.HubitatPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using Loupedeck.AudioDevicePlugin;
    using Loupedeck.AudioDevicePlugin.Settings;
    using Loupedeck.HubitatPlugin.Settings;

    using Newtonsoft.Json;

    public class ToggleHubitatDeviceCommand : PluginDynamicCommand
    {
        private HubitatPluginSettingsConfigurationSection config;

        private readonly Dictionary<string, string> imageCache;

        public ToggleHubitatDeviceCommand() : base()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var configPath = Path.Combine(executingAssembly.GetFilePath(), executingAssembly.GetFilePathName() + ".config");
            using (AppConfigRemapper.Change(configPath))
            {
                this.config = HubitatPluginSettingsConfigurationSection.Current;
            }
                
            var devices = this.EnumerateDevicesAsync().GetAwaiter().GetResult();

            foreach(var device in devices)
            {
                this.AddParameter(device.Value.Label, device.Value.Label, "HomeAutomation");
            }

            var currLoc = Assembly.GetExecutingAssembly().GetFilePath();

            this.imageCache = new Dictionary<string, string>();
            foreach (CustomImageElement image in this.config.CustomImages)
            {
                this.imageCache.Add(image.DeviceName, BitmapImage.FromFile(Path.Combine(currLoc, Constants.ImagesFolderName, image.ImageName)).ToBase64String());
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            this.ToggleState(actionParameter).GetAwaiter().GetResult();

            this.ActionImageChanged(actionParameter);
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var devices = this.EnumerateDevicesAsync().GetAwaiter().GetResult();
            var match = devices.Where(c => c.Value.Label == actionParameter).FirstOrDefault();

            var currentState = match.Value.Attributes.Where(c => c.Name == "switch").FirstOrDefault().CurrentValue;

            BitmapImage image;
            if (currentState == "on")
                image = this.WashOutImage(this.imageCache[actionParameter]);
            else
                image = BitmapImage.FromBase64String(this.imageCache[actionParameter]);

            image.Resize(imageSize.GetSize(), imageSize.GetSize());

            return image;
        }

        private async Task ToggleState(String actionParameter)
        {
            var devices = await this.EnumerateDevicesAsync();

            var match = devices.Where(c => c.Value.Label == actionParameter).FirstOrDefault();

            var currentState = match.Value.Attributes.Where(c => c.Name == "switch");

            switch (currentState.FirstOrDefault().CurrentValue)
            {
                case "on":
                    await this.SendCommand("off", match.Key);
                    break;
                case "off":
                    await this.SendCommand("on", match.Key);
                    break;
            }
        }

        private async Task<Dictionary<int, HubitatDevice>> EnumerateDevicesAsync()
        {
            var utility = new DpapiUtility(DataProtectionScope.LocalMachine);
            var accessToken = utility.DecryptData(this.config.DpapiEncryptedApiKey);

            Uri stateQuery = new Uri($"{this.config.HubitatApiUrl}/devices?access_token={accessToken}");

            var client = new HttpClient();
            using (var results = await client.GetAsync(stateQuery))
            {
                var content = await results.Content.ReadAsStringAsync();

                var devices = JsonConvert.DeserializeObject<List<HubitatDevice>>(content);

                var deviceDict = new Dictionary<int, HubitatDevice>();

                foreach (var device in devices)
                {
                    using (var results2 = await client.GetAsync($"{this.config.HubitatApiUrl}/devices/{device.Id}?access_token={accessToken}"))
                    {
                        var complete = JsonConvert.DeserializeObject<HubitatDevice>(await results2.Content.ReadAsStringAsync());
                        deviceDict.Add(device.Id, complete);
                    }
                }

                return deviceDict;
            }
        }

        private async Task SendCommand(string command, int deviceId)
        {
            var utility = new DpapiUtility(DataProtectionScope.LocalMachine);
            var accessToken = utility.DecryptData(this.config.DpapiEncryptedApiKey);

            var foo = $"{this.config.HubitatApiUrl}/devices/{deviceId}/{command}?access_token={accessToken}";
            Uri commandUri = new Uri($"{this.config.HubitatApiUrl}/devices/{deviceId}/{command}?access_token={accessToken}");

            var client = new HttpClient();
            using (var results = await client.GetAsync(commandUri))
            {
                results.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Loads an image and removes some of the color 
        /// </summary>
        /// <param name="base64Image">The base64 encoded image</param>
        /// <returns>The adjusted image.</returns>
        private BitmapImage WashOutImage(string base64Image)
        {
            BitmapImage convertedImage;

            var bytes = Convert.FromBase64String(base64Image);

            using (var inStream = new MemoryStream(bytes))
            {
                using (var image = Bitmap.FromStream(inStream))
                {
                    using (var graphics = Graphics.FromImage(image))
                    {
                        // Apply a .2 multiplier to the rgb color channels
                        float[][] colorMatrixElements = {
                           new float[] {1f,  0,  0,  0, 0},
                           new float[] {0,  1f,  0,  0, 0},
                           new float[] {0,  0,  0,  0, 0},
                           new float[] {0,  0,  0,  1, 0},
                           new float[] { 0, 0, 0, 0, 1}};

                        var matrix = new ColorMatrix(colorMatrixElements);

                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);

                        graphics.Flush();
                    }

                    byte[] result = null;

                    using (var outStream = new MemoryStream())
                    {
                        image.Save(outStream, ImageFormat.Png);
                        result = outStream.ToArray();
                    }

                    convertedImage = new BitmapImage(result);
                }
            }

            return convertedImage;
        }
    }
}
