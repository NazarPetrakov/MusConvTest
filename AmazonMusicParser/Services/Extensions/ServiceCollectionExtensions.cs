using AmazonMusicParser.Services.Implementations;
using AmazonMusicParser.Services.Interfaces;
using AmazonMusicParser.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AmazonMusicParser.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<IMusicParserService, AmazonMusicParserService>();
        collection.AddTransient<MainWindowViewModel>();
    }
}
