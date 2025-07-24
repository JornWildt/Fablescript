namespace Fablescript.Utility.Base.MessageBus
{
  public interface IBus
  {
    Task SendLocal(object commandMessage, IDictionary<string, string>? optionalHeaders = null);
    Task Send(object commandMessage, IDictionary<string, string>? optionalHeaders = null);
    Task DeferLocal(TimeSpan delay, object message, IDictionary<string, string>? optionalHeaders = null);
    Task Defer(TimeSpan delay, object message, IDictionary<string, string>? optionalHeaders = null);
    Task Reply(object replyMessage, IDictionary<string, string>? optionalHeaders = null);
    Task Publish(object eventMessage, IDictionary<string, string>? optionalHeaders = null);
  }
}
