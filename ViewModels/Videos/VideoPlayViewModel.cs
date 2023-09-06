namespace VideoStreaming.ViewModels.Videos
{
    public class VideoPlayViewModel
    {
        public Guid UserId { get; set; }
        public string VideoId { get; set; }
        public decimal DurationWatched { get; set; }
    }
}
