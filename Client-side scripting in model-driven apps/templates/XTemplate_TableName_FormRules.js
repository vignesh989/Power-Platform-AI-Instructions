"use strict";

var Contoso = Contoso || {};

Contoso.YourModuleName = (function () {


    // Event handler: On form load
    function onFormLoad(executionContext) {
        const formContext = executionContext.getFormContext();
        // Initialization logic here
    }

    // Event handler: On save
    function onSave(executionContext) {
        const formContext = executionContext.getFormContext();
        // Save logic here
    }

    // Event handler: On change
    function onChange(executionContext) {
        const formContext = executionContext.getFormContext();
        const attributeName = executionContext.getEventSource().getName();
        handleFieldChange(formContext, attributeName);
    }


    // Private function: Example Web API update
    function updateRecord(entityLogicalName, id, data, successCallback, errorCallback) {
        Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(
            function success(result) {
                if (typeof successCallback === "function") {
                    successCallback(result);
                }
            },
            function (error) {
                if (typeof errorCallback === "function") {
                    errorCallback(error);
                }
            }
        );
    }

    // Private function: Example field logic
    function handleFieldChange(formContext, attributeName) {
        const value = formContext.getAttribute(attributeName)?.getValue();
        // Add your logic here
    }


    // Publicly exposed methods
    return {
        OnFormLoad: onFormLoad,
        OnSave: onSave,
        OnChange: onChange
    };

})();
