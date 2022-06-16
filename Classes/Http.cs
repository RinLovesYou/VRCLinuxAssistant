using System;
using System.Net;
using System.Net.Http;

namespace VRCLinuxAssistant
{
    static class Http
    {
        private static HttpClient _client = null;

        public static HttpClient HttpClient
        {
            get
            {
                if (_client != null) return _client;

                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };

                _client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(240),
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _client.DefaultRequestHeaders.Add("User-Agent", "VRCLinuxAssistant/" + App.Version);

                return _client;
            }
        }
    }
}
