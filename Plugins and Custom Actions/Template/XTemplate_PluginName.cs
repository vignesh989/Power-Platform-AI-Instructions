using System;
using Microsoft.Xrm.Sdk;

namespace ABC.Assembly.Plugins
{
    /// <summary>
    /// Template every plugin starts from. Copy this file, rename the class and namespace
    /// to match the plugin's purpose, and implement the private Handler* methods for each
    /// message this plugin needs to handle. Do not change the shape of Execute() itself —
    /// service resolution, message routing, and exception handling must stay exactly as
    /// below. This mirrors Microsoft's own plug-in best-practice guidance: services are
    /// resolved fresh on every invocation (never cached as instance fields — see
    /// "Develop IPlugin implementations as stateless"), and every fault is traced before
    /// being rethrown as an InvalidPluginExecutionException.
    /// </summary>
    public class XTemplate_PluginName : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains(Common.Constants.InputParameter.Target)
                && context.InputParameters[Common.Constants.InputParameter.Target] is Entity)
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)
                    serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                Entity entity = (Entity)context.InputParameters[Common.Constants.InputParameter.Target];

                try
                {
                    // Route on message name using the shared Constants, never a string literal.
                    switch (context.MessageName.ToLower())
                    {
                        case Common.Constants.MessageName.Create:
                            HandlerCreate(service, tracingService, entity);
                            break;

                        case Common.Constants.MessageName.Update:
                            HandlerUpdate(service, tracingService, entity);
                            break;

                        case Common.Constants.MessageName.Delete:
                            HandlerDelete(service, tracingService, entity);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Common.SimplifiedServiceProxy.HandleException(ex, tracingService);
                    throw new InvalidPluginExecutionException(
                        $"An error occurred in ABC.Assembly.Plugins: {ex.Message}.");
                }
            }
        }

        /// <summary>
        /// Handles the Create message. Any CRUD or Execute call against `service` must go
        /// through Common.SimplifiedServiceProxy, never called directly on `service`.
        /// </summary>
        private void HandlerCreate(IOrganizationService service, ITracingService tracingService, Entity entity)
        {
            // Business logic here.
        }

        /// <summary>
        /// Handles the Update message. Use the PreImage/PostImage registered on the step
        /// (via Constants.InputParameter) to compare before/after state where needed.
        /// </summary>
        private void HandlerUpdate(IOrganizationService service, ITracingService tracingService, Entity entity)
        {
            // Business logic here.
        }

        /// <summary>
        /// Handles the Delete message.
        /// </summary>
        private void HandlerDelete(IOrganizationService service, ITracingService tracingService, Entity entity)
        {
            // Business logic here.
        }
    }
}
