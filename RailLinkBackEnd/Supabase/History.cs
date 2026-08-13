using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RailLinkBackEnd.Supabase 
{ 
    /// <summary>
    /// 운송안 생성 결과 히스토리
    /// </summary>
    [Table("history")]
    public class History
    {
        /// <summary>
        /// seq (도로:1 / 복합:여러개)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("seq")]
        public int Seq { get; set; }

        /// <summary>
        /// 견적번호 (KR-날짜-0001)
        /// </summary>
        [Column("recept_no")]
        public string ReceptNo { get; set; } = string.Empty;

        /// <summary>
        /// body로 넘어온 json 값
        /// </summary>
        [Column("input_json")]
        public string InputJson { get; set; } = string.Empty;

        /// <summary>
        /// 결과 json 값
        /// </summary>
        [Column("output_json")]
        public string OutputJson { get; set; } = string.Empty;

        /// <summary>
        /// 입력시간
        /// </summary>
        [Column("ent_date_time")]
        public DateTime EntDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 추천운송안
        /// </summary>
        [Column("recommended_mode")]
        public string recommendedMode { get; set; } = string.Empty;

        /// <summary>
        /// 비용 절감률
        /// </summary>
        [Column("cost_change_rate")]
        public double costChangeRate { get; set; }

        /// <summary>
        /// 탄소 절감률
        /// </summary>
        [Column("carbon_reduction_rate")]
        public double carbonReductionRate { get; set; }

        /// <summary>
        /// 출발지
        /// </summary>
        [Column("origin_name")]
        public string OriginName { get; set; } = string.Empty;

        /// <summary>
        /// 도착지
        /// </summary>
        [Column("destination_name")]
        public string DestinationName { get; set; } = string.Empty;

    }
}
