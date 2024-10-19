namespace CapSolverProxyTests
{
    using CapSolverProxy;
    using Newtonsoft.Json;

    [TestClass]
    public class JSONDeserializeTests
    {
        [TestMethod]
        [DataRow("{\"type\":\"FunCaptchaClassification\",\"images\":[\"iVB...\"],\"question\":\"3d_rollball_objects\",\"websiteURL\":\"https://signup.live.com/signup?lic=1&uaid=26337a11500a4f3782fc5e51a3754413\"}")]
        public void TestDeserializeCapSolverTask(string taskJson)
        {
            var task = JsonConvert.DeserializeObject<CapSolverTask>(taskJson);
            Assert.AreEqual("FunCaptchaClassification", task?.type);
            Assert.AreEqual("3d_rollball_objects", task?.question);
            Assert.AreEqual("https://signup.live.com/signup?lic=1&uaid=26337a11500a4f3782fc5e51a3754413", task?.websiteURL);
            Assert.AreEqual(1, task?.images?.Count);
            Assert.AreEqual("iVB...", task?.images?[0]);
        }

        [TestMethod]
        [DataRow("{\"clientKey\":\"CAP-111\",\"source\":\"firefox\",\"version\":\"1.12.1\",\"task\":{\"type\":\"FunCaptchaClassification\",\"images\":[\"iVB...\"],\"question\":\"3d_rollball_objects\",\"websiteURL\":\"https://signup.live.com/signup?lic=1&uaid=26337a11500a4f3782fc5e51a3754413\"}}")]
        public void TestDeserializeCreateTaskRequest(string requestJson)
        {
            var createTask = JsonConvert.DeserializeObject<CreateTaskRequest>(requestJson);                    
            Assert.AreEqual("CAP-111", createTask?.clientKey);
            Assert.AreEqual("firefox", createTask?.source);
            Assert.AreEqual("1.12.1", createTask?.version);
            Assert.IsNotNull(createTask?.task);
        }

        [TestMethod]
        [DataRow("{\"errorId\":0,\"status\":\"ready\",\"solution\":{\"objects\":[5]},\"taskId\":\"a2951c2e-9649-49fd-984c-1ff0fcbf828b\"}")]
        public void TestDeserializeCreateTaskResponse(string responseJson)
        {
            var response = JsonConvert.DeserializeObject<CreateTaskResponse>(responseJson);
            Assert.AreEqual(0, response?.errorId);
            Assert.AreEqual("ready", response?.status);
            Assert.AreEqual(1, response?.solution?.objects?.Count);
            Assert.AreEqual(5, response?.solution?.objects?[0]);
            Assert.AreEqual("a2951c2e-9649-49fd-984c-1ff0fcbf828b", response?.taskId);
        }
    }
}