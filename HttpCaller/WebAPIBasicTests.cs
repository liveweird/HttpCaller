using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace HttpCaller
{
    [TestClass]
    public class WebApiBasicTests
    {
        private readonly string _badServiceUri = "http://localhost:8070";
        private static readonly string _serviceUri = "http://localhost:8080";

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_serviceUri)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            Client = CreateClient();
        }

        private static void CleanupClient(HttpClient client)
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            CleanupClient(Client);
        }

        protected static HttpClient Client { private set; get; }

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
            var responseTask = Client.GetAsync("api/anybody/home");
            responseTask.Wait();
            var result = responseTask.Result;

            result.IsSuccessStatusCode.ShouldBeTrue();

            var anybody = await result.Content.ReadAsAsync<Anybody>();
            anybody.Home.ShouldBeTrue();
        }

        [TestMethod]
        public void ThousandCalls()
        {
            Stopwatch init,
                      call,
                      dispatch,
                      total;
            var count = 1000;
            HttpClient client;

            total = new Stopwatch();
            init = new Stopwatch();
            call = new Stopwatch();
            dispatch = new Stopwatch();

            total.Start();
            
            for (var i = 0;
                 i < count;
                 i++)
            {
                init.Start();
                client = CreateClient();
                init.Stop();

                call.Start();
                var response = client.GetAsync("api/anybody/home")
                                     .Result;
                var result = response.Content.ReadAsAsync<Anybody>()
                                     .Result;
                call.Stop();

                dispatch.Start();
                CleanupClient(client);
                dispatch.Stop();
            }

            total.Stop();

            Console.Out.WriteLine("Init: {0}; {1}", init.ElapsedMilliseconds, ((double) init.ElapsedMilliseconds) / count);
            Console.Out.WriteLine("Call: {0}; {1}", call.ElapsedMilliseconds, ((double) call.ElapsedMilliseconds) / count);
            Console.Out.WriteLine("Dispatch: {0}; {1}", dispatch.ElapsedMilliseconds, ((double) dispatch.ElapsedMilliseconds) / count);
            Console.Out.WriteLine("Total: {0}", total.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ThousandCallsSingleInit()
        {
            Stopwatch init,
                      call,
                      dispatch,
                      total;
            var count = 1000;
            HttpClient client;

            total = new Stopwatch();
            init = new Stopwatch();
            call = new Stopwatch();
            dispatch = new Stopwatch();

            total.Start();

            init.Start();
            client = CreateClient();
            init.Stop();

            for (var i = 0;
                 i < count;
                 i++)
            {
                call.Start();
                var response = client.GetAsync("api/anybody/home")
                                     .Result;
                var result = response.Content.ReadAsAsync<Anybody>()
                                     .Result;
                call.Stop();
            }

            dispatch.Start();
            CleanupClient(client);
            dispatch.Stop();

            total.Stop();

            Console.Out.WriteLine("Init: {0}; {1}", init.ElapsedMilliseconds, ((double)init.ElapsedMilliseconds));
            Console.Out.WriteLine("Call: {0}; {1}", call.ElapsedMilliseconds, ((double)call.ElapsedMilliseconds) / count);
            Console.Out.WriteLine("Dispatch: {0}; {1}", dispatch.ElapsedMilliseconds, ((double)dispatch.ElapsedMilliseconds));
            Console.Out.WriteLine("Total: {0}", total.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task SimplePost()
        {
            var responseTask = Client.PostAsJsonAsync("api/anybody/home/1", string.Empty);
            responseTask.Wait();
            var result = responseTask.Result;

            result.IsSuccessStatusCode.ShouldBeTrue();

            var anybody = await result.Content.ReadAsAsync<Anybody>();
            anybody.Home.ShouldBeFalse();
        }

        public class Junk1L
        {
            public List<Junk2La> junk2La { get; set; }
            public List<Junk2Lb> junk2Lb { get; set; }
            public List<Junk2Lc> junk2Lc { get; set; }
        }

        public class Junk2La
        {
            public List<string> junk3Laa { get; set; }
        }

        public class Junk2Lb
        {
            public List<int> junk3Lba { get; set; }
        }

        public class Junk2Lc
        {
            public List<double> junk3Lca { get; set; }
        }

        [TestMethod]
        public async Task NestedObjectsSimpleGet()
        {
            var responseTask = Client.GetAsync("api/anybody/junk");
            responseTask.Wait();
            var result = responseTask.Result;

            result.IsSuccessStatusCode.ShouldBeTrue();

            var junk1L = await result.Content.ReadAsAsync<Junk1L>();

            junk1L.junk2La.Count.ShouldBe(100);
            junk1L.junk2Lb.Count.ShouldBe(100);
            junk1L.junk2Lc.Count.ShouldBe(100);
            junk1L.junk2La[0].junk3Laa.Count.ShouldBe(100);
            junk1L.junk2Lb[0].junk3Lba.Count.ShouldBe(100);
            junk1L.junk2Lc[0].junk3Lca.Count.ShouldBe(100);
            junk1L.junk2La[0].junk3Laa[0].ShouldBe("Abcdef");
            junk1L.junk2Lb[0].junk3Lba[0].ShouldBe(154538);
            junk1L.junk2Lc[0].junk3Lca[0].ShouldBe(123.54534);
        }
    }
}
