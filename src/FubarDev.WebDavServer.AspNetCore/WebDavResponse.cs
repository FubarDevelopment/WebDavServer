// <copyright file="WebDavResponse.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace FubarDev.WebDavServer.AspNetCore
{
    /// <summary>
    /// The implementation of the <see cref="IWebDavResponse"/>
    /// </summary>
    /// <remarks>
    /// This class wraps a <see cref="HttpResponse"/> to be accessible by the WebDAV serves <see cref="IWebDavResult"/>.
    /// </remarks>
    public class WebDavResponse : IWebDavResponse
    {
        private readonly HttpResponse _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavResponse"/> class.
        /// </summary>
        /// <param name="dispatcher">The WebDAV HTTP method dispatcher</param>
        /// <param name="response">The ASP.NET Core HTTP response</param>
        public WebDavResponse(IWebDavDispatcher dispatcher, HttpResponse response)
        {
            _response = response;
            Dispatcher = dispatcher;
            Headers = new HeadersDictionary(_response.Headers);
        }

        /// <inheritdoc />
        public IWebDavDispatcher Dispatcher { get; }

        /// <inheritdoc />
        public IDictionary<string, string[]> Headers { get; }

        /// <inheritdoc />
        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        /// <inheritdoc />
        public Stream Body => _response.Body;

        private class HeadersDictionary : IDictionary<string, string[]>
        {
            private readonly IHeaderDictionary _headers;

            public HeadersDictionary(IHeaderDictionary headers)
            {
                _headers = headers;
            }

            public int Count => _headers.Count;

            public bool IsReadOnly => _headers.IsReadOnly;

            public ICollection<string> Keys => _headers.Keys;

            public ICollection<string[]> Values => _headers.Values.Select(x => x.ToArray()).ToList();

            public string[] this[string key]
            {
                get { return _headers[key].ToArray(); }
                set { _headers[key] = new StringValues(value); }
            }

            public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
            {
                return _headers.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value.ToArray())).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection<KeyValuePair<string, string[]>>.Add(KeyValuePair<string, string[]> item)
            {
                _headers[item.Key] = new StringValues(item.Value);
            }

            public void Clear()
            {
                _headers.Clear();
            }

            bool ICollection<KeyValuePair<string, string[]>>.Contains(KeyValuePair<string, string[]> item)
            {
                var values = _headers[item.Key].ToArray();
                if (item.Value.Length != values.Length)
                    return false;

                for (var i = 0; i != values.Length; ++i)
                {
                    if (item.Value[i] != values[i])
                        return false;
                }

                return true;
            }

            void ICollection<KeyValuePair<string, string[]>>.CopyTo(KeyValuePair<string, string[]>[] array, int arrayIndex)
            {
                foreach (KeyValuePair<string, StringValues> header in _headers)
                {
                    array[arrayIndex++] = new KeyValuePair<string, string[]>(header.Key, header.Value.ToArray());
                }
            }

            bool ICollection<KeyValuePair<string, string[]>>.Remove(KeyValuePair<string, string[]> item)
            {
                return Remove(item.Key);
            }

            public void Add(string key, string[] value)
            {
                _headers.Add(key, new StringValues(value));
            }

            public bool ContainsKey(string key)
            {
                return _headers.ContainsKey(key);
            }

            public bool Remove(string key)
            {
                return _headers.Remove(key);
            }

            public bool TryGetValue(string key, out string[] value)
            {
                StringValues values;
                if (_headers.TryGetValue(key, out values))
                {
                    value = values.ToArray();
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}
