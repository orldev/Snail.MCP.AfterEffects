// A JSON object for ExtendScript, which has none: the public-domain json2 algorithm, cut to what the bridge uses.
// parse() runs the safety checks of json2 before it hands the text to eval, so a line from the socket cannot run code.
// The character classes are built from codes rather than written as escapes: a literal line separator in a regex is a
// syntax error here, and an editor or a transfer can turn an escape into the character it names without anyone noticing.
if (typeof JSON !== "object") {
    JSON = {};
}
(function () {
    var lineSeparators = String.fromCharCode(0x2028) + String.fromCharCode(0x2029);
    var escapable = new RegExp("[\\x00-\\x1f\"\\\\" + lineSeparators + "]", "g");
    var meta = { "\b": "\\b", "\t": "\\t", "\n": "\\n", "\f": "\\f", "\r": "\\r", "\"": "\\\"", "\\": "\\\\" };

    function hex4(character) {
        return "\\u" + ("0000" + character.charCodeAt(0).toString(16)).slice(-4);
    }

    function quote(text) {
        escapable.lastIndex = 0;
        if (!escapable.test(text)) {
            return "\"" + text + "\"";
        }
        return "\"" + text.replace(escapable, function (a) {
            var c = meta[a];
            return typeof c === "string" ? c : hex4(a);
        }) + "\"";
    }

    function str(key, holder) {
        var value = holder[key];
        if (value && typeof value === "object" && typeof value.toJSON === "function") {
            value = value.toJSON(key);
        }
        switch (typeof value) {
            case "string":
                return quote(value);
            case "number":
                return isFinite(value) ? String(value) : "null";
            case "boolean":
                return String(value);
            case "object":
                if (!value) {
                    return "null";
                }
                var partial = [];
                var i;
                if (Object.prototype.toString.apply(value) === "[object Array]") {
                    for (i = 0; i < value.length; i += 1) {
                        partial[i] = str(i, value) || "null";
                    }
                    return "[" + partial.join(",") + "]";
                }
                for (var k in value) {
                    if (Object.prototype.hasOwnProperty.call(value, k)) {
                        var v = str(k, value);
                        if (v) {
                            partial.push(quote(k) + ":" + v);
                        }
                    }
                }
                return "{" + partial.join(",") + "}";
            default:
                return undefined;
        }
    }

    if (typeof JSON.stringify !== "function") {
        JSON.stringify = function (value) {
            return str("", { "": value });
        };
    }

    if (typeof JSON.parse !== "function") {
        JSON.parse = function (text) {
            text = String(text);
            if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, "@")
                .replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, "]")
                .replace(/(?:^|:|,)(?:\s*\[)+/g, ""))) {
                return eval("(" + text + ")");
            }
            throw new SyntaxError("JSON.parse: not JSON");
        };
    }
})();
