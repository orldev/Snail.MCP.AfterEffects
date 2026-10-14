# Workers

One After Effects serves one client well. Two clients sharing it step on each other's project, selection and undo history, and a long
render in it blocks everyone. The worker pool answers that: the server launches further After Effects instances on its own machine and
lends each to one client.

## Desk and workers

The After Effects a person started is the **desk**. Its bridge calls itself `desk`, it dials every server announced on the machine, and it
is where commands go when a client is bound to nothing. The pool never launches, quits or leases the desk.

A **worker** is an instance the server started: `w1`, `w2` and so on. The server runs the After Effects binary directly, so the process is its
child, with the worker's name, the data directory and the server's pid in the environment. The bridge inside reads them, names itself,
and dials that one server. On Windows `Workers:Headless` adds `-noui`; macOS has no interface-less mode, so a worker there is a second
window.

## Leasing

The `workers` skill holds three tools. `ae_workers` lists the pool: each worker's state (`starting`, `ready`, `leased`, `stopping`,
`gone`), pid, uptime, who holds it and how long it has sat idle. `ae_lease` binds the calling client to a worker: the one named, a free one,
or a fresh one when the pool is below `Workers:Max`; it waits up to `Workers:StartupSeconds` for the bridge to come up. From then on every
command of that client goes to its worker, until `ae_lease` with `release` sends them back to the desk. With `project`, a copy of that file is
placed under `<data>/workers/<name>/` and opened in the worker, so the original and the desk stay untouched. `ae_stop_worker` quits one.

Over HTTP every client that names itself has a session of its own, and so a binding of its own; two clients leasing get two workers.
Over stdio the one client is the process's session. Leasing is `farm`-scope work: a key without that scope cannot launch or stop
instances.

## Adopted workers

A worker need not be launched by the server. An After Effects started by anything else with `SNAIL_BRIDGE_WORKER` in its environment,
a scheduled task, a launchd agent, the scripts under `tools/workers/`, connects under that name and the pool adopts it: `ae_workers`
lists it with `origin: adopted` and no pid, `ae_lease` binds a client to it, `ae_stop_worker` asks it to quit. Nobody here relaunches
it or quits it for idleness: whoever started it owns its life, and its own bridge still quits after `SNAIL_BRIDGE_IDLE_MINUTES`. This
is how the server in a [container](docker.md) gets workers from the host.

## Lifecycle

- **Minimum and maximum.** The pool keeps `Workers:Min` running and never passes `Workers:Max`; a worker that exits is replaced while
  the pool is below the minimum.
- **Idle.** A worker that has gone `Workers:IdleMinutes` without a command closes its project without saving and quits, from inside its
  own bridge, so it releases its memory even if the server is gone. The server also asks a free worker beyond the minimum to quit after
  the same time.
- **Failure.** A worker whose bridge never connects within `Workers:StartupSeconds`, or does not come back within a minute of dropping,
  is killed and listed as `gone` with the reason for five minutes; a client bound to it is unbound and gets a `WorkerGone` answer until it
  leases again.

## Limits

- Memory is the ceiling: an instance takes about a gigabyte at start and several under load. Three or four workers beside the desk suit a
  machine with 24 GB; `ae_performance` caps each one's caches.
- Adobe's licence counts machines, not instances; several instances on one activated machine are fine. Several machines in containers
  are a licensing question to settle before building that.
- A worker renders with `ae_render` the same way the desk does, and is just as unreachable while it renders; renders that must be
  cancellable still go through the [farm](tools/farm.md).
