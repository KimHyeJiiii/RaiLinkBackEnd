namespace RailLinkBackEnd.Models
{
    public sealed record HistoryListItemResponse(
        string ReceptNo,
        string OriginName,
        string DestinationName,
        double CargoWeightTon,
        string RecommendedMode,
        IReadOnlyList<string> TransportLegs,
        double RailRatio,
        double CostSavingRate,
        long CostSavingWon,
        double CarbonSavingRate,
        double CarbonSavingKg,
        DateTime AnalyzedAt);

    public sealed record PaginationResponse(
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages,
        bool HasPrevious,
        bool HasNext);

    public sealed record PagedResponse<T>(
        IReadOnlyList<T> Items,
        PaginationResponse Pagination);

    public sealed record HistorySummaryResponse(
        int WeeklyAnalysisCount,
        int RailRecommendationCount,
        double RailRecommendationRate,
        double AverageCostSavingRate,
        double AverageCarbonSavingRate,
        LatestAnalysisResponse? LatestAnalysis);

    public sealed record LatestAnalysisResponse(
        string ReceptNo,
        string OriginName,
        string DestinationName,
        double CargoWeightTon,
        DateTime AnalyzedAt);
}
