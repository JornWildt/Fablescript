using System.Collections.Concurrent;
using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Database.Engine
{
  internal class PlayerRepository : IPlayerRepository
  {
    // FIXME: Concurrency!
    private static IDictionary<GameId, Player> Players { get; }

    static PlayerRepository()
    {
      // FIXME: Use persistent database
      Players = new ConcurrentDictionary<GameId, Player>();
    }


    void IRepository<Player, GameId>.Add(Player entity)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Player, GameId>.AddAsync(Player player)
    {
      Players.TryAdd(player.Id, player);
      return Task.CompletedTask;
    }

    Player IRepository<Player, GameId>.Get(GameId id)
    {
      throw new NotImplementedException();
    }

    IReadOnlyList<Player> IRepository<Player, GameId>.GetAll()
    {
      throw new NotImplementedException();
    }

    Task<IReadOnlyList<Player>> IRepository<Player, GameId>.GetAllAsync()
    {
      throw new NotImplementedException();
    }

    Task<Player> IRepository<Player, GameId>.GetAsync(GameId id)
    {
      var player = Players[id];

      return Task.FromResult(player);
    }

    void IRepository<Player, GameId>.Remove(GameId id)
    {
      throw new NotImplementedException();
    }

    Task IRepository<Player, GameId>.RemoveAsync(GameId id)
    {
      throw new NotImplementedException();
    }

    bool IRepository<Player, GameId>.TryGet(GameId id, out Player entity)
    {
      throw new NotImplementedException();
    }

    Task<(bool Success, Player? Entity)> IRepository<Player, GameId>.TryGetAsync(GameId id)
    {
      throw new NotImplementedException();
    }
  }
}
