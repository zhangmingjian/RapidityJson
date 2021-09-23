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

    public class QueryOrderResponse
    {
        public int code;
        public string message;
        public List<CpOrder> data;
        public string nextToken;
    }

    public class CpOrder
    {
        public long shipmentBoxId;
        public long orderId;
        public DateTime orderedAt;
        public Orderer orderer;
        public DateTime paidAt;
        public string status;
        public int shippingPrice;
        public int remotePrice;
        public bool remoteArea;
        public string parcelPrintMessage;
        public bool splitShipping;
        public bool ableSplitShipping;
        public Receiver receiver;
        public Orderitem[] orderItems;
        public Overseashippinginfodto overseaShippingInfoDto;
        public string deliveryCompanyName;
        public string invoiceNumber;
        public string inTrasitDateTime;
        public string deliveredDate;
        public string refer;
        public string shipmentType;


        public class Orderer
        {
            public string name;
            public string email;
            public string safeNumber;
            public object ordererNumber;
        }

        public class Receiver
        {
            public string name;
            public string safeNumber;
            public object receiverNumber;
            public string addr1;
            public string addr2;
            public string postCode;
        }

        public class Overseashippinginfodto
        {
            public string personalCustomsClearanceCode;
            public string ordererSsn;
            public string ordererPhoneNumber;
        }

        public class Orderitem
        {
            public int vendorItemPackageId;
            public string vendorItemPackageName;
            public long productId;
            public long vendorItemId;
            public string vendorItemName;
            public int shippingCount;
            public int salesPrice;
            public int orderPrice;
            public int discountPrice;
            public int instantCouponDiscount;
            public int downloadableCouponDiscount;
            public int coupangDiscount;
            public string externalVendorSkuCode;
            public object etcInfoHeader;
            public object etcInfoValue;
            public object etcInfoValues;
            public long sellerProductId;
            public string sellerProductName;
            public string sellerProductItemName;
            public string firstSellerProductItemName;
            public int cancelCount;
            public int holdCountForCancel;
            public string estimatedShippingDate;
            public string plannedShippingDate;
            public DateTime invoiceNumberUploadDate;
            public Extraproperties extraProperties;
            public bool pricingBadge;
            public bool usedProduct;
            public string confirmDate;
            public string deliveryChargeTypeName;
            public bool canceled;
        }

        public class Extraproperties
        {
        }
    }

}
