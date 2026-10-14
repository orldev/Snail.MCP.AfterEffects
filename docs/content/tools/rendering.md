# The render queue

Part of the `rendering` skill: these tools appear once it is loaded — by `ae_enable_skill` with `rendering`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>render_<wbr>queue</code>

The render queue: every item with its composition, status, span, render flag and output modules (file, format and settings, post-render action), whether After Effects is rendering, and whether Media Encoder can take the queue.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `settings` | `boolean` | — | `false` | Include every settable render and output setting of each item. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "settings": {
      "description": "Include every settable render and output setting of each item.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>render_<wbr>templates</code>

The render settings templates (Best Settings, Draft Settings and the user's own) and output module templates (Lossless, H.264, PNG Sequence and the rest) this After Effects has; their names are what ae_add_render takes. With an empty queue a composition is needed to read them.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>add_<wbr>render</code>

Put a composition in the render queue: render settings and output module templates by name, the output file (or a folder, which takes the composition's name and the template's extension), the span as start and end or the work area, skip frames, a post-render action, and any render or output settings by their names. The item is queued and waits for ae_render.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `outputPath` | `string` | yes | — | Where to write the output: a file path, or a folder to take the composition's name. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `renderTemplate` | `string?` | — | `null` | Render settings template by name, as ae_render_templates lists them. |
| `outputTemplate` | `string?` | — | `null` | Output module template by name, as ae_render_templates lists them. |
| `outputModule` | `integer?` | — | `null` | Which output module of the item the path and template apply to, 1 first. |
| `postRenderAction` | `string?` | — | `null` | NONE, IMPORT, IMPORT_AND_REPLACE_USAGE or SET_PROXY. |
| `start` | `number?` | — | `null` | Start of the span to render, in seconds. |
| `end` | `number?` | — | `null` | End of the span to render, in seconds. |
| `workArea` | `boolean` | — | `false` | Render the composition's work area instead of a span. |
| `skipFrames` | `integer?` | — | `null` | Render every nth frame only; 0 renders all. |
| `renderSettings` | `object?` | — | `null` | Render settings by name as the Render Settings dialog shows them, such as Quality, Resolution, Motion Blur, Frame Rate; ae_render_queue with settings lists the settable ones. |
| `outputSettings` | `object?` | — | `null` | Output module settings by name, such as Format, Channels, Depth, Color, Video Output, Audio Output; ae_render_queue with settings lists the settable ones. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "outputPath": {
      "description": "Where to write the output: a file path, or a folder to take the composition\u0027s name.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
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
    "outputModule": {
      "description": "Which output module of the item the path and template apply to, 1 first.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "postRenderAction": {
      "description": "NONE, IMPORT, IMPORT_AND_REPLACE_USAGE or SET_PROXY.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "start": {
      "description": "Start of the span to render, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "end": {
      "description": "End of the span to render, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "workArea": {
      "description": "Render the composition\u0027s work area instead of a span.",
      "type": "boolean",
      "default": false
    },
    "skipFrames": {
      "description": "Render every nth frame only; 0 renders all.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "renderSettings": {
      "description": "Render settings by name as the Render Settings dialog shows them, such as Quality, Resolution, Motion Blur, Frame Rate; ae_render_queue with settings lists the settable ones.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "outputSettings": {
      "description": "Output module settings by name, such as Format, Channels, Depth, Color, Video Output, Audio Output; ae_render_queue with settings lists the settable ones.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "outputPath"
  ]
}
```

</details>

## <code>ae_<wbr>update_<wbr>render</code>

Change an item of the render queue that has not rendered yet: templates, output file, span, skip frames, render flag, post-render action, render or output settings.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `integer` | yes | — | Index of the item in the render queue, 1 first, as ae_render_queue lists them. |
| `renderTemplate` | `string?` | — | `null` | Render settings template by name. |
| `outputTemplate` | `string?` | — | `null` | Output module template by name. |
| `outputPath` | `string?` | — | `null` | Where to write the output: a file path, or a folder to take the composition's name. |
| `outputModule` | `integer?` | — | `null` | Which output module of the item the path and template apply to, 1 first. |
| `postRenderAction` | `string?` | — | `null` | NONE, IMPORT, IMPORT_AND_REPLACE_USAGE or SET_PROXY. |
| `start` | `number?` | — | `null` | Start of the span to render, in seconds. |
| `end` | `number?` | — | `null` | End of the span to render, in seconds. |
| `workArea` | `boolean` | — | `false` | Render the composition's work area instead of a span. |
| `skipFrames` | `integer?` | — | `null` | Render every nth frame only; 0 renders all. |
| `render` | `boolean?` | — | `null` | Queue the item (true) or leave it unqueued (false). |
| `renderSettings` | `object?` | — | `null` | Render settings by name as the Render Settings dialog shows them, such as Quality, Resolution, Motion Blur, Frame Rate; ae_render_queue with settings lists the settable ones. |
| `outputSettings` | `object?` | — | `null` | Output module settings by name, such as Format, Channels, Depth, Color, Video Output, Audio Output; ae_render_queue with settings lists the settable ones. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "Index of the item in the render queue, 1 first, as ae_render_queue lists them.",
      "type": "integer"
    },
    "renderTemplate": {
      "description": "Render settings template by name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outputTemplate": {
      "description": "Output module template by name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outputPath": {
      "description": "Where to write the output: a file path, or a folder to take the composition\u0027s name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outputModule": {
      "description": "Which output module of the item the path and template apply to, 1 first.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "postRenderAction": {
      "description": "NONE, IMPORT, IMPORT_AND_REPLACE_USAGE or SET_PROXY.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "start": {
      "description": "Start of the span to render, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "end": {
      "description": "End of the span to render, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "workArea": {
      "description": "Render the composition\u0027s work area instead of a span.",
      "type": "boolean",
      "default": false
    },
    "skipFrames": {
      "description": "Render every nth frame only; 0 renders all.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "render": {
      "description": "Queue the item (true) or leave it unqueued (false).",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "renderSettings": {
      "description": "Render settings by name as the Render Settings dialog shows them, such as Quality, Resolution, Motion Blur, Frame Rate; ae_render_queue with settings lists the settable ones.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "outputSettings": {
      "description": "Output module settings by name, such as Format, Channels, Depth, Color, Video Output, Audio Output; ae_render_queue with settings lists the settable ones.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "item"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>render</code>

Remove items from the render queue: one by index, all of them, or those with a status such as DONE.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `integer?` | — | `null` | Index of the item in the render queue, 1 first, as ae_render_queue lists them. |
| `all` | `boolean` | — | `false` | Remove every item. |
| `status` | `string?` | — | `null` | Remove the items with this status: DONE, UNQUEUED, QUEUED, NEEDS_OUTPUT, USER_STOPPED, ERR_STOPPED, WILL_CONTINUE. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "Index of the item in the render queue, 1 first, as ae_render_queue lists them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "all": {
      "description": "Remove every item.",
      "type": "boolean",
      "default": false
    },
    "status": {
      "description": "Remove the items with this status: DONE, UNQUEUED, QUEUED, NEEDS_OUTPUT, USER_STOPPED, ERR_STOPPED, WILL_CONTINUE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>render</code>

Render the queued items (or only the ones named) inside After Effects. By default the tool waits: After Effects is busy until the render ends, so give a timeout the render fits in. With wait false the render starts and the call returns; ae_render_queue shows the progress and ae_render_control stops or pauses it. The answer is the queue with each output file and its size. The farm renders through aerender while After Effects stays free.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `items` | `array?` | — | `null` | Indices of the items to render; every queued item when empty. |
| `wait` | `boolean` | — | `true` | Wait for the render (true, the default), or start it and return at once; ae_render_queue then shows the progress and ae_render_control stops or pauses it. |
| `timeoutSeconds` | `integer` | — | `300` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "items": {
      "description": "Indices of the items to render; every queued item when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "wait": {
      "description": "Wait for the render (true, the default), or start it and return at once; ae_render_queue then shows the progress and ae_render_control stops or pauses it.",
      "type": "boolean",
      "default": true
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 300
    }
  }
}
```

</details>

## <code>ae_<wbr>render_<wbr>control</code>

Stop, pause or resume a render that ae_render started without waiting, switch the finished notification, or bring the Render Queue to the front. After Effects answers the bridge only between frames it yields on, so a render in the application usually ends before stop or pause reach it; a render that must be cancellable goes through the farm (ae_farm_submit, ae_farm_cancel).

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `action` | `string?` | — | `null` | stop, pause or resume the render that ae_render started without waiting; empty to only set notify or show. |
| `notify` | `boolean?` | — | `null` | Have After Effects notify when the queue finishes. |
| `show` | `boolean` | — | `false` | Bring the Render Queue panel to the front. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "action": {
      "description": "stop, pause or resume the render that ae_render started without waiting; empty to only set notify or show.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "notify": {
      "description": "Have After Effects notify when the queue finishes.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "show": {
      "description": "Bring the Render Queue panel to the front.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>save_<wbr>render_<wbr>template</code>

Save a queue item's render settings or output module as a named template, so later ae_add_render calls can name it instead of listing every setting.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | yes | — | Name of the new template. |
| `kind` | `string` | — | `output` | render saves the item's render settings; output (the default) saves its output module. |
| `item` | `integer` | — | `1` | Index of the item in the render queue, 1 first, as ae_render_queue lists them. |
| `outputModule` | `integer?` | — | `null` | Which output module of the item, 1 first. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Name of the new template.",
      "type": "string"
    },
    "kind": {
      "description": "render saves the item\u0027s render settings; output (the default) saves its output module.",
      "type": "string",
      "default": "output"
    },
    "item": {
      "description": "Index of the item in the render queue, 1 first, as ae_render_queue lists them.",
      "type": "integer",
      "default": 1
    },
    "outputModule": {
      "description": "Which output module of the item, 1 first.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "name"
  ]
}
```

</details>

## <code>ae_<wbr>watch_<wbr>folder</code>

Turn this After Effects into a watch folder render node, as File &gt; Watch Folder does, or stop or pause it. While it watches, the application renders what arrives in the folder and answers little else; ae_render_queue and ae_diagnose still work.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string?` | — | `null` | Folder to watch for render control files. |
| `stop` | `boolean` | — | `false` | Stop watching. |
| `pause` | `boolean?` | — | `null` | Pause (true) or resume (false) the watch folder. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Folder to watch for render control files.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "stop": {
      "description": "Stop watching.",
      "type": "boolean",
      "default": false
    },
    "pause": {
      "description": "Pause (true) or resume (false) the watch folder.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>render_<wbr>in_<wbr>media_<wbr>encoder</code>

Send the render queue to Adobe Media Encoder, which renders in its own process and leaves After Effects free; with render_immediately it starts at once. Needs Media Encoder of the same version installed.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `renderImmediately` | `boolean` | — | `false` | Start rendering in Media Encoder at once rather than only queueing. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "renderImmediately": {
      "description": "Start rendering in Media Encoder at once rather than only queueing.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

