using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels;
using Amazon.Polly.Model;
using Amazon.Polly;
using Amazon.Runtime;
using Amazon;
using Microsoft.Extensions.Options;

namespace VirtualAssistantGPT.Web.Pages
{
    public class TalkModel : PageModel
    {
        #region Properties

        [BindProperty]
        public IFormFile Upload { get; set; }

        [BindProperty]
        public string Completion { get; set; }

        [BindProperty]
        public string Answer { get; set; }
        #endregion


        private readonly AWSCredentialsOptions _configuration;
        private readonly IOpenAIService _openAIService;
        
        public TalkModel(IOpenAIService openAIService, IOptions<AWSCredentialsOptions> options)
        {
            _configuration = options.Value;
            _openAIService = openAIService;
        }

        public async Task<IActionResult> OnPostUpload()
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
                        return new JsonResult(new { Answer = completionResult.Text, Error = string.Empty });
                    }
                    else if (completionResult.Error != null)
                    {
                        return new JsonResult(new { Answer = string.Empty, Error = string.Empty });
                    }
                }

            }
            return new JsonResult(new { Answer = string.Empty, Error = string.Empty });
        }

        public async Task<IActionResult> OnPostCompletion()
        {

            if (_openAIService != null)
            {
                var completionResult = await _openAIService.Completions.CreateCompletion(new OpenAI.GPT3.ObjectModels.RequestModels.CompletionCreateRequest
                {

                    Prompt = Completion,
                    Model = Models.TextDavinciV3,
                    MaxTokens = 350

                });
                if (completionResult != null)
                {
                    if (completionResult.Successful)
                    {
                        return new JsonResult(new { Answer = completionResult.Choices.FirstOrDefault().Text.Trim(), Error = string.Empty , CompletionText = Completion });
                        //Say = completionResult.Choices.FirstOrDefault().Text;
                    }
                    else if (completionResult.Error != null)
                    {
                        //Error = completionResult.Error.Message;
                        return new JsonResult(new { Answer = string.Empty, Error = completionResult.Error, CompletionText = Completion });
                        }
                }

            }
            return new JsonResult(new { Answer = string.Empty, Error = string.Empty ,  CompletionText = Completion });
        }


        public async Task<IActionResult> OnPostSay()

        {
            AWSCredentials credentials = new Amazon.Runtime.BasicAWSCredentials(_configuration.AccessKey, _configuration.SecretKey);

            var client = new AmazonPollyClient(credentials, RegionEndpoint.USEast1);
            var synthesizeSpeechRequest = new SynthesizeSpeechRequest()
            {
                OutputFormat = OutputFormat.Mp3,
                VoiceId = VoiceId.Ola,
                Text = Answer,
                Engine = Engine.Neural
            };

            var synthesizeSpeechResponse =
                await client.SynthesizeSpeechAsync(synthesizeSpeechRequest);

            MemoryStream memoryStream = new MemoryStream();

            WriteSpeechToStream(synthesizeSpeechResponse.AudioStream, memoryStream);
            return new FileContentResult(memoryStream.ToArray(), "audio/mpeg");
        }

        private static void WriteSpeechToStream(Stream audioStream, MemoryStream outputStream)
        {

            byte[] buffer = new byte[2 * 1024];
            int readBytes;

            while ((readBytes = audioStream.Read(buffer, 0, 2 * 1024)) > 0)
            {
                outputStream.Write(buffer, 0, readBytes);
            }

            // Flushes the buffer to avoid losing the last second or so of
            // the synthesized text.
            outputStream.Flush();

        }
    }
}
