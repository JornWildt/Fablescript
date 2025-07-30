using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Contract.Fablescript;

namespace Fablescript.Web.Client.Utilities
{
  public class CurrentUser : ICurrentUser
  {
    #region Dependencies

    private readonly IHttpContextAccessor HttpContextAccessor;

    #endregion


    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
      HttpContextAccessor = httpContextAccessor;
    }


    GameId? ICurrentUser.CurrentGameId(FableId fableId)
    {
      if (HttpContextAccessor.HttpContext?.Session == null)
        throw new NotImplementedException();

      var id = HttpContextAccessor.HttpContext.Session.GetString(CurrentGameId_SessionKey(fableId));
      return id == null ? null : new GameId(new Guid(id));
    }

    
    void ICurrentUser.SetCurrentGameId(FableId fableId, GameId gameId)
    {
      if (HttpContextAccessor.HttpContext?.Session == null)
        throw new NotImplementedException();

      HttpContextAccessor.HttpContext.Session.SetString(CurrentGameId_SessionKey(fableId), gameId.Value.ToString());
    }
    

    private string CurrentGameId_SessionKey(FableId fableId) => fableId + "_GameId";
  }
}
