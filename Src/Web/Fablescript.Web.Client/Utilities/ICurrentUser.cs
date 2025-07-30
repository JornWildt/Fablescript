using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Web.Client.Utilities
{
  public interface ICurrentUser
  {
    GameId? CurrentGameId(FableId fableId);
    void SetCurrentGameId(FableId fableId, GameId gameId);
  }
}
