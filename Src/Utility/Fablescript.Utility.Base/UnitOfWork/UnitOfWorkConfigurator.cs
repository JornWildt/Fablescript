using Fablescript.Utility.Base.UnitOfWork.Internal;

namespace Fablescript.Utility.Base.UnitOfWork
{
  public class UnitOfWorkConfigurator<TContext> : IUnitOfWorkConfigurator<TContext>
    where TContext : IUnitOfWorkContext
  {
    protected class ParticipantReference<TPayload> : IParticipantReference<TContext>
    {
      IUnitOfWorkParticipant<TContext, TPayload> Participant { get; }

      public ParticipantReference(IUnitOfWorkParticipant<TContext, TPayload> participant)
      {
        Participant = participant;
      }

      IParticipantInvoker<TContext> IParticipantReference<TContext>.GetInvoker()
      {
        return new ParticipantInvoker<TContext, TPayload>(Participant);
      }
    }


    protected List<IParticipantReference<TContext>> Participants { get; set; } = new List<IParticipantReference<TContext>>();


    void IUnitOfWorkConfigurator<TContext>.RegisterParticipant<TPayload>(IUnitOfWorkParticipant<TContext, TPayload> participant)
    {
      var participantReference = new ParticipantReference<TPayload>(participant);
      Participants.Add(participantReference);
    }


    IReadOnlyList<IParticipantReference<TContext>> IUnitOfWorkConfigurator<TContext>.Participants => Participants.AsReadOnly();
  }
}
