using Microsoft.ML.Data;

namespace MovieRecommendations.Model
{
    /// <summary>
    /// Note d'un film.
    /// </summary>
    public class MovieRating
    {
        #region Public Fields

        [LoadColumn(2)] public float Label;
        [LoadColumn(1)] public float MovieId;
        [LoadColumn(0)] public float UserId;

        #endregion Public Fields
    }
}
