using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ScrapingMVC.Models;
using ScrapingMVC.Helpers;

namespace ScrapingMVC.Helpers
{
    public static class GSTNDataHelper
    {


        public static dynamic GetPublicReturnStatus(string gstin, string fy)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appliction/json"));
                    httpClient.DefaultRequestHeaders.Add("username", "ASP117911");
                    httpClient.DefaultRequestHeaders.Add("password", "Qupn--zs");
                    var authUrl = "https://aspone.in/api/public_apis/common_authenticate";

                    // Send the request to the server
                    HttpResponseMessage authResponse = httpClient.PostAsync(authUrl, null).Result;
                    // Get the response
                    var authResponseString = authResponse.Content.ReadAsStringAsync().Result;

                    ShalibhadraAuth authObj = JsonConvert.DeserializeObject<ShalibhadraAuth>(authResponseString);

                    var token = authObj.auth_token;

                    using (HttpClient dataClient = new HttpClient())
                    {
                        try
                        {
                            dataClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("appliction/json"));
                            dataClient.DefaultRequestHeaders.Add("username", "ASP117911");
                            dataClient.DefaultRequestHeaders.Add("password", "Qupn--zs");
                            dataClient.DefaultRequestHeaders.Add("token", token);
                            var dataUrl = "https://aspone.in/api/public_apis/view_and_track_status";

                            HttpContent content = new StringContent("{\"gstin\":\"" + gstin + "\",\"fy\":\"" + fy + "\"}", Encoding.UTF8, "application/json");

                            // Send the request to the server
                            HttpResponseMessage dataResponse = dataClient.PostAsync(dataUrl, content).Result;
                            // Get the response
                            var dataResponseString = dataResponse.Content.ReadAsStringAsync().Result;

                            dynamic dataResponseJson = JsonConvert.DeserializeObject(dataResponseString);

                            var efiled = dataResponseJson[0].EFiledList;

                            return dataResponseJson;

                        }
                        catch (Exception ex)
                        {
                            //LogException(ex);
                            return ex;
                        }

                    }


                    // return resBodyJson;
                }
                catch (Exception ex)
                {
                    //LogException(ex);
                    
                    return ex;

                }
            }


        }


        //private static void LogException(Exception ex)
        //{
        //    SingletonLogger.Instance.Error(@"Message: " + ex.Message + Environment.NewLine +
        //                                          "Stack Trace: " + ex.StackTrace);
        //}

    }
}