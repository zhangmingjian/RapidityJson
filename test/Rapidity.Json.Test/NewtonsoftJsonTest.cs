using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class NewtonsoftJsonTest
    {
        [Fact]
        public void StackTest()
        {
            var json = "[100,123,45]";
            var list = JsonConvert.DeserializeObject(json, typeof(List<int>));
        }
    }
}
