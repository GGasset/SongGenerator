using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Diagnostics;
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
using System.Threading;

namespace MillionSongDatasetDownloader
{
    internal class Program
    {
        static readonly int ArtistNameCol = 1, SongNamesCol = 2;

        static void Main(string[] args)
        {
            Task mainTask = DownloadConvert();
            mainTask.Wait();
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
                await Console.Out.WriteLineAsync("Please unzip ffmpeg.zip so the program can execute.");
                return;
            }

            string songsCsvPath = projectDirectory + $@"spotify_final_dataset.csv";
            if (!File.Exists(songsCsvPath))
            {
                Console.WriteLine("Please unzip SongsDataset.zip so the program can execute");
            }

            StreamReader reader = new StreamReader(songsCsvPath);
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvParser parser = new CsvParser(reader, configuration, true);
            parser.Read();

            /*string googleKeyFilePath = projectDirectory + "GoogleApiKey.txt";
            if (!File.Exists(googleKeyFilePath))
            {
                await Console.Out.WriteLineAsync("You must include an API key in a file called GoogleApiKey.txt with the key I must provide (Contact: gassetgerman@gmail.com)");
                return;
            }
            string googleApiKey = File.ReadAllText(googleKeyFilePath);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = googleApiKey,
                ApplicationName = "SongGenerator"
            });*/

            string googleKeyFilePath = projectDirectory + "GoogleApiKey.txt";
            if (!File.Exists(googleKeyFilePath))
            {
                await Console.Out.WriteLineAsync("You must include an API key in a file called GoogleApiKey.txt with the key I must provide (Contact: gassetgerman@gmail.com)");
                await Console.Out.WriteLineAsync("Files will download at a slower pace if you don't add the key.");
            }
            string googleApiKey = File.ReadAllText(googleKeyFilePath);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = googleApiKey,
                ApplicationName = "SongGenerator"
            });
            YoutubeClient youtube = new YoutubeClient();

            //VideoConverter converter = new VideoConverter("germa", "2122", projectDirectory);
            //converter.FFmpegLibsPath = ffmpegPath;
            for (int i = 0; i < Directory.GetFiles(projectDirectory + $@"Songs\").Length; i++)
            {
                parser.Read();
            }
            while (parser.Read())
            {
                try
                {
                    string songName = parser[SongNamesCol];
                    string artistName = parser[ArtistNameCol];
                    string songArtist = RemoveIllegalChars($"{songName} - {artistName}");

                    string url;

                    /*var searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.Q = songArtist;
                    searchListRequest.MaxResults = 3;

                    var searchListResponse = searchListRequest.ExecuteAsync();
                    searchListResponse.Wait();

                    var searchListResult = searchListResponse.Result;
                    foreach (var searchResult in searchListResult.Items)
                    {
                        if (searchResult.Id.Kind == "youtube#video")
                        {
                            url = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                            break;
                        }
                    }*/
                    try
                    {
                        var searchListRequest = youtubeService.Search.List("snippet");
                        searchListRequest.Q = songArtist;
                        searchListRequest.MaxResults = 3;

                        var searchListResponse = searchListRequest.ExecuteAsync();
                        searchListResponse.Wait();

                        var searchListResult = searchListResponse.Result;
                        foreach (var searchResult in searchListResult.Items)
                        {
                            if (searchResult.Id.Kind == "youtube#video")
                            {
                                url = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        var videos = await youtube.Search.GetVideosAsync(songArtist);
                        url = videos[0].Url;
                    }
                    await Console.Out.WriteLineAsync("Found video");


                    var streamManifestRequest = youtube.Videos.Streams.GetManifestAsync(url);
                    await streamManifestRequest;
                    var streamManifest = streamManifestRequest.Result;
                    var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();

                    string downloadPath = projectDirectory + $@"Songs\{songArtist}.{streamInfo.Container}";
                    await youtube.Videos.Streams.DownloadAsync(streamInfo, downloadPath);
                    await Console.Out.WriteLineAsync("Downloaded video");

                    /*string convertedPath = projectDirectory + $@"Converted\{songArtist}.wav";
                    string command = $@"/C {ffmpegPath} -i {downloadPath} {convertedPath}";
                    Process process = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "cmd.exe",
                            WindowStyle = ProcessWindowStyle.Normal,
                            Arguments = command
                        },
                    };
                    process.Start();
                    while (!File.Exists(convertedPath))
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("Finished conversion");*/

                    /*converter.FileSource = path;

                    converter.FileDestination = convertedPath;

                    converter.AudioCodec = "WAV";
                    converter.AudioBitrate = "100k";
                    converter.AudioSamplerate = "44k";
                    converter.AudioChannels = "6";

                    converter.FromTime = new TimeSpan(0, 0, 0);
                    converter.LengthTime = new TimeSpan(0, 0, 0);
                    converter.Run();
                    Console.WriteLine("Parsed video");*/

                    await Console.Out.WriteLineAsync($"{parser.Row}/100000");

                }
                catch (Exception)
                {

                }
            }
            reader.Close();
            parser.Dispose();
        }

        //static string GetImFeelingLuckyQuery(string searchFor) => $"https://www.google.com/search?q={searchFor}&btnI=Voy+a+tener+suerte".Replace(" ", "+");

        static string RemoveIllegalChars(string input) => input.Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace("*", "").Replace("?", "").Replace(":", "").Replace(" ", "");
    }
}
