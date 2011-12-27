function DashboardUI() {
    this.options = {
        maxLogItemsCount: 1000
    };
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
            agentsStatusesPane: "#agents-statuses-pane",
            agentsLogPane: "#agents-log-pane",
            agentsLogDisplayArea: "#agents-log-display-area"
        },
        settings: {
            treeview: "#settings-tree-pane>UL",
            treePane: "#settings-tree-pane",
            contentPane: "#settings-content-pane"
        }
    };
    this.agentsStatus = new AgentsDashboardUI(this);
    this.serverStatus = new ServerDashboardUI(this);
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

        $(this.selectors.status.serverStatusPane).layout({
            center: { paneSelector: "#server-status-pane-wrapper" }
        });
        //        $(this.selectors.status.agentsStatusesPane).layout({
        //            center: { paneSelector: "#agents-statuses-pane-wrapper" }
        //        });



        this.panes =
            [
                $(this.selectors.testsPane).hide(),
                $(this.selectors.statusPane),
                $(this.selectors.settingsPane).hide()
            ];
        this.statusPanes = [];
        this.statusPanes.$agentsStatusesPane = $(this.selectors.status.agentsStatusesPane);
        this.statusPanes.push($(this.selectors.status.homePane));
        this.statusPanes.push($(this.selectors.status.serverStatusPane).hide());
        this.statusPanes.push(this.statusPanes.$agentsStatusesPane.hide());

        this.statusPanes.$agentsLogPane = $(this.selectors.status.agentsLogPane);
        this.statusPanes.$agentsLogDisplayArea = $(this.selectors.status.agentsLogDisplayArea);

        this.statusPanes.$agentsStatusesPane.layoutInstance = this.statusPanes.$agentsStatusesPane.layout({
            autoResize: true,
            center: { paneSelector: "#agents-statuses-pane-wrapper", size: '50%' },
            south: { paneSelector: this.selectors.status.agentsLogPane, size: '50%', initHidden: true }
        });
        this.$logTemplate = $("#log-entry-template").template();
        //this.statusPanes.$agentsStatusesPane.layoutInstance.panes.center.show();
        me.agentsStatus.init();
        me.serverStatus.init();

        this.isPaneTransitionInProgress = false;
    },

    showOrCreateAgentLogPane: function (agentName) {
        /// <summary>Displays the log area and shows the log for the <paramref name="agentName" /></summary>
        var layoutInstance = this.statusPanes.$agentsStatusesPane.layoutInstance;
        if (layoutInstance.state.south.isHidden) {
            layoutInstance.show('south');
        }
        this.statusPanes.$agentsLogDisplayArea.children().hide();
        var $currentAgentPane = this.statusPanes.$agentsLogDisplayArea.data(agentName);

        if (!$currentAgentPane) {
            $currentAgentPane = $("<div id='" + agentName + "'></div>");
            this.statusPanes.$agentsLogDisplayArea.append($currentAgentPane);
            this.statusPanes.$agentsLogDisplayArea.data(agentName, $currentAgentPane);
        }
        $currentAgentPane.show();
        var $scrollingContainer = $currentAgentPane.parent();
        $scrollingContainer.scrollTop($scrollingContainer.attr("scrollHeight"));

        this.statusPanes.$agentsStatusesPane.find("#current-agent-name").text(agentName);

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

    openAgentsStatusPane: function () {
        this.openPane(this.statusPanes, this.statusPanes.$agentsStatusesPane, "horizontal");
    },

    openServerStatus: function () {
        this.openPane(this.statusPanes, this.statusPanes[1], "horizontal");
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
    },
    showLogItems: function (data, $element) {
        var currentCount = $element.children().length;

        if (currentCount > this.options.maxLogItemsCount) {
            var toBeDeletedCount = currentCount - this.options.maxLogItemsCount;
            var nodesToRemove = $element.children(":lt(" + toBeDeletedCount + ")");
            nodesToRemove.remove();
        }

        var $nodes = $.tmpl(this.$logTemplate, data.data);
        var $scrollingContainer = $element.parent();
        var shouldScroll = ($scrollingContainer.scrollTop() + $scrollingContainer.height() + 10) >= $scrollingContainer.attr("scrollHeight");

        $nodes.appendTo($element);

        if (shouldScroll) {
            $scrollingContainer.scrollTop($scrollingContainer.attr("scrollHeight"));
        }

        $nodes.css('background-color', '#D3BC9E');
        $nodes.animate({ backgroundColor: '#000000' }, 2000);
    }
};



function AgentsDashboardUI(parentUi) {
    /// <summary>Agents' dashboard-related UI activities</summary>
    this.parentUi = parentUi;
}

AgentsDashboardUI.prototype = {
    init: function () {
        this.$progress = $('#agents-statuses-progress');
        this.$logProgress = $('#agents-log-progress');
        this.$error = $('#agents-statuses-error');
        this.$logError = $('#agents-log-error');
        this.$displayArea = $("#agents-statuses-display-area");
        this.$agentsTemplate = $("#agents-statuses-template").template();
    },
    showUpdateProgress: function () { this.$progress.show(); },
    showLogUpdateProgress: function () { this.$logProgress.show(); },
    hideUpdateProgress: function () { this.$progress.hide(); },
    hideLogUpdateProgress: function () { this.$logProgress.hide(); },
    isVisible: function () {
        return !this.parentUi.isPaneTransitionInProgress
            && this.parentUi.statusPanes.$agentsStatusesPane.is(":visible");
    },

    isLogVisible: function () {
        return !this.parentUi.isPaneTransitionInProgress
            && this.parentUi.statusPanes.$agentsStatusesPane.is(":visible");
    },
    showError: function (errorMessage) {
        this.$error.html(errorMessage);
        this.$error.show();
    },
    hideError: function () {
        this.$error.hide();
    },
    showLogError: function (errorMessage) {
        this.$logError.html(errorMessage);
        this.$logError.show();
    },
    hideLogError: function () {
        this.$logError.hide();
    },
    showAllAsUnknown: function () {
        this.$displayArea.find(".agent").addClass("Unknown");
    },
    displayAgents: function (data) {
        this.$displayArea.empty();
        $.tmpl(this.$agentsTemplate, data)
            .appendTo(this.$displayArea);
    },

    displayLogEntries: function (agentName, data) {
        var $currentAgentPane = this.parentUi.statusPanes.$agentsLogDisplayArea.data(agentName);
        this.parentUi.showLogItems(data, $currentAgentPane);
    }
};

function ServerDashboardUI(parentUi) {
    this.parentUi = parentUi;
}

ServerDashboardUI.prototype = {
    init: function () {
        this.$logProgress = $('#server-log-progress');
        this.$logError = $('#server-log-error');
        this.$logDisplayArea = $("#server-log-display-area");
    },
    showLogUpdateProgress: function () { this.$logProgress.show(); },
    hideLogUpdateProgress: function () { this.$logProgress.hide(); },
    isLogVisible: function () {
        return !this.parentUi.isPaneTransitionInProgress
            && this.$logDisplayArea.is(":visible");
    },
    showLogError: function (errorMessage) {
        this.$logError.html(errorMessage);
        this.$logError.show();
    },
    hideLogError: function () {
        this.$logError.hide();
    },

    displayLogEntries: function (data) {
        this.parentUi.showLogItems(data, this.$logDisplayArea);
    }
};