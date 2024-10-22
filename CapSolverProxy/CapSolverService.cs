namespace CapSolverProxy
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Cryptography;
    using System.Text;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    
    public class CapSolverService
    {
        private readonly HttpClient client;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        private readonly MemoryCache cache;
        private readonly ILogger? logger;
        private readonly CapSolverStats stats;
        private readonly string ImagesFolder;
        private readonly bool SaveImages;

        public CapSolverService(CapSolverSettings settings, ILoggerFactory? loggerFactory) {
            client = new();
            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetPriority(CacheItemPriority.High)
                .SetSlidingExpiration(TimeSpan.FromSeconds(settings.CacheSlidingExpiration))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.CacheAbsoluteExpiration));
            cache = new(new MemoryCacheOptions()
            {
                SizeLimit = settings.CacheSizeLimit
            });
            logger = loggerFactory?.CreateLogger("CapSolverService");
            stats = new CapSolverStats();
            ImagesFolder = settings.ImagesFolder ?? "";
            if (ImagesFolder.Length > 0 && !ImagesFolder.EndsWith("/") && !ImagesFolder.EndsWith("\\"))
            {
                ImagesFolder += Path.DirectorySeparatorChar;
            }
            SaveImages = settings.SaveImages;
        }

        public static string? GetImagesHash(CreateTaskRequest? request)
        {
            string? imageHash = null;
            if (request?.task?.images?.Count > 0)
            {
                using (MD5 md5 = MD5.Create())
                {
                    md5.Initialize();
                    foreach (var image in request.task.images)
                    {
                        var imageBytes = Encoding.UTF8.GetBytes(image);
                        md5.TransformBlock(imageBytes, 0, imageBytes.Length, null, 0);
                    }
                    md5.TransformFinalBlock(Encoding.UTF8.GetBytes(string.Empty), 0, 0);
                    var hash = md5.Hash;
                    if (hash != null)
                    {
                        imageHash = Convert.ToHexString(hash);
                    }
                }
            }
            return imageHash;
        }

        private static void SaveImagesToFolder(CreateTaskRequest? request, string imagesFolder, string imagesHash)
        {
            if (request?.task?.images?.Count > 0)
            {
                int imageNum = 0;
                foreach (var image in request.task.images)
                {
                    var imageBytes = Convert.FromBase64String(image);
                    string fileName = string.Format("{0}{1}_{2}.jpg", imagesFolder, imagesHash, imageNum);
                    using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(imageBytes, 0, imageBytes.Length);
                    }
                }
            }
        }

        public async Task<string> CreateTask(string requestJson)
        {
            stats.IncRequests();
            try
            {
                var request = JsonConvert.DeserializeObject<CreateTaskRequest>(requestJson);
                var imagesHash = GetImagesHash(request);
                if (!string.IsNullOrEmpty(imagesHash))
                {
                    if (cache.TryGetValue(imagesHash, out string responseJson))
                    {
                        stats.IncFromCache();
                        logger?.LogInformation("Response from cache for {}", imagesHash);
                        return responseJson;
                    }
                    if (SaveImages && ImagesFolder.Length > 0)
                    {
                        SaveImagesToFolder(request, ImagesFolder, imagesHash);
                    }
                }
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    "https://api.capsolver.com/createTask", request);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CreateTaskResponse>(responseJson);
                    if (string.IsNullOrEmpty(result?.errorCode) && ((result?.errorId ?? 0) == 0))
                    {         
                        cache.Set(imagesHash, responseJson, cacheEntryOptions);
                        stats.IncCached();
                    }
                    stats.IncFromCapSolver();
                    logger?.LogInformation("Response from capsolver API for {}", imagesHash);
                    return responseJson;
                }
                else {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        stats.IncErrors();
                        return responseJson;
                    }
                    stats.IncErrors();
                    return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with status: {1} ({2})\"",
                        (int)response.StatusCode, response.StatusCode, (int)response.StatusCode) + "}";
                }
            } catch (Exception e)
            {
                stats.IncFailed();
                var errorMessage = e.Message.Replace("\\", "\\\\").Replace("\"", "\\\"");
                return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with error: {1}\"",
                    "ERROR_CAPSOLVER_PROXY_EXCEPTION", errorMessage) + "}";
            }
        }

        public string GetStats()
        {
            return stats.ToJson(cache.Count);
        }
    }
}
