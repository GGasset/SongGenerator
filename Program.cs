using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Csv;

namespace MillionSongDatasetDownloader
{
    internal class Program
    {
        static readonly int ArtistNameCol = 8, SongNamesCol = 16;

        static void Main(string[] args)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.FullName;
            if (!projectDirectory.EndsWith(@"\"))
                projectDirectory += @"\";
            string songsCsvPath = projectDirectory += "SongCSV.csv";
            StreamReader reader = new StreamReader(songsCsvPath);
            IEnumerable<ICsvLine> csv = CsvReader.Read(reader);

            List<string> artistNames = new List<string>(CsvReader.GetColumn(csv, ArtistNameCol));
            List<string> songsNames = new List<string>(CsvReader.GetColumn(csv, SongNamesCol));
            for (int i = 0; i < artistNames.Count(); i++)
            {
                string songArtist = $"{songsNames[i]} - {artistNames[i]}";
                string query = GetImFeelingLuckyQuery(songArtist);
            }
        }

        static string GetImFeelingLuckyQuery(string searchFor) => $"https://www.google.com/search?q={searchFor}&btnI=Voy+a+tener+suerte".Replace(" ", "+");
    }
}
