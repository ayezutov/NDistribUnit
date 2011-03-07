// <reference path="NDistribUnit.Dashboard.Dispatcher.js">

function Dashboard()
{
    var me = this;
    this.dispatcher = new DashboardDispatcher({
        settings: {
            beforeAction: function () { me.ui.openSettingsPane(); },
            "": function () { me.ui.showSettingsInfo(); }
        },
        status:
            {
                beforeAction: function () { me.ui.openStatusPane(); },
                clients: function (clientName) {
                    if (typeof (clientName) == undefined) {
                        
                    }
                    me.openClientStatus(clientName);
                },
                server: function () { me.openServerStatus(); }
            },
        tests: {
            beforeAction: function () { me.ui.openTestsPane(); }
        },
        unknownAction: function () { window.location.hash = "tests"; }
    },
    {autoCompleteRouteToLastUsed: true});
    this.ui = new DashboardUI();
    this.server = new DashboardServer();
}

Dashboard.prototype = {
    init: function () {
        var me = this;
        this.ui.init();
        this.dispatcher.init();

        Helper.executeWhen(
            function () { return me.ui.clientStatus.isVisible(); },
            function (finished) {
                me.ui.clientStatus.showUpdateProgress();

                me.server.getClientStatuses(function (result) {
                    me.ui.clientStatus.hideUpdateProgress();
                    if (result.error) {
                        var message = "Ooops... Something bad happened.";
                        if (result.error.code == 0)
                            message += " The server seems to be unavailable.";
                        else 
                            message += "(" + result.error.code + ": " + result.error.text + ")"
                        me.ui.clientStatus.showError(message);
                        me.ui.clientStatus.showAllAsUnknown();
                    }
                    else {
                        me.ui.clientStatus.hideError();
                        me.ui.clientStatus.displayClients(result);
                    }

                    finished();
                });
            },
            5000);
    },

    openServerStatus: function () {
        this.ui.openServerStatus();
    },
    openClientStatus: function () {
        this.ui.openClientStatus();
    }
}