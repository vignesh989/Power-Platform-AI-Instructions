"use strict";

var ABC = ABC || {};

ABC.RibbonButtonModule = (function () {
    "use strict";

    // Private helper: Show a confirmation dialog
    function showConfirmDialog(message, title, onConfirm) {
        var confirmStrings = { text: message, title: title };
        var confirmOptions = { height: 200, width: 450 };
        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(function (result) {
            if (result.confirmed && typeof onConfirm === "function") {
                onConfirm();
            }
        });
    }

    // Private helper: Show an alert dialog
    function showAlertDialog(message, title) {
        Xrm.Navigation.openAlertDialog({ text: message, title: title });
    }

    // Private helper: Web API update
    function updateRecord(entityLogicalName, id, data, onSuccess, onError) {
        Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(
            function (result) {
                if (typeof onSuccess === "function") onSuccess(result);
            },
            function (error) {
                if (typeof onError === "function") onError(error);
            }
        );
    }

    // Ribbon button handler: Example action
    function onExampleButtonClick(primaryControl) {
        // Example: Confirm before updating a record
        showConfirmDialog(
            "Are you sure you want to perform this action?",
            "Example Action",
            function () {
                var recordId = primaryControl.data.entity.getId();
                var data = { /* ...fields to update... */ };
                updateRecord("entitylogicalname", recordId, data,
                    function () {
                        primaryControl.data.refresh();
                        showAlertDialog("Action completed successfully.", "Success");
                    },
                    function (error) {
                        showAlertDialog("An error occurred: " + error.message, "Error");
                    }
                );
            }
        );
    }

    // Ribbon button handler: Another example action
    function onAnotherButtonClick(primaryControl) {
        // Implement logic for another button
    }

    // Expose public button handlers
    return {
        OnExampleButton: onExampleButtonClick,
        OnAnotherButton: onAnotherButtonClick
        // Add more handlers as needed
    };

})();
