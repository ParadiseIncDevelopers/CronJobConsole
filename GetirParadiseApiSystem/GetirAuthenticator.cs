using System.Text.Json;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace GetirParadiseApiSystem
{
    public class GetirAuthenticator
    {

        /// <summary>
        /// GetirApi'deki geliştirici gizli anahtarınız. Geliştirici firmaya Getir'den verilir.
        /// </summary>
        private static string? AppSecretKey 
        {
            get; set;
        }

        /// <summary>
        /// GetirApi'deki restoranın gizli anahtarı. Bu anahtar restorana ait olan anahtardır.
        /// </summary>
        private static string? RestaurantSecretKey 
        { 
            get; set; 
        }

        /// <summary>
        /// Elinizdeki bu tokenler ile kütüphaneyi kullanabilirsiniz.
        /// </summary>
        private static string? Token 
        { 
            get; set; 
        }

        private static HttpClient HttpClient 
        {
            get 
            {
                return new();
            } 
            set 
            { 

            }
        }

        /// <summary>
        /// Geliştirici'nin güvenli bir şekilde giriş yapmasını sağlar.
        /// </summary>
        /// <param name="appSecretKey">GetirApi'deki geliştirici gizli anahtarınız. Geliştirici firmaya Getir'den verilir.</param>
        /// <param name="restaurantSecretKey">GetirApi'deki restoranın gizli anahtarı. Bu anahtar restorana ait olan anahtardır.</param>
        public static void Authenticate(string appSecretKey, string restaurantSecretKey)
        {
            AppSecretKey = appSecretKey;
            RestaurantSecretKey = restaurantSecretKey;

            string authUri = "https://food-external-api-gateway.development.getirapi.com/auth/login";

            Dictionary<string, string?> elements = new()
            {
                { "appSecretKey", AppSecretKey },
                { "restaurantSecretKey", RestaurantSecretKey }
            };

            try
            {
                string? json = JsonSerializer.Serialize(elements);
                StringContent? payload = new(json, Encoding.UTF8, "application/json");
                string? token = HttpClient.PostAsync(authUri, payload).Result.Content.ReadAsStringAsync().Result;

                Dictionary<string, object>? tokenGetter = JsonSerializer.Deserialize<Dictionary<string, object>>(token);

                Token = tokenGetter?["token"].ToString();
            }
            catch (AggregateException)
            {
                string? json = JsonSerializer.Serialize(elements);
                StringContent? payload = new(json, Encoding.UTF8, "application/json");
                string? token = HttpClient.PostAsync(authUri, payload).Result.Content.ReadAsStringAsync().Result;

                Dictionary<string, object>? tokenGetter = JsonSerializer.Deserialize<Dictionary<string, object>>(token);

                Token = tokenGetter?["token"].ToString();
            }
        }

        public string? GetToken() 
        {
            static string? returnString()
            {
                return Token;
            }

            return returnString();
        }
    }

    public class GetirTokenNotFoundException : Exception
    {
        public override IDictionary Data => base.Data;

        public new string? HelpLink { get => base.HelpLink; set => base.HelpLink = value; }

        public override string Message => "Token'iniz bulunamadı. Lütfen tekrar deneyiniz.";

        public override string? Source 
        {
            get 
            {
                return "Token boş iken herhangi bir işlem yapılamaz. Bunun için Token lazım.";
            }
            set { } 
        }

        public override string? StackTrace => base.StackTrace;

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override Exception GetBaseException()
        {
            return new NullReferenceException("Getir Token is null. Please Try again.");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}