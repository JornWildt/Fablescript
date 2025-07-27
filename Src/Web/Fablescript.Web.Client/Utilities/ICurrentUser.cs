using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Web.Client.Utilities
{
  public interface ICurrentUser
  {
    PlayerId? CurrentPlayerId(FableId fableId);
    void SetCurrentPlayerId(FableId fableId, PlayerId playerId);
  }
}
