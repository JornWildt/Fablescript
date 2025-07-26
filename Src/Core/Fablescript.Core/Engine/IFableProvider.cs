using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Core.Engine
{
  internal interface IFableProvider
  {
    Task<FableDTO> GetAsync(FableId fableId);
  }
}
