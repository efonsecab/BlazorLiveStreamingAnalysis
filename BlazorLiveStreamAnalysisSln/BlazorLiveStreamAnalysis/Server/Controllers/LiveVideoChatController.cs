using BlazorLiveStreamAnalysis.Shared;
using Microsoft.AspNetCore.Mvc;
using PTI.Microservices.Library.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorLiveStreamAnalysis.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveVideoChatController : ControllerBase
    {
        public LiveVideoChatController(AzureVideoIndexerService azureVideoIndexerService)
        {
            this.AzureVideoIndexerService = azureVideoIndexerService;
        }

        public AzureVideoIndexerService AzureVideoIndexerService { get; }

        [HttpPost("[action]")]
        public async Task<IActionResult> SendVideoChunkToServer([FromBody] VideoChunkModel model)
        {
            string callBackUrl = $"https://ptimanagedapisv2.azurewebsites.net/" +
                $"api/{1.0}/VideoAnalysis/SendAnalysisResults";
            await Task.Yield();
            if (model != null && !String.IsNullOrWhiteSpace(model.VideoBase64String))
            {
                var allPerosnModels = await this.AzureVideoIndexerService.GetAllPersonModelsAsync();
                var defaultPersonModel = allPerosnModels.Where(p => p.isDefault == true).Single();
                var result = await this.AzureVideoIndexerService.UploadVideoFromBase64StringAsync(model.VideoBase64String,
                    $"LiveAnalysis-{Guid.NewGuid()}",
                    "Live Analysis Video", "liveAnalysis.mkv", Guid.Parse(defaultPersonModel.id),
                    AzureVideoIndexerService.VideoPrivacy.Private, new Uri(callBackUrl));
                return Ok($"VideoId: {result.id}");
            }
            return Ok("Something wrong happened");
        }
    }
}
