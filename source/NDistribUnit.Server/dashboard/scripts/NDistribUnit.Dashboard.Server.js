function DashboardServer() {
    this.urlPrefix = "../";
}

DashboardServer.prototype = {
    getClientStatuses: function (onComplete) {
        this.call('getStatus/client', onComplete);
        
    },
    
    call: function (url, onComplete) {
        var me = this;
        $.ajax({
            url: this.urlPrefix+url,
            dataType: 'json',
            cache: false,
            success: function (data) {
                onComplete.apply(me, [{ data: data, error: false}]);
            },
            error: function (xhr) {
                onComplete.apply(me, [{ data: xhr.response, error: { code: xhr.status, text: xhr.statusText} }]);
            }
        }); 
    }
}