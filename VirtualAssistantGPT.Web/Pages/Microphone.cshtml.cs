using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels;

namespace VirtualAssistantGPT.Web.Pages
{
    public class MicrophoneModel : PageModel
    {
      

        [BindProperty]
        public IFormFile Upload { get; set; }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IOpenAIService _openAIService;
        public MicrophoneModel(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IOpenAIService openAIService)
        {
            _environment = environment;
            _openAIService = openAIService;
        }

        public async Task<IActionResult> OnPost()
        {

            AudioCreateTranscriptionResponse completionResult = null;

            using (var ms = new MemoryStream())
            {
                Upload.CopyTo(ms);

                byte[] bytes = ms.ToArray();
                if (_openAIService != null)
                {
                    completionResult = await _openAIService.Audio.CreateTranscription(new OpenAI.GPT3.ObjectModels.RequestModels.AudioCreateTranscriptionRequest
                    {

                        File = bytes,
                        FileName = Upload.FileName,
                        Model = Models.WhisperV1,
                        Language = "pl",
                        ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson
                    });
                }








                if (completionResult != null)
                {
                    if (completionResult.Successful)
                    {
                        return new  JsonResult(new { Answer=completionResult.Text , Error = string.Empty });
                    }
                    else if (completionResult.Error != null)
                    {
                        return new JsonResult(new { Answer = string.Empty, Error = string.Empty });
                    }
                }

            }
            return new JsonResult(new { Answer = string.Empty, Error = string.Empty });
        }
    }
}
