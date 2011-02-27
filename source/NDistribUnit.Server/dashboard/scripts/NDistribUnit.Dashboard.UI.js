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
            resultsPane: "#test-results-pane"
        }
    };
    this.elements = { };
}

DashboardUI.prototype = {
    init: function () {
        $("body").layout({
            north: { paneSelector: this.selectors.menuPane, size: 30, resizable: false, spacing_open: 0 },
            center: { paneSelector: this.selectors.contentPane }
        });
        $(this.selectors.tests.treeview).treeview();
        $(this.selectors.testsPane).layout({
            west: { paneSelector: this.selectors.tests.treePane, size: 200, resizable: true, slideable: false, minSize: 200, maxSize: 0, spacing_open: 6 },
            center: { paneSelector: this.selectors.tests.resultsPane }
        });
        this.panes =
            [
                $(this.selectors.testsPane),
                $(this.selectors.statusPane),
                $(this.selectors.settingsPane)
            ];
        this.panes.tests = this.panes[0];
        this.panes.status = this.panes[1];
        this.panes.settings = this.panes[2];
    },

    openSettingsPane: function () {
        this.openPane(this.panes, this.panes.settings, "horizontal");
    },

    openStatusPane: function () {
        this.openPane(this.panes, this.panes.status, "horizontal");
    },

    openTestsPane: function () {
        this.openPane(this.panes, this.panes.tests, "horizontal");
    },

    showSettingsInfo: function ()
    { },

    openPane: function (panes, paneToOpen, effect) {
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
                    if (counter >= newIndex)
                        return;
                    panes[counter].hide("slide", { direction: previousEffectDirection }, 500);
                    panes[counter + 1].show("slide", { direction: nextEffectDirection }, 500, function () {
                        animateForward(counter + 1);
                    });
                };
                animateForward(currentIndex);
            }
            else if (currentIndex > newIndex) {
                var animateBackwards = function (counter) {
                    if (counter <= newIndex)
                        return;
                    panes[counter].hide("slide", { direction: nextEffectDirection }, 500);
                    panes[counter - 1].show("slide", { direction: previousEffectDirection }, 500, function () {
                        animateBackwards(counter - 1);
                    });
                };
                animateBackwards(currentIndex);
            }

        }
    }
}