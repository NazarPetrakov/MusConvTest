using System.Threading.Tasks;
using AmazonMusicParser.Models;

namespace AmazonMusicParser.Services.Interfaces;

public interface IMusicParserService
{
    Task<Playlist> ParsePlaylistAsync(string url);
}
