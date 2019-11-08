using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MovieRecommendations.Model
{
    /// <summary>
    /// Repository pour manipuler les films.
    /// </summary>
    public static class MovieRepository
    {
        #region Private Fields

        private static readonly Lazy<List<Movie>> movies = new Lazy<List<Movie>>(() => LoadMovieData(moviesDataPath));

        /// <summary>
        /// Chemin vers le fichier des films.
        /// </summary>
        private static readonly string moviesDataPath = Path.Combine(Environment.CurrentDirectory, "Data/recommendation-movies.csv");

        private static List<Movie> LoadMovieData(string moviesDataPath)
        {
            var result = new List<Movie>();
            var fileReader = File.OpenRead(moviesDataPath);
            using (var reader = new StreamReader(fileReader))
            {
                bool header = true;
                int index = 0;
                var line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    int movieId = int.Parse(fields[0].ToString().TrimStart(new char[] { '0' }));
                    string movieTitle = fields[1].ToString();
                    result.Add(new Movie() { Id = movieId, Title = movieTitle });
                    index++;
                }
            }

            return result;
        }

        #endregion Private Fields

        #region Public Fields

        /// <summary>
        /// Obtient tous les films
        /// </summary>
        public static List<Movie> All = movies.Value;

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Obtient un film via son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du film recherché.</param>
        /// <returns>Film recherché.</returns>
        public static Movie Get(int id) => All.Single(m => m.Id == id);

        #endregion Public Methods
    }
}
