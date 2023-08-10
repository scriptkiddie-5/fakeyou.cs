using System.Text;

namespace FakeYou
{
    class TTS
    {
        public TTSModel? GetModelByToken(string token)
        {
            List<TTSModel> models = GetModels();
            foreach (var model in models)
            {
                if (model.modelToken == token)
                    return model;
            }

            return null;
        }
        public TTSModel? GetModelByTitle(string title)
        {
            List<TTSModel> models = GetModels();
            foreach (var model in models)
            {
                if (model.title == title)
                    return model;
            }

            return null;
        }
        public List<TTSModel> GetModels()
        {
            List<TTSModel> models = new List<TTSModel>();

            var request = new HttpRequestMessage(HttpMethod.Get, Configuration.url + "/tts/list");
            var response = Configuration.http.Send(request);
            var body = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            dynamic? bodyJSON = Newtonsoft.Json.JsonConvert.DeserializeObject(body);

            if (bodyJSON != null)
            {
                var modelsJSON = bodyJSON.models;
                if (modelsJSON != null)
                {
                    foreach (dynamic modelJSON in modelsJSON)
                    {
                        TTSModel model = new TTSModel();

                        model.modelToken = modelJSON.model_token;
                        model.ttsModelType = modelJSON.tts_model_type;
                        model.creatorUserToken = modelJSON.creator_user_token;
                        model.creatorUsername = modelJSON.creator_username;
                        model.creatorDisplayName = modelJSON.creator_display_name;
                        model.creatorGravatarHash = modelJSON.creator_gravatar_hash;
                        model.title = modelJSON.title;
                        model.ietfLanguageTag = modelJSON.ietf_language_tag;
                        model.ietfPrimaryLanguageSubtag = modelJSON.ietf_primary_language_subtag;
                        model.isFrontPageFeatured = modelJSON.is_front_page_featured;
                        model.isTwitchFeatured = modelJSON.is_twitch_featured;
                        model.maybeSuggestedUniqueBotCommand = modelJSON.maybe_suggested_unique_bot_command;
                        model.creatorSetVisibility = modelJSON.creator_set_visibility;
                        model.userRatings = new TTSModel.UserRatings
                        {
                            positiveCount = modelJSON.user_ratings.positive_count,
                            negativeCount = modelJSON.user_ratings.negative_count,
                            totalCount = modelJSON.user_ratings.total_count
                        };
                        model.categoryTokens = modelJSON.category_tokens.ToObject<List<string>>();
                        model.createdAt = modelJSON.created_at;
                        model.updatedAt = modelJSON.updated_at;

                        models.Add(model);
                    }
                }  

            }

            return models;
        }
    }

    class TTSResult
    {
        public string? url { get; set; }
    }

    class TTSModel 
    {
        public TTSResult? Inference(string text, int maxAttempts = 60 /* number of attempts, one is made every second */)
        {
            var inferencePayload = new StringContent("{\"tts_model_token\": \"" + modelToken + "\", \"uuid_idempotency_token\": \"" + Configuration.GenerateUUIDV4Token() + "\", \"inference_text\": \"" + text + "\"}", Encoding.UTF8, "application/json");
            var inferenceRequest = new HttpRequestMessage(HttpMethod.Post, Configuration.url + "/tts/inference");
            inferenceRequest.Content = inferencePayload;
            if (Configuration.session.Length > 0)
            {
                inferenceRequest.Headers.Add("Credentials", "include");
                inferenceRequest.Headers.Add("Cookie", "session=" + Configuration.session);
            }

            var inferenceResponse = Configuration.http.Send(inferenceRequest);
            var inferenceBody = new StreamReader(inferenceResponse.Content.ReadAsStream()).ReadToEnd();
            dynamic? inferenceBodyJSON = Newtonsoft.Json.JsonConvert.DeserializeObject(inferenceBody);

            if (inferenceBodyJSON != null)
            {
                if ((bool)inferenceBodyJSON.success.Value)
                {
                    int tries = 0;
                    while(true && tries <= maxAttempts)
                    {
                        tries++;
                        Thread.Sleep(1000);

                        var pollRequest = new HttpRequestMessage(HttpMethod.Get, Configuration.url + "/tts/job/" + inferenceBodyJSON.inference_job_token);
                        if (Configuration.session.Length > 0)
                        {
                            inferenceRequest.Headers.Add("Credentials", "include");
                            inferenceRequest.Headers.Add("Cookie", "session=" + Configuration.session);
                        }

                        var pollResponse = Configuration.http.Send(pollRequest);
                        var pollBody = new StreamReader(pollResponse.Content.ReadAsStream()).ReadToEnd();
                        dynamic? pollBodyJSON = Newtonsoft.Json.JsonConvert.DeserializeObject(pollBody);

                        if (pollBodyJSON != null) 
                        {
                            if (pollBodyJSON.state.status == "complete_success")
                            {
                                TTSResult result = new TTSResult();
                                result.url = "https://storage.googleapis.com/vocodes-public" + pollBodyJSON.state.maybe_public_bucket_wav_audio_path;
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public string? modelToken { get; set; }
        public string? ttsModelType { get; set; }
        public string? creatorUserToken { get; set; }
        public string? creatorUsername { get; set; }
        public string? creatorDisplayName { get; set; }
        public string? creatorGravatarHash { get; set; }
        public string? title { get; set; }
        public string? ietfLanguageTag { get; set; }
        public string? ietfPrimaryLanguageSubtag { get; set; }
        public bool isFrontPageFeatured { get; set; }
        public bool isTwitchFeatured { get; set; }
        public string? maybeSuggestedUniqueBotCommand { get; set; }
        public string? creatorSetVisibility { get; set; }
        public UserRatings? userRatings { get; set; }
        public List<string>? categoryTokens { get; set; }
        public string? createdAt { get; set; }
        public string? updatedAt { get; set; }

        public class UserRatings
        {
            public int positiveCount { get; set; }
            public int negativeCount { get; set; }
            public int totalCount { get; set; }
        }
    }
}