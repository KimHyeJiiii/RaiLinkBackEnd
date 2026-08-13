using RailLinkBackEnd.Entity;
using System.Text.Json;

namespace RailLinkBackEnd.RailLogisticsApi
{
    public class RailLogisticsApiService
    {
        private readonly HttpClient _httpClient;

        public RailLogisticsApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri(
                "https://rail-ai-logistics-api-12709338150.asia-northeast3.run.app/");
        }

        public async Task<TransportAnalysisResult> AnalyzeTransportAsync(
        TransportRequest request)
        {
            // 외부 Rail Logistics API 호출
            var response = await _httpClient.PostAsJsonAsync(
                "transport-analysis",
                request);

            // 응답 내용을 먼저 문자열로 가져옴
            var rawBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"[RailLogisticsApiService] Status: {response.StatusCode}");

            Console.WriteLine(
                $"[RailLogisticsApiService] Response: {rawBody}");

            // 200번대가 아니면 예외 발생
            response.EnsureSuccessStatusCode();

            // 응답이 비어있는 경우
            if (string.IsNullOrWhiteSpace(rawBody))
            {
                throw new InvalidOperationException(
                    "Rail Logistics API에서 빈 응답을 반환했습니다.");
            }

            // 외부 API 응답 JSON을 바로 TransportAnalysisResult로 변환
            var result = JsonSerializer.Deserialize<TransportAnalysisResult>(
                rawBody);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Rail Logistics API 응답을 TransportAnalysisResult로 변환하지 못했습니다.");
            }

            return result;
        }
    }
}
