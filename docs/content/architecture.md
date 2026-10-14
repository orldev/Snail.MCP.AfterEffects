# Architecture

## Two processes, one line of JSON

The server is a .NET process the MCP client starts over stdio. After Effects runs the **Snail bridge**, an ExtendScript file in
its `Scripts/Startup` folder. The bridge dials the server, not the other way round: every request from the server is one JSON line
and every reply one JSON line, matched by id:

```json
{"id": "7", "command": "add_layer", "params": {"comp": "Main", "kind": "text", "text": "Hello"}, "deadline_s": 30}
{"id": "7", "ok": true, "result": {"index": 1, "name": "Hello"}}
```

A failure comes back as data, never as a dropped connection:

```json
{"id": "8", "ok": false, "error": {"type": "Error", "message": "Unable to call \"property\" because of parameter 1. null is not an object", "line": 212}}
```

## Why the bridge dials out

ExtendScript's `Socket.listen()` does not work on macOS: it answers an I/O error on every port, with or without a timeout. So After
Effects cannot be the listening side. Outbound connections work well: `Socket.open("127.0.0.1:port")` connects at once, and with
`socket.timeout = 0` a `readln()` on a connection that holds no data returns an empty string in under ten milliseconds instead of
blocking. Measured on After Effects 26.5 with the server on the same machine: a hundred request-reply round trips take 15 ms when
the script loops over them itself, and a one-megabyte line travels there and back in 10 ms without loss.

The server therefore listens on the loopback — on the port it is configured with, or on a free one — and announces itself with a
file under `<data>/state/servers/`, one file per server process. The bridge reads that folder once a second and opens a connection
to every server it finds there, which is how several MCP clients, each with a server process of their own, share one After Effects.
A server that exits removes its file; a connection that drops is noticed on the next read and reopened when the server is back.

## The main thread

After Effects runs ExtendScript on its main thread, and only when it is idle. The bridge schedules a tick with
`app.scheduleTask(..., 50, true)`; After Effects fires it every 50 to 100 ms and asking for less changes nothing. On each tick the
bridge reads every connection without waiting, runs the requests it finds one after another, writes the replies and returns. So one
request costs about one tick of latency, several requests sent together are served in one tick, and an idle bridge costs After
Effects about three percent of a core.

A command runs inside `app.beginUndoGroup`/`app.endUndoGroup`, so every call is one undo step for the user. Every handler and the tick
itself run inside `try`/`catch`: an uncaught exception in a script makes After Effects show a modal dialog, and a modal dialog stops
the ticks and with them every client until a person dismisses it. For the same reason no command opens a dialog of its own.

A long operation holds the main thread for as long as it takes: `renderQueue.render()` on a two-second composition held it for
2.5 s and every other request waited behind it. Short renders and frame captures are fine there; anything longer belongs in
`aerender`, which the server starts as a separate process and which renders beside the running After Effects.

## Pictures

`CompItem.saveFrameToPng(time, file)` writes a frame of a composition; it returns before the file is complete, so the bridge waits
until the file exists and stops growing. The server reads the picture and hands it to the model as image content next to the JSON.

## Installing the bridge

After Effects runs the scripts of two `Scripts/Startup` folders when it starts: the one inside the application and the one of the
user, `~/Library/Preferences/Adobe/After Effects/<version>/Scripts/Startup` on macOS, which needs no administrator rights. The server
copies the bridge there, and it is up about nine seconds after After Effects starts. A running After Effects takes the bridge without
a restart: on macOS through `osascript -e 'tell application "Adobe After Effects 2026" to DoScriptFile "<path>"'`, on Windows through
`AfterFX.exe -r <path>`. `DoScriptFile` returns nothing useful, so the script's own answer travels over the socket, never back
through that call.

Two things the bridge cannot do for itself. ExtendScript has no `JSON` object (`typeof JSON` is `undefined` in ExtendScript 4.5.6),
so the bridge ships `json2`. And sockets and file writes are refused until the preference *Allow Scripts to Write Files and Access
Network* is on; a script cannot switch it on, so `ae_diagnose` reports it and the person does it once in Preferences → Scripting &
Expressions.

## aerender

`aerender` renders a saved project from the command line and runs beside an open After Effects. Its output is localised: the per-frame
lines read `PROGRESS:  0:00:00:01 (2): 0 секунд` on a Russian system, so the server parses the timecode and the frame number in
parentheses and never the words. Starting the render engine costs about ten seconds before the first frame; a two-second
composition took 18 s in all. The output module the project's defaults choose decides the file's extension, whatever the requested
name says.
