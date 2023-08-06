﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

using System.Threading;
using Printborg.Types;
using Printborg.Json;
using System.Net;
using System.Runtime.CompilerServices;

namespace Printborg.API
{
    static public class Auto1111Controller
    {

        /// <summary>
        /// Skip any previously started jobs
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        public static async Task<string> Skip(string baseAddress)
        {
            if (baseAddress == null || baseAddress == "") throw new ArgumentException(baseAddress, nameof(baseAddress));

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                string endpoint = "/sdapi/v1/skip";
                var response = await client.PostAsync(endpoint, null);
                return await response.Content.ReadAsStringAsync();
            }
        }

        public static async Task<string> GetControlnetModules(string baseAddress, Action<string, double> ReportProgress)
        {
            if (baseAddress == null || baseAddress == "") throw new ArgumentException(baseAddress, nameof(baseAddress));

            return await Auto1111Controller.Get(baseAddress, "/controlnet/module_list", ReportProgress, 30);
        }
        public static async Task<string> GetControlnetModels(string baseAddress, Action<string, double> ReportProgress)
        {
            if (baseAddress == null || baseAddress == "") throw new ArgumentException(baseAddress, nameof(baseAddress));

            return await Auto1111Controller.Get(baseAddress, "/controlnet/model_list", ReportProgress, 30);
        }

        /// <summary>
        /// Generic Get request
        /// </summary>
        /// <param name="baseAddress">base address if API provider</param>
        /// <param name="endpoint">specific Get endpoint (e.g. /endpoint )</param>
        /// <param name="ReportProgress">Delegate function to report the task progress. first value is a short label, second value is percentage in decimal points (0.-1)</param>
        /// <param name="timeout">time out in seconds before request is cancelled.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<string> Get(string baseAddress, string endpoint, Action<string, double> ReportProgress, int timeout = 30)
        {
            if (baseAddress == null || baseAddress == "") throw new ArgumentException(baseAddress, nameof(baseAddress));


            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                if (timeout == 0) client.Timeout = Timeout.InfiniteTimeSpan;
                else client.Timeout = TimeSpan.FromSeconds(timeout);

                var rawResponse = client.GetAsync(endpoint);

                while (!rawResponse.IsCompleted)
                {
                    ReportProgress("Fetching", 0.5d);
                    await Task.Delay(2000);
                }

                rawResponse.Result.EnsureSuccessStatusCode();

                string responseContent = await rawResponse.Result.Content.ReadAsStringAsync();
                return responseContent;
            }
        }

        public static async Task<string> Auto1111TextToImageWithProgressPolling(Action<string, double> ReportProgress, string baseAddress, Auto1111Payload payload, int pollFrequency = 1000)
        {
            using (HttpClient client = new HttpClient())
            {
                if (baseAddress == null || baseAddress == "") throw new ArgumentException(baseAddress, nameof(baseAddress));

                if (payload == null) throw new ArgumentNullException(nameof(payload));

                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = Timeout.InfiniteTimeSpan;
                string textToImageUri = "/sdapi/v1/txt2img";

                // prep payload
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // send post request
                var rawResponse = client.PostAsync(textToImageUri, content);


                // continuously poll api for task progress until completed
                while (!rawResponse.IsCompleted)
                {
                    //request to api to get progress status
                    string progressUri = "/sdapi/v1/progress";
                    var progressResponse = await client.GetAsync(progressUri);
                    // throw error if not successful
                    progressResponse.EnsureSuccessStatusCode();

                    string responseContent = await progressResponse.Content.ReadAsStringAsync();
                    var status = JsonConvert.DeserializeObject<ProgressStatus>(responseContent);
                    ReportProgress("generating", status.Progress);

                    await Task.Delay(pollFrequency);
                }

                ReportProgress("finished", 1.0);

                return await rawResponse.Result.Content.ReadAsStringAsync();
            }
        }



        public static async Task<ResponseObject> Auto1111TextToImage(string baseAddress, Auto1111Payload payload)
        {
            using (HttpClient client = new HttpClient())
            {

                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = Timeout.InfiniteTimeSpan;

                // IApiHandler handler = new Auto1111Handler(client);

                // skip any jobs that are running from before
                //var interruptStatus = await Skip(baseAddress, client);
                //Console.WriteLine(interruptStatus);
                string uri = baseAddress + "/sdapi/v1/txt2img";

                var requestMsg = new HttpRequestMessage();
                var json = JsonConvert.SerializeObject(payload);

                // requestMsg.Content = new StringContent(json);
                // requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                Console.WriteLine("test");
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync("/sdapi/v1/txt2img", content);


                // continuously poll api for task progress until completed
                while (!response.IsCompleted)
                {
                    Console.WriteLine("still in progress ...");
                    await pollProgess(baseAddress, client);

                    await Task.Delay(2000);
                }

                Console.WriteLine("Image generation finished ...");

                var result = await response.Result.Content.ReadAsStringAsync();

                var resultObject = JsonConvert.DeserializeObject<ResponseObject>(result);
                return resultObject;
            }
        }

        public static async Task<ProgressStatus> pollProgess(string baseAddress, HttpClient client)
        {
            string progressUri = baseAddress + "/sdapi/v1/progress";

            var progressResponse = await client.GetAsync(progressUri);
            progressResponse.EnsureSuccessStatusCode();

            string responseContent = await progressResponse.Content.ReadAsStringAsync();
            var status = JsonConvert.DeserializeObject<ProgressStatus>(responseContent);
            if (status != null)
            {
                Console.WriteLine("Progress: " + status.Progress);
                Console.WriteLine("estimated eta: " + status.Eta);
                Console.WriteLine("Job: " + status.stateObject.Job);
                return status;
            }
            return null;
        }

        public static async Task<string> Auto1111_T2I(string address, string username, string password, Auto1111Payload payload)
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("... Attempting to send POST request /sdapi/v1/txt2img");
                string endpoint = address + "/sdapi/v1/txt2img";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

                var json = JsonConvert.SerializeObject(payload);
                request.Content = new StringContent(json);

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request);
                Console.WriteLine(response.StatusCode);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("... results coming in ....");


                return result;
            }
        }
    }
}