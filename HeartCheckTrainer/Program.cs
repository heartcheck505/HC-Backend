using Microsoft.ML;
using Microsoft.ML.Data;

namespace HeartCheckTrainer;

public class ModelInput
{
    [LoadColumn(0)] public float BpmValue { get; set; }
    [LoadColumn(1)] public string Context { get; set; } = string.Empty;
    [LoadColumn(2)] public float Age { get; set; }
    [LoadColumn(3)] public bool HasSymptoms { get; set; }
    [LoadColumn(4)] public string RiskLevel { get; set; } = string.Empty;
}

public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; } = string.Empty;
    public float[] Score { get; set; } = [];
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ENTRENANDO MODELO DE MACHINE LEARNING DE HEARTCHECK ===");
        var mlContext = new MLContext(seed: 0);

        string datasetPath = @"C:\Users\craxc\Downloads\heartcheck_risk_dataset.csv";

        if (!File.Exists(datasetPath))
        {
            Console.WriteLine($"[ERROR] No se encontró el archivo en: {datasetPath}");
            return;
        }

        IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
            path: datasetPath,
            hasHeader: true,
            separatorChar: ',');

        // Pipeline con conversión explícita de bool -> float
        var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ModelInput.RiskLevel))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("ContextEncoded", nameof(ModelInput.Context)))
            .Append(mlContext.Transforms.Conversion.ConvertType("HasSymptomsFloat", nameof(ModelInput.HasSymptoms), DataKind.Single))
            .Append(mlContext.Transforms.Concatenate("Features", "BpmValue", "ContextEncoded", "Age", "HasSymptomsFloat"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        Console.WriteLine("Entrenando algoritmo...");
        var model = pipeline.Fit(dataView);

        // Guardar el archivo .zip dentro de la API principal
        string outputPath = @"C:\HeartCheck\HeartCheck\HeartCheckML.zip";
        mlContext.Model.Save(model, dataView.Schema, outputPath);

        Console.WriteLine($"\n==========================================");
        Console.WriteLine($"¡MODELO GENERADO CON ÉXITO! 🎉");
        Console.WriteLine($"Guardado en: {outputPath}");
        Console.WriteLine($"==========================================");
    }
}