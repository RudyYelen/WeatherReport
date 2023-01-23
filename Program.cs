using System;
using System.Threading.Tasks;
using DSharpPlus;
using WeatherReport;

namespace MyFirstBot {
    class Program {
        static void Main(string[] args) {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}