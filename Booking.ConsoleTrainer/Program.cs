using Microsoft.ML;
using Microsoft.ML.Data;

namespace Booking.ConsoleTrainer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mlContext = new MLContext();
            string filePath = Path.Combine(AppContext.BaseDirectory, "train-data.csv");

            Console.WriteLine("Loading...");

            var data = mlContext.Data.LoadFromTextFile<SearchModel>(
                path: filePath,
                hasHeader: true,
                separatorChar: ',');

            Console.WriteLine("Finished loading!");

            Console.WriteLine("Records: " + mlContext.Data.CreateEnumerable<SearchModel>(data, reuseRowObject: false).Count());

            Console.WriteLine("Start to train model...");

            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label", nameof(SearchModel.HotelName))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("Destination"))
                .Append(mlContext.Transforms.Concatenate("Features", "Destination", "Budget", "NumPeople", "Rating"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            try
            {
                var model = pipeline.Fit(data);

                Console.WriteLine("Done!");

                var modelPath = Path.Combine(AppContext.BaseDirectory, "model.zip");
                mlContext.Model.Save(model, data.Schema, modelPath);

                Console.WriteLine("Save at: " + modelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed: " + ex.Message);
            }
        }
        public class SearchModel
        {
            [LoadColumn(0)]
            public string Destination { get; set; }

            [LoadColumn(1)]
            public float Budget { get; set; }

            [LoadColumn(2)]
            public float NumPeople { get; set; }

            [LoadColumn(3)]
            public float Rating { get; set; }

            [LoadColumn(4)]
            public string HotelName { get; set; } = "";
        }
    }
}
