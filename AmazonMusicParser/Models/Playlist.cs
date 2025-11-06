using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace AmazonMusicParser.Models;

public class Playlist
{
    public string Name { get; set; } = "";
    public Bitmap? Avatar { get; set; }
    public string Description { get; set; } = "";
    public List<Song> Songs { get; set; } = new();
}
