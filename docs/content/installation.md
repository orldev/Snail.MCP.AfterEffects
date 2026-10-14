# Installation

## Requirements

- The .NET 10 SDK to build, or the .NET 10 runtime to run a published build.
- Adobe After Effects 2026 or newer on the same machine, with *Preferences → Scripting & Expressions → Allow Scripts to Write Files and
  Access Network* switched on; without it the bridge cannot open a socket and `ae_diagnose` says so.
- An MCP client: Claude Code, Claude Desktop, Cursor, VS Code, Codex — anything that speaks MCP over stdio.

## The server

```bash
git clone https://github.com/orldev/Snail.MCP.AfterEffects
cd Snail.MCP.AfterEffects
dotnet build -c Release
```

Register it with the client as the command `dotnet` and the argument `<repository>/src/bin/Release/net10.0/Snail.MCP.AfterEffects.dll`.
For Claude Code:

```bash
claude mcp add aftereffects -- dotnet /path/to/Snail.MCP.AfterEffects/src/bin/Release/net10.0/Snail.MCP.AfterEffects.dll
```

Once published, the same server installs as a .NET tool: `dotnet tool install -g Snail.MCP.AfterEffects` and the command
`s3l-mcp-aftereffects`.

## The bridge

The server bundles the bridge and installs it itself: ask the agent for `ae_install_bridge`. It copies `snail_bridge.jsx` into After
Effects' user `Scripts/Startup` folder and its modules into `Scripts/(snail_bridge)` beside it, and starts the bridge in the running After
Effects. From then on the bridge starts with After Effects and dials every server that is running. `ae_diagnose` reports whether the
installed bridge is the build this server ships, by a digest of its files, and offers `ae_install_bridge` again when it is not.

On macOS the folder is `~/Library/Preferences/Adobe/After Effects/<version>/Scripts/`; on Windows
`%APPDATA%\Adobe\After Effects\<version>\Scripts\`. Nothing goes into the application bundle, so no administrator rights are needed.

## Several servers, one After Effects

Each client starts a server of its own; every server announces its port under `~/.s3l-mcp-aftereffects/state/servers/` and the bridge
dials all of them, so two agents can work in one After Effects. Over HTTP one server serves every client instead — [Over HTTP](http.md).
