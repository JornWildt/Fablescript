using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Fablescript
{
  internal interface IFablescriptParser
  {
    Task<FableDefinition> GetFableAsync(FableId fableId);
  }
}
