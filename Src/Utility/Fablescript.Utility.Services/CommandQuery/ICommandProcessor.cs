using Fablescript.Utility.Services.Contract.CommandQuery;

namespace Fablescript.Utility.Services.CommandQuery
{
  public interface ICommandProcessor
  {
    Task InvokeCommandAsync<TCmd>(TCmd cmd) where TCmd : ICommand;
  }

  public interface ICommandProcessor<T> : ICommandProcessor
  {
  }
}
