using Booking.Application.DTOs.ML;
using Booking.Application.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Booking.Application.Implementations
{
    public class MLModelService : IMLService
    {
        private readonly PredictionEngine<SearchData, PredictionOutput> _predictionEngine;

        public MLModelService()
        {
            var mlContext = new MLContext();
            var modelPath = Path.Combine(AppContext.BaseDirectory, "model.zip");
            var model = mlContext.Model.Load(modelPath, out var schema);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<SearchData, PredictionOutput>(model);
        }

        public HotelPrediction Predict(SearchData input)
        {
            var result = _predictionEngine.Predict(input);

            return new HotelPrediction
            {
                HotelName = result.PredictedLabel,
                Score = result.Score?.Max() ?? 0
            };
        }

        private class PredictionOutput
        {
            [ColumnName("PredictedLabel")]
            public string PredictedLabel { get; set; }

            public float[] Score { get; set; }
        }
    }
}
