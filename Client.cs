using System.Text;

namespace FakeYou 
{
    class Client
    {
        public Client()
        {
            tts = new TTS();
        }

        public TTS tts;
        public bool Login (string _username, string _password) {
            Configuration.username = _username;
            Configuration.password = _password;

            var payload = new StringContent("{\"username_or_email\": \"" + Configuration.username + "\", \"password\": \"" + Configuration.password + "\"}", Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, Configuration.url + "/login")
            {
                Content = payload
            };
            var response = Configuration.http.Send(request);
            var body = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            dynamic? bodyJSON = Newtonsoft.Json.JsonConvert.DeserializeObject(body);

            if (bodyJSON != null)
            {
                // Access and iterate through cookies in the response headers
                if (response.Headers.TryGetValues("Set-Cookie", out var cookieValues))
                {
                    foreach (var cookieValue in cookieValues)
                    {
                        string[] parts = cookieValue.Split(';'); // Split by semicolon
                        string[] nameValuePair = parts[0].Split('='); // Split by equal sign
                        string Key = nameValuePair[0];
                        string Value = nameValuePair[1];

                        if (Key == "session" && Value != null)
                            Configuration.session = Value;
                    }
                }
            }

            if (Configuration.session.Length != 0)
                return true;
            else
                return false;
        }
        
        public bool Logout() {
            if (Configuration.session.Length < 0)
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, Configuration.url + "/logout");
            request.Headers.Add("Credentials", "include");
            request.Headers.Add("Cookie", "session=" + Configuration.session);

            var response = Configuration.http.Send(request);
            var body = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            dynamic? bodyJSON = Newtonsoft.Json.JsonConvert.DeserializeObject(body);

            Configuration.session = "";
            return true;
        }
    }
}