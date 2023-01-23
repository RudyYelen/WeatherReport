using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherReport;

namespace WeatherReport {
    public class City {

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("coord")]
        public Coord coord { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("population")]
        public int population { get; set; }

        [JsonProperty("timezone")]
        public int timezone { get; set; }

        [JsonProperty("sunrise")]
        public int sunrise { get; set; }

        [JsonProperty("sunset")]
        public int sunset { get; set; }
    }

    public class WeatherDataForecast {

        [JsonProperty("cod")]
        public string cod { get; set; }

        [JsonProperty("message")]
        public int message { get; set; }

        [JsonProperty("cnt")]
        public int cnt { get; set; }

        [JsonProperty("list")]
        public IList<WeatherData> list { get; set; }

        [JsonProperty("city")]
        public City city { get; set; }
    }
}
