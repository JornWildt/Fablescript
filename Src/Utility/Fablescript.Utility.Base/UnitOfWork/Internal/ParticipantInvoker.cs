using System.Diagnostics.CodeAnalysis;

namespace Fablescript.Utility.Base.UnitOfWork.Internal
{
  public class ParticipantInvoker<TContext, TPayload> : IParticipantInvoker<TContext>
    where TContext : IUnitOfWorkContext
  {
    protected IUnitOfWorkParticipant<TContext, TPayload> Participant { get; set; }
    protected TPayload? Payload { get; set; }

    public ParticipantInvoker(IUnitOfWorkParticipant<TContext, TPayload> participant)
    {
      Participant = participant;
    }

    async Task IParticipantInvoker<TContext>.StartAsync(TContext context)
    {
      if (Payload != null)
        throw new InvalidOperationException($"Do not call StartAsync() on ParticipantInvoker more than once.");
      Payload = await Participant.StartAsync(context);
    }

    
    Task IParticipantInvoker<TContext>.CommitAsync(TContext context)
    {
      if (Payload == null)
        throw new InvalidOperationException($"Do not call CommitAsync() on ParticipantInvoker before calling InitializeAsync().");
      return Participant.CommitAsync(context, Payload);
    }

    
    Task IParticipantInvoker<TContext>.RollbackAsync(TContext context)
    {
      if (Payload == null)
        throw new InvalidOperationException($"Do not call RollbackAsync() on ParticipantInvoker before calling InitializeAsync().");
      return Participant.RollbackAsync(context, Payload);
    }

    
    bool IParticipantInvoker<TContext>.TryGetPayload<TPayloadOut>([MaybeNullWhen(false)] out TPayloadOut payload)
    {
      if (Payload is TPayloadOut payloadOut)
      {
        payload = payloadOut;
        return true;
      }
      else
      {
        payload = default;
        return false;
      }
    }
  }
}
