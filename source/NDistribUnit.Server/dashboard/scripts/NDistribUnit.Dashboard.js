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
                client: function (clientName) {
                    me.dashboard.openClientStatus(clientName);
                },
                server: function () { me.dashboard.openServerStatus(); }
            },
        tests: {
            beforeAction: function () { me.ui.openTestsPane(); }
        },
        unknownAction: function () { window.location.hash = "tests"; }
    });
    this.ui = new DashboardUI();
}

Dashboard.prototype = {
    init: function () {
        this.ui.init();
        this.dispatcher.init();
    },
    openServerStatus: function () {

    },
    openClientStatus: function () {

    }
}