using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Rachel
{
    public class httpclientRachel
    {
        public static string err = "";
        //public static HttpClient client = new HttpClient() { BaseAddress = new Uri("https://localhost:7021/") };
        public static HttpClient client = new HttpClient() { BaseAddress = new Uri("https://userdatawebapi20220502202651.azurewebsites.net/") };
        public static async Task<string> Getstate()
        {
            try
            {
                //使用 async 方法從網路 url 上取得回應
                var response = await client.GetAsync($"Rachel");
                err = await response.Content.ReadAsStringAsync();
                //如果 httpstatus code 不是 200 時會直接丟出 expection
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<String> Post(string UID)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            string json = "";
            // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
            // 發出 post 並取得結果
            HttpResponseMessage response = client.PostAsync($"Rachel/{UID}", contentPost).GetAwaiter().GetResult();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
