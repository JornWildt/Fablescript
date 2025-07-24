using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Engine;
using Fablescript.Core.GameConfiguration;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Database.Engine
{
  internal class PlayerRepository : IPlayerRepository
  {
    void IRepository<Player, PlayerId>.Add(Player entity)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Player, PlayerId>.AddAsync(Player entity)
    {
      throw new NotImplementedException();
    }

    Player IRepository<Player, PlayerId>.Get(PlayerId id)
    {
      throw new NotImplementedException();
    }

    IReadOnlyList<Player> IRepository<Player, PlayerId>.GetAll()
    {
      throw new NotImplementedException();
    }

    Task<IReadOnlyList<Player>> IRepository<Player, PlayerId>.GetAllAsync()
    {
      throw new NotImplementedException();
    }

    Task<Player> IRepository<Player, PlayerId>.GetAsync(PlayerId id)
    {
      var player = new Player(
        TemporaryConstants.PlayerId,
        TemporaryConstants.InitialLocationId);

      return Task.FromResult(player);
    }

    void IRepository<Player, PlayerId>.Remove(PlayerId id)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Player, PlayerId>.RemoveAsync(PlayerId id)
    {
      throw new NotImplementedException();
    }

    bool IRepository<Player, PlayerId>.TryGet(PlayerId id, out Player entity)
    {
      throw new NotImplementedException();
    }

    Task<(bool Success, Player? Entity)> IRepository<Player, PlayerId>.TryGetAsync(PlayerId id)
    {
      throw new NotImplementedException();
    }
  }
}
