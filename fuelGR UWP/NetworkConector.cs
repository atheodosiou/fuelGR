using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace fuelGR_UWP
{
    class NetworkConector
    {
        public async Task<string> GetResponese(string url)
        {
            string response = string.Empty;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            Uri requestUri = new Uri(url);

            try
            {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadAsStringAsync();
            }
            catch(Exception ex)
            {
                response= "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            httpResponse.Dispose();
            httpClient.Dispose();
            return response;
        }
    }
}
