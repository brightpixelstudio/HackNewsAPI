using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackNewsAPI.Models;
using HackNewsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HackNewsAPI.Controllers
{
    [Route("HackerNews")] 
    public class HackerNewsController : ControllerBase 
    {
        private IHackerRepository _hackerRepository;
        private IConfiguration _configuration;

        public HackerNewsController(IConfiguration configuration, IHackerRepository hackerRepository)
        {
            _hackerRepository = hackerRepository;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]int take = 10, [FromQuery]int skip = 0)
        {
            int maxLastestCount;
            if (!int.TryParse(_configuration.GetSection("AppSettings").GetSection("MaxLastestCount").Value, out maxLastestCount))
            {
                throw new InvalidOperationException("Invalid MaxLastestCount in AppSettings.json");
            }

            // ensure take is valid - could use Fluent Validation
            if (take > maxLastestCount)
            {
                return StatusCode(400, $"Take must be less than {maxLastestCount}.");
            }

            // get the latest news (returns 500)
            List<int> newestArticles = await _hackerRepository.GetLatestNewsListAsync();

            // get the right subset
            newestArticles = newestArticles.Skip(skip).Take(take).ToList();

            // TODO - Cache
            // compare to startup cache to see if there are any new stories 
            // take new stories ID's and add them to the start of the cached ones.

            // loop through each one getting the article.
            List<HackerItem> items = new List<HackerItem>();
            foreach (int newestArticleId in newestArticles)
            {
                var item = await _hackerRepository.GetNewsItemAsync(newestArticleId);

                // convert the time to date time
                DateTime newDatetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                item.date = newDatetime.AddSeconds(item.time).ToLocalTime();

                // add to list
                items.Add(item);
            }
            return Ok(items);
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchLatestNews([FromQuery]string searchString, [FromQuery]int take)
        {
            int maxLastestCount;
            if (!int.TryParse(_configuration.GetSection("AppSettings").GetSection("MaxLastestCount").Value, out maxLastestCount))
            {
                throw new InvalidOperationException("Invalid MaxLastestCount in AppSettings.json");
            }

            // ensure take is valid - could use Fluent Validation
            if (take > maxLastestCount)
            {
                return StatusCode(400, $"Take must be less than {maxLastestCount}.");
            }

            int minSearchLength;
            if (!int.TryParse(_configuration.GetSection("AppSettings").GetSection("MinSearchLength").Value, out minSearchLength))
            {
                throw new InvalidOperationException("Invalid MinSearchLength in AppSettings.json");
            }

            // ensure string it long enough - could use Fluent Validation
            if (searchString.Length < minSearchLength)
            {
                return StatusCode(400, $"Search string must be at least {minSearchLength} characters.");
            }

            // get the latest news (returns 500)
            List<int> newestArticles = await _hackerRepository.GetLatestNewsListAsync();

            // take the requested amount
            newestArticles = newestArticles.Take(take).ToList();

            // loop through each one looking for the search text
            searchString = searchString.ToLower();
            List<HackerItem> items = new List<HackerItem>();
            foreach (int newestArticleId in newestArticles)
            {
                var item = await _hackerRepository.GetNewsItemAsync(newestArticleId);

                // Do a lowercase search on any part of the string
                if (item.title.ToLower().Contains(" " + searchString + " "))
                {
                    // highlight the search text
                    item.title = item.title.ToLower().Replace(" " + searchString + " ", "<mark>" + searchString + "</mark>");
                    
                    // convert the time to date time
                    DateTime newDatetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    item.date = newDatetime.AddSeconds(item.time).ToLocalTime();

                    // add to list
                    items.Add(item);
                }
            }
            return Ok(items);
        }
    }
}