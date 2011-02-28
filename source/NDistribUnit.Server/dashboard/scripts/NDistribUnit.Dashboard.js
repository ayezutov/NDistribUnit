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
    });
    this.ui = new DashboardUI();
}

Dashboard.prototype = {
    init: function () {
        this.ui.init();
        this.dispatcher.init();
    },
    openServerStatus: function () {
        this.ui.openServerStatus();
    },
    openClientStatus: function () {
        this.ui.openClientStatus();
        $.ajax({
            url: 'getStatus/client',
            dataType: 'json',
            success: function () {
                var i = 0;
            },
            error: function () { alert('Error!'); }
        });

    }
}