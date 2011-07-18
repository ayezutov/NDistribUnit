// <reference path="NDistribUnit.Dashboard.Dispatcher.js">

function Dashboard()
{
    var me = this;
    this.dispatcher = new DashboardDispatcher({
        settings: {
            beforeAction: function () { me.ui.openSettingsPane(); },
            "": function () { me.ui.showSettingsInfo(); },
            updates: function () {
                me.ui.showUpdatesScreen();
            }
        },
        status:
            {
                beforeAction: function () { me.ui.openStatusPane(); },
                agents: function (agentName) {
                    me.openAgentsStatus(agentName);
                },
                server: {
                    log: function () {
                        me.openServerStatus();
                    }
                }
            },
        tests: {
            beforeAction: function () { me.ui.openTestsPane(); }
        },
        unknownAction: function () { window.location.hash = "tests"; }
    },
    { autoCompleteRouteToLastUsed: true });
    this.ui = new DashboardUI();
    this.server = new DashboardServer();
}

Dashboard.prototype = {
    init: function () {
        this.ui.init();
        this.dispatcher.init();

        this.__lastFetchedServerLogId = null;
        this.__lastFetchedAgentLogId = {};
        this.__maxFetchedServerLogCount = this.__maxFetchedAgentsLogCount = 1000;

        this.initializeAgentsStatusesUpdate();
        this.initializeAgentsLogUpdate();
        this.initializeServerLogUpdate();

    },

    openServerStatus: function () {
        this.ui.openServerStatus();
    },
    openAgentsStatus: function (agentName) {
        this.ui.openAgentsStatusPane();
        if (typeof (agentName) != 'undefined' && agentName != "") {
            this.__logShowingAgentName = agentName;
            this.ui.showOrCreateAgentLogPane(agentName);
        }
    },
    showUpdatesScreen: function () {

    },
    getMessageFromError: function (error) {
        var message = "Ooops... Something bad happened.";
        if (error.code == 0 || error.code == 12029)
            message += " The server seems to be unavailable.";
        else
            message += "(" + error.code + ": " + error.text + ")";
        return message;
    },
    initializeAgentsStatusesUpdate: function () {
        var me = this;
        Helper.executeWhen(
            function () {
                return me.ui.agentsStatus.isVisible();
            },
            function (finished) {
                me.ui.agentsStatus.showUpdateProgress();

                me.server.getAgentsStatuses(function (result) {
                    me.ui.agentsStatus.hideUpdateProgress();
                    if (result.error) {
                        var message = me.getMessageFromError(result.error);
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
    initializeAgentsLogUpdate: function () {
        var me = this;
        Helper.executeWhen(
            function () { return me.ui.agentsStatus.isLogVisible(); },
            function fetchLog(finished) {

                var currentAgentName = me.__logShowingAgentName;

                me.ui.agentsStatus.showLogUpdateProgress();

                me.server.getAgentsLog(currentAgentName, me.__lastFetchedAgentLogId[currentAgentName], me.__maxFetchedAgentsLogCount, function (result) {
                    me.ui.agentsStatus.hideLogUpdateProgress();
                    if (result.error) {
                        var message = me.getMessageFromError(result.error);
                        me.ui.agentsStatus.showLogError(message);
                    }
                    else {
                        if (result.data.length > 0) {
                            me.__lastFetchedAgentLogId[currentAgentName] = result.data[result.data.length - 1].Id;
                            me.ui.agentsStatus.hideLogError();
                            me.ui.agentsStatus.displayLogEntries(currentAgentName, result);
                        }

                        if (result.data.length >= me.__maxFetchedServerLogCount) {
                            setTimeout(function () { fetchLog(finished); }, 50);
                            return;
                        }
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

                me.server.getServerLog(me.__lastFetchedServerLogId, me.__maxFetchedServerLogCount, function (result) {
                    me.ui.serverStatus.hideLogUpdateProgress();
                    if (result.error) {
                        var message = me.getMessageFromError(result.error);
                        me.ui.serverStatus.showLogError(message);
                    }
                    else {
                        if (result.data.length > 0) {
                            me.__lastFetchedServerLogId = result.data[result.data.length - 1].Id;
                            me.ui.serverStatus.hideLogError();
                            me.ui.serverStatus.displayLogEntries(result);
                        }

                        if (result.data.length >= me.__maxFetchedServerLogCount) {
                            setTimeout(function () { fetchLog(finished); }, 50);
                            return;
                        }
                    }

                    finished();
                });
            },
            5000);
    },
    initializeUpdatesListUpdate: function () {
        var me = this;
        Helper.executeWhen(
            function () { return me.ui.updateSettings.isVisible(); },
            function fetchLog(finished) {
                me.ui.updateSettings.showUpdateProgress();

                me.server.getAvailableUpdates(function (result) {
                    me.ui.updateSettings.hideUpdateProgress();
                    if (result.error) {
                        var message = me.getMessageFromError(result.error);
                        me.ui.updateSettings.showLogError(message);
                    }
                    else {
                        if (result.data.length > 0) {
                            me.ui.updateSettings.hideError();
                            me.ui.updateSettings.display(result);
                        }
                    }

                    finished();
                });
            },
            5000);
    }
}