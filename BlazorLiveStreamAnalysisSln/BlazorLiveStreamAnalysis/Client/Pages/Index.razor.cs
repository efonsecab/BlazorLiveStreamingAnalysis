using BlazorLiveStreamAnalysis.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorLiveStreamAnalysis.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public HttpClient Http { get; set; }
        public ElementReference VideoElement { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        // Load the module and keep a reference to it
        // You need to use .AsTask() to convert the ValueTask to Task as it may be awaited multiple times
        private Task<IJSObjectReference> _module;
        private Task<IJSObjectReference> Module => _module ??= JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/liveVideoChat.js").AsTask();
        private List<string> Responses { get; set; } = new List<string>();
        private static Action OnNewVideoChunkFoundAction { get; set; }

        private bool IsLoading { get; set; }

        protected override void OnInitialized()
        {
            OnNewVideoChunkFoundAction = SendVideoChunkToServer;
        }


        public async Task StartLiveChat()
        {
            var module = await Module;
            await module.InvokeVoidAsync("startRecording", VideoElement);
        }

        private async void SendVideoChunkToServer()
        {
            try
            {
                IsLoading = true;
                StateHasChanged();
                var module = await this.Module;
                string jsonElement = await module.InvokeAsync<string>("getBinaryData");
                string value = jsonElement;
                VideoChunkModel model = new VideoChunkModel()
                {
                    VideoBase64String = value.ToString()
                };
                var response = await Http.PostAsJsonAsync("api/LiveVideoChat/SendVideoChunkToServer", model);
                var result = await response.Content.ReadAsStringAsync();
                Responses.Add(result);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        [JSInvokable]
        public static void OnNewVideoChunkFound()
        {
            OnNewVideoChunkFoundAction();
        }
    }
}
