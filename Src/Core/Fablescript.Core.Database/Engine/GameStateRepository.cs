using Fablescript.Core.Contract.Engine;
using Fablescript.Core.Engine;
using Fablescript.Utility.Base.Persistence;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Fablescript.Core.Database.Engine
{
  public class GameStateRepository : IGameStateRepository
  {
    private static ConcurrentDictionary<GameId, GameState> Games = new ConcurrentDictionary<GameId, GameState>();


    void IRepository<GameState, GameId>.Add(GameState game)
    {
      Games.TryAdd(game.Id, game);
    }

    Task IRepository<GameState, GameId>.AddAsync(GameState game)
    {
      Games.TryAdd(game.Id, game);
      return Task.CompletedTask;
    }

    GameState IRepository<GameState, GameId>.Get(GameId id)
    {
      return Games[id];
    }

    IReadOnlyList<GameState> IRepository<GameState, GameId>.GetAll()
    {
      throw new NotImplementedException();
    }

    Task<IReadOnlyList<GameState>> IRepository<GameState, GameId>.GetAllAsync()
    {
      throw new NotImplementedException();
    }

    Task<GameState> IRepository<GameState, GameId>.GetAsync(GameId id)
    {
      return Task.FromResult(Games[id]);
    }

    void IRepository<GameState, GameId>.Remove(GameId id)
    {
      throw new NotImplementedException();
    }

    Task IRepository<GameState, GameId>.RemoveAsync(GameId id)
    {
      throw new NotImplementedException();
    }

    bool IRepository<GameState, GameId>.TryGet(GameId id, [MaybeNullWhen(false)] out GameState game)
    {
      return Games.TryGetValue(id, out game);
    }

    Task<(bool Success, GameState? Entity)> IRepository<GameState, GameId>.TryGetAsync(GameId id)
    {
      bool found = Games.TryGetValue(id, out var game);
      return Task.FromResult((found, game));
    }
  }
}
