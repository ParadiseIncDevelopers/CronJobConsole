using CronJobConsole;
using System.Text;
using System.Text.Json;

namespace ApiDeneme
{
    public class Program
    {
        private static void WriteLastSeen(int index)
        {
            if (!System.IO.File.Exists(Path))
            {
                return;
            }

            FileStream? dbFile = null;
            List<Api>? lines;
            try
            {
                dbFile = System.IO.File.OpenRead(Path);
                lines = JsonSerializer.Deserialize<List<Api>>(dbFile);

                if (lines?.Count != 0 || lines != null)
                {
                    JsonSerializerOptions options = new()
                    {
                        WriteIndented = true
                    };
                    dbFile.Close();
                    lines[index].LastRequest = DateTime.Now;
                    string allLines = JsonSerializer.Serialize(lines, options);
                    System.IO.File.WriteAllLines(Path, new List<string> { allLines });
                }
                else
                {
                    dbFile.Close();
                }
            }
            catch (Exception)
            {
                dbFile?.Close();
            }
        }
        private static List<Api> ReadFile()
        {
            if (!System.IO.File.Exists(Path))
            {
                return null;
            }

            FileStream? dbFile = null;
            List<Api>? lines;
            try
            {
                dbFile = System.IO.File.OpenRead(Path);
                lines = JsonSerializer.Deserialize<List<Api>>(dbFile);

                if (lines?.Count != 0 || lines != null)
                {
                    dbFile.Close();
                    return lines;
                }
                else
                {
                    dbFile.Close();
                    return new List<Api>();
                }
            }
            catch (Exception)
            {
                dbFile?.Close();
                return null;
            }
        }

        private static readonly List<Api> Lines = ReadFile();
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
        private static string Path
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + "CronJob" + "\\" + "db.json";
            }
        }

        private static void Main(string[] args)
        {
            List<Thread> tasks = new();

            for (int i = 0; i < Lines.Count; i++)
            {
                tasks.Add(new Thread(Get));
            }
            while (true)
            {
                for (int i = 0; i < Lines.Count; i++)
                {
                    if (!tasks[i].IsAlive && ((bool)Lines[i].Active))
                    {
                        tasks[i] = new Thread(Get);
                        tasks[i].Start(i);
                    }
                }
            }
        }

        public static void Get(object index)
        {
            int theIndex = Convert.ToInt32(index);

            try
            {
                Console.WriteLine("{0} => {1}:{2}:{3}", Lines[theIndex].Description, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                var tok = TokenGetter(theIndex, Lines[theIndex].AppSecretKey, Lines[theIndex].RestaurantSecretKey);
                ApiRequester(theIndex, Lines[theIndex].OrderPostUrl, tok);
                WriteLastSeen(theIndex);
                var periodTimeSpan = TimeSpan.FromSeconds(Convert.ToDouble(Lines[theIndex].Timeout - 1));
                Thread.Sleep(periodTimeSpan);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string TokenGetter(int index, string appSecretKey, string restaurantSecretKey)
        {
            string authUri = "";

            if (Lines[index].BaseName == "GETIR")
            {
                authUri = "https://food-external-api-gateway.development.getirapi.com/auth/login";

                Dictionary<string, object> elements = new()
                {
                    { "appSecretKey", appSecretKey },
                    { "restaurantSecretKey", restaurantSecretKey }
                };
                try
                {
                    string? json = JsonSerializer.Serialize(elements);
                    StringContent? payload = new(json, Encoding.UTF8, "application/json");
                    string? token = HttpClient.PostAsync(authUri, payload).Result.Content.ReadAsStringAsync().Result;

                    Dictionary<string, object>? tokenGetter = JsonSerializer.Deserialize<Dictionary<string, object>>(token);

                    return tokenGetter["token"].ToString();
                }
                catch (AggregateException)
                {
                    string? json = JsonSerializer.Serialize(elements);
                    StringContent? payload = new(json, Encoding.UTF8, "application/json");
                    string? token = HttpClient.PostAsync(authUri, payload).Result.Content.ReadAsStringAsync().Result;

                    Dictionary<string, object>? tokenGetter = JsonSerializer.Deserialize<Dictionary<string, object>>(token);

                    return tokenGetter["token"].ToString();
                }
            }
            else if (Lines[index].BaseName == "TRENDYOL")
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(restaurantSecretKey + ":" + appSecretKey));
            }
            else
            {
                return authUri;
            }
        }

        private static void ApiRequester(int index, string postApi, string token)
        {
            string requestString(string baseName)
            {
                return baseName switch
                {
                    "GETIR" => "https://food-external-api-gateway.development.getirapi.com/food-orders/active",
                    "TRENDYOL" => "https://api.trendyol.com/mealgw/suppliers/" + Lines[index].SupplierId + "/packages",
                    _ => "",
                };
            }

            string reqString = requestString(Lines[index].BaseName);

            if (Lines[index].BaseName == "GETIR")
            {
                string requestMethod = "POST";

                HttpRequestMessage? request = new(new HttpMethod(requestMethod), reqString);
                request.Headers.TryAddWithoutValidation("accept", "application/json");
                request.Headers.TryAddWithoutValidation("token", token);

                string? response = HttpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

                if (Lines?.Count == 0 || Lines == null)
                {
                    return;
                }

                StringContent? payload = new(response, Encoding.UTF8, "application/x-www-form-urlencoded");
                string? newToken = HttpClient.PostAsync(postApi, payload).Result.Content.ReadAsStringAsync().Result;
            }
            else if (Lines[index].BaseName == "TRENDYOL")
            {
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage(new HttpMethod("GET"), reqString);
                request.Headers.TryAddWithoutValidation("authorization", "Basic " + token);
                request.Headers.TryAddWithoutValidation("cache-control", "no-cache");
                request.Headers.TryAddWithoutValidation("x-agentname", "test");
                request.Headers.TryAddWithoutValidation("x-executor-user", "test");

                var response = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

                StringContent? payload = new(response, Encoding.UTF8, "application/x-www-form-urlencoded");
                string? newToken = HttpClient.PostAsync(postApi, payload).Result.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return;
            }
        }
    }
}