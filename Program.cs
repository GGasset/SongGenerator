using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Search;
using YoutubeExplode.Common;
using CSVideoConverter;

namespace MillionSongDatasetDownloader
{
    internal class Program
    {
        static readonly int ArtistNameCol = 8, SongNamesCol = 16;

        static void Main(string[] args)
        {
            DownloadConvert().Wait();
        }

        static async Task DownloadConvert()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            if (!projectDirectory.EndsWith(@"\"))
                projectDirectory += @"\";

            string ffmpegPath = projectDirectory + "ffmpeg.exe";
            if (!File.Exists(ffmpegPath))
            {
                await Console.Out.WriteLineAsync("Please unzip ffmpeg.zip so the program will execute.");
                return;
            }

            string songsCsvPath = projectDirectory + "SongCSV.csv";
            StreamReader reader = new StreamReader(songsCsvPath);
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvParser parser = new CsvParser(reader, configuration, true);
            parser.Read();

            string googleKeyFilePath = projectDirectory + "GoogleApiKey.txt";
            if (!File.Exists(googleKeyFilePath))
            {
                await Console.Out.WriteLineAsync("You must include an API key in a file called GoogleApiKey.txt with the key I must provide (Contact: gassetgerman@gmail.com)");
            }
            string googleApiKey = File.ReadAllText(googleKeyFilePath);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = googleApiKey,
                ApplicationName = "SongGenerator"
            });

            YoutubeClient youtube = new YoutubeClient();

            VideoConverter converter = new VideoConverter();
            converter.FFmpegLibsPath = ffmpegPath;
            while (parser.Read())
            {
                string songName = parser[SongNamesCol];
                string artistName = parser[ArtistNameCol];
                string songArtist = $"{songName} - {artistName}".Replace("b'", "").Replace("'", "");

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = songArtist;
                searchListRequest.MaxResults = 3;

                var searchListResponse = searchListRequest.ExecuteAsync();
                searchListResponse.Wait();

                string url = null;
                var searchListResult = searchListResponse.Result;
                foreach (var searchResult in searchListResult.Items)
                {
                    if (searchResult.Id.Kind == "youtube#video")
                    {
                        url = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                        break;
                    }
                }
                await Console.Out.WriteLineAsync("Found video");


                var streamManifestRequest = youtube.Videos.Streams.GetManifestAsync(url);
                await streamManifestRequest;
                var streamManifest = streamManifestRequest.Result;
                var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();

                string path = projectDirectory += $@"Songs\{songArtist}.{streamInfo.Container}";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, path);
                await Console.Out.WriteLineAsync("Downloaded video");

                converter.FileSource = path;

                string convertedPath = projectDirectory + $@"Converted\{songArtist}";
                converter.FileDestination = convertedPath;

                converter.AudioCodec = "WAV";
                converter.AudioBitrate = "100k";
                converter.AudioSamplerate = "44k";
                converter.AudioChannels = "6";

                converter.FromTime = new TimeSpan(0, 0, 0);
                converter.LengthTime = new TimeSpan(0, 0, 0);
                converter.Run();
                Console.WriteLine("Parsed video");

                await Console.Out.WriteLineAsync($"{parser.Row}/100000");
            }
            reader.Close();
            parser.Dispose();
        }

        //static string GetImFeelingLuckyQuery(string searchFor) => $"https://www.google.com/search?q={searchFor}&btnI=Voy+a+tener+suerte".Replace(" ", "+");
    }
}
