TestCase("Dashboard.Helper.Tests", {
    testEnumNameIsReturnedForExistingValue: function () {
        var enumeration = { Busy: 0, Connected: 1 };
        assertEquals("Connected", Helper.getEnumNameByValue(enumeration, 1));
    },

    testValueIsReturnedForUnexistingValue: function () {
        var enumeration = { Busy: 0, Connected: 1 };
        assertEquals("2", Helper.getEnumNameByValue(enumeration, 2));
    },

    testJsonDateParsingIsCorrect: function () {
        var parsedDate = Helper.getJsonDate("/Date(1299501884317)/");

        assertEquals(new Date(1299501884317), parsedDate);
    },

    testJsonDateParsingThrosExceptionForIncorrectInput: function () {

        var parsedDate;
        try {
            parsedDate = Helper.getJsonDate("//Date(1299501884317)/");
        }
        catch (e) {
            parsedDate = "exception";
        }
        assertEquals("exception", parsedDate);
    },

    testJsonDateParsingThrosExceptionForIncorrectInput2: function () {

        var parsedDate;
        try {
            parsedDate = Helper.getJsonDate("/Date(1299501884317)//");
        }
        catch (e) {
            parsedDate = "exception";
        }
        assertEquals("exception", parsedDate);
    },

    testJsonDateParsingThrosExceptionForIncorrectInput3: function () {

        var parsedDate;
        try {
            parsedDate = Helper.getJsonDate("/Date(129950a1884317)/");
        }
        catch (e) {
            parsedDate = "exception";
        }
        assertEquals("exception", parsedDate);
    }
})