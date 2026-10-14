# Over HTTP

The same server runs on its own beside After Effects and serves several clients over Streamable HTTP. Each client presents a key of its
own, which names it and says what it may do.

## Starting it

```bash
mkdir -p ~/.config/s3l-mcp-aftereffects
echo '{ "Snail": { "Http": { "Token": "<a long random secret>" } } }' > ~/.config/s3l-mcp-aftereffects/config.json
chmod 600 ~/.config/s3l-mcp-aftereffects/config.json
export SNAIL_MCP_AFTEREFFECTS_TRANSPORT=Http
export SNAIL_MCP_AFTEREFFECTS_HTTP__URL=http://0.0.0.0:8080
dotnet Snail.MCP.AfterEffects.dll clients add titles --scope read,write,farm
dotnet Snail.MCP.AfterEffects.dll
```

The server refuses to start without a token: it would turn every client away while looking healthy. The token is the operator's key and
holds every scope; `clients add` mints a key per client, printed once, and `clients list` and `clients revoke <name>` manage them. A revoked
key is turned away on its next request, without restarting the server.

## Kept running by launchd

A server that is meant to be there whenever After Effects is runs as a job of this Mac, not from a terminal. The template is
`deploy/s3l.mcp.aftereffects.plist`; installing writes it with this machine's paths beside the publications:

```bash
dotnet publish src -c Release -p:Install=true
```

The install restarts the job when launchd already holds this very one, and otherwise prints the two commands that load it:

```bash
launchctl bootstrap gui/$UID ~/.s3l-mcp/Snail.MCP.AfterEffects/s3l.mcp.aftereffects.plist
launchctl kickstart -k gui/$UID/s3l.mcp.aftereffects
```

The job carries the transport and the address only. The operator's key stays in `~/.config/s3l-mcp-aftereffects/config.json`, so no
secret is in a file a backup may carry. Its log is `~/Library/Logs/Snail.MCP.AfterEffects.log`. After Effects itself is not started by
the job: the bridge in its Startup folder dials the server whenever a person opens the application.

## Keys and scopes

A client connects to `/mcp` with `Authorization: Bearer <key>`. The key's scopes are checked before a call reaches After Effects, inside
`run_tools` and `run_program` too, and over the file endpoints:

| Scope | Opens |
|---|---|
| `read` | reading the project, the files, the diagnostics |
| `write` | changing the project and writing files |
| `delete` | removing items, layers and files, undo, cancelling renders, replacing files that exist |
| `script` | `ae_script`, `ae_run_menu_command`, and the `script` command inside sequences and programs |
| `farm` | `ae_render`, `ae_render_in_media_encoder`, `ae_farm_submit` and `ae_farm_retry` |

A key minted without `--scope` gets everything but `script`. The call a key does not open is answered with a refusal naming the scope.

## One tool list per client

Each client has a tool list of its own: a skill one client loads is in its list and nobody else's, and `tools/list_changed` reaches the
client that holds a session. A client of the sessionless protocol revision is recognised by the name on its key.

## Pages and files

`/` shows the status (After Effects, the bridge, the farm, the storage, the server), and the areas `files`, `captures`, `renders` and
`jobs` with previews, downloads, deletion and the cancelling of a running job; sign in with any key. `/healthz` answers anyone.
`/raw/<area>/<path>` serves a file, or a folder as a tar or zip, and takes uploads into the files area, with a key or with a signed link
that works for `HTTP__LINK_MINUTES`; `ae_upload` and `ae_download` hand such links out as shell commands for the client's machine.
