TestCase("Dashboard.Dispatcher.Tests", {
    testSimpleRoute: function () {
        var executed = false;
        var dispatcher = new DashboardDispatcher({
            "test me": function () { },
            "test": function () { executed = true; },
            "test you": function () { }
        });
        dispatcher.route("test");
        assertTrue(executed);
    },

    testSimpleRouteReceivesParameter: function () {
        var value = false;
        var dispatcher = new DashboardDispatcher({
            "test me": function () { },
            "test": function (par) { value = par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1");
        assertEquals("parameter1", value);
    },

    testSimpleRouteReceivesMultipleParameters: function () {
        var value = false;
        var dispatcher = new DashboardDispatcher({
            "test me": function () { },
            "test": function (par, par2) { value = par2 + par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1/parameter2");
        assertEquals("parameter2parameter1", value);
    },

    testBeforeActionIsInvoked: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            beforeAction: function () { value += "before"; },
            "test me": function () { },
            test: function (par, par2) { value += par2 + par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1/parameter2");
        assertEquals("beforeparameter2parameter1", value);
    },

    testBeforeActionIsInvokedOnIntermediateObject: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            beforeAction: function () { value += "before1 "; },
            "test": {
                beforeAction: function () { value += "before2 "; },
                test2: {
                    beforeAction: function () { value += "before3 "; },
                    test3: function (par, par2) { value += par2 + par; }
                }
            },
            "test/test2/test3": function () { value += "full route"; },
            unknownAction: function (route) { value += "unknown: " + route; }
        });
        dispatcher.route("test/test2/test3/parameter1/parameter2");
        assertEquals("before1 before2 before3 parameter2parameter1", value);
    },

    testBeforeActionIsInvokedOnIntermediateAndTargetObjectsEvenIfThereIsExactMatch: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            beforeAction: function () { value += "before1 "; },
            "test": {
                beforeAction: function () { value += "before2 "; },
                test2: {
                    beforeAction: function () { value += "before3 "; },
                    test3: function (par, par2) { value += par2 + par; }
                }
            },
            "test/test2/test3": function () { value += "full route"; },
            unknownAction: function (route) { value += "unknown: " + route; }
        });
        dispatcher.route("test/test2");
        assertEquals("before1 before2 before3 ", value);
    },
        
    testBeforeActionIsNotInvokedAtAllIfUnknownAction: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            beforeAction: function () { value += "before1 "; },
            "test": {
                beforeAction: function () { value += "before2 "; },
                test2: {
                    beforeAction: function () { value += "before3 "; },
                    test3: function (par, par2) { value += par2 + par; }
                }
            },
            "test/test2/test3": function () { value += "full route"; },
            unknownAction: function (route) { value += "unknown: " + route; }
        });
        dispatcher.route("test/test2/test4/parameter1/parameter2");
        assertEquals("unknown: test/test2/test4/parameter1/parameter2", value);
    },

    testComplexRoute: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test/parameter1": function (par) { value = par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1/parameter2");
        assertEquals("parameter2", value);
    },

    testComplexRouteReceivesMultipleParameters: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test/parameter1": function (par, par2) { value = par2 + par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1/parameter2/parameter3");
        assertEquals("parameter3parameter2", value);
    },

    testSimpleRouteHasMorePrioirityThanComplex: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": function (par, par2) { value = par2 + par; },
            "test/parameter1": function (par, par2) { value = par2 + par; },
            "test you": function () { }
        });
        dispatcher.route("test/parameter1/parameter2/parameter3");
        assertEquals("parameter2parameter1", value);
    },

    testUnknownActionIsInvokedIfNotFound: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": function (par, par2) { value = par2 + par; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test2/parameter1");
        assertEquals("unknown: test2/parameter1", value);
    },

    testRoutingToInnerObjectsIsPerformed: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { value = par2 + par; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test/test2/test3/parameter1/parameter2");
        assertEquals("parameter2parameter1", value);
    },

    testUnknownActionIsInvokedOnTheTargetObject: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { value = par2 + par; },
                    unknownAction: function (route) { value = "unknown on test2: " + route; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test/test2/test2/parameter1/parameter2");
        assertEquals("unknown on test2: test/test2/test2/parameter1/parameter2", value);
    },

    testUnknownActionIsInvokedOnIntermediateObjectIfNotInvokedOnInner: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { value = par2 + par; }
                },
                unknownAction: function (route) { value = "unknown: " + route; }
            },
            "test/test2/test3": function () { value = "full route"; }
        });
        dispatcher.route("test/test2/test2/parameter1/parameter2");
        assertEquals("unknown: test/test2/test2/parameter1/parameter2", value);
    },

    testUnknownActionIsInvokedOnMostOuterObjectIfNotInvokedOnInner: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { value = par2 + par; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test/test2/test2/parameter1/parameter2");
        assertEquals("unknown: test/test2/test2/parameter1/parameter2", value);
    },

    testEmptyActionShouldBeInvokedIfExactMatchOnIntermediateObject: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    "": function () { value = "default for this"; },
                    test3: function (par, par2) { value = par2 + par; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test/test2");
        assertEquals("default for this", value);
    },

    testEmptyActionShouldBeInvokedIfExactMatchWithSlashOnIntermediateObject: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    "": function () { value = "default for this"; },
                    test3: function (par, par2) { value = par2 + par; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("test/test2/");
        assertEquals("default for this", value);
    },

    testSlashesAreNotImportantForRouting: function () {
        var value = "";
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { value = par2 + par; }
                }
            },
            "test/test2/test3": function () { value = "full route"; },
            unknownAction: function (route) { value = "unknown: " + route; }
        });
        dispatcher.route("/test/test2/test3/parameter1/parameter2/");
        assertEquals("parameter2parameter1", value);
    },
        
    testHistoryWorksOkForStepByStepCalls: function () {
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { }
                }
            },
            "test/test2/test3": function () { },
            unknownAction: function (route) { }
        });
        dispatcher.route("test");
        assertEquals("test", dispatcher.getLatestOrSelf("test"));
        assertEquals("test/test2", dispatcher.getLatestOrSelf("test/test2"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2/test3"));
    },
        
    testHistoryWorksOkForStepByStepCalls2: function () {
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { }
                }
            },
            "test/test2/test3": function () { },
            unknownAction: function (route) { }
        });
        
        dispatcher.route("test");
        dispatcher.route("test/test2");
        assertEquals("test/test2", dispatcher.getLatestOrSelf("test"));
        assertEquals("test/test2", dispatcher.getLatestOrSelf("test/test2"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2/test3"));
        
    },
        
    testHistoryWorksOkForStepByStepCalls3: function () {
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { }
                }
            },
            "test/test2/test3": function () { },
            unknownAction: function (route) { }
        });
        dispatcher.route("test");
        dispatcher.route("test/test2");
        dispatcher.route("test/test2/test3");
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2/test3"));
    },
        
    testHistoryWorksOkForStepByStepCalls4: function () {
        var dispatcher = new DashboardDispatcher({
            "test": {
                test2: {
                    test3: function (par, par2) { }
                }
            },
            "test/test2/test3": function () { },
            unknownAction: function (route) { }
        });
        dispatcher.route("test");
        dispatcher.route("test/test2");
        dispatcher.route("test/test2/test3");
        dispatcher.route("test/test2/test4");
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2"));
        assertEquals("test/test2/test3", dispatcher.getLatestOrSelf("test/test2/test3"));
        assertEquals("test/test2/test4", dispatcher.getLatestOrSelf("test/test2/test4"));
        
    }

    });