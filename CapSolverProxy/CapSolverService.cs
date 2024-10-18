namespace CapSolverProxy
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using Newtonsoft.Json;

    public class CapSolverService
    {
        static readonly HttpClient client = new();

        public static async Task<string> CreateTask(string requestJson)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<CreateTaskRequest>(requestJson);
                HttpResponseMessage response = await client.PostAsJsonAsync(
                    "https://api.capsolver.com/createTask", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with status: {1} ({2})\"",
                        (int)response.StatusCode, response.StatusCode, (int)response.StatusCode) + "}";
                }
            } catch (Exception e)
            {
                var errorMessage = e.Message.Replace("\"", "\\\"");
                return "{" + string.Format("\"errorId\":1,\"errorCode\":\"{0}\", \"errorDescription\":\"CreateTask failed with error: {1}\"",
                    "ERROR_CAPSOLVER_PROXY_EXCEPTION", errorMessage) + "}";
            }
        }
    }
}
