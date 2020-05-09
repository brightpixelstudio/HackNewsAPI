using HackNewsAPI.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HackNewsAPI.Services
{
    public interface IHackerRepository
    {
        Task<List<int>> GetLatestNewsListAsync(); 
        Task<HackerItem> GetNewsItemAsync(int id);
    }

    public class HackerRepository : IHackerRepository  
    {
        IConfiguration _configuration;

        public HackerRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<List<int>> GetLatestNewsListAsync()
        {
            string baseUrl = _configuration.GetSection("AppSettings").GetSection("HackerNewsBaseURL").Value;

            // Make the API call to obtain the latest new story Id's
            List<int> latestStories;
            using (var httpClient = new HttpClient())  
            {
                using (var response = await httpClient.GetAsync($"{baseUrl }newstories.json?print=pretty"))
                {
                    latestStories = JsonConvert.DeserializeObject<List<int>>(await response.Content.ReadAsStringAsync());
                }
            }

            return latestStories;  
        }

        public async Task<HackerItem> GetNewsItemAsync(int id)
        {
            string baseUrl = _configuration.GetSection("AppSettings").GetSection("HackerNewsBaseURL").Value;

            // Make the API call to obtain a specific new story
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync($"{baseUrl}item/{id}.json?print=pretty"))
                {
                    return JsonConvert.DeserializeObject<HackerItem>(await response.Content.ReadAsStringAsync());
                }
            }
        }
    }
}
