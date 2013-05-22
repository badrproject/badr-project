//
// HttpResponse.cs
//
// Author: najmeddine nouri
//
// Copyright (c) 2013 najmeddine nouri, amine gassem
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Except as contained in this notice, the name(s) of the above copyright holders
// shall not be used in advertising or otherwise to promote the sale, use or other
// dealings in this Software without prior written authorization.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Badr.Net.Http.Request;
using Badr.Net.Utils;

namespace Badr.Net.Http.Response
{

	public class HttpResponse{

        /// <summary>
        /// Response body content-type to use by default (initial value is 'text/html').
        /// </summary>
        public const string DEFAULT_CONTENT_TYPE = "text/html";
        /// <summary>
        /// Response body charset to use by default (initial value is 'utf-8').
        /// </summary>
        public const string DEFAULT_CHARSET = "utf-8";

        public HttpResponse(HttpRequest request, string contentType = DEFAULT_CONTENT_TYPE, string charset = DEFAULT_CHARSET)
        {
            Request = request;

            try
            {
                Encoding = Encoding.GetEncoding(charset);
            }
            catch { Encoding = Encoding.UTF8; }

            Status = HttpResponseStatus._200;
            Cookies = new HttpCookie();

            Headers = new Dictionary<HttpResponseHeaders, string>();
            Headers.Add(HttpResponseHeaders.Date, DateTime.Now.ToUniversalTime().ToString("r"));
            Headers.Add(HttpResponseHeaders.ContentType, string.Format("{0}; charset={1}", contentType, charset));
        }

		public byte[] Data{ get { return GetData (); } }
        protected HttpRequest Request { get; private set; }
        public HttpCookie Cookies { get; private set; }
        public Encoding Encoding { get; private set; }
        public Dictionary<HttpResponseHeaders, string> Headers { get; private set; }
        public string Body { get; set; }
        public bool ConnectionKeepAlive { get; private set; }
        public HttpResponseStatus Status { get; set; }

		public static HttpResponse CreateResponse (HttpRequest request, HttpResponseStatus status)
		{
			HttpResponse response = new HttpResponse (request) { Status = status };
			
			if (status == HttpResponseStatus._404)
				response.Body = @"<html><body style=""font-size:404;font-family:lucida console"">404 Not found</body></html>" + HttpRequest.WR_SEPARATOR;
			else if (status == HttpResponseStatus._403)
				response.Body = @"<html><body style=""font-size:403;font-family:lucida console"">403 Forbidden</body></html>" + HttpRequest.WR_SEPARATOR;
			else if (status == HttpResponseStatus._408)
				response.Body = @"<html><body style=""font-size:408;font-family:lucida console"">408 Bad Request</body></html>" + HttpRequest.WR_SEPARATOR;
			
			return response;			
		}

		protected virtual byte[] GetData(){

			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0} {1}{2}", Request.Protocol, Status.ToResponseHeaderText(), HttpRequest.WR_SEPARATOR);

			// Response body bytes
			byte[] bodyBytes = GetBodyData();
			if(bodyBytes != null)
				if (bodyBytes.Length > 128 && Request.CanGzip && !Request.IsAjax) {

					if(Headers.ContainsKey(HttpResponseHeaders.ContentEncoding))
						Headers.Remove(HttpResponseHeaders.ContentEncoding);

					bodyBytes = bodyBytes.Compress();
					if (bodyBytes.Length > 0)
					    Headers [HttpResponseHeaders.ContentLength] = (bodyBytes.Length).ToString ();

					Headers[HttpResponseHeaders.ContentEncoding] = "gzip";

				}

            if (!Headers.ContainsKey(HttpResponseHeaders.Connection))
                Headers[HttpResponseHeaders.Connection] = "Close";

            ConnectionKeepAlive = Headers[HttpResponseHeaders.Connection] == "keep-alive";            

			if (Headers.Count > 0) {
				foreach (KeyValuePair<HttpResponseHeaders, string> headerValues in Headers)
					if (headerValues.Key != HttpResponseHeaders.SetCookie)
						sb.AppendFormat ("{0}:{1}{2}", headerValues.Key.TotHeaderText(), headerValues.Value, HttpRequest.WR_SEPARATOR);

				if (Cookies.Count > 0)
                    sb.AppendFormat("{0}{1}", Cookies.ToHttpHeader(header: HttpResponseHeaders.SetCookie.TotHeaderText() + ":"), HttpRequest.WR_SEPARATOR);
			}
			
			sb.Append (HttpRequest.WR_SEPARATOR);

			// Response headers bytes
            byte[] bytes = sb.ToString().GetBytes(Encoding);

			// Response headers+body bytes
			int totalLength = bytes.Length + (bodyBytes != null ? bodyBytes.Length : 0);
			byte[] allBytes = new byte[totalLength];

			bytes.CopyTo(allBytes, 0);
			if(bodyBytes != null)
				bodyBytes.CopyTo(allBytes, bytes.Length);

			return allBytes;
		}

		protected virtual byte[] GetBodyData()
		{
			return (Body + HttpRequest.WR_SEPARATOR).GetBytes(Encoding);
		}
	}
}