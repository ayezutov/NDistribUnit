(function ($, window, undefined) {

    function Layouter(element, isChild) {
        this.$element = $(element);
        this.isChild = (!!isChild);
    };

    $.extend(Layouter, {
        layouterInstance: "layouter.instance",
        layoutPositionAttribute: "layout-position",
        layoutViewAttribute: "layout-view",
        layouterAnimationTarget: "layouter.animation.target"
    });

    Layouter.prototype = {
        initialize: function () {
            if (this.$element.length != 1)
                return;

            if (this.$element.data(Layouter.layouterInstance))
                return;

            var counts = this.counts = {
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                fill: 0,
                views: 0,
                noAttribute: 0,
                positioned: 0
            };
            var $children = this.$element.children();
            var me = this;
            $children.each(function (index, childElement) {

                var $child = $(childElement);

                var layoutPosition = $child.data(Layouter.layoutPositionAttribute);
                if (layoutPosition) {
                    counts.positioned++;
                    counts[layoutPosition] = counts.hasOwnProperty(layoutPosition) ? counts[layoutPosition] + 1 : 1;
                } else {

                    if ($child.data(Layouter.layoutViewAttribute)) {
                        counts["views"] = counts.hasOwnProperty("views") ? counts["views"] + 1 : 1;
                    } else {
                        counts.noAttribute++;
                    }
                }
            });

            if (counts.positioned + counts.views == 0)
                return;

            if (counts.fill + counts.noAttribute > 1)
                return;

            if (counts.views > 0 && counts.positioned > 0 ||
                    counts.views > 0 && counts.noAttribute > 0)
                return;
            $children.each(function (index, childElement) {
                var $child = $(childElement);
                $child.css("margin-right", "0");
                $child.css("margin-left", "0");
                $child.css("margin-top", "0");
                $child.css("margin-bottom", "0");

                new Layouter(childElement, true).initialize();
            });
            this.$element.data(Layouter.layouterInstance, this);


            if (!this.isChild) {
                this.repositionElements();

                $(window).resize(function () {
                    if (window.layoutRepositioningTimeout) {
                        clearTimeout(window.layoutRepositioningTimeout);
                        window.layoutRepositioningTimeout = null;
                    }
                    window.layoutRepositioningTimeout = setTimeout(function () {
                        if (window.layoutRepositioningTimeout) {
                            clearTimeout(window.layoutRepositioningTimeout);
                            window.layoutRepositioningTimeout = null;
                        }
                        me.repositionElements();
                    }, 30);
                });
            }
        },

        setHeight: function ($element, value) {
            $element.height(value - (parseFloat($element.css("border-top-width")) || 0) - (parseFloat($element.css("border-bottom-width")) || 0)
            - (parseFloat($element.css("padding-top")) || 0) - (parseFloat($element.css("padding-bottom")) || 0));
        },

        setWidth: function ($element, value) {
            $element.width(value - (parseFloat($element.css("border-left-width")) || 0) - (parseFloat($element.css("border-right-width")) || 0)
            - (parseFloat($element.css("padding-left")) || 0) - (parseFloat($element.css("padding-right")) || 0));
        },
        setOffset: function ($element, top, left) {
            var op = $element.offsetParent();
            var opo = op.offset();
            $element.offset({ top: top + opo.top + (parseFloat(op.css("border-top-width")) || 0) + (parseFloat(op.css("padding-top")) || 0),
                left: left + opo.left + (parseFloat(op.css("border-left-width")) || 0) + (parseFloat(op.css("padding-left") || 0))
            });
        },
        repositionElements: function () {
            var $children = this.$element.children();
            $children.each(function (childIndex, childElement) {
                var $child = $(childElement);
                $child.css("position", "absolute");
            });

            var me = this;
            var totalHeight = this.$element.height();
            var totalWidth = this.$element.width();

            var rect = { top: 0, left: 0, right: totalWidth, bottom: totalHeight };

            if (!this.isChild) {
                this.$element.css("position", "relative");
            }
            this.$element.css("overflow", "hidden");
            var fills = [];

            if (me.counts.views > 0) {
                var $visible = $children.filter(":visible");

                if ($visible.length > 1) {
                    $visible.each(function (i, e) {
                        if (i > 0)
                            $(e).hide();
                    });
                }
            }

            $children.each(function (childIndex, childElement) {
                var $child = $(childElement);
                $child.css("overflow", "auto");

                if (me.counts.views) {
                    me.setOffset($child, 0, 0);
                    me.setWidth($child, totalWidth);
                    me.setHeight($child, totalHeight);
                } else {
                    var position = $child.data(Layouter.layoutPositionAttribute);

                    if (!position)
                        position = "fill";

                    switch (position) {
                        case "top":
                            {
                                me.setOffset($child, rect.top, rect.left);
                                me.setWidth($child, rect.right - rect.left);
                                rect.top += $child.outerHeight(true);
                                break;
                            }
                        case "left":
                            {
                                me.setOffset($child, rect.top, rect.left);
                                me.setWidth($child, (rect.right - rect.left) * 0.25);
                                me.setHeight($child, rect.bottom - rect.top);

                                rect.left += $child.outerWidth(true);
                                break;
                            }
                        case "right":
                            {
                                me.setHeight($child, rect.bottom - rect.top);

                                var width = (rect.right - rect.left) * 0.25;
                                me.setWidth($child, width);
                                var effectiveWidth = $child.outerWidth(true);

                                me.setOffset($child, rect.top, rect.right - effectiveWidth);
                                rect.right -= effectiveWidth;
                                break;
                            }
                        case "bottom":
                            {
                                me.setWidth($child, rect.right - rect.left);
                                var height = $child.outerHeight(true);
                                me.setOffset($child, rect.bottom - height, rect.left);
                                rect.bottom -= height;
                                break;
                            }
                        case "fill":
                            {
                                fills.push($child);
                                break;
                            }
                        default:
                            {
                            }
                    }
                }
            });

            $.each(fills, function (i, $child) {
                me.setOffset($child, rect.top, rect.left);
                me.setWidth($child, rect.right - rect.left);
                me.setHeight($child, rect.bottom - rect.top);
                rect.top += $child.outerHeight(true);
                rect.left += $child.outerWidth(true);
            });

            $children.each(function (childIndex, childElement) {
                var $child = $(childElement);
                var childLayouter = $child.data(Layouter.layouterInstance);
                if (childLayouter) {
                    childLayouter.repositionElements();
                }
            });
        },

        showView: function (pathArray) {
            if (!$.isArray(pathArray))
                return;

            if (pathArray.length == 0)
                return;

            var $view = $("*[data-" + Layouter.layoutViewAttribute + "='" + pathArray[0] + "']");

            if ($view.length == 0)
                return;

            this.__makeVisible($view, pathArray[0]);

            var furtherPathArray = $.grep(pathArray, function (e, i) { return i > 0; });
            if (furtherPathArray.length == 0)
                return;

            var layouter = $view.data(Layouter.layouterInstance);
            if (layouter) {
                layouter.showView(furtherPathArray);
            }

        },

        __makeVisible: function ($view, viewName) {
            var $parent = $view.parent();
            
            var target = $parent.data(Layouter.layouterAnimationTarget);
            if (target && target === viewName) {
                return;
            }

            $parent.children(":animated").stop(true, true);
            $parent.data(Layouter.layouterAnimationTarget, viewName);

            if ($view.is(":visible"))
                return;
            var $visibleView = $view.parent().children(":visible");
            var visibleIndex = $visibleView.index();
            var index = $view.index();
            var animation = $parent.data("layout-view-animation") || "none";

            var animationDescription = {};
            var animationOptions = { duration: "slow" };
            var animationInitialOffset = { top: 0, left: 0 };

            var parentDimensions = { width: $parent.width(), height: $parent.height() };

            var animationSign = index > visibleIndex ? "-" : "+";

            switch (animation) {
                case "vertical":
                    {
                        animationDescription = { top: animationSign + "=" + parentDimensions.height };
                        animationInitialOffset.top = animationSign == "-" ? parentDimensions.height : -parentDimensions.height;
                    }
                    break;

                case "horizontal":
                    {
                        animationDescription = { left: animationSign + "=" + parentDimensions.width };
                        animationInitialOffset.left = animationSign == "-" ? parentDimensions.width : -parentDimensions.width;
                    }
                    break;

                default:
                    animationOptions.duration = 0;
            }

            $view.show();

            var layouter = $view.data(Layouter.layouterInstance);
            if (layouter) {
                layouter.repositionElements();
            }

            this.setOffset($view, animationInitialOffset.top, animationInitialOffset.left);
            this.setWidth($view, parentDimensions.width);
            this.setHeight($view, parentDimensions.height);

            $view.animate(animationDescription, animationOptions);
            $visibleView.animate(animationDescription,
                $.extend({ complete: function () {
                    $visibleView.hide();
                    $parent.removeData(Layouter.layouterAnimationTarget);
                }
                }, animationOptions));
        }
    };

    $.fn.extend({
        layout: function () {
            this.each(function (index, element) {
                new Layouter(element).initialize();
            });
        },

        refreshLayout: function () {
            var layouter = this.data(Layouter.layouterInstance);
            if (layouter) {
                layouter.repositionElements();
            }
        },

        showLayoutView: function (viewName) {
            var splitted = viewName.split("/");
            var layouter = this.data(Layouter.layouterInstance);
            if (layouter) {
                layouter.showView(splitted);
            }
        }
    });
})(jQuery, window);