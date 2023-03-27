using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;

namespace VirtualAssistantGPT.Web.Pages
{
    public class ImageGenerationModel : PageModel
    {
        private readonly IOpenAIService _openAIService;
        [BindProperty]
        public string ImageSrc { get; set; }

        [BindProperty]
        public string Error { get; set; }

        [BindProperty]
        public string Question { get; set; }

        public ImageGenerationModel(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {

            if (_openAIService != null)
            {
                var createImageResult = await _openAIService.Image.CreateImage(
                    new OpenAI.GPT3.ObjectModels.RequestModels.ImageCreateRequest {
                        Prompt = Question
                    });
                
                if (createImageResult != null)
                {
                    if (createImageResult.Successful)
                    {
                        ImageSrc = createImageResult.Results.FirstOrDefault().Url;
                        return Page();
                    }
                    else if (createImageResult.Error != null)
                    {
                        Error = createImageResult.Error.Message;
                    }
                }

            }
            ImageSrc = null;
            return Page();

        }
    }
}
