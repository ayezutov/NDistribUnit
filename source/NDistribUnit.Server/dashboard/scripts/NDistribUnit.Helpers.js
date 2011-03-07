function Helper() {
}

Helper.executeWhen = function (condition, action, actionInterval) {
    /// <summary>Executes some action when and while the condition is true. If some condition becomes false, the action is not executed until the condition becomes true again.</summary>
    /// <param name="condition">The condition, which should be checked</param>
    /// <param name="actionInterval">The amount of time in milliseconds, at which the action should be called</param>
    /// <param name="action">The action, which should be called: action(callbackFunction). On complete the action should call a parameterless function, which is its first argument</param>
    var me = this;

    var callBackFunction = function () {
        if (actionInterval == -1)
            return;
        setTimeout(function () { me.executeWhen(condition, action, actionInterval); }, actionInterval);
    };

    if (condition.call(this)) {
        action.apply(this, [callBackFunction]);
    } else {
        setTimeout(function checkFunction() {
            if (condition.call(this)) {
                action.apply(this, [callBackFunction]);
            }
            else {
                setTimeout(checkFunction, 500);
            }
        }, 500);
    }
};

Helper.getEnumNameByValue = function (enumeration, value) {
    /// <summary>Returns textual representation of enum value. Return the value itself, if there is no such enum value.</summary>
    /// <param name="enumeration">The enumeration object. Is supposed to be a simple (property: intValue)[] object. </param>
    /// <param name="value">The value to check for.</param>
    for (var enumName in enumeration) {
        if (enumeration.hasOwnProperty(enumName) && enumeration[enumName] == value) {
            return enumName;
        }
    }
    return value;
};

Helper.getJsonDate = function (value) {
    /// <summary>Returns textual representation of enum value. Return the value itself, if there is no such enum value.</summary>
    /// <param name="enumeration">The enumeration object. Is supposed to be a simple (property: intValue)[] object. </param>
    /// <param name="value">The value to check for.</param>
    var regex = /\/Date\((\d+)\)\//gi;
    if (!value.match(regex))
        throw "The provided value is not a JSON date (" + value + ").";
    return eval(value.replace(regex, "new Date($1)"));
};