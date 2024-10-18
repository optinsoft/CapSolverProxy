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
        [DataRow("{\"errorId\":0,\"errorCode\":\"\",\"errorDescription\":\"\",\"taskId\":\"37223a89-06ed-442c-a0b8-22067b79c5b4\"}")]
        public void TestDeserializeCreateTaskResponse(string responseJson)
        {
            var response = JsonConvert.DeserializeObject<CreateTaskResponse>(responseJson);
            Assert.AreEqual(0, response?.errorId);
            Assert.AreEqual("", response?.errorCode);
            Assert.AreEqual("", response?.errorDescription);
            Assert.AreEqual("37223a89-06ed-442c-a0b8-22067b79c5b4", response?.taskId);
        }
    }
}