namespace Loupedeck.HubitatPlugin
{
    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class HubitatAttribute
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "currentValue")]
        public string CurrentValue { get; set; }

        [JsonProperty(PropertyName = "dataType")]
        public string DataType { get; set; }
    }
}
