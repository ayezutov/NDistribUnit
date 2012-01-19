// <reference path="jquery.history.js"/>

function DashboardDispatcher(dispatchingObject, options) {
    /// <summary>Initializes a new instance</summary>
    /// <param name="dispatchingObject">The object, which contains dispatching information</param>
    this.dispatcher = dispatchingObject;
    this.history = { };
    this.options = options ? options : {};
}


DashboardDispatcher.prototype = {

    init: function () {
        /// <summary>Initializes a new instance of dispatcher, by subscribing to hash changes</summary>
        var me = this;
        $.history.init(function (newHash) {
            var route = newHash;
            if (me.options.autoCompleteRouteToLastUsed) {
                route = me.getLatestOrSelf(newHash);

                if (route != newHash) {
                    window.location.hash = route;
                    return;
                }
            }
            me.route(route);
        });
    },

    route: function (route) {
        /// <summary>Performs routing to the dispatching object provided while Dispatcher creation based on route</summary>
        /// <param name="route">The route for performing routing</param>
        route = this._trimSlashes(route);
        var splittedRoute = route.split("/");
        this.dispatcher.currentRoute = route;
        this._invokeRouting(route, splittedRoute, 0, this.dispatcher, [], []);
    },
    _trimSlashes: function (s) {
        return s.replace(/^\/*([^\/]*(\/+[^\/]+)*)\/*$/, "$1");
    },
    _invokeRouting: function (route, splittedRoute, startIndex, routingObject, beforeActions, selectedParts) {
        var me = this;

        for (var i = startIndex; i < splittedRoute.length; i++) {
            if (i == startIndex)
                actionName = splittedRoute[i];
            else
                actionName = actionName + "/" + splittedRoute[i];
            if (routingObject.hasOwnProperty(actionName)) {
                var namedObject = routingObject[actionName];

                if (typeof (routingObject.beforeAction) == "function") {
                    beforeActions.push(routingObject.beforeAction);
                }

                if (typeof (namedObject) == "function") {
                    me._callActionListOnObject(me.dispatcher, beforeActions);

                    var arguments = [];
                    for (var j = i + 1; j < splittedRoute.length; j++) {
                        arguments.push(splittedRoute[j]);
                    }

                    // call the requested action handler
                    namedObject.apply(me.dispatcher, arguments);

                    selectedParts.push(actionName);
                    me._registerHistory(selectedParts, route);
                }
                else {
                    selectedParts.push(actionName);
                    if (!this._invokeRouting(route, splittedRoute, i + 1, namedObject, beforeActions, selectedParts)) {

                        // call the unknown action handler
                        if (typeof (routingObject.unknownAction) == "function") {
                            routingObject.unknownAction.apply(me.dispatcher, [route]);
                            return true;
                        }
                        return false;
                    }
                }
                return true;
            }
        };

        // call function "" if this is an object and there are no more route parts
        if (startIndex == splittedRoute.length || (startIndex == splittedRoute.length - 1 && splittedRoute[startIndex] == "")) {
            if (typeof (routingObject.beforeAction) == "function") {
                beforeActions.push(routingObject.beforeAction);
            }

            me._callActionListOnObject(me.dispatcher, beforeActions);

            if (typeof (routingObject[""]) == "function") {
                routingObject[""].call(me.dispatcher);
            }

            me._registerHistory(selectedParts, route);

            return true;
        };

        if (typeof (routingObject.unknownAction) == "function") {
            routingObject.unknownAction.apply(me.dispatcher, [route]);
            return true;
        }

        return false;
    },

    getLatestOrSelf: function (routePrefix) {
        /// <summary>Gets the latest URL, which started with the given "routePrefix".</summary>
        /// <param name="routePrefix">The prefix, which the URL should start with.</param>

        if (this.history[routePrefix])
            return this.history[routePrefix];

        return routePrefix;
    },

    _callActionListOnObject: function (obj, functionList) {
        for (var i = 0; i < functionList.length; i++) {
            functionList[i].call(obj);
        }
    },

    _registerHistory: function (selectedParts, route) {
        var prefix = "";
        for (var i = 0; i < selectedParts.length; i++) {
            if (prefix != "")
                prefix += "/";
            prefix += selectedParts[i];
            this.history[prefix] = route;
        }
    }
}