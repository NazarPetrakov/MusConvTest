using System.Threading.Tasks;
using AmazonMusicParser.Models;
using AmazonMusicParser.Services.Interfaces;

namespace AmazonMusicParser.Services.Implementations;

public class AmazonMusicParserService : IMusicParserService
{
    public Task<Playlist> ParsePlaylistAsync(string url)
    {
        throw new System.NotImplementedException();
    }
}
