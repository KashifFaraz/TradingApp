// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const DocumentStatus = {
    Draft: 0,
    Finalized: 1,
    FinalizedPartialReconciled: 2,
    FinalizedReconciled: 3,

    getName: function (value) {
        switch (value) {
            case this.Draft:
                return "Draft";
            case this.Finalized:
                return "Finalized";
            case this.FinalizedPartialReconciled:
                return "Partial Reconciled";
            case this.FinalizedReconciled:
                return "Reconciled";
            default:
                return "Unknown";
        }
    }
};

