using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Linq;

namespace ABC.Assembly.Common
{
    /// <summary>
    /// Static utility class that wraps standard <see cref="IOrganizationService"/> operations
    /// with diagnostic tracing and Stopwatch timing. Provides automatic paging for RetrieveMultiple,
    /// structured exception logging, and an extension method for aliased attribute extraction.
    /// All CRM operations throughout the solution should route through this proxy when the
    /// tracing-enabled overloads are available, to ensure consistent runtime diagnostics.
    /// </summary>
    public static class SimplifiedServiceProxy
    {

        /// <summary>
        /// Logs structured diagnostic information from a caught exception, handling
        /// <see cref="FaultException{OrganizationServiceFault}"/>, <see cref="TimeoutException"/>,
        /// and generic exceptions with their inner faults. Does not re-throw — callers are
        /// expected to throw after calling this method.
        /// </summary>
        /// <param name="exceptionFromSource">The exception thrown by the source code.</param>
        /// <param name="tracingService">The tracing service to log the exception details.</param>
        public static void HandleException(Exception exceptionFromSource, ITracingService tracingService)
        {
            tracingService.Trace("The application terminated with an error.");

            try
            {
                throw exceptionFromSource;
            }
            catch (FaultException<OrganizationServiceFault> fe)
            {
                tracingService.Trace("Timestamp: {0}", fe.Detail.Timestamp);
                tracingService.Trace("Code: {0}", fe.Detail.ErrorCode);
                tracingService.Trace("Message: {0}", fe.Detail.Message);
                tracingService.Trace("Plugin Trace: {0}", fe.Detail.TraceText);
                tracingService.Trace("Inner Fault: {0}",
                    null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (TimeoutException te)
            {
                tracingService.Trace("Message: {0}", te.Message);
                tracingService.Trace("Stack Trace: {0}", te.StackTrace);
                tracingService.Trace("Inner Fault: {0}",
                    te.InnerException == null ? "No Inner Fault" : te.InnerException.Message);

            }
            catch (Exception ex)
            {
                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    tracingService.Trace(ex.InnerException.Message);

                    FaultException<OrganizationServiceFault> fe = ex.InnerException
                        as FaultException<OrganizationServiceFault>;
                    if (fe != null)
                    {
                        tracingService.Trace("Timestamp: {0}", fe.Detail.Timestamp);
                        tracingService.Trace("Code: {0}", fe.Detail.ErrorCode);
                        tracingService.Trace("Message: {0}", fe.Detail.Message);
                        tracingService.Trace("Plugin Trace: {0}", fe.Detail.TraceText);
                        tracingService.Trace("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
        }

        /// <summary>
        /// Associates records via a relationship, logging the relationship name,
        /// entity details, and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="relationship">The relationship to use for the association.</param>
        /// <param name="relatedEntities">The collection of related entities to associate.</param>
        public static void Associate(IOrganizationService service, ITracingService tracingService, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            tracingService.Trace($"Associate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            tracingService.Trace("Associated record(s):{0}", relatedEntities.Select(r => $"\n  {r.LogicalName} {r.Id} {r.Name}"));
            var watch = Stopwatch.StartNew();
            service.Associate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            tracingService.Trace($"Associated in: {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Creates a record, assigns the returned Id back to the entity, and logs
        /// the entity type, Id, attribute count, and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The created entity with the assigned Id.</returns>
        public static Entity Create(IOrganizationService service, ITracingService tracingService, Entity entity)
        {
            var watch = Stopwatch.StartNew();
            entity.Id = service.Create(entity);
            tracingService.Trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            watch.Stop();
            tracingService.Trace($"Created in: {watch.ElapsedMilliseconds} ms");
            return entity;
        }

        /// <summary>
        /// Deletes a record by entity name and Id, logging the operation and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="id">The Id of the record to delete.</param>
        public static void Delete(IOrganizationService service, ITracingService tracingService, string entityName, Guid id)
        {
            tracingService.Trace($"Delete({entityName}, {id})");
            var watch = Stopwatch.StartNew();
            service.Delete(entityName, id);
            watch.Stop();
            tracingService.Trace($"Deleted in: {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Updates a record, logging the entity type, Id, attribute count, and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entity">The entity to update.</param>
        public static void Update(IOrganizationService service, ITracingService tracingService, Entity entity)
        {
            tracingService.Trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            service.Update(entity);
            watch.Stop();
            tracingService.Trace($"Updated in: {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Disassociates records from a relationship, logging the relationship name,
        /// entity details, and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="relationship">The relationship to use for the disassociation.</param>
        /// <param name="relatedEntities">The collection of related entities to disassociate.</param>
        public static void Disassociate(IOrganizationService service, ITracingService tracingService, string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            tracingService.Trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            tracingService.Trace("Disassociated record(s):{0}", relatedEntities.Select(r => $"\n  {r.LogicalName} {r.Id} {r.Name}"));
            var watch = Stopwatch.StartNew();
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            tracingService.Trace($"Disassociated in: {watch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Executes an arbitrary <see cref="OrganizationRequest"/>, logging the request name
        /// and elapsed time. For <see cref="ExecuteFetchRequest"/> requests, the FetchXML is also traced.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="request">The request to execute.</param>
        /// <returns>The response from the executed request.</returns>
        public static OrganizationResponse Execute(IOrganizationService service, ITracingService tracingService, OrganizationRequest request)
        {
            tracingService.Trace($"Execute({request.RequestName})");
            if (request is ExecuteFetchRequest)
            {
                tracingService.Trace("FetchXML: {0}", ((ExecuteFetchRequest)request).FetchXml);
            }
            var watch = Stopwatch.StartNew();
            var result = service.Execute(request);
            watch.Stop();
            tracingService.Trace($"Executed in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        /// <summary>
        /// Retrieves a single record by entity name, Id, and column set, logging the operation
        /// and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <param name="id">The Id of the record to retrieve.</param>
        /// <param name="columnSet">The column set specifying the attributes to retrieve.</param>
        /// <returns>The retrieved entity.</returns>
        public static Entity Retrieve(IOrganizationService service, ITracingService tracingService, string entityName, Guid id, ColumnSet columnSet)
        {
            tracingService.Trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            var watch = Stopwatch.StartNew();
            var result = service.Retrieve(entityName, id, columnSet);
            watch.Stop();
            return result;
        }

        /// <summary>
        /// Retrieves all records matching the query, automatically paging through results using
        /// the MoreRecords flag and PagingCookie. Returns an empty <see cref="EntityCollection"/>
        /// if no records match. Logs the total record count and elapsed time.
        /// </summary>
        /// <param name="service">The organization service.</param>
        /// <param name="tracingService">The tracing service.</param>
        /// <param name="query">The query expressing the criteria for record retrieval.</param>
        /// <returns>A collection of entities matching the query.</returns>
        public static EntityCollection RetrieveMultiple(IOrganizationService service, ITracingService tracingService, QueryExpression query)
        {
            tracingService.Trace("RetrieveMultiple({0})", query.EntityName);
            //var fetch = ((QueryExpressionToFetchXmlResponse)service.Execute(new QueryExpressionToFetchXmlRequest() { Query = query })).FetchXml;
            //tracingService.Trace("Query: {0}", fetch);
            var watch = Stopwatch.StartNew();
            EntityCollection resultsList = new EntityCollection();
            while (true)
            {
                EntityCollection result = service.RetrieveMultiple(query);
                resultsList.Entities.AddRange(new List<Entity>(result.Entities));
                if (result.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = result.PagingCookie;
                }
                else
                {
                    break;  // Exit loop
                }
            }


            if (resultsList.Entities.Count < 1)
            {
                tracingService.Trace("RetriveMultiple: Did not get any results");
                return new EntityCollection();
            }
            watch.Stop();
            tracingService.Trace($"Retrieved {resultsList.Entities.Count} records in: {watch.ElapsedMilliseconds} ms");
            return resultsList;
        }

        /// <summary>
        /// Extracts aliased attributes from a joined entity into a standalone <see cref="Entity"/>.
        /// Filters attributes where the AliasedValue's EntityLogicalName matches the target entity.
        /// Note: the returned entity has no LogicalName set, so the filter will only match
        /// aliased values with an empty entity logical name unless the caller sets it first.
        /// </summary>
        /// <param name="entity">The entity containing aliased attributes from a joined query.</param>
        /// <returns>A new entity containing the extracted aliased attributes.</returns>
        public static Entity GetRelatedEntity(Entity entity)
        {
            Entity relatedEntity = new Entity();
            entity.Attributes.Where(attr => attr.Value is AliasedValue && (attr.Value as AliasedValue).EntityLogicalName.Equals(relatedEntity.LogicalName))
             .Select(attr => attr.Value as AliasedValue).ToList().ForEach(attr =>
             {
                 relatedEntity.Attributes.Add(attr.AttributeLogicalName, attr.Value);
             });
            return relatedEntity;
        }

        /// <summary>
        /// Extension method to safely extract a typed value from an <see cref="AliasedValue"/> attribute.
        /// Returns default(T) if the entity is null, the attribute is missing, or the value type does not match.
        /// </summary>
        /// <typeparam name="T">The expected CLR type of the aliased value.</typeparam>
        /// <param name="entity">The entity containing aliased attributes from a joined query.</param>
        /// <param name="attributeName">The full aliased attribute name (e.g. "Colleague.psa_staffnumber").</param>
        /// <returns>The extracted aliased value, or default(T) if extraction fails.</returns>
        public static T GetAliasedAttributeValue<T>(this Entity entity, string attributeName)
        {
            if (entity == null)
                return default(T);

            AliasedValue fieldAliasValue = entity.GetAttributeValue<AliasedValue>(attributeName);

            if (fieldAliasValue == null)
                return default(T);

            if (fieldAliasValue.Value != null && fieldAliasValue.Value.GetType() == typeof(T))
            {
                return (T)fieldAliasValue.Value;
            }

            return default(T);
        }


    }
}
