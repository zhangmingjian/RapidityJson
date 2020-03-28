using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Test
{
    public class Person
    {
        [JsonProperty(Sort = 10)]
        public int Id { get; set; }

        public string Name { get; set; }

        public Guid? Number { get; set; }

        public DateTime Birthday { get; set; }

        [JsonProperty("Children", Sort = -1)]
        public Person Child { get; set; }

        public float floadField;

        public string strField;

        public ICollection<DateTimeKind?> dateTimeKinds { get; set; }

        [JsonProperty(Ignore = true)]
        public EnvironmentVariableTarget EnumField;

    }

}
