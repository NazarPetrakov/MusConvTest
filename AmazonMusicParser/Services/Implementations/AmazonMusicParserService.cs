using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AmazonMusicParser.Models;
using AmazonMusicParser.Services.Interfaces;
using Avalonia.Media.Imaging;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace AmazonMusicParser.Services.Implementations;

public class AmazonMusicParserService : IMusicParserService
{
    public async Task<Playlist> ParsePlaylistAsync(string url)
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);

        var chromeService = ChromeDriverService.CreateDefaultService();
        chromeService.HideCommandPromptWindow = true;
        using var driver = new ChromeDriver(chromeService, options);

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        driver.Navigate().GoToUrl(url);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        IWebElement detailHeaderElement;
        string contentType;

        var atfElement = driver.FindElement(By.Id("atf"));
        detailHeaderElement = atfElement.FindElement(By.TagName("music-detail-header"));
        contentType = detailHeaderElement.GetAttribute("label")?.ToLower() ?? "unknown";

        if (contentType == "album")
        {
            wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("music-text-row")));
        }
        else
        {
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//music-image-row//div[@class='col4']//span")));
        }

        await Task.Delay(1000);

        var avatarStringUrl = detailHeaderElement.GetAttribute("image-src")?.Trim() ?? "";

        if (avatarStringUrl.StartsWith("//"))
        {
            avatarStringUrl = "https:" + avatarStringUrl;
        }

        Uri.TryCreate(avatarStringUrl, UriKind.Absolute, out Uri? avatarUri);
        Bitmap? avatarBitmap = new Bitmap("Assets/default_cover.png");

        if (avatarUri != null)
        {
            try
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(avatarUri);
                using var ms = new MemoryStream(imageBytes);
                avatarBitmap = new Bitmap(ms);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load image from {avatarUri}: {ex.Message}");
            }
        }

        var playlist = new Playlist { Songs = new List<Song>(), Avatar = avatarBitmap };

        if (contentType == "album")
        {
            playlist.Name = detailHeaderElement.GetAttribute("headline")?.Trim() ?? "Unknown Album";
            playlist.Description = detailHeaderElement.GetAttribute("tertiary-text")?.Trim() ?? "";
        }
        else
        {
            playlist.Name = detailHeaderElement.GetAttribute("headline")?.Trim() ?? "Unknown Playlist";
            playlist.Description = detailHeaderElement.GetAttribute("secondary-text")?.Trim() ?? "";
        }

        var pageSource = driver.PageSource;
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(pageSource);

        if (contentType == "album")
        {
            var albumArtist = detailHeaderElement.GetAttribute("primary-text")?.Trim() ?? "N/A";
            var albumName = playlist.Name;

            var songNodes = htmlDoc.DocumentNode.SelectNodes("//music-text-row[@data-key]");

            if (songNodes != null)
            {
                foreach (var songNode in songNodes)
                {
                    var songName = songNode.GetAttributeValue("primary-text", "");
                    var durationNode = songNode.SelectSingleNode(".//div[@class='col4']//span");
                    var duration = durationNode?.InnerText?.Trim() ?? "N/A";

                    playlist.Songs.Add(new Song
                    {
                        Name = songName.Trim(),
                        Artist = albumArtist,
                        Album = albumName,
                        Duration = duration
                    });
                }
            }
        }
        else
        {
            var songNodes = htmlDoc.DocumentNode.SelectNodes("//music-image-row[@data-key]");

            if (songNodes != null)
            {
                foreach (var songNode in songNodes)
                {
                    var songName = songNode.GetAttributeValue("primary-text", "");
                    var artistName = songNode.GetAttributeValue("secondary-text-1", "");
                    var albumName = songNode.GetAttributeValue("secondary-text-2", "");

                    var durationNode = songNode.SelectSingleNode(".//div[@class='col4']/music-link/span");
                    var duration = durationNode?.InnerText?.Trim() ?? "N/A";

                    playlist.Songs.Add(new Song
                    {
                        Name = songName.Trim(),
                        Artist = artistName.Trim(),
                        Album = albumName.Trim().Replace(" [Explicit]", ""),
                        Duration = duration
                    });
                }
            }
        }

        return playlist;
    }
}
