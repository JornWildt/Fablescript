using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fablescript.Core.LLM
{
  internal interface ILLMStructuredGenerator
  {
    Task<T> GenerateStructuredOutputAsync<T>(
      string promptId,
      string model,
      LLMParameters?
      parameters,
      string system,
      string prompt,
      LLMTools? tools,
      T defaultValue);
  }
}
