# Configuration

Everything is optional over stdio. Environment variables use the prefix `SNAIL_MCP_AFTEREFFECTS_`, with `__` between a section and a
field. The same keys go into a settings file, under a `Snail` section, which is read first, and the variables win over it setting by
setting. The chain is Snail.Sdk's, the same for every Snail server. A file named in `SNAIL_MCP_AFTEREFFECTS_CONFIG` (a leading `~/`
expanded) is the only one read, and when it does not exist no file is. Without that variable only the first file that exists is read:
`.s3l-mcp-aftereffects.json` and `s3l-mcp-aftereffects.json` in the working directory, `~/.config/s3l-mcp-aftereffects/config.json`,
`~/.s3l-mcp-aftereffects.json`. The names from before the rename are not read.

A token — `HTTP__TOKEN`, `BRIDGE__TOKEN` — belongs in `~/.config/s3l-mcp-aftereffects/config.json`, readable by you alone
(`chmod 600`), never in a launchd job or a client's registration, which travel wherever they are copied:

```json
{
  "Snail": {
    "Http": { "Token": "<a long random secret>" }
  }
}
```

| Variable | Default | Meaning |
|---|---|---|
| `SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY` | `~/.s3l-mcp-aftereffects` | The server's own files: the bridge token, the announcements the bridge reads, captures and renders when there is no project, the farm's logs, the client keys |
| `SNAIL_MCP_AFTEREFFECTS_PROJECT_DIRECTORY` | the folder the client starts the server in | Where captures and renders land when nobody names a place, and the files area over HTTP. The home folder and the root count as no project; a server over HTTP ignores it |
| `SNAIL_MCP_AFTEREFFECTS_SCRIPTING` | `Allow` | `Off` refuses `ae_script` and the `script` command inside `run_tools` and `run_program` |
| `SNAIL_MCP_AFTEREFFECTS_SKILL_IDLE_MINUTES` | `0` | Unload a skill unused for this many minutes; `0` keeps skills loaded, which suits clients that cache the tool list. The `farm` skill stays while a job of the client runs |
| `SNAIL_MCP_AFTEREFFECTS_IDLE_TIMEOUT_MINUTES` | `0` | Over stdio: exit after this long without a tool call; `0` disables it, since a client that ends closes the pipe and the server exits with it |
| `SNAIL_MCP_AFTEREFFECTS_FARM_PARALLEL` | `1` | How many `aerender` renders the farm runs at once, 1 to 16 |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__MAX` | `2` | Most After Effects workers the server keeps alive at once, 0 to 16; `0` switches launching off — [Workers](workers.md) |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__MIN` | `0` | Workers kept running at all times; `0` launches them only when a client leases one |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__IDLE_MINUTES` | `15` | Minutes a worker may sit without a command before it quits on its own; `0` keeps workers alive |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__STARTUP_SECONDS` | `120` | How long a launched After Effects has to connect its bridge before it counts as failed |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__HEADLESS` | `false` | Start workers without the interface; After Effects honours it on Windows only |
| `SNAIL_MCP_AFTEREFFECTS_WORKERS__EXECUTABLE` | the newest installed | The After Effects binary to launch, when it is not where the server looks |
| `SNAIL_MCP_AFTEREFFECTS_AGENT` | — | The name this server signs its commands with; over HTTP a client's key names it instead |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT` | `0` | The port the bridge dials; `0` takes a free one, which the announcement names |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__HOST` | `127.0.0.1` | The address written into the announcement for the bridge to dial; the server's service name when After Effects runs in another container |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__LISTEN` | — | The address the listener binds; empty binds the loopback for a loopback host and every interface otherwise |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__REQUEST_TIMEOUT_SECONDS` | `30` | One reply; captures, renders, imports and scripts pass their own |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__TOKEN` | — | The secret the bridge presents; empty reads the one the first server minted into `<data>/state/token` |
| `SNAIL_MCP_AFTEREFFECTS_BRIDGE__TOKEN_FILE` | `<data>/state/token` | Where that token lives when it is kept elsewhere |
| `SNAIL_MCP_AFTEREFFECTS_TRANSPORT` | `Stdio` | `Http` runs the server on its own beside After Effects — [Over HTTP](http.md) |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__URL` | `http://127.0.0.1:8080` | Over HTTP: where to listen; a container listens on `http://0.0.0.0:8080` |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN` | — | The operator's key, every scope; required over HTTP unless `HTTP__TOKEN_FILE` holds one |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN_FILE` | — | A file holding that key, the way a container receives a secret |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__PUBLIC_URL` | `HTTP__URL` | The address behind a reverse proxy, which signed links are built on; an `https` address marks the session cookie Secure |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__LINK_MINUTES` | `15` | How long a signed link to a file works, 1 to 1440 |
| `SNAIL_MCP_AFTEREFFECTS_HTTP__UPLOAD_PART_MB` | `16` | Megabytes an upload sends in one request, which a reverse proxy must read within its own body timeout |
