using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AgriStreamIoT.Functions
{
    public class AgriStreamFunctions
    {
        private readonly ILogger<AgriStreamFunctions> _logger;

        public AgriStreamFunctions(ILogger<AgriStreamFunctions> logger)
        {
            _logger = logger;
        }

        // -----------------------------------------------------------------
        // 1. Blob-Triggered Function (Runs when drone image is uploaded)
        // -----------------------------------------------------------------
        [Function(nameof(ProcessDroneImageBlob))]
        public async Task ProcessDroneImageBlob(
            [BlobTrigger("drone-images/{name}", Connection = "AzureStorageConnectionString")] Stream stream,
            string name)
        {
            _logger.LogInformation($"[Blob Trigger] New drone image detected: {name} | Size: {stream.Length} bytes");

            // TODO: Add your logic here to record upload details in Azure Table Storage or Azure SQL
            await Task.CompletedTask;
        }

        // -----------------------------------------------------------------
        // 2. HTTP-Triggered Function (GET - Retrieve Telemetry Data)
        // -----------------------------------------------------------------
        [Function("GetTelemetry")]
        public HttpResponseData GetTelemetry(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "telemetry")] HttpRequestData req)
        {
            _logger.LogInformation("[HTTP GET] Telemetry data requested from MVC application.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            // Mock telemetry response for local testing (Replace with Azure Table Storage queries later)
            var mockTelemetry = new[]
            {
                new { SensorId = "Sensor-01", SoilMoisture = 42.5, Temperature = 24.1, EnvironmentalStatus = "Normal" },
                new { SensorId = "Sensor-02", SoilMoisture = 18.2, Temperature = 29.8, EnvironmentalStatus = "Dry" }
            };

            response.WriteString(JsonSerializer.Serialize(mockTelemetry));
            return response;
        }

        // -----------------------------------------------------------------
        // 3. HTTP-Triggered Function (POST - Trigger Irrigation)
        // -----------------------------------------------------------------
        [Function("TriggerIrrigation")]
        public async Task<HttpResponseData> TriggerIrrigation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "TriggerIrrigation")] HttpRequestData req)
        {
            _logger.LogInformation("[HTTP POST] Irrigation command received from MVC dashboard.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            _logger.LogInformation($"[Payload] {requestBody}");

            // TODO: Parse payload and push a message to an Azure Storage Queue if needed

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync("{\"status\": \"Irrigation command processed successfully\"}");

            return response;
        }

        // -----------------------------------------------------------------
        // 4. Queue-Triggered Function (Processes queued irrigation commands)
        // -----------------------------------------------------------------
        [Function(nameof(ProcessIrrigationQueue))]
        public void ProcessIrrigationQueue(
            [QueueTrigger("irrigation-queue", Connection = "AzureStorageConnectionString")] string myQueueItem)
        {
            _logger.LogInformation($"[Queue Trigger] Processing queued irrigation item: {myQueueItem}");

            // TODO: Add logic here to update the device's operational status in your Azure SQL Database
        }
    }
}