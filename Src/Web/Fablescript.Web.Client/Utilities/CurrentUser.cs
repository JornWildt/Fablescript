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


    PlayerId? ICurrentUser.CurrentPlayerId(FableId fableId)
    {
      if (HttpContextAccessor.HttpContext?.Session == null)
        throw new NotImplementedException();

      var id = HttpContextAccessor.HttpContext.Session.GetString(CurrentPlayerId_SessionKey(fableId));
      return id == null ? null : new PlayerId(id);
    }

    
    void ICurrentUser.SetCurrentPlayerId(FableId fableId, PlayerId playerId)
    {
      if (HttpContextAccessor.HttpContext?.Session == null)
        throw new NotImplementedException();

      HttpContextAccessor.HttpContext.Session.SetString(CurrentPlayerId_SessionKey(fableId), playerId.Value);
    }
    

    private string CurrentPlayerId_SessionKey(FableId fableId) => fableId + "_PlayerId";
  }
}
