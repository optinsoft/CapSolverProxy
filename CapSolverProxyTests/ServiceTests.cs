using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapSolverProxyTests
{
    using CapSolverProxy;
    using Newtonsoft.Json;

    [TestClass]
    public class ServiceTests
    {
        [TestMethod]
        [DataRow("{\"clientKey\":\"CAP-111\",\"source\":\"firefox\",\"version\":\"1.12.1\",\"task\":{\"type\":\"FunCaptchaClassification\",\"images\":[\"iVB...\"],\"question\":\"3d_rollball_objects\",\"websiteURL\":\"https://signup.live.com/signup?lic=1&uaid=26337a11500a4f3782fc5e51a3754413\"}}")]
        public void TestDeserializeCreateTaskRequest(string requestJson)
        {
            var request = JsonConvert.DeserializeObject<CreateTaskRequest>(requestJson);
            var imageshash = CapSolverService.GetImagesHash(request);
            Assert.AreEqual("61D5025D11E85EBB9181489AA639FBC7", imageshash);
        }

    }
}
