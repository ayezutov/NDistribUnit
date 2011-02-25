// jquery.history plugin is required

function DashboardDispatcher(dashboard) {
    this.dashboard = dashboard;
    this.init();
    var me = this;
    this.dispatcher = {
        "": function () { me.dashboard.openTestResults(); },
        "settings": function () { me.dashboard.openSettings(); },
        "status": function () { me.dashboard.openStatus(); }
    }
}

DashboardDispatcher.prototype = {
    init: function () {
        var me = this;
        $.history.init(function (newHash) {
            var splittedHash = newHash.split("/");

        });
    }
}