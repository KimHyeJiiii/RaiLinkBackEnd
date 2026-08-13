using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.Supabase;
using System.Text.Json;

namespace RailLinkBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
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
                    message = "해당 견적번호의 견적서를 찾을 수 없습니다.",
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
            [FromQuery] string? keyword,
            [FromQuery] int period,
            [FromQuery] string transportMode,
            [FromQuery] string costSaving,
            [FromQuery] string sort)
        {
            if (period is not (7 or 30 or 90))
            {
                return BadRequest(new
                {
                    message = "period는 7, 30, 90 중 하나여야 합니다."
                });
            }

            if (transportMode is not ("all" or "rail" or "road"))
            {
                return BadRequest(new
                {
                    message = "transportMode는 all, rail, road 중 하나여야 합니다."
                });
            }

            if (costSaving is not ("all" or "savingOnly" or "min10" or "min20"))
            {
                return BadRequest(new
                {
                    message = "costSaving은 all, savingOnly, min10, min20 중 하나여야 합니다."
                });
            }

            if (sort is not ("latest" or "costSaving" or "carbonSaving"))
            {
                return BadRequest(new
                {
                    message = "sort는 latest, costSaving, carbonSaving 중 하나여야 합니다."
                });
            }

            var koreaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
            var koreaNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, koreaTimeZone);
            var startDateTime = koreaNow.AddDays(-period);

            var query = _context.Histories
                .AsNoTracking()
                .Where(x => x.EntDateTime >= startDateTime);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var searchPattern = $"%{keyword.Trim()}%";

                query = query.Where(x =>
                    EF.Functions.ILike(x.OriginName, searchPattern) ||
                    EF.Functions.ILike(x.DestinationName, searchPattern));
            }

            query = transportMode switch
            {
                "rail" => query.Where(x =>
                    EF.Functions.ILike(x.recommendedMode, "rail") ||
                    EF.Functions.ILike(x.recommendedMode, "multimodal") ||
                    EF.Functions.ILike(x.recommendedMode, "철도") ||
                    EF.Functions.ILike(x.recommendedMode, "복합")),
                "road" => query.Where(x =>
                    EF.Functions.ILike(x.recommendedMode, "road") ||
                    EF.Functions.ILike(x.recommendedMode, "road_only") ||
                    EF.Functions.ILike(x.recommendedMode, "truck") ||
                    EF.Functions.ILike(x.recommendedMode, "도로")),
                _ => query
            };

            query = costSaving switch
            {
                "savingOnly" => query.Where(x => x.costChangeRate > 0),
                "min10" => query.Where(x => x.costChangeRate >= 10),
                "min20" => query.Where(x => x.costChangeRate >= 20),
                _ => query
            };

            query = sort switch
            {
                "costSaving" => query
                    .OrderByDescending(x => x.costChangeRate)
                    .ThenByDescending(x => x.EntDateTime),
                "carbonSaving" => query
                    .OrderByDescending(x => x.carbonReductionRate)
                    .ThenByDescending(x => x.EntDateTime),
                _ => query
                    .OrderByDescending(x => x.EntDateTime)
                    .ThenByDescending(x => x.Seq)
            };

            var historyEntities = await query.ToListAsync();

            var histories = historyEntities.Select(x => new
            {
                x.Seq,
                x.ReceptNo,
                InputJson = JsonSerializer.Deserialize<JsonElement>(x.InputJson),
                OutputJson = JsonSerializer.Deserialize<JsonElement>(x.OutputJson),
                x.EntDateTime,
                x.recommendedMode,
                x.costChangeRate,
                x.carbonReductionRate,
                x.OriginName,
                x.DestinationName
            });

            return Ok(histories);
        }
    }
}
