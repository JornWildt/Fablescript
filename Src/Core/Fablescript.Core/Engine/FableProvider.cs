using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;
using Fablescript.Core.Fablescript;

namespace Fablescript.Core.Engine
{
  internal class FableProvider : IFableProvider
  {
    #region Dependencies

    private readonly IFablescriptParser FablescriptParser;

    #endregion


    public FableProvider(IFablescriptParser fablescriptParser)
    {
      FablescriptParser = fablescriptParser;
    }


    async Task<FableDTO> IFableProvider.GetAsync(FableId fableId)
    {
      var f = await FablescriptParser.GetFableAsync(fableId);
      return new FableDTO(
        fableId,
        f.Title,
        f.Description,
        new LocationId(f.InitialLocation));
    }
  }
}
