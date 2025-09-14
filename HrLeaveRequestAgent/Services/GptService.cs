using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HrLeaveRequestAgent.Services
{
    public class GptService
    {
        private readonly HttpClient _httpClient;
        private readonly string _authToken;

        public GptService(HttpClient httpClient, string authToken)
        {
            _httpClient = httpClient;
            _authToken = authToken;
        }

        public class Message
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        public class AIChatResponse
        {
            [JsonProperty("choices")]
            public Choice[] Choices { get; set; }
        }

        public class Choice
        {
            [JsonProperty("message")]
            public Message Message { get; set; }
        }

        public async Task<string> GptResponseWithContextAsync(string userPrompt, int casualLeavesAvailable)
        {
            string systemInstruction = $"You are an HR leave assistant. The employee has {casualLeavesAvailable} casual leaves available. " +
                                       $"Help them check leave balances, apply for leaves, and handle leave workflow naturally.";

            var messages = new[]
            {
                new Message { Role = "system", Content = systemInstruction },
                new Message { Role = "user", Content = userPrompt }
            };

            var requestData = new
            {
                model = "sonar",
                messages = messages
            };

            var jsonContent = JsonConvert.SerializeObject(requestData);

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.perplexity.ai/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var parsedResponse = JsonConvert.DeserializeObject<AIChatResponse>(responseString);

            return parsedResponse?.Choices?[0]?.Message?.Content ?? "No response from AI.";
        }
    }
}
