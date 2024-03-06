namespace Safepot.Web.Api
{
    public class StockVarianceModel
    {
        public int MakeModelMasterId { get; set; }
        public int TotalStock { get; set; }
        public int SoldStock { get; set; }
        public int Balance { get; set; }
        public string? MakeName { get; set; }
        public string? ModelName { get; set; }
        public string? UomName { get; set; }
        public string? Price { get; set; }
        public decimal? UnitQuantity { get; set; }
    }
}
