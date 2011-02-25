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

    testSimpleRouteInvokesBeforeAction: function () {
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
    }
    
});