using System.Text.Json.Serialization;

namespace CronJobConsole
{
    [Serializable]
    public class Api
    {
        private string supp = "";

        [JsonPropertyName("SupplierId")]
        public string? SupplierId
        {
            get
            {
                if (AppSecretKey?.Length == 20)
                {
                    if (supp == null)
                    {
                        return "718787";
                    }
                    else
                    {
                        return supp;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                supp = value;
            }
        }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("ProductPostUrl")]
        public string? ProductPostUrl { get; set; }

        [JsonPropertyName("OrderPostUrl")]
        public string? OrderPostUrl { get; set; }

        [JsonPropertyName("AppSecretKey")]
        public string? AppSecretKey { get; set; }

        [JsonPropertyName("RestaurantSecretKey")]
        public string? RestaurantSecretKey { get; set; }

        [JsonPropertyName("Active")]
        public bool? Active { get; set; }

        [JsonPropertyName("Timeout")]
        public int? Timeout { get; set; }

        [JsonPropertyName("LastRequest")]
        public DateTime LastRequest { get; set; }

        [JsonPropertyName("BaseName")]
        public string? BaseName
        {
            get
            {
                return AppSecretKey?.Length == 20 ? "TRENDYOL" : "GETIR";
            }
        }
    }
}
