using Fablescript.Utility.Base.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace Fablescript
{
  internal class ConsoleUnitOfWorkProvider<TContext> : UnitOfWorkProvider<TContext>
    where TContext : IUnitOfWorkContext
  {
    public ConsoleUnitOfWorkProvider(
      IUnitOfWorkConfigurator<TContext> unitOfWorkConfigurator,
      ILoggerFactory loggerFactory)
      : base(unitOfWorkConfigurator, loggerFactory)
    {
    }


    static IUnitOfWork? StaticUoW;

    protected override IUnitOfWork? Current
    {
      set => StaticUoW = value;
      get => StaticUoW;
    }
  }
}
