using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Newtonsoft.Json.Linq;

namespace MythicForge.Services
{
    /// <summary>
    /// Generates photorealistic creature preview images with Amazon Bedrock using
    /// Stability AI's Stable Image Ultra model. The model runs in us-west-2 (the region
    /// that hosts the Stability text-to-image models) regardless of where the app runs.
    /// Credentials come from the default chain (the EC2 instance role on Elastic
    /// Beanstalk), so no keys are stored in the app.
    /// </summary>
    public class BedrockImageService
    {
        // Stability AI text-to-image model. Alternatives (same request/response schema,
        // cheaper): "stability.sd3-5-large-v1:0" or "stability.stable-image-core-v1:1".
        private const string ModelId = "stability.stable-image-ultra-v1:1";

        // Stability text-to-image models are hosted in us-west-2.
        private static readonly RegionEndpoint BedrockRegion = RegionEndpoint.USWest2;

        // Default negatives: steer away from common artifacts and from rendering more than
        // one creature. The caller can supply a tailored negative prompt (e.g. multi-headed
        // creatures like the Hydra must NOT suppress "extra heads").
        public const string DefaultNegativePrompt =
            "multiple creatures, a group, herd, pack, crowd, two separate beings, two characters, " +
            "person riding a horse, horse and rider, mounted rider, jockey, saddle, reins, " +
            "extra heads, extra limbs, duplicated subject, deformed, disfigured, text, watermark, " +
            "signature, logo, frame, border, blurry, low quality";

        /// <summary>
        /// Invokes Stable Image Ultra with the given prompt and returns a
        /// "data:image/jpeg;base64,..." URI ready to drop into an &lt;img&gt; tag.
        /// </summary>
        public async Task<string> GenerateImageDataUriAsync(string prompt, string negativePrompt = null)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            // Stability accepts prompts up to ~10,000 characters.
            if (prompt.Length > 9000)
            {
                prompt = prompt.Substring(0, 9000);
            }

            var payload = new JObject
            {
                ["prompt"] = prompt,
                ["negative_prompt"] = string.IsNullOrWhiteSpace(negativePrompt) ? DefaultNegativePrompt : negativePrompt,
                ["mode"] = "text-to-image",
                ["aspect_ratio"] = "1:1",
                ["output_format"] = "jpeg",
                ["seed"] = new Random().Next(0, int.MaxValue)
            };

            using (var client = new AmazonBedrockRuntimeClient(BedrockRegion))
            {
                var request = new InvokeModelRequest
                {
                    ModelId = ModelId,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()))
                };

                var response = await client.InvokeModelAsync(request).ConfigureAwait(false);

                string responseJson;
                using (var reader = new StreamReader(response.Body))
                {
                    responseJson = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                var parsed = JObject.Parse(responseJson);

                // A non-null finish reason means the image was filtered or otherwise not produced.
                var finishReason = parsed["finish_reasons"]?[0];
                if (finishReason != null && finishReason.Type != JTokenType.Null)
                {
                    throw new InvalidOperationException(
                        "The image could not be generated (" + finishReason + "). Try different selections.");
                }

                var base64 = (string)(parsed["images"]?[0]);
                if (string.IsNullOrEmpty(base64))
                {
                    throw new InvalidOperationException("Bedrock did not return an image.");
                }

                return "data:image/jpeg;base64," + base64;
            }
        }
    }
}
