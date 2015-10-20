using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace HttpCaller
{
    [TestClass]
    public class WebApiBasicTests
    {
        private readonly string _badServiceUri = "http://localhost:8070";
        private readonly string _serviceUri = "http://localhost:8080";

        [TestMethod]
        public void NoOneIsListening()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_badServiceUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Should.Throw<HttpRequestException>(async () =>
                                                   {
                                                       await client.GetAsync("api/anybody/home");
                                                   });
            }
        }

        public class Anybody
        {
            public bool Home { get; set; }
        }

        [TestMethod]
        public async Task SimpleGet()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("api/anybody/home");
                responseTask.Wait();
                var result = responseTask.Result;

                result.IsSuccessStatusCode.ShouldBeTrue();

                var anybody = await result.Content.ReadAsAsync<Anybody>();
                anybody.Home.ShouldBeTrue();
            }
        }
    }
}
