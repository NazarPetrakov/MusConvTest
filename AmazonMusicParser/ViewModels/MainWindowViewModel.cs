using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AmazonMusicParser.Models;
using AmazonMusicParser.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmazonMusicParser.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IMusicParserService _musicParserService1;
    [ObservableProperty]
    private bool _isLoading = true;
    public MainWindowViewModel(IMusicParserService musicParserService)
    {
        _musicParserService1 = musicParserService;
        LoadAlbums();
    }
    public ObservableCollection<Playlist> Playlists { get; } = new();
    private async void LoadAlbums()
    {
        IsLoading = true;

        var urlsFilePath = "Assets/urlsToParse.txt";
        var urls = File.ReadLines(urlsFilePath).ToList();

        var parsingTasks = urls.Select(url =>
        {
            return Task.Run(async () =>
            {
                return await _musicParserService1.ParsePlaylistAsync(url);
            });
        }).ToList();

        try
        {
            Playlist[] results = await Task.WhenAll(parsingTasks);

            foreach (var playlist in results.Where(p => p != null && p.Songs != null && p.Songs.Any()))
            {
                Playlists.Add(playlist);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"A parsing task failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }

    }
}
