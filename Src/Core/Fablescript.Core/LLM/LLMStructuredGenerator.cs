using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core.LLM
{
  internal class LLMStructuredGenerator : ILLMStructuredGenerator
  {
    #region Dependencies

    readonly ILLMGenerator LLMGenerator;
    readonly ILogger Logger;

    #endregion


    public LLMStructuredGenerator(
      ILLMGenerator llmGenerator,
      ILogger<LLMStructuredGenerator> logger)
    {
      LLMGenerator = llmGenerator;
      Logger = logger;
    }


    async Task<T> ILLMStructuredGenerator.GenerateStructuredOutputAsync<T>(
      string promptId,
      string model,
      LLMParameters? parameters,
      string system,
      string prompt,
      LLMTools? tools,
      T defaultValue)
    {
      var json = (await LLMGenerator.GenerateAsync(promptId, model, parameters, system, prompt, outputJson: true, tools, null, null))?.Response;
      if (json != null)
      {
        var result = DeserializeJson<T>(json, defaultValue);

        return result;
      }
      else
        return defaultValue;
    }


    T DeserializeJson<T>(string json, T defaultValue)
    {
      // Make sure we do not get JObject for objects, but instead we get dictionaries (and lists for arrays).
      //serializer.Converters.Add(new DictionaryConverter());

      try
      {
        return JsonSerializer.Deserialize<T>(json) ?? defaultValue;
      }
      catch (JsonException ex)
      {
        Logger.LogError("Failed to parse JSON: {Message}. Input was: {JSON}", ex.Message, json);
        return defaultValue;
      }
    }
  }
}
