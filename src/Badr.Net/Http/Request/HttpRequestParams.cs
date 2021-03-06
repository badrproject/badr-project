//
// HttpRequestMethodParams.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Badr.Net.Http.Request
{
    public class HttpRequestParams: IEnumerable
    {
        private Dictionary<string, object> _methodParams;

        public HttpRequestParams()
        {
            _methodParams = new Dictionary<string, object>();
        }

        internal void Add(string key, string value)
        {
            if (key != null && value != null && value != "")
                if (!IsArrayParamKey(key))
                    _methodParams[key] = value;
                else
                {
                    string[] arrValue;

                    if (!_methodParams.ContainsKey(key)){
                        arrValue = new string[0];
                        _methodParams.Add(key, arrValue);
                    }else
                        arrValue = (string[])_methodParams[key];

                    Array.Resize(ref arrValue, arrValue.Length + 1);
                    arrValue[arrValue.Length - 1] = value;
                    _methodParams[key] = arrValue;
                }
        }

        internal bool IsArrayParamKey(string key)
        {
            return key != null && key.EndsWith("[]");
        }

        /// <summary>
        /// Convert a request param to the specified type T. Supported target types are: int, double, decimal & float
        /// </summary>
        /// <typeparam name="T">The type to convert to (int, double, decimal or float)</typeparam>
        /// <param name="key">The param key</param>
        /// <returns>The converted value -or- throws an exception if T is not supported.</returns>
        public T Get<T> (string key, T defaultValue = default(T))
		{
			if (!Contains (key))
				return defaultValue;

			Type typeOfT = typeof(T);

			if (!IsArrayParamKey (key))
			{
				if(typeOfT.Equals(typeof(string)))
					return (T)this[key];
				else
					return (T)ConvertTo<T> ((string)this [key]);
			}
            else
                throw new Exception(string.Format("Conversion of an 'array' to <{0}> not supported", typeOfT));
        }

        protected object ConvertTo<T>(string value)
        {
            Type typeOfT = typeof(T);

            if (typeOfT.Equals(typeof(int)))
                return int.Parse(value);
            else if (typeOfT.Equals(typeof(double)))
                return double.Parse(value);
            else if (typeOfT.Equals(typeof(decimal)))
                return decimal.Parse(value);
            else if (typeOfT.Equals(typeof(float)))
                return float.Parse(value);
            else
                throw new Exception(string.Format("Conversion of request param to <{0}> not supported. Can convert only to: int, double, decimal & float", typeOfT));
        }

        public T[] GetArrayOf<T>(string key)
        {
            object arrObj = this[key];

			if(arrObj == null)
				return null;

            if(!(arrObj is string[]))
                throw new Exception(string.Format("Request param '{0}' is not an array.", key));

            string[] arr = (string[])arrObj;
            int arrLength = arr.Length;
            T[] result = new T[arrLength];
            for(int i=0;i<arrLength;i++)
            {
                result[i] = (T)ConvertTo<T>(arr[i]);
            }
            return result;
        }

        public bool Contains(string key)
        {
            return _methodParams.ContainsKey(key);
        }

        public object this[string key]
        {
            get
            {
                if (_methodParams.ContainsKey(key))
                    return _methodParams[key];
                return null;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _methodParams.Values.GetEnumerator();
        }
    }	

	public class RequestParam
	{
		public String Value { get; private set; }
		public RequestParam (string value)
		{
			Value = value;
		}
	}
}