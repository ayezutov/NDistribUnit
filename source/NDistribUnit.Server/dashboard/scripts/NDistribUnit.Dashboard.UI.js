function DashboardUI() {
    this.selectors = {
        menuPane: "#menu-pane",
        contentPane: "#content-pane",
        testsPane: "#tests-pane",
        statusPane: "#status-pane",
        settingsPane: "#settings-pane",
        tests: {
            treeview: "#tests-tree-pane>UL", 
            treePane: "#tests-tree-pane",
            contentPane: "#test-content-pane"
        },
        status: {
            treeview: "#status-tree-pane>UL",
            treePane: "#status-tree-pane",
            contentPane: "#status-content-pane",
            homePane: "#status-home-page",
            serverStatusPane: "#server-status-pane",
            clientsStatusesPane: "#clients-statuses-pane"
        },
        settings: {
            treeview: "#settings-tree-pane>UL",
            treePane: "#settings-tree-pane",
            contentPane: "#settings-content-pane"
        }
    };
    this.elements = { };
}

DashboardUI.prototype = {
    init: function () {
        /// <summary>Initializes the UI of the application</summary>

        var me = this;

        $("body").layout({
            north: { paneSelector: this.selectors.menuPane, size: 30, resizable: false, spacing_open: 0 },
            center: { paneSelector: this.selectors.contentPane }
        });
        $(this.selectors.tests.treeview).treeview();
        $(this.selectors.testsPane).layout({
            west: { paneSelector: this.selectors.tests.treePane, size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 },
            center: { paneSelector: this.selectors.tests.contentPane }
        });

        $(this.selectors.statusPane).layout({
            west: { paneSelector: this.selectors.status.treePane, size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 },
            center: { paneSelector: this.selectors.status.contentPane }
        });
        $(this.selectors.settingsPane).layout({
            west: { paneSelector: this.selectors.settings.treePane, size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 },
            center: { paneSelector: this.selectors.settings.contentPane }
        });
        this.panes =
            [
                $(this.selectors.testsPane),
                $(this.selectors.statusPane).hide(),
                $(this.selectors.settingsPane).hide()
            ];
        this.statusPanes =
            [
                $(this.selectors.status.homePane),
                $(this.selectors.status.serverStatusPane).hide(),
                $(this.selectors.status.clientsStatusesPane).hide()
            ];

        this.statusPanes.clientsStatusesPane = this.statusPanes[2];
        $("#clients-statuses-display-area").setTemplateElement("clients-statuses-template");

        this.isPaneTransitionInProgress = false;

        this.clientStatus = new ClientsDashboardUI(this);
    },

    openSettingsPane: function () {
        this.openPane(this.panes, this.panes[2], "horizontal");
    },

    openStatusPane: function () {
        this.openPane(this.panes, this.panes[1], "horizontal");
    },

    openTestsPane: function () {
        this.openPane(this.panes, this.panes[0], "horizontal");
    },

    openClientStatus: function () {
        this.openPane(this.statusPanes, this.statusPanes.clientsStatusesPane, "vertical");
    },

    openServerStatus: function () {
        this.openPane(this.statusPanes, this.statusPanes[1], "vertical");
    },

    showSettingsInfo: function ()
    { },

    openPane: function (panes, paneToOpen, effect) {
        /// <summary>Opens a panel with a sliding effect, moving the intermediate panels on higher speed.</summary>

        var me = this;
        me.isPaneTransitionInProgress = true;
        var previousEffectDirection = effect == "horizontal" ? "left" : "up";
        var nextEffectDirection = effect == "horizontal" ? "right" : "down";
        var currentIndex = -1;
        var newIndex = -1;
        for (var i = 0; i < panes.length; i++) {
            if (panes[i].is(":visible"))
                currentIndex = i;
            if (panes[i] == paneToOpen)
                newIndex = i;
        }

        if (currentIndex == -1 && newIndex == -1) {
            me.isPaneTransitionInProgress = false;
            return;
        }
        else if (currentIndex != -1 && newIndex == -1) {
            panes[currentIndex].hide();
            paneToOpen.show();
        }
        else if (currentIndex == -1 && newIndex != -1) {
            panes[newIndex].show("slide", { direction: nextEffectDirection }, 500);
        }
        else {
            if (currentIndex < newIndex) {
                var animateForward = function (counter) {
                    if (counter >= newIndex) {
                        me.isPaneTransitionInProgress = false;
                        return;
                    }
                    var animationSpeed = counter == newIndex - 1 ? 800 : 200;
                    panes[counter].hide("slide", { direction: previousEffectDirection }, animationSpeed);
                    panes[counter + 1].show("slide", { direction: nextEffectDirection }, animationSpeed, function () {
                        animateForward(counter + 1);
                    });
                    $(window).resize();
                };
                animateForward(currentIndex);
            }
            else if (currentIndex > newIndex) {
                var animateBackwards = function (counter) {
                    if (counter <= newIndex) {
                        me.isPaneTransitionInProgress = false;
                        return;
                    }
                    var animationSpeed = counter == newIndex + 1 ? 800 : 200;
                    panes[counter].hide("slide", { direction: nextEffectDirection }, animationSpeed);
                    panes[counter - 1].show("slide", { direction: previousEffectDirection }, animationSpeed, function () {
                        animateBackwards(counter - 1);
                    });
                    $(window).resize();
                };
                animateBackwards(currentIndex);
            }
            else {
                me.isPaneTransitionInProgress = false;
            }
        }
    }
};

function ClientsDashboardUI(parentUi) {
    this.parentUi = parentUi;
    this.$progress = $('#client-status-progress');
    this.$error = $('#client-status-error');
    this.$displayArea = $("#clients-statuses-display-area");
}

ClientsDashboardUI.prototype = {
    showUpdateProgress: function () { this.$progress.show(); },
    hideUpdateProgress: function () { this.$progress.hide(); },
    isVisible: function () {
        return !this.parentUi.isPaneTransitionInProgress
            && this.parentUi.statusPanes.clientsStatusesPane.is(":visible");
    },
    showError: function (errorMessage) {
        this.$error.html(errorMessage);
        this.$error.show();
    },
    hideError: function () {
        this.$error.hide();
    },
    showAllAsUnknown: function () {
        this.$displayArea.find(".agent").addClass("Unknown");
    },
    displayClients: function (data) {
        this.$displayArea.processTemplate(data);
    }
};