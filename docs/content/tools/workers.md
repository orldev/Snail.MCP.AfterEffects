# After Effects workers of your own

Part of the `workers` skill: these tools appear once it is loaded — by `ae_enable_skill` with `workers`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>workers</code>

The pool of After Effects workers: each one's name, state (starting, ready, leased, stopping, gone), origin (launched by this server, or adopted when a named instance somebody else started connected), pid, uptime, who holds it and how long it sat idle; with the pool's limits and which worker this client is bound to. The desk is not a worker.

*read-only · repeat-safe · stays inside After Effects.*

Takes no parameters.

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {}
}
```

</details>

## <code>ae_<wbr>lease</code>

Take an After Effects of your own: binds this client to a free worker, to the one named, or to a fresh one the server launches when the pool has room, waiting for it to come up; from then on every command of this client goes there. With project, a copy of that file is opened in it. release gives the worker back and commands return to the desk.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `worker` | `string?` | — | `null` | The worker by name, such as w1, as ae_workers lists them. |
| `launch` | `boolean` | — | `true` | Launch a new After Effects when no worker is free; true by default. |
| `project` | `string?` | — | `null` | A project file on the server's machine to copy into the worker's folder and open there. |
| `release` | `boolean` | — | `false` | Give the worker back instead of taking one. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "worker": {
      "description": "The worker by name, such as w1, as ae_workers lists them.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "launch": {
      "description": "Launch a new After Effects when no worker is free; true by default.",
      "type": "boolean",
      "default": true
    },
    "project": {
      "description": "A project file on the server\u0027s machine to copy into the worker\u0027s folder and open there.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "release": {
      "description": "Give the worker back instead of taking one.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>stop_<wbr>worker</code>

Quit a worker, dropping its project unsaved, and free whoever held it; the pool launches another when it is below its minimum.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `worker` | `string` | yes | — | The worker by name, such as w1, as ae_workers lists them. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "worker": {
      "description": "The worker by name, such as w1, as ae_workers lists them.",
      "type": "string"
    }
  },
  "required": [
    "worker"
  ]
}
```

</details>

