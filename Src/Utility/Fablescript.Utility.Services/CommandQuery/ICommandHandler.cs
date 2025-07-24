using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Utility.Services.CommandQuery
{
  public interface ICommandHandler<TCommand>
    where TCommand : ICommand
  {
    Task InvokeAsync(TCommand cmd);
  }
}
