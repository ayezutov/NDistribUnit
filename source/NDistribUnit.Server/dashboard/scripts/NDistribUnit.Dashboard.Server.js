function DashboardServer() {
    this.urlPrefix = "../";
}

DashboardServer.prototype = {
    getAgentsStatuses: function (onComplete) {
        this.call('agent/getStatus', onComplete);
    },
    getServerLog: function (lastFetchedEntryId, maxItemsCount, onComplete) {
        this.call('server/getLog', onComplete,
            { method: "POST", data: { maxItemsCount: maxItemsCount, lastFetchedEntryId: lastFetchedEntryId }, resultContainer: "GetServerLogResult" });
    },

    getAgentsLog: function (agentName, lastFetchedEntryId, maxItemsCount, onComplete) {
        this.call('agent/getLog', onComplete,
            { method: "POST", data: { agentName: agentName, maxItemsCount: maxItemsCount, lastFetchedEntryId: !!lastFetchedEntryId ? lastFetchedEntryId : null },
                resultContainer: "GetAgentLogResult"
            });
    },

    call: function (url, onComplete, options) {
        var mergedOptions = {
            method: "GET",
            data: null,
            resultContainer: null
        };

        $().extend(mergedOptions, options);
        var me = this;
        $.ajax({
            url: this.urlPrefix + url,
            dataType: 'json',
            cache: false,
            type: mergedOptions.method,
            data: JSON.stringify(mergedOptions.data),
            processData: true,
            contentType: "application/json; charset=utf-8",
            dataFilter: function (data, type) {
                return JSON.parse(data, function (key, value) {
                    var regex = /\/Date\((\d+)\)\//gi;
                    if (typeof (value) == "string" && value.match(regex))
                        return new Date(parseInt(value.substr(6)));
                    return value;
                });
            },
            success: function (data) {
                if (mergedOptions.resultContainer != null && data.hasOwnProperty(mergedOptions.resultContainer)) {
                    data = data[mergedOptions.resultContainer];
                }
                onComplete.apply(me, [{ data: data, error: false}]);
            },
            error: function (xhr) {
                onComplete.apply(me, [{ data: xhr.response, error: { code: xhr.status, text: xhr.statusText}}]);
            }
        });
    }
}