using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.Entity;
using RailLinkBackEnd.Models;
using RailLinkBackEnd.Supabase;
using System.Text.Json;

namespace RailLinkBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private static readonly string[] RoadModes = ["road", "road_only", "truck", "도로"];
        private static readonly string[] RailModes = ["rail", "multimodal", "철도", "복합"];

        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{receptNo}")]
        public async Task<IActionResult> GetHistoryDetail(
            [FromRoute] string receptNo,
            CancellationToken cancellationToken)
        {
            var history = await _context.Histories
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ReceptNo == receptNo,
                    cancellationToken);

            if (history is null)
            {
                return NotFound(new
                {
                    message = "해당 접수번호의 분석 기록을 찾을 수 없습니다.",
                    receptNo
                });
            }

            return Ok(new
            {
                history.Seq,
                history.ReceptNo,
                InputJson = JsonSerializer.Deserialize<JsonElement>(history.InputJson),
                OutputJson = JsonSerializer.Deserialize<JsonElement>(history.OutputJson),
                history.EntDateTime,
                history.recommendedMode,
                history.costChangeRate,
                history.carbonReductionRate,
                history.OriginName,
                history.DestinationName
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string? keyword = null,
            [FromQuery] int period = 30,
            [FromQuery] string transportMode = "all",
            [FromQuery] string costSaving = "all",
            [FromQuery] string sort = "latest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var validationResult = ValidateListRequest(
                period,
                transportMode,
                costSaving,
                sort,
                page,
                pageSize);

            if (validationResult is not null)
            {
                return validationResult;
            }

            var koreaNow = GetKoreaNow();
            var startDateTime = koreaNow.AddDays(-period);

            var query = _context.Histories
                .AsNoTracking()
                .Where(x => x.EntDateTime >= startDateTime);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escapedKeyword = EscapeLikePattern(keyword.Trim());
                var searchPattern = $"%{escapedKeyword}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.OriginName, searchPattern, "\\") ||
                    EF.Functions.ILike(x.DestinationName, searchPattern, "\\"));
            }

            query = transportMode switch
            {
                "rail" => query.Where(x => RailModes.Contains(x.recommendedMode.ToLower())),
                "road" => query.Where(x => RoadModes.Contains(x.recommendedMode.ToLower())),
                _ => query
            };

            query = costSaving switch
            {
                "savingOnly" => query.Where(x =>
                    RoadModes.Contains(x.recommendedMode.ToLower())
                        ? x.costChangeRate > 0
                        : -x.costChangeRate > 0),
                "min10" => query.Where(x =>
                    RoadModes.Contains(x.recommendedMode.ToLower())
                        ? x.costChangeRate >= 10
                        : -x.costChangeRate >= 10),
                "min20" => query.Where(x =>
                    RoadModes.Contains(x.recommendedMode.ToLower())
                        ? x.costChangeRate >= 20
                        : -x.costChangeRate >= 20),
                _ => query
            };

            var totalItems = await query.CountAsync(cancellationToken);

            query = sort switch
            {
                "costSaving" => query
                    .OrderByDescending(x => RoadModes.Contains(x.recommendedMode.ToLower())
                        ? x.costChangeRate
                        : -x.costChangeRate)
                    .ThenByDescending(x => x.EntDateTime)
                    .ThenByDescending(x => x.Seq),
                "carbonSaving" => query
                    .OrderByDescending(x => RoadModes.Contains(x.recommendedMode.ToLower())
                        ? -x.carbonReductionRate
                        : x.carbonReductionRate)
                    .ThenByDescending(x => x.EntDateTime)
                    .ThenByDescending(x => x.Seq),
                _ => query
                    .OrderByDescending(x => x.EntDateTime)
                    .ThenByDescending(x => x.Seq)
            };

            var historyEntities = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HistoryListSource
                {
                    ReceptNo = x.ReceptNo,
                    OutputJson = x.OutputJson,
                    EntDateTime = x.EntDateTime,
                    RecommendedMode = x.recommendedMode,
                    CostChangeRate = x.costChangeRate,
                    CarbonReductionRate = x.carbonReductionRate,
                    OriginName = x.OriginName,
                    DestinationName = x.DestinationName
                })
                .ToListAsync(cancellationToken);

            var items = historyEntities
                .Select(CreateListItem)
                .ToList();

            var totalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)pageSize);

            return Ok(new PagedResponse<HistoryListItemResponse>(
                items,
                new PaginationResponse(
                    page,
                    pageSize,
                    totalItems,
                    totalPages,
                    page > 1,
                    page < totalPages)));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetHistorySummary(
            CancellationToken cancellationToken)
        {
            var koreaNow = GetKoreaNow();
            var daysSinceMonday = ((int)koreaNow.DayOfWeek + 6) % 7;
            var weekStart = koreaNow.Date.AddDays(-daysSinceMonday);

            var weeklyQuery = _context.Histories
                .AsNoTracking()
                .Where(x => x.EntDateTime >= weekStart && x.EntDateTime <= koreaNow);

            var weeklyAnalysisCount = await weeklyQuery.CountAsync(cancellationToken);
            var railRecommendationCount = await weeklyQuery.CountAsync(
                x => RailModes.Contains(x.recommendedMode.ToLower()),
                cancellationToken);

            var averageCostSavingRate = await weeklyQuery
                .Select(x => (double?)(RoadModes.Contains(x.recommendedMode.ToLower())
                    ? x.costChangeRate
                    : -x.costChangeRate))
                .AverageAsync(cancellationToken) ?? 0;

            var averageCarbonSavingRate = await weeklyQuery
                .Select(x => (double?)(RoadModes.Contains(x.recommendedMode.ToLower())
                    ? -x.carbonReductionRate
                    : x.carbonReductionRate))
                .AverageAsync(cancellationToken) ?? 0;

            var latestHistory = await _context.Histories
                .AsNoTracking()
                .OrderByDescending(x => x.EntDateTime)
                .ThenByDescending(x => x.Seq)
                .Select(x => new
                {
                    x.ReceptNo,
                    x.OriginName,
                    x.DestinationName,
                    x.EntDateTime,
                    x.OutputJson
                })
                .FirstOrDefaultAsync(cancellationToken);

            LatestAnalysisResponse? latestAnalysis = null;

            if (latestHistory is not null)
            {
                latestAnalysis = new LatestAnalysisResponse(
                    latestHistory.ReceptNo,
                    latestHistory.OriginName,
                    latestHistory.DestinationName,
                    GetCargoWeightTon(latestHistory.OutputJson),
                    latestHistory.EntDateTime);
            }

            var railRecommendationRate = weeklyAnalysisCount == 0
                ? 0
                : railRecommendationCount * 100d / weeklyAnalysisCount;

            return Ok(new HistorySummaryResponse(
                weeklyAnalysisCount,
                railRecommendationCount,
                railRecommendationRate,
                averageCostSavingRate,
                averageCarbonSavingRate,
                latestAnalysis));
        }

        private BadRequestObjectResult? ValidateListRequest(
            int period,
            string transportMode,
            string costSaving,
            string sort,
            int page,
            int pageSize)
        {
            if (period is not (7 or 30 or 90))
            {
                return BadRequest(new { message = "period는 7, 30, 90 중 하나여야 합니다." });
            }

            if (transportMode is not ("all" or "rail" or "road"))
            {
                return BadRequest(new { message = "transportMode는 all, rail, road 중 하나여야 합니다." });
            }

            if (costSaving is not ("all" or "savingOnly" or "min10" or "min20"))
            {
                return BadRequest(new { message = "costSaving은 all, savingOnly, min10, min20 중 하나여야 합니다." });
            }

            if (sort is not ("latest" or "costSaving" or "carbonSaving"))
            {
                return BadRequest(new { message = "sort는 latest, costSaving, carbonSaving 중 하나여야 합니다." });
            }

            if (page < 1)
            {
                return BadRequest(new { message = "page는 1 이상이어야 합니다." });
            }

            if (pageSize is < 1 or > 100)
            {
                return BadRequest(new { message = "pageSize는 1 이상 100 이하여야 합니다." });
            }

            return null;
        }

        private static HistoryListItemResponse CreateListItem(HistoryListSource history)
        {
            var isRoadRecommendation = IsRoadMode(history.RecommendedMode);
            var cargoWeightTon = 0d;
            var railRatio = 0d;
            var costDifferenceWon = 0L;
            var carbonReductionKg = 0d;
            var firstMileDistance = 0d;
            var lastMileDistance = 0d;

            try
            {
                using var document = JsonDocument.Parse(history.OutputJson);
                var root = document.RootElement;

                cargoWeightTon = GetDouble(root, "cargo_weight_ton");

                if (root.TryGetProperty("distance", out var distance))
                {
                    railRatio = GetDouble(distance, "rail_ratio");
                }

                if (root.TryGetProperty("cost", out var cost))
                {
                    costDifferenceWon = GetInt64(cost, "cost_difference_won");
                }

                if (root.TryGetProperty("carbon", out var carbon))
                {
                    carbonReductionKg = GetDouble(carbon, "carbon_reduction_kg");
                }

                if (root.TryGetProperty("first_mile", out var firstMile))
                {
                    firstMileDistance = GetDouble(firstMile, "distance_km");
                }

                if (root.TryGetProperty("last_mile", out var lastMile))
                {
                    lastMileDistance = GetDouble(lastMile, "distance_km");
                }
            }
            catch (JsonException)
            {
                // 과거 데이터의 JSON이 손상되어도 목록 전체 조회는 계속한다.
            }

            var transportLegs = CreateTransportLegs(
                isRoadRecommendation,
                firstMileDistance,
                lastMileDistance);

            return new HistoryListItemResponse(
                history.ReceptNo,
                history.OriginName,
                history.DestinationName,
                cargoWeightTon,
                history.RecommendedMode,
                transportLegs,
                isRoadRecommendation ? 0 : railRatio,
                isRoadRecommendation ? history.CostChangeRate : -history.CostChangeRate,
                isRoadRecommendation ? costDifferenceWon : -costDifferenceWon,
                isRoadRecommendation ? -history.CarbonReductionRate : history.CarbonReductionRate,
                isRoadRecommendation ? -carbonReductionKg : carbonReductionKg,
                history.EntDateTime);
        }

        private static IReadOnlyList<string> CreateTransportLegs(
            bool isRoadRecommendation,
            double firstMileDistance,
            double lastMileDistance)
        {
            if (isRoadRecommendation)
            {
                return ["road"];
            }

            var legs = new List<string>();

            if (firstMileDistance > 0)
            {
                legs.Add("road");
            }

            legs.Add("rail");

            if (lastMileDistance > 0)
            {
                legs.Add("road");
            }

            return legs;
        }

        private static double GetCargoWeightTon(string outputJson)
        {
            try
            {
                using var document = JsonDocument.Parse(outputJson);
                return GetDouble(document.RootElement, "cargo_weight_ton");
            }
            catch (JsonException)
            {
                return 0;
            }
        }

        private static double GetDouble(JsonElement parent, string propertyName)
        {
            return parent.TryGetProperty(propertyName, out var value) &&
                   value.ValueKind == JsonValueKind.Number &&
                   value.TryGetDouble(out var result)
                ? result
                : 0;
        }

        private static long GetInt64(JsonElement parent, string propertyName)
        {
            return parent.TryGetProperty(propertyName, out var value) &&
                   value.ValueKind == JsonValueKind.Number &&
                   value.TryGetInt64(out var result)
                ? result
                : 0;
        }

        private static bool IsRoadMode(string recommendedMode)
        {
            return RoadModes.Contains(recommendedMode.Trim().ToLowerInvariant());
        }

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        private static DateTime GetKoreaNow()
        {
            var koreaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, koreaTimeZone);
        }

        private sealed class HistoryListSource
        {
            public string ReceptNo { get; init; } = string.Empty;
            public string OutputJson { get; init; } = string.Empty;
            public DateTime EntDateTime { get; init; }
            public string RecommendedMode { get; init; } = string.Empty;
            public double CostChangeRate { get; init; }
            public double CarbonReductionRate { get; init; }
            public string OriginName { get; init; } = string.Empty;
            public string DestinationName { get; init; } = string.Empty;
        }
    }
}
