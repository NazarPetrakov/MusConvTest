using System;
using System.Collections.Generic;

namespace AmazonMusicParser.Models;

public class Playlist
{
    public string Name { get; set; } = "";
    public Uri? AvatarUrl { get; set; }
    public string Description { get; set; } = "";
    public List<Song> Songs { get; set; } = new();
}
