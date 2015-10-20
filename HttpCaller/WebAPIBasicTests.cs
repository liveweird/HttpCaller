using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace HttpCaller
{
    [TestClass]
    public class WebApiBasicTests
    {
        private readonly string _serviceUri = "http://localhost:4000";

        [TestMethod]
        public void NoOneIsListening()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Should.Throw<HttpRequestException>(async () =>
                                                   {
                                                       await client.GetAsync("api/anybody/home");
                                                   });
            }
        }

        [TestMethod]
        public void SimpleGet()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_serviceUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                /*
                var responseTask = client.GetAsync("api/products/1");
                responseTask.Wait();
                if (response.IsSuccessStatusCode)
                {
                    Product product = await response.Content.ReadAsAsync<Product>();
                    Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
                }
                */
            }
        }
    }
}
