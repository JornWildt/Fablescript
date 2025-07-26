using System.Diagnostics;
using Fablescript.Core.Contract;
using Fablescript.Utility.Services.CommandQuery;
using Microsoft.Extensions.Logging;

namespace Fablescript.Core
{
  public class CoreCommandProcessor : CommandProcessor<CoreUnitOfWorkContext>
  {
    public CoreCommandProcessor(
      //IAuditService auditService,
      //IIdentityProvider identityProvider,
      //ICommandValidator validator,
      ActivitySource activitySource,
      IServiceProvider serviceProvider,
      ILogger<CommandProcessor<CoreUnitOfWorkContext>> logger)
      : base(activitySource, serviceProvider, logger)
    {
    }


    protected override CoreUnitOfWorkContext CreateContext()
    {
      return new CoreUnitOfWorkContext();
    }
  }
}
