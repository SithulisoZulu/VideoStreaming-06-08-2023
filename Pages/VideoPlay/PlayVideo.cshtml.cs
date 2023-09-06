using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VideoStreaming.ViewModels.Videos;

namespace VideoStreaming.Pages.VideoPlay
{
    public class PlayVideoModel : VideoStreamingPageModel
    {
        public VideoPlayViewModel ObjectToDisplay { get; set; }

		public async Task OnGet(string Vid, Guid UserId)
		{
        }
    }
}
