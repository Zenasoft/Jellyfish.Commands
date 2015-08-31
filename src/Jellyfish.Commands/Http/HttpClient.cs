// Copyright (c) Zenasoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfish.Commands.Http
{
    public class HttpClientBuilder
    {
        public const string RequestIdHeader = "X-Jellyfish-RequestId";

        private Uri _uri;
        private string _version;
        private IEnumerable<HttpClientHandler> _handlers;
        private HttpMethod _method = HttpMethod.Get;
        private string _content;
        private IDictionary<string, string> _requestHeaders;
        private TimeSpan _timeout;
        private string _contentType;

        private HttpClientBuilder(Uri uri, string version, string correlationId)
        {
            _uri = uri;
            _version = version;
            _timeout = TimeSpan.FromSeconds(1);
            SetHeader(RequestIdHeader, correlationId!=null?correlationId: Guid.NewGuid().ToString("N"));
        }

        public static HttpClientBuilder Create(int port, string correlationId=null, string version = null)
        {
            if (port < 1024) throw new ArgumentOutOfRangeException("Port must be greater than 1024");

            return new HttpClientBuilder(new UriBuilder("http", "localhost", port).Uri, version, correlationId);
        }

        public static HttpClientBuilder Create([NotNull]Uri uri, string version=null, string correlationId=null)
        {
            return new HttpClientBuilder(uri, version, correlationId);
        }

        public HttpClientBuilder WithContentType([NotNull]string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public HttpClientBuilder WithHandlers([NotNull]IEnumerable<HttpClientHandler> handlers)
        {
            _handlers = handlers.ToArray();
            return this;
        }

        public HttpClientBuilder SetTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public HttpClientBuilder UseMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        public HttpClientBuilder SetContent([NotNull]string content)
        {
            _content = content;
            return this;
        }

        public HttpClientBuilder SetHeader([NotNull]string key, [NotNull]string value)
        {
            if (_requestHeaders == null)
                _requestHeaders = new Dictionary<string, string>();
            _requestHeaders[key] = value;
            return this;
        }

        public async Task<ServiceHttpResponse> ExecuteAsync([NotNull]string path, CancellationToken token)
        {
            var req = new ServiceHttpClient(_uri, _version, _handlers, _timeout);
            return await req.RequestAsync(token, _method, path, _content, true, _requestHeaders, _contentType);
        }

        public async Task<T> ExecuteAsync<T>(string path, CancellationToken token)
        {
            var result = await ExecuteAsync(path, token);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result.Content);
        }
    }

    internal class ServiceHttpClient
    {
        private HttpClient _client;
        /// <summary>
        /// The URI for the Service.
        /// </summary>
        private readonly Uri _serviceUri;

        private string _pathPrefix;

        /// <summary>
        /// Factory method for creating the default http client handler
        /// </summary>
        internal static Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

        /// <summary>
        /// Content type for request bodies and accepted responses.
        /// </summary>
        private const string RequestJsonContentType = "application/json";

        public ServiceHttpClient([NotNull]Uri serviceUri, string pathPrefix, IEnumerable<HttpClientHandler> handlers, TimeSpan timeout)
        {
            _pathPrefix = pathPrefix;
            _serviceUri = serviceUri;
            _client = new HttpClient(CreatePipeline(handlers));
            if( timeout!= null)
                _client.Timeout = timeout;
        }

        /// <summary>
        /// Returns a default HttpMessageHandler that supports automatic decompression.
        /// </summary>
        /// <returns>
        /// A default HttpClientHandler that supports automatic decompression
        /// </returns>
        private static HttpMessageHandler GetDefaultHttpClientHandler()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }

            return handler;
        }

        /// <summary>
        /// Makes an HTTP request that includes the standard Mobile Services
        /// headers. It will use an HttpClient that optionally has user-defined 
        /// http handlers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method used to request the resource.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="content">
        /// Optional content to send to the resource.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <returns>
        /// The content of the response as a string.
        /// </returns>
        public async Task<ServiceHttpResponse> RequestAsync(CancellationToken token, HttpMethod method,
                                                        string uriPathAndQuery,
                                                        string content = null,
                                                        bool ensureResponseContent = true,
                                                        IDictionary<string, string> requestHeaders = null,
                                                        string contentType=null)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            // Create the request
            HttpContent httpContent = CreateHttpContent(content);
            using (HttpRequestMessage request = this.CreateHttpRequestMessage(method, uriPathAndQuery, requestHeaders, httpContent))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType ?? RequestJsonContentType));

                // Get the response
                HttpClient client = this._client;
                using (HttpResponseMessage response = await this.SendRequestAsync(token, client, request, ensureResponseContent))
                {
                    string responseContent = await GetResponseContent(response);
                    string etag = null;
                    if (response.Headers.ETag != null)
                    {
                        etag = response.Headers.ETag.Tag;
                    }

                    LinkHeaderValue link = null;
                    if (response.Headers.Contains("Link"))
                    {
                        link = LinkHeaderValue.Parse(response.Headers.GetValues("Link").FirstOrDefault());
                    }

                    string correlationId = null;
                    if (response.Headers.Contains(HttpClientBuilder.RequestIdHeader))
                    {
                        correlationId = response.Headers.GetValues(HttpClientBuilder.RequestIdHeader).FirstOrDefault();
                    }


                    return new ServiceHttpResponse(responseContent, etag, link, correlationId);

                    // Dispose of the request and response
                }
            }
        }

        /// <summary>
        /// Returns the content from the <paramref name="response"/> as a string.
        /// </summary>
        /// <param name="response">
        /// The <see cref="HttpResponseMessage"/> from which to read the content as a string.
        /// </param>
        /// <returns>
        /// The response content as a string.
        /// </returns>
        private static async Task<string> GetResponseContent(HttpResponseMessage response)
        {
            string responseContent = null;
            if (response.Content != null)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            return responseContent;
        }

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> with all of the 
        /// required Mobile Service headers.
        /// </summary>
        /// <param name="method">
        /// The HTTP method of the request.
        /// </param>
        /// <param name="uriPathAndQuery">
        /// The URI of the resource to request (relative to the Mobile Services
        /// runtime).
        /// </param>
        /// <param name="requestHeaders">
        /// Additional request headers to include with the request.
        /// </param>
        /// <param name="content">
        /// The content of the request.
        /// </param>
        /// <returns>
        /// An <see cref="HttpRequestMessage"/> with all of the 
        /// required Mobile Service headers.
        /// </returns>
        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string uriPathAndQuery, IDictionary<string, string> requestHeaders, HttpContent content)
        {
            Debug.Assert(method != null);
            Debug.Assert(!string.IsNullOrEmpty(uriPathAndQuery));

            HttpRequestMessage request = new HttpRequestMessage();

            // Set the Uri and Http Method
            var path = _pathPrefix != null ? _pathPrefix + uriPathAndQuery : uriPathAndQuery;
            request.RequestUri = new Uri(this._serviceUri, path.TrimStart('/'));
            request.Method = method;

            // Add the user's headers
            if (requestHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in requestHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

//            request.Headers.Add(RequestInstallationIdHeader, this.installationId);

            // Add the content
            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        /// <summary>
        /// Sends the <paramref name="request"/> with the given <paramref name="client"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="HttpClient"/> to send the request with.
        /// </param>
        /// <param name="request">
        /// The <see cref="HttpRequestMessage"/> to be sent.
        /// </param>
        /// <param name="ensureResponseContent">
        /// Optional parameter to indicate if the response should include content.
        /// </param>
        /// <returns>
        /// An <see cref="HttpResponseMessage"/>.
        /// </returns>
        private async Task<HttpResponseMessage> SendRequestAsync(CancellationToken token, HttpClient client, HttpRequestMessage request, bool ensureResponseContent)
        {
            Debug.Assert(client != null);
            Debug.Assert(request != null);

           // Console.WriteLine("Send request : " + request.RequestUri.ToString());

            // Send the request and get the response back as string
            HttpResponseMessage response = await client.SendAsync(request, token);

            // Throw errors for any failing responses
            if (!response.IsSuccessStatusCode)
            {
                await ThrowInvalidResponse(request, response);
            }

            // If there was supposed to be response content and there was not, throw
            if (ensureResponseContent)
            {
                long? contentLength = null;
                if (response.Content != null)
                {
                    contentLength = response.Content.Headers.ContentLength;
                }

                if (contentLength == null || contentLength <= 0)
                {
                    throw new ServiceInvalidOperationException("The server did not provide a response with the expected content.", request, response);
                }
            }

            return response;
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //// free managed resources
                //if (this.httpHandler != null)
                //{
                //    this.httpHandler.Dispose();
                //    this.httpHandler = null;
                //}

                if (this._client != null)
                {
                    this._client.Dispose();
                    this._client = null;
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="HttpContent"/> instance from a string.
        /// </summary>
        /// <param name="content">
        /// The string content from which to create the <see cref="HttpContent"/> instance. 
        /// </param>
        /// <returns>
        /// An <see cref="HttpContent"/> instance or null if the <paramref name="content"/>
        /// was null.
        /// </returns>
        private static HttpContent CreateHttpContent(string content)
        {
            HttpContent httpContent = null;
            if (content != null)
            {
                httpContent = new StringContent(content, Encoding.UTF8, RequestJsonContentType);
            }

            return httpContent;
        }

        /// <summary>
        /// Throws an exception for an invalid response to a web request.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private static async Task ThrowInvalidResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            Debug.Assert(request != null);
            Debug.Assert(response != null);
            Debug.Assert(!response.IsSuccessStatusCode);

            string responseContent = response.Content == null ? null : await response.Content.ReadAsStringAsync();

            // Create either an invalid response or connection failed message
            // (check the status code first because some status codes will
            // set a protocol ErrorStatus).
            string message = null;
            if (!response.IsSuccessStatusCode)
            {
                if (responseContent != null)
                {
                    JToken body = null;
                    try
                    {
                        body = JToken.Parse(responseContent);
                    }
                    catch
                    {
                    }

                    if (body != null)
                    {
                        if (body.Type == JTokenType.String)
                        {
                            // User scripts might return errors with just a plain string message as the
                            // body content, so use it as the exception message
                            message = body.ToString();
                        }
                        else if (body.Type == JTokenType.Object)
                        {
                            // Get the error message, but default to the status description
                            // below if there's no error message present.
                            JToken error = body["error"];
                            if (error != null && error.Type == JTokenType.String)
                            {
                                message = (string)error;
                            }
                            else
                            {
                                JToken description = body["description"];
                                if (description != null && description.Type == JTokenType.String)
                                {
                                    message = (string)description;
                                }
                            }
                        }
                    }
                    else if (response.Content.Headers.ContentType != null &&
                                response.Content.Headers.ContentType.MediaType != null &&
                                response.Content.Headers.ContentType.MediaType.Contains("text"))
                    {
                        message = responseContent;
                    }
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The request could not be completed.  ({0})",
                        response.ReasonPhrase);
                }
            }
            else
            {
                message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The request could not be completed.  ({0})",
                    response.ReasonPhrase);
            }

            // Combine the pieces and throw the exception
            throw new ServiceInvalidOperationException(message, request, response);
        }

        /// <summary>
        /// Transform an IEnumerable of <see cref="HttpMessageHandler"/>s into
        /// a chain of <see cref="HttpMessageHandler"/>s.
        /// </summary>
        /// <param name="handlers">
        /// Chain of <see cref="HttpMessageHandler" /> instances. 
        /// All but the last should be <see cref="DelegatingHandler"/>s. 
        /// </param>
        /// <returns>A chain of <see cref="HttpMessageHandler"/>s</returns>
        private static HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
        {
            if (handlers == null) return DefaultHandlerFactory();

            HttpMessageHandler pipeline = handlers.LastOrDefault() ?? DefaultHandlerFactory();
            DelegatingHandler dHandler = pipeline as DelegatingHandler;
            if (dHandler != null)
            {
                dHandler.InnerHandler = DefaultHandlerFactory();
                pipeline = dHandler;
            }

            // Wire handlers up in reverse order
            IEnumerable<HttpMessageHandler> reversedHandlers = handlers.Reverse().Skip(1);
            foreach (HttpMessageHandler handler in reversedHandlers)
            {
                dHandler = handler as DelegatingHandler;
                if (dHandler == null)
                {
                    throw new ArgumentException(
                        string.Format(
                        "All message handlers except the last must be of the type '{0}'",
                        typeof(DelegatingHandler).Name));
                }

                dHandler.InnerHandler = pipeline;
                pipeline = dHandler;
            }

            return pipeline;
        }
    }
}
