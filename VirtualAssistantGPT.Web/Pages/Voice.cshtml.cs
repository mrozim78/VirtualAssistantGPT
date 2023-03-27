using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.Interfaces;

namespace VirtualAssistantGPT.Web.Pages
{
    public class VoiceModel : PageModel
    {
        [BindProperty]
        public string Answer { get; set; }

        [BindProperty]
        public string Error { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }

        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private readonly IOpenAIService _openAIService;
        public VoiceModel(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IOpenAIService openAIService)
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
                            ResponseFormat = StaticValues.AudioStatics.ResponseFormat.VerboseJson
                        });
                    }
           

           
                

                

              
                if (completionResult != null)
                {
                    if (completionResult.Successful)
                    {
                        Answer = completionResult.Text;
                    }
                    else if (completionResult.Error != null)
                    {
                        Error = completionResult.Error.Message;
                    }
                }

            }
            return Page();
        }
    }
}
