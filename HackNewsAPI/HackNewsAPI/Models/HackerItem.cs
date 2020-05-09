using System;

namespace HackNewsAPI.Models
{
    public class HackerItem
    {
        public string by { get; set; }
        public int descendants { get; set; }  
        public int id { get; set; } 
        public int score { get; set; } 
        public long time { get; set; }
        public DateTime date { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }
}
