using Enable.Extensions.Configuration;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

configuration.ApplyDevelopmentOverrides();

Console.WriteLine(configuration["AllPlaceholders"]);
