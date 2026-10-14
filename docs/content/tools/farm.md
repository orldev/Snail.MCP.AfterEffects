# Background renders through aerender

Part of the `farm` skill: these tools appear once it is loaded — by `ae_enable_skill` with `farm`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>farm_<wbr>submit</code>

Queue a render of a composition on the farm: aerender renders the saved project in its own engine, so After Effects stays free and the job runs on after this call returns. The open project is saved first unless save is false; a project path renders that file instead. Frames, templates and settings are aerender's own. The answer is the job with its id; poll it with ae_farm_jobs.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string` | yes | — | The composition to render, by name or id. |
| `output` | `string?` | — | `null` | Where to write the output: a file path, or a folder to take the composition's name; After Effects adds the extension the output module gives. Empty renders under the project folder's renders. |
| `project` | `string?` | — | `null` | A saved project file to render instead of the open project. |
| `save` | `boolean` | — | `true` | Save the open project before rendering it, so aerender sees the latest changes; true by default. |
| `startFrame` | `integer?` | — | `null` | First frame to render; the composition's start when empty. |
| `endFrame` | `integer?` | — | `null` | Last frame to render, inclusive; the composition's end when empty. |
| `increment` | `integer?` | — | `null` | Render every nth frame; 1 renders all. |
| `renderTemplate` | `string?` | — | `null` | Render settings template by name, as ae_render_templates lists them. |
| `outputTemplate` | `string?` | — | `null` | Output module template by name, as ae_render_templates lists them. |
| `renderSettings` | `object?` | — | `null` | As aerender takes them, an object of names to values; the After Effects guide to render and output settings names them. |
| `outputSettings` | `object?` | — | `null` | As aerender takes them, an object of names to values; the After Effects guide to render and output settings names them. |
| `priority` | `integer` | — | `0` | Higher runs first among queued jobs; 0 by default. |
| `retries` | `integer` | — | `0` | How many times a failed job is tried again; 0 by default. |
| `multiFrame` | `boolean?` | — | `null` | Multi-frame rendering on or off; aerender's default when empty. |
| `cpuPercent` | `integer?` | — | `null` | Most CPU percent multi-frame rendering may take, 1 to 100. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "comp": {
      "description": "The composition to render, by name or id.",
      "type": "string"
    },
    "output": {
      "description": "Where to write the output: a file path, or a folder to take the composition\u0027s name; After Effects adds the extension the output module gives. Empty renders under the project folder\u0027s renders.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "project": {
      "description": "A saved project file to render instead of the open project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "save": {
      "description": "Save the open project before rendering it, so aerender sees the latest changes; true by default.",
      "type": "boolean",
      "default": true
    },
    "startFrame": {
      "description": "First frame to render; the composition\u0027s start when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "endFrame": {
      "description": "Last frame to render, inclusive; the composition\u0027s end when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "increment": {
      "description": "Render every nth frame; 1 renders all.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "renderTemplate": {
      "description": "Render settings template by name, as ae_render_templates lists them.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outputTemplate": {
      "description": "Output module template by name, as ae_render_templates lists them.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "renderSettings": {
      "description": "As aerender takes them, an object of names to values; the After Effects guide to render and output settings names them.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "outputSettings": {
      "description": "As aerender takes them, an object of names to values; the After Effects guide to render and output settings names them.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "priority": {
      "description": "Higher runs first among queued jobs; 0 by default.",
      "type": "integer",
      "default": 0
    },
    "retries": {
      "description": "How many times a failed job is tried again; 0 by default.",
      "type": "integer",
      "default": 0
    },
    "multiFrame": {
      "description": "Multi-frame rendering on or off; aerender\u0027s default when empty.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "cpuPercent": {
      "description": "Most CPU percent multi-frame rendering may take, 1 to 100.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "comp"
  ]
}
```

</details>

## <code>ae_<wbr>farm_<wbr>jobs</code>

The farm's jobs, newest first, or one job by id: state (queued, running, done, failed, cancelled), frames rendered against the total when the span is known, the last frame and timecode, attempts and retries left, the error if any, and the tail of the log.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string?` | — | `null` | The job id as ae_farm_submit and ae_farm_jobs give it. |
| `state` | `string?` | — | `null` | Only jobs in this state: queued, running, done, failed or cancelled. |
| `logLines` | `integer?` | — | `null` | Lines from the end of each job's log to include; 10 for one job, 0 for the list. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "id": {
      "description": "The job id as ae_farm_submit and ae_farm_jobs give it.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "state": {
      "description": "Only jobs in this state: queued, running, done, failed or cancelled.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "logLines": {
      "description": "Lines from the end of each job\u0027s log to include; 10 for one job, 0 for the list.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>farm_<wbr>cancel</code>

Stop a queued or running job; a running render is killed and its partial output stays on disk.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | — | The job id as ae_farm_submit and ae_farm_jobs give it. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "id": {
      "description": "The job id as ae_farm_submit and ae_farm_jobs give it.",
      "type": "string"
    }
  },
  "required": [
    "id"
  ]
}
```

</details>

## <code>ae_<wbr>farm_<wbr>retry</code>

Queue a finished, failed or cancelled job again as a new job with the same request.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `string` | yes | — | The job id as ae_farm_submit and ae_farm_jobs give it. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "id": {
      "description": "The job id as ae_farm_submit and ae_farm_jobs give it.",
      "type": "string"
    }
  },
  "required": [
    "id"
  ]
}
```

</details>

## <code>ae_<wbr>farm_<wbr>cleanup</code>

Forget finished jobs older than a number of minutes, and delete their logs; 0 forgets every finished job.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `olderThanMinutes` | `integer` | — | `0` | Forget finished jobs older than this many minutes; 0 forgets all of them. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "olderThanMinutes": {
      "description": "Forget finished jobs older than this many minutes; 0 forgets all of them.",
      "type": "integer",
      "default": 0
    }
  }
}
```

</details>

