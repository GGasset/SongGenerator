using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
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
            DownloadConvert();
        }

        static async void DownloadConvert()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            if (!projectDirectory.EndsWith(@"\"))
                projectDirectory += @"\";

            string ffmpegPath = projectDirectory + "ffmpeg.exe";
            if (!File.Exists(ffmpegPath))
            {
                await Console.Out.WriteLineAsync("Please uzip ffmpeg.zip so the program will execute.");
                return;
            }

            string songsCsvPath = projectDirectory += "SongCSV.csv";
            StreamReader reader = new StreamReader(songsCsvPath);
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvParser parser = new CsvParser(reader, configuration, true);
            parser.Read();

            VideoConverter converter = new VideoConverter();
            converter.FFmpegLibsPath = ffmpegPath;
            YoutubeClient youtube = new YoutubeClient();
            while (parser.Read())
            {
                string songName = parser[SongNamesCol];
                string artistName = parser[ArtistNameCol];
                string songArtist = $"{songName} - {artistName}".Replace("b'", "").Replace("'", "");
                IReadOnlyList<ISearchResult> videos;
                try
                {
                    videos = await youtube.Search.GetResultsAsync(songArtist);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                string url = null;
                for (int i = 0; url == null; i++)
                {
                    switch (videos[i])
                    {
                        case VideoSearchResult video:
                            url = video.Url;
                            break;
                        default:
                            break;
                    }
                }
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
                var streamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();

                string path = $@"Songs\{songArtist}.{streamInfo.Container}";
                await youtube.Videos.Streams.DownloadAsync(streamInfo, path);

                string convertedPath = projectDirectory + $@"Converted\{songArtist}";
                converter.FileSource = projectDirectory + path;
                converter.FileDestination = convertedPath;

                converter.AudioCodec = "WAV";
                converter.AudioBitrate = "100k";
                converter.AudioSamplerate = "44k";
                converter.AudioChannels = "6";

                converter.FromTime = new TimeSpan(0, 0, 0);
                converter.LengthTime = new TimeSpan(0, 0, 0);
                converter.Run();

                if (parser.Row % 10 == 0)
                    await Console.Out.WriteLineAsync($"{parser.Row}/100000");
            }
            reader.Close();
            parser.Dispose();
        }

        //static string GetImFeelingLuckyQuery(string searchFor) => $"https://www.google.com/search?q={searchFor}&btnI=Voy+a+tener+suerte".Replace(" ", "+");
    }
}
