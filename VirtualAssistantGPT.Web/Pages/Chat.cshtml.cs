using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Security.Cryptography.Xml;

namespace VirtualAssistantGPT.Web.Pages
{
    public class ChatModel : PageModel
    {

        private readonly IOpenAIService _openAIService;

        [BindProperty]
        public string Message { get; set; }

        [BindProperty]
        public List<string> HistoryRole { get; set; }

        [BindProperty]
        public List<string> HistoryText { get; set; }

        public ChatModel(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPost() {

            if (HistoryRole is not null && HistoryText is not null &&
                HistoryRole.Count == HistoryText.Count
                )
            {
                List<ChatMessage> chatMessages = ConvertFromHistoryToChatMessages(historyRole: HistoryRole, historyText: HistoryText);
                chatMessages.Add(ChatMessage.FromUser(Message));


                if (_openAIService != null)
                {

                    var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new OpenAI.GPT3.ObjectModels.RequestModels.ChatCompletionCreateRequest
                    {
                        Messages = chatMessages,
                        Model = Models.ChatGpt3_5Turbo
                    });


                    ChatMessage chatMessage = null;
                    if (completionResult != null)
                    {
                        if (completionResult.Successful)
                        {
                            chatMessage = completionResult.Choices.First().Message;
                        }

                    }
                    if (chatMessage is not null)
                    {
                        chatMessages.Add(chatMessage);
                    }
                    return GetJsonResultFromChatMessages(chatMessages);
                }

            }
            return new JsonResult(new { });
        }

        public List<ChatMessage> ConvertFromHistoryToChatMessages(List<string> historyRole, List<string> historyText)
        {
            List<ChatMessage> chatMessages = new List<ChatMessage>();
            for (int i = 0; i < historyRole.Count; i++)
            {
                string role = historyRole[i];
                if (role == "system")
                    chatMessages.Add(ChatMessage.FromSystem(historyText[i]));
                if (role == "assistant")
                    chatMessages.Add(ChatMessage.FromAssistant(historyText[i]));
                if (role == "user")
                    chatMessages.Add(ChatMessage.FromUser(historyText[i]));

            }
            return chatMessages;
        }

        public JsonResult GetJsonResultFromChatMessages(List<ChatMessage> chatMessages)
        {
            return new JsonResult(chatMessages.Select(a => new { role = a.Role, text = a.Content }).ToList());
        }
    }
}
