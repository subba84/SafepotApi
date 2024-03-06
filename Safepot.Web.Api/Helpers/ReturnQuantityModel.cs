using Safepot.Entity;

namespace Safepot.Web.Api.Helpers
{
    public class ReturnQuantityModel : SfpReturnQuantity
    {
        public List<KeyValuePair<string,string>> Files { get; set; }
        public List<byte[]> FileUrls { get; set; }
    }
}
