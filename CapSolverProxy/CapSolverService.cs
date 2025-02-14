﻿namespace CapSolverProxy
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Runtime.CompilerServices;
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
        private readonly bool UseCache;

        public CapSolverService(CapSolverSettings? settings, ILoggerFactory? loggerFactory) {
            client = new();
            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(1)
                .SetPriority(CacheItemPriority.High)
                .SetSlidingExpiration(TimeSpan.FromSeconds(settings?.CacheSlidingExpiration ?? 3600))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings?.CacheAbsoluteExpiration ?? 86400));
            cache = new(new MemoryCacheOptions()
            {
                SizeLimit = settings?.CacheSizeLimit ?? 16384
            });
            logger = loggerFactory?.CreateLogger("CapSolverService");
            stats = new CapSolverStats();
            ImagesFolder = settings?.ImagesFolder ?? "";
            if (ImagesFolder.Length > 0 && !ImagesFolder.EndsWith('/') && !ImagesFolder.EndsWith('\\'))
            {
                ImagesFolder += Path.DirectorySeparatorChar;
            }
            SaveImages = settings?.SaveImages ?? false;
            UseCache = settings?.UseCache ?? false;
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

        public async Task<string> GetBalance(string requestJson)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<GetBalanceRequest>(requestJson);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(
                    "https://api.capsolver.com/getBalance", request);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GetBalanceResponse>(responseJson);
                    if (response?.balance != null)
                    {
                        stats.UpdateLastBalance(response.balance.Value);
                    }
                    return responseJson;
                }
                else
                {
                    if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        stats.IncErrors();
                        return responseJson;
                    }
                    return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"GetBalance failed with status: {1} ({2})\"",
                        (int)responseMessage.StatusCode, responseMessage.StatusCode, (int)responseMessage.StatusCode) + "}";
                }
            }
            catch (Exception e)
            {
                var errorMessage = e.Message.Replace("\\", "\\\\").Replace("\"", "\\\"");
                return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"GetBalance failed with error: {1}\"",
                    "ERROR_CAPSOLVER_PROXY_EXCEPTION", errorMessage) + "}";
            }
        }

        public async Task<string> CreateTask(string requestJson)
        {
            stats.IncCreateTaskRequests();
            try
            {
                var request = JsonConvert.DeserializeObject<CreateTaskRequest>(requestJson);
                var imagesHash = GetImagesHash(request);
                if (!string.IsNullOrEmpty(imagesHash))
                {
                    if (UseCache)
                    {
                        if (cache.TryGetValue(imagesHash, out string? responseJson))
                        {
                            if (!string.IsNullOrEmpty(responseJson)) {
                                stats.IncSuccessFromCache();
                                logger?.LogInformation("Response from cache for {}", imagesHash);
                                return responseJson;
                            }
                        }
                    }
                    if (SaveImages && ImagesFolder.Length > 0)
                    {
                        SaveImagesToFolder(request, ImagesFolder, imagesHash);
                    }
                }
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(
                    "https://api.capsolver.com/createTask", request);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    if (UseCache && !string.IsNullOrEmpty(imagesHash))
                    {
                        var result = JsonConvert.DeserializeObject<CreateTaskResponse>(responseJson);
                        if (string.IsNullOrEmpty(result?.errorCode) && ((result?.errorId ?? 0) == 0))
                        {
                            cache.Set(imagesHash, responseJson, cacheEntryOptions);
                            stats.IncCached();
                        }
                    }
                    stats.IncSuccessFromCapSolver();
                    logger?.LogInformation("Response from capsolver API for {}", imagesHash);
                    return responseJson;
                }
                else {
                    if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        stats.IncErrors();
                        return responseJson;
                    }
                    stats.IncErrors();
                    return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with status: {1} ({2})\"",
                        (int)responseMessage.StatusCode, responseMessage.StatusCode, (int)responseMessage.StatusCode) + "}";
                }
            } catch (Exception e)
            {
                stats.IncFailed();
                var errorMessage = e.Message.Replace("\\", "\\\\").Replace("\"", "\\\"");
                return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with error: {1}\"",
                    "ERROR_CAPSOLVER_PROXY_EXCEPTION", errorMessage) + "}";
            }
        }

        public async Task<string> GetTaskResult(string requestJson)
        {
            stats.IncGetTaskResultRequests();
            try
            {
                var request = JsonConvert.DeserializeObject<GetTaskResultRequest>(requestJson);
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync(
                    "https://api.capsolver.com/getTaskResult", request);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    stats.IncSuccessFromCapSolver();
                    return responseJson;
                }
                else
                {
                    if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        stats.IncErrors();
                        return responseJson;
                    }
                    stats.IncErrors();
                    return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"GetTaskResult failed with status: {1} ({2})\"",
                        (int)responseMessage.StatusCode, responseMessage.StatusCode, (int)responseMessage.StatusCode) + "}";
                }
            }
            catch (Exception e)
            {
                stats.IncFailed();
                var errorMessage = e.Message.Replace("\\", "\\\\").Replace("\"", "\\\"");
                return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"GetTaskResult failed with error: {1}\"",
                    "ERROR_CAPSOLVER_PROXY_EXCEPTION", errorMessage) + "}";
            }
        }

        public string GetStats()
        {
            return stats.ToJson(cache.Count);
        }
    }
}
