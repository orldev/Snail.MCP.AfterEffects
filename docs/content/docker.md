# Containers

After Effects runs on a workstation: it needs the operating system's display stack, its licence is signed in by a person, and its GPU
is the machine's own. The server has no such needs. So the container holds the server, and After Effects stays on the host beside it:
the same split the Blender server uses, except that Blender fits in a container and After Effects does not.

## The shape

| Where | What | Talks to |
|---|---|---|
| Container | this server over HTTP: `/mcp`, the pages, the client keys, the worker pool in adopting mode | the proxy in front, the bridge behind |
| Host | After Effects as a person uses it, the **desk** | dials the container's bridge port on the loopback |
| Host | After Effects **workers** started by `tools/workers/start-worker.*`, with `-noui` on Windows | dial the same port under their names; the server adopts them |

One folder joins the two sides: the state directory, `~/.s3l-mcp-aftereffects` on the host, mounted into the container **at the
same path**. The server writes its token and its announcement there, the bridge reads them as it would on a workstation, and every
file path the server hands After Effects, a capture, a project copy, an upload, names the same file on both sides. On a Windows host
mount `C:\Users\you\.s3l-mcp-aftereffects` at `/c/Users/you/.s3l-mcp-aftereffects` and set the data directory to that: After
Effects reads `/c/...` as `C:\...`, so the paths still agree.

The container's bridge listener binds every interface inside and is published on `127.0.0.1:9876` only; the announcement names
`127.0.0.1` as the host to dial, which is what After Effects on the host sees. The MCP endpoint is published on the address the reverse
proxy reaches, and `HTTP__PUBLIC_URL` is the address clients use, which signed links and the session cookie are built on.

## Docker Desktop on Windows

Leave Docker Desktop in **Linux containers** mode. That is the mode under WSL 2 in which the Blender server gets CUDA, and it is the
mode this image is built for. Windows-container mode is exclusive, has no GPU for After Effects' purposes, and would only matter for
putting After Effects itself in a container, which this layout does not do: After Effects on the host keeps its GPU, its fonts and its
licence as on any workstation.

## Running it

```bash
mkdir -p secrets && openssl rand -hex 24 > secrets/client.token
export STATE_DIRECTORY="$HOME/.s3l-mcp-aftereffects"      # Windows: /c/Users/you/.s3l-mcp-aftereffects, mounted from C:\Users\you\...
export PUBLIC_URL=https://aftereffects.example.com         # the address behind the proxy; http://localhost:8080 without one
docker compose up -d                                       # pulls ghcr.io/orldev/s3l-aftereffects-mcp:latest; --build makes it from the checkout
docker compose exec server dotnet Snail.MCP.AfterEffects.dll clients add titles --scope read,write,farm
```

Every release publishes the image for `linux/amd64` and `linux/arm64` as `ghcr.io/orldev/s3l-aftereffects-mcp:<version>`, smoke-tests it
without an After Effects, and moves `latest` once the release has gone through; `SERVER_TAG` pins a version.

Start After Effects on the host as usual: `ae_install_bridge` once from any client puts the bridge in its Startup folder, and from then
on the bridge finds the container through the announcement. `ae_diagnose` shows `bridge.connected` with the desk.

Workers come from the host too, one script per worker, kept alive by a scheduled task on Windows or a launchd agent on macOS:

```powershell
# Windows: After Effects without an interface, named w1, quitting after 15 idle minutes and coming back
Register-ScheduledTask -TaskName "Snail AE worker w1" -Trigger (New-ScheduledTaskTrigger -AtLogOn) `
    -Action (New-ScheduledTaskAction -Execute powershell.exe -Argument "-NoProfile -ExecutionPolicy Bypass -File C:\snail\tools\workers\start-worker.ps1 w1")
```

```bash
# macOS: a second window named w1
tools/workers/start-worker.sh w1 15
```

The server adopts each one as its bridge connects: `ae_workers` lists it with `origin: adopted`, `ae_lease` binds a client to it, and
`ae_stop_worker` asks it to quit, after which the host script starts it again clean. The pool launches nothing itself in this layout
(`WORKERS__MAX=0`); on a host where the server runs outside a container it does, see [Workers](workers.md).

## What stays on the host

- **The farm.** `aerender` lives beside After Effects, so the farm skill answers that it is unavailable inside the container. Renders
  go through `ae_render` in a worker or the desk; a cancellable background render needs the server on the host.
- **Launching workers.** The container cannot start a process on the host, hence the scripts above and adoption.
- **Secrets.** `secrets/client.token` is the operator's key, every scope; keys for other clients are minted with `clients add` and live
  under the state directory, so they survive the container.

## Verified and not

Verified on macOS with Docker Desktop: the image builds, the desk on the host dials the container, a worker started by the script is
adopted, leased and stopped, and a capture requested through the container comes back as a picture. Not yet run on a Windows host: the
`/c/...` mount, `-noui`, and the scheduled task; those are the first things to check there.
