using System.Diagnostics;
using System.Net.Http;

using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.Utilities.Diagnostics;

namespace AudioStation.Core.Service.Vendor
{
    public abstract class VendorServiceBase : IAudioStationDataService
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;
        private uint _throttleLimitMilliseconds;
        private DateTime _lastServiceCall;
        private const int SERVICE_WAIT_MILLISEC = 100;
        private const int SERVICE_THROTTLE_MILLISEC_MIN = 500;
        private const int SERVICE_THROTTLE_MILLISEC_MAX = 5000;

        // Rate Limit Data: Music Brainz (has their own); AcoustID (none); ...
        private Dictionary<string, RateLimitInfo> _serviceRateLimits;


        // Check for response headers (rate limiting, auth, etc...)
        // private readonly HttpEventListener _httpListener;
        private readonly HttpRequestResponseObserver _httpObserver;
        private readonly IDisposable _httpObserverDisposable;

        private readonly string _serviceName;
        private readonly string _serviceDisplayName;
        private readonly LogMessageServiceType _serviceType;

        protected VendorServiceBase(string serviceName, string serviceDisplayName, LogMessageServiceType serviceType)
        {
            _status = IAudioStationDataService.Status.Disabled;
            _throttleLimitMilliseconds = 3000;                      // Default at 3000 (ms)
            _lastServiceCall = DateTime.MinValue;
            _serviceName = serviceName;
            _serviceDisplayName = serviceDisplayName;
            _serviceType = serviceType;
            _serviceRateLimits = new Dictionary<string, RateLimitInfo>();

            // Must Initialize
            _httpObserver = new HttpRequestResponseObserver();
            _httpObserver.HttpRequestEvent += OnHttpRequestEvent;
            _httpObserver.HttpResponseEvent += OnHttpResponseEvent;
            _httpObserver.ErrorEvent += OnHttpErrorEvent;
            _httpObserverDisposable = DiagnosticListener.AllListeners.Subscribe(_httpObserver);
        }

        private void OnHttpResponseEvent(HttpResponseMessage sender)
        {
            if (sender != null)
            {
                //// Rate Limit: Data may be overridden by service
                //var serviceUrl = sender.RequestMessage.RequestUri.GetLeftPart(UriPartial.Path);

                //// Let inherited class set these
                //if (!_serviceRateLimits.ContainsKey(serviceUrl))
                //{
                //    _serviceRateLimits.Add(serviceUrl, new RateLimitInfo(serviceUrl)
                //    {
                //        IsSet = false,
                //        LastRequest = DateTime.Now
                //    });
                //}

                // Information
                ApplicationHelpers.Log(sender.RequestMessage.RequestUri.ToString(), _serviceType, LogLevel.Information, null);

                // Trace
                ApplicationHelpers.Log(sender.ToString(), _serviceType, LogLevel.Trace, null);
            }


            // TODO: Look for rate limit information

            //this.AllowedRequests = RateLimitInfo.GetIntHeader(headers, "X-RateLimit-Limit");
            //this.LastRequest = DateTimeOffset.UtcNow;
            //this.RemainingRequests = RateLimitInfo.GetIntHeader(headers, "X-RateLimit-Remaining");
            //this.ResetAt = RateLimitInfo.GetUnixTimeHeader(headers, "X-RateLimit-Reset");
            //this.ResetIn = RateLimitInfo.GetIntHeader(headers, "X-RateLimit-Reset-In");
        }
        private void OnHttpRequestEvent(HttpRequestMessage sender)
        {
            if (sender != null)
                ApplicationHelpers.Log(sender.ToString(), _serviceType, LogLevel.Information, null);
        }
        private void OnHttpErrorEvent(Exception sender)
        {
            if (sender != null)
                ApplicationHelpers.Log("Http Error: {0}", _serviceType, LogLevel.Error, sender, sender.Message);
        }

        /// <summary>
        /// Forces the service to wait the configured amount of time before making another service call. This wait
        /// happens on the thread that calls it.
        /// </summary>
        protected void ServiceWait()
        {
            // Throttle limit for service calls
            while (DateTime.Now < _lastServiceCall.AddMilliseconds(_throttleLimitMilliseconds))
            {
                Thread.Sleep(SERVICE_WAIT_MILLISEC);
            }

            // UPDATE SERVICE WAIT
            _lastServiceCall = DateTime.Now;
        }

        protected void SetThrottleLimit(uint milliseconds)
        {
            if (milliseconds < SERVICE_THROTTLE_MILLISEC_MIN ||
                milliseconds > SERVICE_THROTTLE_MILLISEC_MAX)
                throw new ArgumentException(string.Format("Throttle limit must be between {0} and {1}", SERVICE_THROTTLE_MILLISEC_MIN, SERVICE_THROTTLE_MILLISEC_MAX));

            _throttleLimitMilliseconds = milliseconds;
        }

        /// <summary>
        /// Allows inherited classes to set their rate limit data. The base class will attempt to locate this
        /// data in the Http headers using an Http Observer; but you can override it here. This data will appear
        /// in the status logs and messages for each component.
        /// </summary>
        protected void SetRateLimit(string serviceUrl, int allowedRequests, int remainingRequests, DateTimeOffset lastRequest, DateTimeOffset resetAt)
        {
            // Let inherited class set these
            if (!_serviceRateLimits.ContainsKey(serviceUrl))
            {
                _serviceRateLimits.Add(serviceUrl, new RateLimitInfo(serviceUrl));
            }

            _serviceRateLimits[serviceUrl].IsSet = true;
            _serviceRateLimits[serviceUrl].LastRequest = lastRequest;
            _serviceRateLimits[serviceUrl].ResetAt = resetAt;
            _serviceRateLimits[serviceUrl].AllowedRequsts = allowedRequests;
            _serviceRateLimits[serviceUrl].RemainingRequests = remainingRequests;
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return _serviceName;
        }
        public string GetDisplayName()
        {
            return _serviceDisplayName;
        }
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public LogMessageServiceType GetLogServiceType()
        {
            return _serviceType;
        }
        public virtual IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            return _status;
        }

        public virtual Task<IAudioStationDataService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public virtual IAudioStationDataService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationDataService.Status.Idle;
        }

        public virtual Task<IAudioStationDataService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationDataService.Status.Idle);
        }

        public virtual string GetStatusMessage()
        {
            var baseMessage = this.GetDisplayName() + " " + IAudioStationDataService.GetDefaultStatusMessage(_status);

            if (_serviceRateLimits.Any())
            {
                var rateLimit = _serviceRateLimits.First().Value;
                var rateLimitFormat = "Rate Limit:  {0} remaining of {1}. Resets at {2}";
                var rateLimitInfo = string.Format(rateLimitFormat, rateLimit.RemainingRequests, rateLimit.AllowedRequsts, rateLimit.ResetAt);

                return string.Format("{0} ({1})", baseMessage, rateLimitInfo);
            }
            else
                return baseMessage;
        }
        protected void OnStatusChanged(IAudioStationDataService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        public void Dispose()
        {
            _httpObserverDisposable.Dispose();
        }
        #endregion
    }
}
