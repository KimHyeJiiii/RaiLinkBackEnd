using System.Text.Json.Serialization;

namespace RailLinkBackEnd.Entity
{
    public class TransportAnalysisResult
    {
        [JsonPropertyName("origin")]
        public LocationInfo Origin { get; set; } = new();

        [JsonPropertyName("destination")]
        public LocationInfo Destination { get; set; } = new();

        [JsonPropertyName("departure_station")]
        public string DepartureStation { get; set; } = string.Empty;

        [JsonPropertyName("arrival_station")]
        public string ArrivalStation { get; set; } = string.Empty;

        [JsonPropertyName("cargo_weight_ton")]
        public double CargoWeightTon { get; set; }

        [JsonPropertyName("shipping_date")]
        public string ShippingDate { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = string.Empty;

        [JsonPropertyName("weights")]
        public WeightInfo Weights { get; set; } = new();

        [JsonPropertyName("road_only")]
        public RoadOnlyInfo RoadOnly { get; set; } = new();

        [JsonPropertyName("first_mile")]
        public FirstMileInfo FirstMile { get; set; } = new();

        [JsonPropertyName("rail")]
        public RailInfo Rail { get; set; } = new();

        [JsonPropertyName("last_mile")]
        public LastMileInfo LastMile { get; set; } = new();

        [JsonPropertyName("schedule")]
        public ScheduleInfo Schedule { get; set; } = new();

        [JsonPropertyName("cost")]
        public CostInfo Cost { get; set; } = new();

        [JsonPropertyName("distance")]
        public DistanceInfo Distance { get; set; } = new();

        [JsonPropertyName("time")]
        public TimeInfo Time { get; set; } = new();

        [JsonPropertyName("carbon")]
        public CarbonInfo Carbon { get; set; } = new();

        [JsonPropertyName("recommendation")]
        public RecommendationInfo Recommendation { get; set; } = new();

        [JsonPropertyName("ai_explanation")]
        public string AiExplanation { get; set; } = string.Empty;

        [JsonPropertyName("ai_explanation_error")]
        public string? AiExplanationError { get; set; }
    }

    // 출발지 / 도착지
    public class LocationInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
    }

    // 사용자 가중치
    public class WeightInfo
    {
        [JsonPropertyName("cost")]
        public double Cost { get; set; }

        [JsonPropertyName("time")]
        public double Time { get; set; }

        [JsonPropertyName("carbon")]
        public double Carbon { get; set; }
    }

    // 도로 단독 운송
    public class RoadOnlyInfo
    {
        [JsonPropertyName("distance_km")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("duration_min")]
        public double DurationMin { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }
    }

    // First Mile
    public class FirstMileInfo
    {
        [JsonPropertyName("distance_km")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("duration_min")]
        public double DurationMin { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }
    }

    // 철도 운송
    public class RailInfo
    {
        [JsonPropertyName("origin_station")]
        public string OriginStation { get; set; } = string.Empty;

        [JsonPropertyName("destination_station")]
        public string DestinationStation { get; set; } = string.Empty;

        [JsonPropertyName("distance_km")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("duration_min")]
        public double DurationMin { get; set; }

        [JsonPropertyName("fare_per_ton_won")]
        public double FarePerTonWon { get; set; }

        [JsonPropertyName("operation_days")]
        public string OperationDays { get; set; } = string.Empty;

        [JsonPropertyName("train_numbers")]
        public string TrainNumbers { get; set; } = string.Empty;

        [JsonPropertyName("main_lines")]
        public string MainLines { get; set; } = string.Empty;

        [JsonPropertyName("cargo_types")]
        public string CargoTypes { get; set; } = string.Empty;

        [JsonPropertyName("performance_record_count")]
        public int PerformanceRecordCount { get; set; }
    }

    // Last Mile
    public class LastMileInfo
    {
        [JsonPropertyName("distance_km")]
        public double DistanceKm { get; set; }

        [JsonPropertyName("duration_min")]
        public double DurationMin { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }
    }

    // 운행 일정
    public class ScheduleInfo
    {
        [JsonPropertyName("shipping_date")]
        public string ShippingDate { get; set; } = string.Empty;

        [JsonPropertyName("shipping_weekday")]
        public string ShippingWeekday { get; set; } = string.Empty;

        [JsonPropertyName("operation_days")]
        public List<string> OperationDays { get; set; } = new();

        [JsonPropertyName("available_today")]
        public bool AvailableToday { get; set; }

        [JsonPropertyName("next_available_date")]
        public string NextAvailableDate { get; set; } = string.Empty;

        [JsonPropertyName("next_available_weekday")]
        public string NextAvailableWeekday { get; set; } = string.Empty;

        [JsonPropertyName("waiting_days")]
        public int WaitingDays { get; set; }
    }

    // 비용 정보
    public class CostInfo
    {
        [JsonPropertyName("road_only")]
        public RoadCostInfo RoadOnly { get; set; } = new();

        [JsonPropertyName("first_mile")]
        public FirstMileCostInfo FirstMile { get; set; } = new();

        [JsonPropertyName("rail")]
        public RailCostInfo Rail { get; set; } = new();

        [JsonPropertyName("last_mile")]
        public LastMileCostInfo LastMile { get; set; } = new();

        [JsonPropertyName("multimodal_total_cost_won")]
        public long MultimodalTotalCostWon { get; set; }

        [JsonPropertyName("cost_difference_won")]
        public long CostDifferenceWon { get; set; }

        [JsonPropertyName("cost_change_rate")]
        public double CostChangeRate { get; set; }
    }

    public class RoadCostInfo
    {
        [JsonPropertyName("estimated_freight_fare_won")]
        public long EstimatedFreightFareWon { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }

        [JsonPropertyName("total_cost_won")]
        public long TotalCostWon { get; set; }

        [JsonPropertyName("weight_class")]
        public string WeightClass { get; set; } = string.Empty;
    }

    public class FirstMileCostInfo
    {
        [JsonPropertyName("estimated_freight_fare_won")]
        public long EstimatedFreightFareWon { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }

        [JsonPropertyName("total_cost_won")]
        public long TotalCostWon { get; set; }
    }

    public class RailCostInfo
    {
        [JsonPropertyName("fare_per_ton_won")]
        public double FarePerTonWon { get; set; }

        [JsonPropertyName("total_cost_won")]
        public long TotalCostWon { get; set; }
    }

    public class LastMileCostInfo
    {
        [JsonPropertyName("estimated_freight_fare_won")]
        public long EstimatedFreightFareWon { get; set; }

        [JsonPropertyName("toll_won")]
        public long TollWon { get; set; }

        [JsonPropertyName("total_cost_won")]
        public long TotalCostWon { get; set; }
    }

    // 거리 정보
    public class DistanceInfo
    {
        [JsonPropertyName("road_only_distance_km")]
        public double RoadOnlyDistanceKm { get; set; }

        [JsonPropertyName("multimodal_total_distance_km")]
        public double MultimodalTotalDistanceKm { get; set; }

        [JsonPropertyName("multimodal_road_distance_km")]
        public double MultimodalRoadDistanceKm { get; set; }

        [JsonPropertyName("rail_distance_km")]
        public double RailDistanceKm { get; set; }

        [JsonPropertyName("road_ratio")]
        public double RoadRatio { get; set; }

        [JsonPropertyName("rail_ratio")]
        public double RailRatio { get; set; }
    }

    // 시간 정보
    public class TimeInfo
    {
        [JsonPropertyName("road_only_time_min")]
        public double RoadOnlyTimeMin { get; set; }

        [JsonPropertyName("multimodal_total_time_min")]
        public double MultimodalTotalTimeMin { get; set; }

        [JsonPropertyName("time_difference_min")]
        public double TimeDifferenceMin { get; set; }

        [JsonPropertyName("time_change_rate")]
        public double TimeChangeRate { get; set; }
    }

    // 탄소 정보
    public class CarbonInfo
    {
        [JsonPropertyName("road_only_emission_kg")]
        public double RoadOnlyEmissionKg { get; set; }

        [JsonPropertyName("first_mile_emission_kg")]
        public double FirstMileEmissionKg { get; set; }

        [JsonPropertyName("rail_emission_kg")]
        public double RailEmissionKg { get; set; }

        [JsonPropertyName("last_mile_emission_kg")]
        public double LastMileEmissionKg { get; set; }

        [JsonPropertyName("multimodal_emission_kg")]
        public double MultimodalEmissionKg { get; set; }

        [JsonPropertyName("carbon_reduction_kg")]
        public double CarbonReductionKg { get; set; }

        [JsonPropertyName("carbon_reduction_rate")]
        public double CarbonReductionRate { get; set; }
    }

    // 추천 결과
    public class RecommendationInfo
    {
        [JsonPropertyName("recommended_mode")]
        public string RecommendedMode { get; set; } = string.Empty;

        [JsonPropertyName("recommended_name")]
        public string RecommendedName { get; set; } = string.Empty;

        [JsonPropertyName("road_score")]
        public double RoadScore { get; set; }

        [JsonPropertyName("multimodal_score")]
        public double MultimodalScore { get; set; }

        [JsonPropertyName("weights")]
        public WeightInfo Weights { get; set; } = new();

        [JsonPropertyName("normalized_scores")]
        public NormalizedScoresInfo NormalizedScores { get; set; } = new();

        [JsonPropertyName("ratios")]
        public RatioInfo Ratios { get; set; } = new();

        [JsonPropertyName("differences")]
        public DifferenceInfo Differences { get; set; } = new();

        [JsonPropertyName("change_rates")]
        public ChangeRatesInfo ChangeRates { get; set; } = new();
    }

    public class NormalizedScoresInfo
    {
        [JsonPropertyName("road")]
        public ScoreDetailInfo Road { get; set; } = new();

        [JsonPropertyName("multimodal")]
        public ScoreDetailInfo Multimodal { get; set; } = new();
    }

    public class ScoreDetailInfo
    {
        [JsonPropertyName("cost")]
        public double Cost { get; set; }

        [JsonPropertyName("time")]
        public double Time { get; set; }

        [JsonPropertyName("carbon")]
        public double Carbon { get; set; }
    }

    public class RatioInfo
    {
        [JsonPropertyName("road")]
        public RatioDetailInfo Road { get; set; } = new();

        [JsonPropertyName("multimodal")]
        public RatioDetailInfo Multimodal { get; set; } = new();
    }

    public class RatioDetailInfo
    {
        [JsonPropertyName("cost")]
        public double Cost { get; set; }

        [JsonPropertyName("time")]
        public double Time { get; set; }

        [JsonPropertyName("carbon")]
        public double Carbon { get; set; }
    }

    public class DifferenceInfo
    {
        [JsonPropertyName("cost_won")]
        public long CostWon { get; set; }

        [JsonPropertyName("time_min")]
        public double TimeMin { get; set; }

        [JsonPropertyName("carbon_kg")]
        public double CarbonKg { get; set; }
    }

    public class ChangeRatesInfo
    {
        [JsonPropertyName("cost_percent")]
        public double CostPercent { get; set; }

        [JsonPropertyName("time_percent")]
        public double TimePercent { get; set; }

        [JsonPropertyName("carbon_percent")]
        public double CarbonPercent { get; set; }
    }
}
