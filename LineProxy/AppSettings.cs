using System.IO;
using Microsoft.Extensions.Configuration;

namespace LineProxy
{
    public class AppSettings
    {
        public class AzureSettings
        {
            public string InstrumentationKey { get; set; }
        }

        public string ApplicationName { get; set; }

        public AzureSettings Azure { get; set; }

        private AppSettings()
        {
        }

        public static AppSettings Current { get; private set; }

        public static void Load()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            Current = new AppSettings {Azure = new AzureSettings()};
            config.Bind(Current);
            config.Bind("Azure", Current.Azure);
        }
    }
}