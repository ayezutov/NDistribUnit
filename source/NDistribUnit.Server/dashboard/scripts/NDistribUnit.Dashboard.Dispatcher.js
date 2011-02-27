// <reference path="jquery.history.js"/>

function DashboardDispatcher(dispatchingObject) {
    /// <summary>Initializes a new instance</summary>
    /// <param name="dispatchingObject">The object, which contains dispatching information</param>
    this.dispatcher = dispatchingObject;
}


DashboardDispatcher.prototype = {

    init: function () {
        /// <summary>Initializes a new instance of dispatcher, by subscribing to hash changes</summary>
        var me = this;
        $.history.init(function (newHash) {
            me.route(newHash);
        });
    },

    route: function (route) {
        /// <summary>Performs routing to the dispatching object provided while Dispatcher creation based on route</summary>
        /// <param name="route">The route for performing routing</param>
        route = this._trimSlashes(route);
        var splittedRoute = route.split("/");
        this._invokeRouting(route, splittedRoute, 0, this.dispatcher, []);
    },
    _trimSlashes: function (s) {
        return s.replace(/^\/*([^\/]*(\/+[^\/]+)*)\/*$/, "$1");
    },
    _invokeRouting: function (route, splittedRoute, startIndex, routingObject, beforeActions) {
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
                    namedObject.apply(me.dispatcher, arguments);
                }
                else {
                    if (!this._invokeRouting(route, splittedRoute, i + 1, namedObject, beforeActions)) {
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

            return true;
        };

        if (typeof (routingObject.unknownAction) == "function") {
            routingObject.unknownAction.apply(me.dispatcher, [route]);
            return true;
        }

        return false;
    },
    _callActionListOnObject: function (obj, functionList) {
        for (var i = 0; i < functionList.length; i++) {
            functionList[i].call(obj);
        }
    }
}