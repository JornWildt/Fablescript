using System.Collections.Concurrent;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Database.Engine
{
  internal class PlayerRepository : IPlayerRepository
  {
    private static IDictionary<PlayerId, Player> Players { get; }

    static PlayerRepository()
    {
      // FIXME: Use persistent database
      Players = new ConcurrentDictionary<PlayerId, Player>();
    }


    void IRepository<Player, PlayerId>.Add(Player entity)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Player, PlayerId>.AddAsync(Player player)
    {
      Players.TryAdd(player.Id, player);
      return Task.CompletedTask;
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
      var player = Players[id];

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
