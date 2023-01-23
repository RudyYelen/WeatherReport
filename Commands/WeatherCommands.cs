using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherReport.Commands {
    public class WeatherCommands : BaseCommandModule {
        private string APIKey = "c6cd94e38bcf644302b34641eadc7c29";
        private string defaultLocation;

        [Command("setlocation")]
        [Description("Sets location to use as a default")]
        public async Task SetLocation(CommandContext ctx, 
            [Description("Location to set as a default")][RemainingText] string location) {
            defaultLocation = location;
            await ctx.Channel.SendMessageAsync(String.Format("Default location set to **{0}**", GetWeatherData(defaultLocation).name));
        }

        [Command("weather")]
        public async Task Weather(CommandContext ctx) {
            if (defaultLocation == null) {
                await ctx.Channel.SendMessageAsync("Default location not set. You can set default location using ?setlocation command");
                return;
            }
            await ctx.RespondAsync(GenerateEmbed(GetWeatherData(defaultLocation)).Build());
        }

        [Command("weather")]
        [Description("Shows current weather for specified location, uses default if no location given")]
        public async Task Weather(CommandContext ctx,
            [Description("Location to show weather for")][RemainingText] string location) {
            await ctx.RespondAsync(GenerateEmbed(GetWeatherData(location)).Build());
        }

        [Command("forecast")]
        public async Task Forecast(CommandContext ctx) {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, GeneratePagesForecast(GetWeatherDataForecast(defaultLocation)));
        }

        [Command("forecast")]
        [Description("Gives 5 day weather forecast for specified location, uses default if no location given")]
        public async Task Forecast(CommandContext ctx,
            [Description("Location to show forecast for")][RemainingText] string location) {
            var interactivity = ctx.Client.GetInteractivity();

            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, GeneratePagesForecast(GetWeatherDataForecast(location)));
        }

        [Command("weatheron")]
        public async Task WeatherOn(CommandContext ctx, 
            [Description("Date to give forecast for")]string date) {
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member,
                GeneratePagesForecast(GetWeatherAt(defaultLocation, date)));
        }

        [Command("weatheron")]
        [Description("Gives forecast for specified location for a specified date, uses default if no location given")]
        public async Task WeatherOn(CommandContext ctx, 
            [Description("Location to show weather for")][RemainingText] string location,
            [Description("Date to give forecast for")] string date) {
            await ctx.Channel.SendPaginatedMessageAsync(ctx.Member,
                GeneratePagesForecast(GetWeatherAt(location, date)));
        }

        public WeatherData GetWeatherData(string location) {
            //Get data from Open Weather API
            string url = String.Format("https://api.openweathermap.org/data/2.5/weather?q={0}&appid={1}",
                location,
                APIKey);
            var json = string.Empty;
            using (var web = new HttpClient()) 
                json = web.GetStringAsync(url).Result;
      
            var weatherData = JsonConvert.DeserializeObject<WeatherData>(json);

            return weatherData;
        }

        private WeatherDataForecast GetWeatherDataForecast(string location) {
            //Get forecast data from Open Weather API
            string url = String.Format("https://api.openweathermap.org/data/2.5/forecast?q={0}&appid={1}",
                location,
                APIKey);
            var json = string.Empty;
            using (var web = new HttpClient())
                json = web.GetStringAsync(url).Result;

            var weatherDataForecast = JsonConvert.DeserializeObject<WeatherDataForecast>(json);

            return weatherDataForecast;
        }

        private DiscordEmbedBuilder GenerateEmbed(WeatherData weatherData) {
            //Create Embeded message with weather data
            var embed = new DiscordEmbedBuilder {
                Title = String.Format("Current weather in {0}", weatherData.name),
                Url = String.Format("https://openweathermap.org/city/{0}", weatherData.id),
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(weatherData.dt + weatherData.timezone - 3600)
            };
            embed.WithThumbnail(
                String.Format("http://openweathermap.org/img/wn/{0}@2x.png",
                weatherData.weather[0].icon),
                0, 0);
            embed.WithFooter("Data by OpenWeatherMap",
                "https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/icons/logo_16x16.png");
            embed.AddField("Temperature",
                String.Format("{0:0.0}°C", weatherData.main.temp - 273.15),
                true);
            embed.AddField("Feels like",
                String.Format("{0:0.0}­°C", weatherData.main.feels_like - 273.15),
                true);
            embed.AddField("Weather",
                weatherData.weather[0].description,
                true);
            embed.AddField("Wind speed",
                String.Format("{0} m/s", weatherData.wind.speed),
                true);
            embed.AddField("Humidity",
                String.Format("{0}%", weatherData.main.humidity),
                true);
            embed.AddField("Cloudniess",
                String.Format("{0}%", weatherData.clouds.all),
                true);

            return embed;
        }

        private IEnumerable<Page> GeneratePagesForecast(WeatherDataForecast weatherDataForecast) {
            //Generate List of pages with embeded forecast
            IList<Page> pages = new List<Page>();
            var count = 1;

            foreach (WeatherData weatherData in weatherDataForecast.list) {
                var embed = new DiscordEmbedBuilder {
                    Title = String.Format("Weather forecast for {0}", weatherDataForecast.city.name),
                    Url = String.Format("https://openweathermap.org/city/{0}", weatherDataForecast.city.id)
                };
                embed.WithThumbnail(
                    String.Format("http://openweathermap.org/img/wn/{0}@2x.png",
                    weatherData.weather[0].icon),
                    0, 0);
                embed.WithFooter(String.Format("Data by OpenWeatherMap | Page {0}/{1}", count++, weatherDataForecast.list.LongCount()),
                    "https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/icons/logo_16x16.png");
                embed.AddField("Temperature",
                    String.Format("{0:0.0}°C", weatherData.main.temp - 273.15),
                    true);
                embed.AddField("Feels like",
                    String.Format("{0:0.0}­°C", weatherData.main.feels_like - 273.15),
                    true);
                embed.AddField("Weather",
                    weatherData.weather[0].description,
                    true);
                embed.AddField("Wind speed",
                    String.Format("{0} m/s", weatherData.wind.speed),
                    true);
                embed.AddField("Humidity",
                    String.Format("{0}%", weatherData.main.humidity),
                    true);
                embed.AddField("Cloudniess",
                    String.Format("{0}%", weatherData.clouds.all),
                    true);
                embed.AddField("Time",
                    String.Format("{0:HH:mm - dd.MM.yyyy}", 
                    DateTimeOffset.FromUnixTimeSeconds(weatherData.dt + weatherData.timezone - 3600)));
                //Add page to the list
                pages.Add(new Page("", embed));
            }

            return pages.AsEnumerable();
        }

        private WeatherDataForecast GetWeatherAt(string location, string dateString) {
            //Gets forecast for a specific day
            var forecast = GetWeatherDataForecast(location);
            DateTimeOffset date = DateTimeOffset.Parse(dateString);

            IList<WeatherData> listAt = new List<WeatherData>();

            foreach (WeatherData data in forecast.list) {
                if (date.Date == DateTimeOffset.FromUnixTimeSeconds(data.dt).Date) {
                    listAt.Add(data);
                }
            }

            WeatherDataForecast forecastAt = new WeatherDataForecast {
                list = listAt,
                city = forecast.city
            };

            return forecastAt;
        }
    }
}
