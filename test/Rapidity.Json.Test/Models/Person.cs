using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Test
{
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Guid? Number { get; set; }

        public DateTime Birthday { get; set; }

        public Person Child { get; set; }

        public float floadField;

        public string strField;

    }
}
