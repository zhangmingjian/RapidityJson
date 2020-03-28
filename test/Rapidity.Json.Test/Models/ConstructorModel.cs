using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Test.Models
{
    public class ConstructorModel
    {
        public Person Person { get; set; }
        public ConstructorModel(Person p, IEnumerable<int> list, int i, bool? b, Guid g, DateTime? d, Dictionary<string, string> dic)
        {
            Person = p;
        }
    }


    public class Rootobject
    {
        public string date { get; set; }
        public Story[] stories { get; set; }
        [JsonProperty("top_stories")]
        [Newtonsoft.Json.JsonProperty("top_stories")]
        public IEnumerable<Top_Stories> topstories { get; set; }
    }

    public class Story
    {
        public string image_hue { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string hint { get; set; }
        public string ga_prefix { get; set; }
        public string[] images { get; set; }
        public int type { get; set; }
        public int id { get; set; }
    }

    public class Top_Stories
    {
        public string image_hue { get; set; }
        public string hint { get; set; }
        public string url { get; set; }
        public string image;
        public string title;
        public string ga_prefix;
        public int type;
        public int id;
    }

}
