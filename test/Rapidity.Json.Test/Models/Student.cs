using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Test.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Sex Sex { get; set; }

        public DateTime? Birthday { get; set; }

        public string Address { get; set; }
    }

    public enum Sex
    {
        Unkown,
        Male,
        Female,
    }
}
