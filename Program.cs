using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Csv;
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

        static async void Main(string[] args)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            if (!projectDirectory.EndsWith(@"\"))
                projectDirectory += @"\";
            string ffmpegPath = projectDirectory + "ffmpeg.exe";
            string songsCsvPath = projectDirectory += "SongCSV.csv";
            StreamReader reader = new StreamReader(songsCsvPath);
            IEnumerable<ICsvLine> csv = CsvReader.Read(reader);

            List<string> artistNames = new List<string>(CsvReader.GetColumn(csv, ArtistNameCol));
            List<string> songsNames = new List<string>(CsvReader.GetColumn(csv, SongNamesCol));

            VideoConverter converter = new VideoConverter();
            converter.FFmpegLibsPath = ffmpegPath;
            YoutubeClient youtube = new YoutubeClient();
            for (int i = 0; i < artistNames.Count(); i++)
            {
                string songArtist = $"{songsNames[i]} - {artistNames[i]}";
                var videos = new List<VideoSearchResult>(await youtube.Search.GetVideosAsync(songArtist));
                string url = videos[0].Url;
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
            }
        }

        //static string GetImFeelingLuckyQuery(string searchFor) => $"https://www.google.com/search?q={searchFor}&btnI=Voy+a+tener+suerte".Replace(" ", "+");
    }
}
