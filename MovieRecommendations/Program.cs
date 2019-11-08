using System;
using System.IO;
using System.Linq;

using Microsoft.ML;
using Microsoft.ML.Trainers;

using MovieRecommendations.Model;

namespace MovieRecommendations
{
    class Program
    {
        #region Private Fields

        /// <summary>
        /// Chemin vers les données de tests.
        /// </summary>
        private static readonly string testDataPath = Path.Combine(Environment.CurrentDirectory, "Data/recommendation-ratings-test.csv");

        /// <summary>
        /// Chemin vers les données d'entrainement.
        /// </summary>
        private static readonly string trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data/recommendation-ratings-train.csv");

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        /// Point d'entrée du programme.
        /// </summary>
        /// <param name="args">Arguments du programme.</param>
        static void Main(string[] args)
        {
            // Création d'un contexte de machine learning.
            var mlContext = new MLContext();

            // Chargement des données d'entrainement et de tests.
            var trainingDataView = mlContext.Data.LoadFromTextFile<MovieRating>(trainingDataPath, hasHeader: true, separatorChar: ',');
            var testDataView = mlContext.Data.LoadFromTextFile<MovieRating>(testDataPath, hasHeader: true, separatorChar: ',');

            // Options de factorisation de la matrice.
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "MovieIdEncoded",
                LabelColumnName = "Label",
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            /**
             * Pipeline d'entrainement.
             * Step 1 : Map des identifiants(user et movie) à des clés.
             */
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(
                     inputColumnName: "UserId",
                     outputColumnName: "UserIdEncoded")
                 .Append(mlContext.Transforms.Conversion.MapValueToKey(
                     inputColumnName: "MovieId",
                     outputColumnName: "MovieIdEncoded"))
                 // Step 2 : Recherche des recommendations avec de la factorisation de matrice.
                 .Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));

            // Entrainement du modèle.
            Console.WriteLine("Entrainement du modèle...");
            var model = pipeline.Fit(trainingDataView);
            Console.WriteLine();

            // Evaluation du modèle.
            Console.WriteLine("Evaluation du modèle...");
            var predictions = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(
                predictions,
                labelColumnName: nameof(MovieRatingPrediction.Label),
                scoreColumnName: nameof(MovieRatingPrediction.Score));
            Console.WriteLine($"  R2:   {metrics.RSquared:#.##} | Doit s'approcher de 1.00");
            Console.WriteLine($"  MAE:  {metrics.MeanAbsoluteError:#.##} | Doit s'approcher de 0.00");
            Console.WriteLine($"  MSE:  {metrics.MeanSquaredError:#.##} | Doit s'approcher de 0.00");
            Console.WriteLine($"  RMSE: {metrics.RootMeanSquaredError:#.##} | Doit s'approcher de 0.00");
            Console.WriteLine();

            // Test d'un utilisateur pour le film GoldenEye.
            Console.WriteLine("Calcul du score pour l'utilisateur 6 pour le film GoldenEye");
            var predictionEngine = mlContext.Model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(model);
            var prediction = predictionEngine.Predict(new MovieRating
            {
                UserId = 6,
                MovieId = 10 // GoldenEye
            });
            Console.WriteLine($"  Score: {prediction.Score}");
            Console.WriteLine();

            // find the top 5 movies for a given user
            Console.WriteLine("Calculating the top 5 movies for user 6...");
            var top5 = (from m in MovieRepository.All
                        let p = predictionEngine.Predict(
                           new MovieRating()
                           {
                               UserId = 6,
                               MovieId = m.Id
                           })
                        orderby p.Score descending
                        select (MovieId: m.Id, Label: p.Score)).Take(5);

            foreach (var (MovieId, Label) in top5)
            {
                Console.WriteLine($"  Score:{Label}\tMovie: {MovieRepository.Get(MovieId)?.Title}");
            }

            Console.ReadKey();
        }

        #endregion Private Methods
    }
}
