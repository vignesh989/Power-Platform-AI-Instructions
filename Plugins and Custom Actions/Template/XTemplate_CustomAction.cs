using System;
using System.Activities;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using ABC.Assembly.Common;

namespace ABC.Assembly.CustomActions
{
    public class Template_CustomAction : CodeActivity
    {
        #region Inputs
        [RequiredArgument]
        [Input("Example Text Input")]
        public InArgument<string> ExampleText { get; set; }

        [Input("Example GUID Input")]
        public InArgument<string> ExampleGuid { get; set; }
        #endregion

        #region Outputs
        [Output("SuccessStatus")]
        public OutArgument<bool> SuccessStatus { get; set; }

        [Output("Output Message")]
        public OutArgument<string> OutputMessage { get; set; }
        #endregion

        protected override void Execute(CodeActivityContext executionContext)
        {
            #region Declaration
            ITracingService tracer = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            #endregion

            try
            {
                tracer.Trace("Start Action Execution");

                // ===== Read Inputs =====
                string textInput = ExampleText.Get<string>(executionContext);
                Guid guidText = ExampleGuid.Get<Guid>(executionContext);

                // ===== Core Logic =====
                string message = ExecuteCore(service, tracer, textInput, guidText);

                // ===== Set Outputs =====
                SuccessStatus.Set(executionContext, true);
                OutputMessage.Set(executionContext, message);

                tracer.Trace("Completed successfully.");
            }
            catch (Exception ex)
            {
                tracer.Trace("Unhandled error in {0}: {1}", nameof(Template_CustomAction), ex.Message);

                SuccessStatus.Set(executionContext, false);
                OutputMessage.Set(executionContext, ex.Message);

                throw new InvalidPluginExecutionException(
                    $"Error in ABC.Assembly.CustomActions.{nameof(Template_CustomAction)}: {ex.Message}", ex);
            }
        }

        #region Helpers

        /// <summary>
        /// Put your business logic here. Return a message to surface to flows/workflows.
        /// </summary>
        private static string ExecuteCore(IOrganizationService service, ITracingService tracer, string textInput, Guid exampleId)
        {
            // TODO: Implement your business logic

            // Examples:
            // var entity = service.Retrieve("account", exampleId, new ColumnSet(true));
            // service.Update(entity);
            // var response = service.Execute(new OrganizationRequest("YourRequestName") { ... });

            return "Action executed.";
        }
        #endregion
    }
}
