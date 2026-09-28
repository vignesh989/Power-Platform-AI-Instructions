using System;
using System.Collections.Generic;
using System.Text;

namespace ABC.Assembly.Common
{
    /// <summary>
    /// Central constants used across all plugin and custom action assemblies.
    /// Avoids magic strings for CRM message names, input parameter keys, and display values.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Display text constants used in plugin registration and telemetry.
        /// </summary>
        internal struct DisplayText
        {
            public const string PluginName = "ABC.SamplePlugin";
        }

        /// <summary>
        /// CRM SDK message names (lowercase) used in plugin Execute method switch statements
        /// to route logic based on the triggering message.
        /// </summary>
        internal struct MessageName
        {
            public const string SetState = "setstate";
            public const string RevokeAccess = "revokeaccess";
            public const string AddMember = "addmember";
            public const string AddMembers = "addmembers";
            public const string Assign = "assign";
            public const string Create = "create";
            public const string Delete = "delete";
            public const string Update = "update";
            public const string RemoveMember = "removemember";
            public const string Send = "send";
            public const string Retrieve = "retrieve";
        }

        /// <summary>
        /// Standard input parameter and image keys used to extract the target entity,
        /// pre-image, and post-image from the plugin execution context.
        /// </summary>
        internal struct InputParameter
        {
            public const string Target = "Target";
            public const string PreImage = "PreImage";
            public const string PostImage = "PostImage";
        }
    }
}
