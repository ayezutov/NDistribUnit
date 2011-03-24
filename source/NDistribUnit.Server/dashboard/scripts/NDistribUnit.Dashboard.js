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
        this.ui.init();
        this.dispatcher.init();
        this.initializeAgentsStatusesUpdate();
        this.initializeServerLogUpdate();
        this.lastFetchedServerLogId = null;
        this.maxFetchedServerLogCount = 1000;
    },

    openServerStatus: function () {
        this.ui.openServerStatus();
    },
    openClientStatus: function () {
        this.ui.openClientStatus();
    },
    initializeAgentsStatusesUpdate: function () {
        var me = this;
        Helper.executeWhen(
            function () {
                return me.ui.agentsStatus.isVisible();
            },
            function (finished) {
                me.ui.agentsStatus.showUpdateProgress();

                me.server.getClientStatuses(function (result) {
                    me.ui.agentsStatus.hideUpdateProgress();
                    if (result.error) {
                        var message = "Ooops... Something bad happened.";
                        if (result.error.code == 0 || result.error.code == 12029)
                            message += " The server seems to be unavailable.";
                        else
                            message += "(" + result.error.code + ": " + result.error.text + ")";
                        me.ui.agentsStatus.showError(message);
                        me.ui.agentsStatus.showAllAsUnknown();
                    }
                    else {
                        me.ui.agentsStatus.hideError();
                        me.ui.agentsStatus.displayAgents(result);
                    }

                    finished();
                });
            },
            5000);
    },
    initializeServerLogUpdate: function () {
        var me = this;
        Helper.executeWhen(
            function () { return me.ui.serverStatus.isLogVisible(); },
            function fetchLog(finished) {
                me.ui.serverStatus.showLogUpdateProgress();

                me.server.getServerLog(me.lastFetchedServerLogId, me.maxFetchedServerLogCount, function (result) {
                    me.ui.serverStatus.hideLogUpdateProgress();
                    if (result.error) {
                        var message = "Ooops... Something bad happened.";
                        if (result.error.code == 0 || result.error.code == 12029)
                            message += " The server seems to be unavailable.";
                        else
                            message += "(" + result.error.code + ": " + result.error.text + ")";
                        me.ui.serverStatus.showLogError(message);
                    }
                    else {
                        if (result.data.length > 0) {
                            me.lastFetchedServerLogId = result.data[result.data.length - 1].Id;
                            me.ui.serverStatus.hideLogError();
                            me.ui.serverStatus.displayLogEntries(result);
                        }

                        if (result.data.length >= me.maxFetchedServerLogCount) {
                            setTimeout(function () { fetchLog(finished); }, 50);
                            return;
                        }
                    }

                    finished();
                });
            },
            5000);
    }
}