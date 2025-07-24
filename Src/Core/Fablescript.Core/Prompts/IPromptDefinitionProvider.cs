using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fablescript.Core.Prompts
{
  internal interface IPromptDefinitionProvider
  {
    PromptDefinition LoadPrompt(string name, string? language);
  }
}
