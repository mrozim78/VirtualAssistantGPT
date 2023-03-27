using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;

namespace VirtualAssistantGPT.Web.Pages
{
    public class TextCompletionModel : PageModel
    {

        private readonly IOpenAIService _openAIService;
        [BindProperty]
        public string Answer { get; set; }

        [BindProperty]
        public string Error { get; set; }

        [BindProperty]
        public string Question { get; set; }

        public TextCompletionModel(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        /*
        public void OnGet()
        {
        }*/

        public async Task<IActionResult> OnPost()
        {
                     
            if (_openAIService!=null)
            {
                var completionResult = await _openAIService.Completions.CreateCompletion(new OpenAI.GPT3.ObjectModels.RequestModels.CompletionCreateRequest {

                    Prompt = Question,
                    Model = Models.TextDavinciV3,
                    MaxTokens = 350

                });
                if (completionResult!=null)
                {
                    if (completionResult.Successful)
                    {
                        Answer = completionResult.Choices.FirstOrDefault().Text.Trim();
                    } else if (completionResult.Error!=null)
                    {
                        Error = completionResult.Error.Message;
                    }
                }
               
            }
            return Page();
        }
    }
}
