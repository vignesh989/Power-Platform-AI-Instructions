using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABC.Assembly.Common
{
    /// <summary>
    /// Wraps Dataverse environment variable retrieval, providing a clean API for reading
    /// both standard and secret environment variable values. Used throughout the solution
    /// to resolve configuration such as template GUIDs, email sender addresses, calculation
    /// parameters, and working-day settings without hardcoding values.
    /// </summary>
    public class EnvironmentVariableManager
    {
        private IOrganizationService OrgService { get; set; }
        private ITracingService TraceService { get; set; }

        /// <summary>
        /// Creates a new EnvironmentVariableManager instance bound to the specified CRM services.
        /// </summary>
        /// <param name="service">Organization service used to execute environment variable retrieval requests.</param>
        /// <param name="traceService">Tracing service for diagnostic logging.</param>
        public EnvironmentVariableManager(IOrganizationService service, ITracingService traceService)
        {
            OrgService = service;
            TraceService = traceService;
        }

        /// <summary>
        /// Retrieves the current value of a Dataverse environment variable by its schema name.
        /// Returns null if the variable exists but has no value set.
        /// Throws if the CRM request fails (e.g. variable does not exist).
        /// </summary>
        /// <param name="schemaName">The schema name of the environment variable definition (e.g. "abc_ESPGUIDNPVExcelTemplate").</param>
        /// <returns>The environment variable value as a string, or null if no value is configured.</returns>
        public string GetEnvironmentVariableValue(string schemaName)
        {
            RetrieveEnvironmentVariableValueRequest retrieveEnvVarVal = new RetrieveEnvironmentVariableValueRequest()
            {
                DefinitionSchemaName = schemaName
            };
            try
            {
                RetrieveEnvironmentVariableValueResponse envVarValResponse = (RetrieveEnvironmentVariableValueResponse)OrgService.Execute(retrieveEnvVarVal);

                if (envVarValResponse.Value == null || envVarValResponse.Value == string.Empty)
                {
                    TraceService.Trace("GetEnvironmentVariableValue: Did not get any results!");
                    return null;
                }
                else
                {
                    TraceService.Trace($"GetEnvironmentVariableValue: Result {envVarValResponse.Value}!");
                    return envVarValResponse.Value;
                }
            }
            catch (Exception ex)
            {
                TraceService.Trace($"ERROR: GetEnvironmentVariableValue: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the decrypted value of a secret environment variable by its schema name.
        /// Returns null if the variable exists but has no secret value set.
        /// Throws if the CRM request fails (e.g. variable does not exist or caller lacks permissions).
        /// </summary>
        /// <param name="schemaName">The schema name of the secret environment variable (e.g. "abc_ESPSecretAPIKey").</param>
        /// <returns>The decrypted secret value as a string, or null if no value is configured.</returns>
        public string GetEnvironmentVariableSecrectValue(string schemaName)
        {
            RetrieveEnvironmentVariableSecretValueRequest retrieveEnvVarVal = new RetrieveEnvironmentVariableSecretValueRequest()
            {
                EnvironmentVariableName = schemaName
            };
            try
            {
                RetrieveEnvironmentVariableSecretValueResponse envVarValResponse = (RetrieveEnvironmentVariableSecretValueResponse)OrgService.Execute(retrieveEnvVarVal);

                if (envVarValResponse.EnvironmentVariableSecretValue == null || envVarValResponse.EnvironmentVariableSecretValue == string.Empty)
                {
                    TraceService.Trace("GetEnvironmentVariableSecrectValue: Did not get any results!");
                    return null;
                }
                else
                {
                    return envVarValResponse.EnvironmentVariableSecretValue;
                }
            }
            catch (Exception ex)
            {
                TraceService.Trace($"ERROR: GetEnvironmentVariableSecrectValue: {ex.Message}");
                throw;
            }
        }

    }
}
