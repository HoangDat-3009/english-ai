namespace EngAce.Api.DTO
{
    public class UploadStatsDto
    {
        public int TotalUploaded { get; set; }
        public int TotalAI { get; set; }
        public int TotalManual { get; set; }
        public int WeeklyUploads { get; set; }
        public decimal TotalStorageUsed { get; set; } // in MB
        public int ProcessingCount { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}