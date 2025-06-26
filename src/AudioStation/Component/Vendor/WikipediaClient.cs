using System.Net.Http;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.Vendor.Interface;
using AudioStation.Component.Vendor.Wikipedia;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using MetaBrainz.Common;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component.Vendor
{
    [IocExport(typeof(IWikipediaClient))]
    public class WikipediaClient : IWikipediaClient
    {
        // https://awik.io/get-extract-wikipedia-page-wikipedia-api/
        //
        private const string URL_REQUEST_SUMMARY_FORMAT = "https://en.wikipedia.org/w/api.php?format=json&origin=*&action=query&prop=extracts&explaintext=false&exintro&titles={0}";
        private const string URL_REQUEST_BODY_FORMAT = "https://en.wikipedia.org/w/api.php?format=json&origin=*&action=query&prop=extracts&explaintext=false&titles={0}";

        private readonly IOutputController _outputController;
        private readonly HttpClient _httpClient;

        [IocImportingConstructor]
        public WikipediaClient(IOutputController outputController)
        {
            _outputController = outputController;
            _httpClient = new HttpClient();
        }

        public async Task<WikipediaData> GetExcerpt(string artistName)
        {
            try
            {
                var result = await _httpClient.GetAsync(string.Format(URL_REQUEST_SUMMARY_FORMAT, artistName));
                var json = result.GetStringContent();

                var summary = JObject.Parse(json).Descendants().First(x => x.Path.Contains("extract")).First().Value<string>();

                result = await _httpClient.GetAsync(string.Format(URL_REQUEST_BODY_FORMAT, artistName));
                json = result.GetStringContent();

                var body = JObject.Parse(json).Descendants().First(x => x.Path.Contains("extract")).First().Value<string>();

                return new WikipediaData()
                {
                    ExtractBody = body.ToString() ?? string.Empty,
                    ExtractSummary = summary.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                RaiseLog("Error getting Wikipedia excerpt:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Invokes logger on the application dispatcher thread
        /// </summary>
        protected void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, message, type, level, parameters);

            else
                _outputController.Log(message, type, level, parameters);
        }
    }
}
