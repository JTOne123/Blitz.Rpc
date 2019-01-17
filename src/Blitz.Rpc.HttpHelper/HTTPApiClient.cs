using Blitz.Rpc.Client.BaseClasses;
using Blitz.Rpc.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blitz.Rpc.Client.Helper
{
    /// <summary>
    /// This is the default implementation for a ApiClient, the serializer is replaceable.
    /// The serializer must ofcourse match the serializer in the other end.
    /// It is based on HTTP(S) and works with det default implementation of Blitz.Rpc.Server
    /// </summary>
    public class HttpApiClient : IApiClient
    {
        private readonly HttpClient httpClient;
        private readonly IList<ISerializer> serializers;

        public HttpApiClient(HttpClient httpClient,IList<ISerializer> serializers)
        {
            this.httpClient = httpClient;
            this.serializers = serializers;
        }

        public async Task<object> Invoke(RpcMethodInfo toCall, object[] param)
        {
            //This format is recognized by the default implementation of the server.
            //The base url for the request is expected to be set on the HttpClient
            var requestUri = $"{toCall.ServiceId}.{toCall.Name}-{toCall.ParamType}";

            var theHttpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var outstream = new System.IO.MemoryStream();

            var theSerializer = serializers[0]; //Use the first seriallizer, this can be expanded..  

            switch (param.Length)
            {
                case 0:
                    break;

                case 1:
                    theSerializer.ToStream(outstream, param[0]);
                    break;

                default:
                    theSerializer.ToStream(outstream, param);
                    break;
            }

            outstream.Position = 0;
            theHttpRequest.Content = new StreamContent(outstream);

            theHttpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(theSerializer.ProduceMimeType);

            var response = await httpClient.SendAsync(theHttpRequest);

            if (response.IsSuccessStatusCode)
            {
                if (toCall.ReturnType == typeof(void)) return null;
                var ret = theSerializer.FromStream(await response.Content.ReadAsStreamAsync(), toCall.ReturnType);

                return ret;
            }
            else if ((int)response.StatusCode >= 500)
            {
                var remoteExceptionInfo = theSerializer.FromStream(await response.Content.ReadAsStreamAsync(), typeof(RemoteExceptionInfo));
                throw new WebRpcCallFailedException((RemoteExceptionInfo)remoteExceptionInfo);
            }
            else
            {
                throw new HttpRequestException($"{response.StatusCode} {response.Content.ReadAsStringAsync().Result}");
            }
        }
    }
}