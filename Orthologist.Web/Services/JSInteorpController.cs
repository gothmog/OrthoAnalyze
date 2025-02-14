using Microsoft.JSInterop;
using Orthologist.Web.Services.Bussiness;

namespace Orthologist.Web.Services
{
    public class JsInteropController : IDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<JsInteropController> _dotNetRef;
        private ITreeService _treeService;

        public JsInteropController(IJSRuntime jsRuntime, ITreeService treeService)
        {
            _jsRuntime = jsRuntime;
            _dotNetRef = DotNetObjectReference.Create(this);
            _treeService = treeService;
        }

        [JSInvokableAttribute]
        public async Task<string> LoadSubTree(string nodeId)
        {
            return await _treeService.LoadSubTree(nodeId);
        }

        public void Dispose()
        {
            // Uvolni DotNetObjectReference
            _dotNetRef?.Dispose();
        }
    }
}
