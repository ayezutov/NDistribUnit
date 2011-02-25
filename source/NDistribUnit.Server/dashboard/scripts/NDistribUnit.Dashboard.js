// <reference path="NDistribUnit.Dashboard.Dispatcher.js">

function Dashboard()
{
    var me = this;
    this.dispatcher = new DashboardDispatcher({
        "": function () { me.dashboard.openTestResults(); },
        settings: function () { me.dashboard.openSettings(); },
        status:
            {
                client: function (clientName) {
                    me.dashboard.openClientStatus(clientName);
                },
                server: function () { me.dashboard.openServerStatus(); }
            },
        unknownAction: function () { window.location.hash = ""; }
        });
}

Dashboard.prototype = {
    init: function () {
        // init ui: move to UI
        $("body").layout({
            north: { paneSelector: "#top-pane", size: 30, resizable: false, spacing_open: 0 },
            center: { paneSelector: "#main-pane" }
            //,west: { paneSelector: "#tests-tree-pane", size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 }
        });
        $("#tests-tree-pane>UL").treeview();

        $("#tests-content").layout({
            center: { paneSelector: "#test-results-pane" },
            west: { paneSelector: "#tests-tree-pane", size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 }
        });
        this.dispatcher.init();
    }
}