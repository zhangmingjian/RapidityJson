using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonElementTest
    {
        [Fact]
        public void ToStringTest()
        {
            var obj = new JsonObject();
            obj.AddProperty("id", 100);
            obj.AddProperty("name", "zhangsan");
            obj["Birthday"] = "2000-10";
            obj.AddProperty("loopObject", new JsonNumber(1000));
            var json = obj.ToString(new JsonOption { LoopReferenceProcess = Converters.LoopReferenceProcess.Error });

            var person = obj.To<Person>();

            var jStr = new JsonString("look");
            var ss = jStr.ToString();
        }

        [Fact]
        public void JsonNumberTest()
        {
            var obj = new JsonObject();
            obj.AddProperty("id", new JsonNumber("12.23324"));
            var number = new JsonNumber("12.23324");
            var json = obj.ToString();
        }

        [Fact]
        public void GetTest()
        {
            var json = "{\"Items\":[{\"Code\":\"THPHR\",\"enable\":\"TRUE1\",\"id\":11111,\"CName\":\"云途全球专线挂号（特惠普货）\",\"EName\":\"Yunexpress Global Driect Economical Line （general ）\",\"HasTrackingNumber\":true,\"DisplayName\":\" 云途全球专线挂号（特惠普货）\"},{\"Code\":\"WISHTHZX\",\"CName\":\"wish  邮云途全球专线挂号\",\"EName\":\"Wish Yunexpress Global Economical Line\",\"HasTrackingNumber\":true,\"DisplayName\":\"wish  邮云途全球专线挂号\"},{\"Code\":\"JMZXR\",\"CName\":\"Joom  邮云途专线挂号\",\"EName\":\"Joom YunExpress Direct Line\",\"HasTrackingNumber\":true,\"DisplayName\":\"Joom  邮云途专线挂号\"},{\"Code\":\"DEZXR\",\"CName\":\" 德国专线挂号\",\"EName\":\"YunExpress Germany Direct Line\",\"HasTrackingNumber\":true,\"DisplayName\":\" 德国专线挂号\"},{\"Code\":\"THZXR\",\"CName\":\" 云途全球专线挂号（特惠带电）\",\"EName\":\"Yunexpress Global Driect Economical Line\",\"HasTrackingNumber\":true,\"DisplayName\":\" 云途全球专线挂号（特惠带电）\"},{\"Code\":\"CNDWR\",\"CName\":\" 华南快速小包挂号(DG)\",\"EName\":\"China Post Registered Air Mail-DongGuan\",\"HasTrackingNumber\":true,\"DisplayName\":\" 华南快速小包挂号(DG)\"},{\"Code\":\"GBZXR\",\"CName\":\" 英国专线标准\",\"EName\":\"YunExpress Britain Direct Line\",\"HasTrackingNumber\":true,\"DisplayName\":\" 英国专线标准\"},{\"Code\":\"BKZXR\",\"CName\":\" 云途全球专线挂号（标快）\",\"EName\":\"YunExpress Global Direct Line\",\"HasTrackingNumber\":true,\"DisplayName\":\" 云途全球专线挂号（标快）\"}],\"Code\":1,\"Message\":\" 提交成功\",\"RequestId\":\"1911ac4cb7e445e09472fceacb82d863\",\"TimeStamp\":\"2020-02-20T09:58:45.4372247+00:00\"}";
            var jObj = JsonElement.Create(json);
            var ele = jObj.Get("Items/0/code");
            var ival = jObj.TryGetInt("Items/0/id");
        }

        [Fact]
        public void SelectTest()
        {
            var arr = new JsonArray();

            arr.Add(10);
            arr.Add("fefe");
            arr.Add(false);
            arr.Add(new JsonObject
            {
                ["name"] = "fawejflkwjkef",
                ["other"] = null
            });
            var arr2 = arr.Slice(0, 4, 4);
            var token = Newtonsoft.Json.Linq.JToken.Parse(arr2.ToString());
            var token2 = token.SelectTokens("$..name");
        }
    }
}
