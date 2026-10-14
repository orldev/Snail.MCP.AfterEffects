# Project and items

## <code>ae_<wbr>project_<wbr>info</code>

The open project: its file, whether it has unsaved changes, how many compositions, footage items and folders it holds, the active item and the selection. Read it first to learn what is there.

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

## <code>ae_<wbr>project_<wbr>new</code>

Start a new empty project. Refused while the open project has unsaved changes unless discard is true, so a user's work is never thrown away in silence.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `discard` | `boolean` | — | `false` | Throw away the open project's unsaved changes instead of refusing. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "discard": {
      "description": "Throw away the open project\u0027s unsaved changes instead of refusing.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>project_<wbr>open</code>

Open a project file, replacing the open one. Refused while the open project has unsaved changes unless discard is true. Dialogs about missing footage are suppressed; ae_list_items with kind footage then shows what is missing.

*may overwrite or delete · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string` | yes | — | Path of the .aep, .aepx or .aet file on After Effects' machine. |
| `discard` | `boolean` | — | `false` | Throw away the open project's unsaved changes instead of refusing. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Path of the .aep, .aepx or .aet file on After Effects\u0027 machine.",
      "type": "string"
    },
    "discard": {
      "description": "Throw away the open project\u0027s unsaved changes instead of refusing.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "path"
  ]
}
```

</details>

## <code>ae_<wbr>project_<wbr>save</code>

Save the project in place, or under the path given; the folder is created when it does not exist.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string?` | — | `null` | Where to save, for a project saved for the first time or under a new name; empty saves in place. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Where to save, for a project saved for the first time or under a new name; empty saves in place.",
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

## <code>ae_<wbr>project_<wbr>close</code>

Close the project and leave After Effects with a new empty one. Refused while it has unsaved changes unless discard is true.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `discard` | `boolean` | — | `false` | Throw away the open project's unsaved changes instead of refusing. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "discard": {
      "description": "Throw away the open project\u0027s unsaved changes instead of refusing.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>list_<wbr>items</code>

The items of the project panel — compositions, footage and folders — with ids, sizes, durations and, for footage, the file and whether it is missing. Filter by kind, folder or name; the id is what later calls take when names repeat.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `kind` | `string?` | — | `null` | Keep only items of this kind: comp, footage or folder. |
| `folder` | `string?` | — | `null` | Keep only items directly inside this folder, by name or id. |
| `nameContains` | `string?` | — | `null` | Keep only items whose name contains this text, case-insensitive. |
| `limit` | `integer` | — | `200` | How many items to return at most; the total is reported regardless. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "kind": {
      "description": "Keep only items of this kind: comp, footage or folder.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "folder": {
      "description": "Keep only items directly inside this folder, by name or id.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "nameContains": {
      "description": "Keep only items whose name contains this text, case-insensitive.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "limit": {
      "description": "How many items to return at most; the total is reported regardless.",
      "type": "integer",
      "default": 200
    }
  }
}
```

</details>

## <code>ae_<wbr>item_<wbr>info</code>

Everything about one item: a composition's settings and its layer list, a footage item's source, a folder's children, and the compositions the item is used in.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    }
  },
  "required": [
    "item"
  ]
}
```

</details>

## <code>ae_<wbr>item_<wbr>update</code>

Rename an item, change its comment or label colour, or move it into a folder.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |
| `name` | `string?` | — | `null` | The new name. |
| `comment` | `string?` | — | `null` | The comment shown in the project panel. |
| `label` | `integer?` | — | `null` | Label colour as a number 0 to 16 as After Effects numbers them; 0 is none. |
| `folder` | `string?` | — | `null` | Folder to move the item into, by name or id. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    },
    "name": {
      "description": "The new name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "comment": {
      "description": "The comment shown in the project panel.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "label": {
      "description": "Label colour as a number 0 to 16 as After Effects numbers them; 0 is none.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "folder": {
      "description": "Folder to move the item into, by name or id.",
      "type": [
        "string",
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

## <code>ae_<wbr>item_<wbr>remove</code>

Delete an item from the project. Layers that use it are deleted with it, and so is everything inside a folder; ae_item_info shows where it is used first.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    }
  },
  "required": [
    "item"
  ]
}
```

</details>

## <code>ae_<wbr>folder_<wbr>create</code>

Create a folder in the project panel, at the root or inside another folder.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | yes | — | Name of the new folder. |
| `folder` | `string?` | — | `null` | Parent folder, by name or id; the project root otherwise. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Name of the new folder.",
      "type": "string"
    },
    "folder": {
      "description": "Parent folder, by name or id; the project root otherwise.",
      "type": [
        "string",
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

## <code>ae_<wbr>import_<wbr>file</code>

Import a file into the project: video, audio, a still, an image sequence, a layered .psd or .ai as a composition, or another .aep merged in. The imported item comes back with its id; use ae_add_layer to put it into a composition.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string` | yes | — | Path on After Effects' machine: footage, image, sound, a sequence's first frame, .psd/.ai, or another .aep. |
| `as` | `string?` | — | `null` | footage (default), comp (layered file as a composition), comp_cropped (layers cropped to content) or project (merge). |
| `sequence` | `boolean` | — | `false` | Import the file as the first frame of a numbered sequence. |
| `folder` | `string?` | — | `null` | Folder to put the item into, by name or id. |
| `name` | `string?` | — | `null` | Name for the item; the file's name otherwise. |
| `forceAlphabetical` | `boolean` | — | `true` | Sequence: order frames alphabetically; on unless a range is given. |
| `rangeStart` | `integer?` | — | `null` | Sequence: first frame number to import. |
| `rangeEnd` | `integer?` | — | `null` | Sequence: last frame number to import. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Path on After Effects\u0027 machine: footage, image, sound, a sequence\u0027s first frame, .psd/.ai, or another .aep.",
      "type": "string"
    },
    "as": {
      "description": "footage (default), comp (layered file as a composition), comp_cropped (layers cropped to content) or project (merge).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "sequence": {
      "description": "Import the file as the first frame of a numbered sequence.",
      "type": "boolean",
      "default": false
    },
    "folder": {
      "description": "Folder to put the item into, by name or id.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "name": {
      "description": "Name for the item; the file\u0027s name otherwise.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "forceAlphabetical": {
      "description": "Sequence: order frames alphabetically; on unless a range is given.",
      "type": "boolean",
      "default": true
    },
    "rangeStart": {
      "description": "Sequence: first frame number to import.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "rangeEnd": {
      "description": "Sequence: last frame number to import.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "path"
  ]
}
```

</details>

