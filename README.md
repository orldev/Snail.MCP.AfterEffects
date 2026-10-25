# Snail.MCP.AfterEffects

An MCP server that lets your AI assistant drive a running **Adobe After Effects**. It runs on the machine After Effects runs on,
talks to your client over stdio, and reaches After Effects through a small ExtendScript bridge that lives in its scripts folder and
dials the server on its own.

[![NuGet](https://img.shields.io/nuget/v/Snail.MCP.AfterEffects.svg)](https://www.nuget.org/packages/Snail.MCP.AfterEffects/)
[![Documentation](https://img.shields.io/badge/docs-orldev.github.io-blue)](https://orldev.github.io/Snail.MCP.AfterEffects/)

## What it can do

- **Always on, 40 tools**: the project (new, open, save, items, folders, import), compositions, layers of every kind with their switches,
  timing and transforms, the whole property tree by match name (`ae_property_tree`, `ae_get_property`, `ae_set_property`), a frame of a
  composition as a picture (`ae_capture_frame`), undo and redo, menu commands, and `ae_script` for the ExtendScript nothing else reaches.
- **Skills, loaded on demand, 103 tools**: `animation` (keyframes, easing presets and curves, expressions checked for errors, markers, time
  remapping), `text` (fonts, the document with every attribute down to a range of characters, animators with range, wiggly and expression
  selectors, text on a path), `shapes` (shape layer contents by plain names, masks from rectangles, ellipses or vertices), `effects` (the
  effects catalog, effects and their parameters, animation presets, layer styles), `threed` (cameras, lights, materials, geometry, the
  renderer), `compositing` (precomposing, nested compositions, replacing and interpreting footage, proxies, missing files, cleanup,
  collecting files), `rendering` (the render queue, templates, rendering in place, Media Encoder) and `farm` (background renders through
  `aerender`, with a queue, progress by frame, retries and cancellation, while After Effects stays free), `templates` (Essential Graphics
  controllers, filling and substituting template compositions, `.mogrt` in and out), `transfer` (the server's files by area, uploads and
  downloads), `layout` (bounds, selection, guides, splitting and placing layers, copying between compositions), `settings` (project and
  application settings, fonts, caches, memory, preferences and script settings by key, the machine, a shell command), `viewer` (the
  viewer panel's views, channels, exposure and zoom) and `workers` (an After Effects of your own: the server launches an instance, binds
  the client to it, and it quits when idle). `ae_skills` lists them and `ae_enable_skill` loads one.
- **The machinery every Snail server shares**: `find_tool` searches the whole catalog by what you want to do and answers with each tool's
  command and parameters; `run_tools` sends a list of steps in one call; `run_program` runs a small JavaScript program that calls the
  commands itself, for builds of many steps.

## Install

1. `dotnet tool install --global Snail.MCP.AfterEffects`, then give your MCP client the command `s3l-mcp-aftereffects`
   (for Claude Code: `claude mcp add aftereffects -- s3l-mcp-aftereffects`). To run it from a checkout instead:
   `dotnet build -c Release` and the command `dotnet` with `src/bin/Release/net10.0/Snail.MCP.AfterEffects.dll` as its argument.
2. In After Effects open Preferences → Scripting & Expressions and switch on *Allow Scripts to Write Files and Access Network*, once.
3. Ask the agent to run `ae_install_bridge`: it copies the bridge into After Effects' user `Scripts/Startup` folder and starts it in the
   running After Effects; from then on the bridge starts with After Effects and dials every server that is running.
4. Ask for `ae_diagnose`: `afterEffects.reachable` is true once the bridge is connected.

## How it works

The server listens on the loopback and announces its port in `~/.s3l-mcp-aftereffects/state/servers/`; the bridge, an ExtendScript
started from After Effects' Startup folder, reads that folder once a second and opens a connection to every server it finds, presenting
the token the server keeps next to its state. Requests and replies are JSON lines matched by id; the bridge serves them on After Effects'
main thread between the application's own work, every 50 to 100 ms. [docs/content/architecture.md](docs/content/architecture.md) explains
why it is built this way round.

## Over HTTP

The same server runs on its own beside After Effects and serves several clients over Streamable HTTP, each with a key of its own:

```bash
mkdir -p ~/.config/s3l-mcp-aftereffects
echo '{ "Snail": { "Http": { "Token": "<a long random secret>" } } }' > ~/.config/s3l-mcp-aftereffects/config.json   # the operator's key, every scope
chmod 600 ~/.config/s3l-mcp-aftereffects/config.json
export SNAIL_MCP_AFTEREFFECTS_TRANSPORT=Http
export SNAIL_MCP_AFTEREFFECTS_HTTP__URL=http://0.0.0.0:8080
dotnet Snail.MCP.AfterEffects.dll clients add titles --scope read,write,farm   # prints a key once
dotnet Snail.MCP.AfterEffects.dll
```

A client connects to `/mcp` with `Authorization: Bearer <key>`; the key names the client and says what it may do: `read`, `write`, `delete`
(removing items, layers and files, undo, cancelling renders), `script` (ExtendScript and menu commands) and `farm` (rendering). A call the key
does not open is refused before it reaches After Effects, inside `run_tools` and `run_program` too. Each client has a tool list of its own:
a skill one client loads is in its list and nobody else's. `clients list` and `clients revoke <name>` manage the keys; a revoked key is turned
away at once. The pages at `/` (sign in with any key) show the status, the files by area with previews and downloads, and the farm's jobs;
`/healthz` answers anyone; `/raw/<area>/<path>` serves and takes files with a key or a signed link, which `ae_upload` and `ae_download` hand
out as shell commands.

## Settings

Every setting is an environment variable with the prefix `SNAIL_MCP_AFTEREFFECTS_`; `.mcp/server.json` lists them all. The same keys go into
a settings file under a `Snail` section: the one `SNAIL_MCP_AFTEREFFECTS_CONFIG` names, the only one read when it is set, or else the
first that exists of `.s3l-mcp-aftereffects.json` and `s3l-mcp-aftereffects.json` in the working directory,
`~/.config/s3l-mcp-aftereffects/config.json`, `~/.s3l-mcp-aftereffects.json`; the variables win over it. A token belongs in
`~/.config/s3l-mcp-aftereffects/config.json`
(`chmod 600`), never in a launchd job or a client's registration —
[docs/content/configuration.md](docs/content/configuration.md). The ones that matter first:

| Variable | Default | Meaning |
|---|---|---|
| `SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY` | `~/.s3l-mcp-aftereffects` | Where the server keeps its state, token, announcements and captures |
| `SNAIL_MCP_AFTEREFFECTS_SCRIPTING` | `Allow` | `Off` refuses `ae_script` and the `script` command in programs |
| `SNAIL_MCP_AFTEREFFECTS_SKILL_IDLE_MINUTES` | `0` | Unload a skill unused for this many minutes; `0` keeps skills loaded, which suits clients that cache the tool list |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__REQUEST_TIMEOUT_SECONDS` | `30` | How long a tool waits for After Effects before answering with a timeout |
| `SNAIL_MCP_AFTEREFFECTS_FARM_PARALLEL` | `1` | How many `aerender` renders the farm runs at once |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__MAX` | `2` | Most After Effects workers the server launches for clients that lease one; `0` switches it off |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__IDLE_MINUTES` | `15` | Minutes an idle worker waits before quitting on its own |
| `SNAIL_MCP_AFTEREFFECTS_TRANSPORT` | `Stdio` | `Http` runs the server on its own beside After Effects |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__URL` | `http://127.0.0.1:8080` | Where the HTTP server listens |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN` | | The operator's key; `HTTP__TOKEN_FILE` reads it from a file |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__PUBLIC_URL` | | The address behind a reverse proxy, which signed links are built on |

## Requirements

| | |
|---|---|
| **.NET 10 SDK** | [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **An MCP client** | Claude Code, Claude Desktop, Cursor, VS Code, Codex — anything that speaks MCP over stdio |
| **Adobe After Effects 2026 or newer** | with *Allow Scripts to Write Files and Access Network* switched on in Preferences → Scripting & Expressions |

## Developing

| Folder | Holds |
|---|---|
| `src/` | the server: `Configuration`, `Domain`, `Application`, `Adapters`, `Hosting`, `Extensions`, and `Tools` — `Base` for the layer that is always on, one folder per skill, and `Prompts` for the descriptions, messages and guidance the model reads |
| `extension/` | the ExtendScript bridge After Effects runs |
| `tests/` | a folder per layer of `src`, `Contracts` for the guards, `Support` for the fakes |

```bash
dotnet build
dotnet test
```

## License

MIT
