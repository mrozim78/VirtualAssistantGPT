using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels;
using Amazon.Polly;
using static System.Net.Mime.MediaTypeNames;
using Amazon.Polly.Model;
using Amazon.Runtime;
using System.Net;
using Amazon;
using System.IO;
using Microsoft.Extensions.Options;

namespace VirtualAssistantGPT.Web.Pages
{
    public class SayModel : PageModel
    {
        private readonly AWSCredentialsOptions _configuration;

        public SayModel(IOptions<AWSCredentialsOptions> options)
        {
            _configuration = options.Value;
        }
        [BindProperty]
        public string Question
        {
            get; set;
        }
        public async Task<IActionResult> OnPost()

        {
            AWSCredentials credentials = new Amazon.Runtime.BasicAWSCredentials(_configuration.AccessKey, _configuration.SecretKey);

            var client = new AmazonPollyClient(credentials, RegionEndpoint.USEast1);
            var synthesizeSpeechRequest = new SynthesizeSpeechRequest()
            {
                OutputFormat = OutputFormat.Mp3,
                VoiceId = VoiceId.Ola,
                Text = Question,
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
