﻿using Fablescript.Core.Contract.Engine;
using Fablescript.Utility.Base.Persistence;

namespace Fablescript.Core.Engine
{
  public interface IGameStateRepository : IRepository<GameState,GameId>
  {
  }
}
