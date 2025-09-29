using System;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace ApiTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
                if (args.Length == 1 && args[0] == "--help")
    {
        Console.WriteLine(@"
📘 REST API CLI Tester Usage Guide v1.0.0
    
Usage:
  ApiTester <METHOD> <URL> [key=value] [options]

METHOD:
  GET       Send a GET request
  POST      Send a POST request
  PUT       Send a PUT request
  DELETE    Send a DELETE request

URL:
  The base URL of the API endpoint

Parameters:
  key=value           URL query parameters
  header:key=value    Custom HTTP headers
  body:{json}         JSON body for POST/PUT
  save:filename       Save response to a file

Examples:
  ApiTester GET https://api.example.com/data userId=1
  ApiTester POST https://api.example.com/data body:{""name"":""Ali""} header:Authorization=BearerToken123
  ApiTester PUT https://api.example.com/update id=42 body:{""status"":""active""}
  ApiTester DELETE https://api.example.com/delete id=42
  ApiTester POST https://api.example.com/data body:{""name"":""Ali""} save:response.json

");
        return;
    }

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ApiTester <GET|POST|PUT|DELETE> <base_url> [key=value]...");
                return;
            }

            string method = args[0].ToUpper();
            string baseUrl = args[1];
            var queryParams = new Dictionary<string, string>();
            var headers = new Dictionary<string, string>();
            string jsonBody = null;
            string outputFile = null;

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("header:"))
                {
                    var headerParts = args[i].Substring(7).Split('=');
                    if (headerParts.Length == 2)
                        headers[headerParts[0]] = headerParts[1];
                }
                else if (args[i].StartsWith("body:"))
                {
                    jsonBody = args[i].Substring(5);
                }
                else if (args[i].StartsWith("save:"))
                {
                    outputFile = args[i].Substring(5);
                }
                else
                {
                    var parts = args[i].Split('=');
                    if (parts.Length == 2)
                        queryParams[parts[0]] = parts[1];
                }
            }

            var client = new HttpClient();
            foreach (var h in headers)
                client.DefaultRequestHeaders.Add(h.Key, h.Value);

            var uriBuilder = new UriBuilder(baseUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var kvp in queryParams)
                query[kvp.Key] = kvp.Value;
            uriBuilder.Query = query.ToString();
            var finalUrl = uriBuilder.ToString();

            try
            {
                HttpResponseMessage response;
                var content = jsonBody != null ? new StringContent(jsonBody, Encoding.UTF8, "application/json") : null;

                switch (method)
                {
                    case "POST":
                        response = await client.PostAsync(finalUrl, content ?? new StringContent(""));
                        break;
                    case "PUT":
                        response = await client.PutAsync(finalUrl, content ?? new StringContent(""));
                        break;
                    case "DELETE":
                        response = await client.DeleteAsync(finalUrl);
                        break;
                    default:
                        response = await client.GetAsync(finalUrl);
                        break;
                }

                string responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"🔗 Requested URL: {finalUrl}");
                Console.WriteLine($"📦 Status Code: {response.StatusCode}");
                Console.WriteLine($"📝 Response:\n{responseText}");

                if (outputFile != null)
                {
                    await File.WriteAllTextAsync(outputFile, responseText);
                    Console.WriteLine($"💾 Response saved to: {outputFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Request failed: {ex.Message}");
            }
        }
    }
}

