// A list of commands in one request and one undo step: twenty steps cost one round trip, and a failed step stops the rest.
(function () {
    function now() { return new Date().getTime(); }

    SnailBridge.command("sequence", { params: ["steps", "stop_on_error"], destructive: true }, function (params) {
        if (!(params.steps instanceof Array) || params.steps.length === 0) { throw SnailBridge.fail("BadRequest", "steps must be a list of {command, params}."); }
        var stopOnError = params.stop_on_error !== false;
        var results = [];
        var failed = 0;
        for (var i = 0; i < params.steps.length; i++) {
            var step = params.steps[i] || {};
            var entry = { step: i + 1, command: step.command };
            var startedAt = now();
            try {
                entry.ok = true;
                entry.result = SnailBridge.invoke(step.command, step.params, {});
            } catch (e) {
                entry.ok = false;
                entry.error = SnailBridge.errorOf(e);
                failed++;
            }
            entry.ms = now() - startedAt;
            results.push(entry);
            if (!entry.ok && stopOnError) { break; }
        }
        return { steps: results, ran: results.length, of: params.steps.length, failed: failed, stopped: failed > 0 && stopOnError && results.length < params.steps.length };
    });
})();
