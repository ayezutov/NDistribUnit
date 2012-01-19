function DashboardLog($div, $logTemplate, options) {
    this.$div = $div;
    this.$logDiv = $div;
    this.options = options;
    this.$logTemplate = $logTemplate;
}

DashboardLog.prototype = {
    addItem: function () {
        var me = this;
        var currentCount = me.$logDiv.children().length;

        if (currentCount > me.options.maxLogItemsCount) {
            var toBeDeletedCount = currentCount - me.options.maxLogItemsCount;

            var nodesToRemove = me.$logDiv.children(":lt(" + toBeDeletedCount + ")");
            nodesToRemove.remove();
        }

        var $nodes = $.tmpl(me.$logTemplate, data.data);
        var $scrollingContainer = me.$logDiv.parent();
        var shouldScroll = ($scrollingContainer.scrollTop() + $scrollingContainer.height() + 10) >= $scrollingContainer.attr("scrollHeight");

        $nodes.appendTo(me.$logDiv);

        if (shouldScroll) {
            $scrollingContainer.scrollTop($scrollingContainer.attr("scrollHeight"));
        }

        $nodes.css('background-color', '#D3BC9E');
        $nodes.animate({ backgroundColor: '#000000' }, 2000);
    }
};