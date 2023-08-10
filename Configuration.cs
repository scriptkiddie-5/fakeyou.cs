namespace FakeYou
{
    public static class Configuration
    {
        // Reusable
        public static string url { get; } = "https://api.fakeyou.com";
        public static string GenerateUUIDV4Token()
        {
            // Create a new UUID v4
            System.Guid guid = System.Guid.NewGuid();

            // Convert the Guid to a string in the standard UUID format
            string uuid = guid.ToString("D"); // "D" format for standard 8-4-4-12 format

            return uuid;
        }


        // Auth
        public static string session { get; set; } = "";
        public static string username { get; set; } = "";
        public static string password { get; set; } = "";

        public static HttpClient http = new HttpClient();
    }
}