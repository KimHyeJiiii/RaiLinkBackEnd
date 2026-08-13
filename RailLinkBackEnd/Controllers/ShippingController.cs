using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.Entity;
using RailLinkBackEnd.RailLogisticsApi;
using RailLinkBackEnd.Supabase;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace RailLinkBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController : ControllerBase
    {
        private readonly RailLogisticsApiService _railApi;
        private readonly AppDbContext _context;

        public ShippingController(RailLogisticsApiService railApi, AppDbContext context)
        {
            _railApi = railApi;
            _context = context;
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend(
        [FromBody] JsonElement body)
        {
            try
            {
                // 1. 프론트에서 들어온 JSON 원문
                string inputJson = body.GetRawText();

                // 2. JSON → TransportRequest
                var request = body.Deserialize<TransportRequest>();

                if (request is null)
                    return BadRequest("잘못된 요청입니다.");

                // 3. Rail AI API 호출
                var result = await _railApi.AnalyzeTransportAsync(request);

                // 4. Rail AI API 성공
                //    → 결과 JSON을 문자열로 변환
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };
                string outputJson = JsonSerializer.Serialize(result, options);

                // 5. DB 트랜잭션 시작
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 접수번호 생성
                    string receptNo = await GenerateReceptNoAsync();

                    var koreaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");

                    var koreaNow = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        koreaTimeZone
                    );

                    // history 데이터 생성
                    var history = new History
                    {
                        InputJson = inputJson,
                        ReceptNo = receptNo,
                        OutputJson = outputJson,
                        recommendedMode = result.Recommendation.RecommendedMode,
                        costChangeRate = result.Cost.CostChangeRate,
                        carbonReductionRate = result.Carbon.CarbonReductionRate,
                        OriginName = result.Origin.Name,
                        DestinationName = result.Destination.Name,
                        EntDateTime = koreaNow
                    };
                    _context.Histories.Add(history);

                    // INSERT
                    await _context.SaveChangesAsync();

                    // COMMIT
                    await transaction.CommitAsync();

                    Console.WriteLine($"[Shipping] 저장 완료: {receptNo}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        message = "운송 분석 결과 저장에 실패했습니다.",
                        error = ex.Message,
                        innerError = ex.InnerException?.Message
                    });
                }

                // 5. 프론트에 결과 반환
                return Ok(result);
            }
            catch (RailApiException ex)
            {
                // Rail AI API가 4xx/5xx로 응답한 경우 → 원문 그대로 프론트에 전달
                object errorDetail;
                try
                {
                    errorDetail = JsonSerializer.Deserialize<object>(ex.ResponseBody)!;
                }
                catch
                {
                    errorDetail = ex.ResponseBody;
                }

                Console.WriteLine(
                    $"[ShippingController] Rail API 오류: {ex.StatusCode} / {ex.ResponseBody}");

                return StatusCode((int)ex.StatusCode, new
                {
                    message = "AI 분석 서버에서 오류를 반환했습니다.",
                    detail = errorDetail
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ShippingController ERROR] {ex}");

                return StatusCode(500, new
                {
                    message = "운송 분석 처리 중 오류가 발생했습니다.",
                    error = ex.Message
                });
            }
        }

        private async Task<string> GenerateReceptNoAsync()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string prefix = $"KR-{date}-";

            // 오늘 날짜의 마지막 접수번호 조회
            var lastReceptNo = await _context.Histories
                .Where(x => x.ReceptNo != null &&
                            x.ReceptNo.StartsWith(prefix))
                .OrderByDescending(x => x.ReceptNo)
                .Select(x => x.ReceptNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastReceptNo))
            {
                string numberPart = lastReceptNo.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }

            return $"{prefix}{nextNumber:D4}";
        }
    }
}