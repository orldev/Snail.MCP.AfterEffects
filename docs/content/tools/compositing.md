# Precomposing, footage, proxies and the project's files

Part of the `compositing` skill: these tools appear once it is loaded — by `ae_enable_skill` with `compositing`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>precompose</code>

Move layers into a new nested composition in their place: the layers named, the new composition's name, and whether their attributes (transform, effects, masks) move with them or stay on the new layer, which works for one layer only.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layers` | `array` | yes | — | Layers to move into the new composition, each by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name of the new composition. |
| `moveAttributes` | `boolean` | — | `true` | Move the layers' attributes into the new composition (true, the default) or leave them on the new layer (one layer only). |
| `open` | `boolean` | — | `false` | Open the new composition in the viewer. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layers": {
      "description": "Layers to move into the new composition, each by name or index.",
      "type": "array",
      "items": {
        "type": "string"
      }
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "name": {
      "description": "Name of the new composition.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "moveAttributes": {
      "description": "Move the layers\u0027 attributes into the new composition (true, the default) or leave them on the new layer (one layer only).",
      "type": "boolean",
      "default": true
    },
    "open": {
      "description": "Open the new composition in the viewer.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "layers"
  ]
}
```

</details>

## <code>ae_<wbr>comp_<wbr>tree</code>

The compositions nested in a composition, as a tree with the layer each sits on, down to a depth; and the compositions this one is used in. Read it before changing a composition several others depend on.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `depth` | `integer` | — | `4` | How many levels of nesting to open; 4 by default. |

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
    },
    "depth": {
      "description": "How many levels of nesting to open; 4 by default.",
      "type": "integer",
      "default": 4
    }
  }
}
```

</details>

## <code>ae_<wbr>replace_<wbr>footage</code>

Relink or replace a footage item for every layer that uses it: with a file (or an image sequence starting at that file), with a solid of a colour, or with a placeholder. Interpretation stays as it was.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |
| `file` | `string?` | — | `null` | Path of the new file; the first frame when sequence is true. |
| `sequence` | `boolean` | — | `false` | Treat the file as the start of an image sequence. |
| `forceAlphabetical` | `boolean` | — | `false` | For a sequence: take the frames in alphabetical order rather than by number. |
| `solidColor` | `any` | — | `null` | Replace with a solid of this colour instead of a file. |
| `placeholder` | `boolean` | — | `false` | Replace with a placeholder instead of a file. |
| `name` | `string?` | — | `null` | Name for the solid or placeholder; the item's name when empty. |
| `width` | `integer?` | — | `null` | Width of the solid or placeholder; the item's when empty. |
| `height` | `integer?` | — | `null` | Height of the solid or placeholder; the item's when empty. |
| `duration` | `number?` | — | `null` | Duration of the placeholder in seconds. |
| `frameRate` | `number?` | — | `null` | Frame rate of the placeholder. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    },
    "file": {
      "description": "Path of the new file; the first frame when sequence is true.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "sequence": {
      "description": "Treat the file as the start of an image sequence.",
      "type": "boolean",
      "default": false
    },
    "forceAlphabetical": {
      "description": "For a sequence: take the frames in alphabetical order rather than by number.",
      "type": "boolean",
      "default": false
    },
    "solidColor": {
      "description": "Replace with a solid of this colour instead of a file.",
      "default": null
    },
    "placeholder": {
      "description": "Replace with a placeholder instead of a file.",
      "type": "boolean",
      "default": false
    },
    "name": {
      "description": "Name for the solid or placeholder; the item\u0027s name when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "width": {
      "description": "Width of the solid or placeholder; the item\u0027s when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "height": {
      "description": "Height of the solid or placeholder; the item\u0027s when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Duration of the placeholder in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "frameRate": {
      "description": "Frame rate of the placeholder.",
      "type": [
        "number",
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

## <code>ae_<wbr>interpret_<wbr>footage</code>

Interpret footage as the Interpret Footage dialog does: alpha as ignored, straight or premultiplied (or guessed), inverted, frame rate to conform to, field separation, pulldown removal (or guessed), loop count and pixel aspect ratio. With nothing to change the answer reports the interpretation as it stands.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |
| `alpha` | `string?` | — | `null` | IGNORE, STRAIGHT or PREMULTIPLIED. |
| `invertAlpha` | `boolean?` | — | `null` | Invert the alpha channel. |
| `premulColor` | `any` | — | `null` | The colour the alpha was premultiplied with, as #rrggbb or [r, g, b]. |
| `guessAlpha` | `boolean` | — | `false` | Let After Effects guess the alpha interpretation. |
| `frameRate` | `number?` | — | `null` | Frame rate to conform the footage to. |
| `fields` | `string?` | — | `null` | OFF, UPPER_FIELD_FIRST or LOWER_FIELD_FIRST. |
| `highQualityFields` | `boolean?` | — | `null` | Separate fields at high quality. |
| `removePulldown` | `string?` | — | `null` | Pulldown phase to remove, as After Effects names them (OFF, WSSWW, SSWWW, SWWWS, WWWSS, WWSSW, or the advanced phases). |
| `guessPulldown` | `boolean` | — | `false` | Let After Effects guess the 3:2 pulldown. |
| `loop` | `integer?` | — | `null` | How many times the footage loops. |
| `pixelAspect` | `number?` | — | `null` | Pixel aspect ratio, 1 for square pixels. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    },
    "alpha": {
      "description": "IGNORE, STRAIGHT or PREMULTIPLIED.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "invertAlpha": {
      "description": "Invert the alpha channel.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "premulColor": {
      "description": "The colour the alpha was premultiplied with, as #rrggbb or [r, g, b].",
      "default": null
    },
    "guessAlpha": {
      "description": "Let After Effects guess the alpha interpretation.",
      "type": "boolean",
      "default": false
    },
    "frameRate": {
      "description": "Frame rate to conform the footage to.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "fields": {
      "description": "OFF, UPPER_FIELD_FIRST or LOWER_FIELD_FIRST.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "highQualityFields": {
      "description": "Separate fields at high quality.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "removePulldown": {
      "description": "Pulldown phase to remove, as After Effects names them (OFF, WSSWW, SSWWW, SWWWS, WWWSS, WWSSW, or the advanced phases).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "guessPulldown": {
      "description": "Let After Effects guess the 3:2 pulldown.",
      "type": "boolean",
      "default": false
    },
    "loop": {
      "description": "How many times the footage loops.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "pixelAspect": {
      "description": "Pixel aspect ratio, 1 for square pixels.",
      "type": [
        "number",
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

## <code>ae_<wbr>set_<wbr>proxy</code>

Set, switch or remove the proxy of a footage item or composition: a file, an image sequence, a solid or a placeholder stands in while use is true, and the full item renders when it is false.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string` | yes | — | The item by its name as the project panel shows it, or by its id when names repeat. |
| `file` | `string?` | — | `null` | Path of the proxy file; the first frame when sequence is true. |
| `sequence` | `boolean` | — | `false` | Treat the file as the start of an image sequence. |
| `none` | `boolean` | — | `false` | Remove the proxy. |
| `use` | `boolean?` | — | `null` | Use the proxy (true) or the full item (false); the proxy stays set either way. |
| `solidColor` | `any` | — | `null` | A solid of this colour as the proxy. |
| `placeholder` | `boolean` | — | `false` | A placeholder as the proxy. |
| `width` | `integer?` | — | `null` | Width of the solid or placeholder proxy. |
| `height` | `integer?` | — | `null` | Height of the solid or placeholder proxy. |
| `frameRate` | `number?` | — | `null` | Frame rate of the placeholder proxy. |
| `duration` | `number?` | — | `null` | Duration of the placeholder proxy in seconds. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The item by its name as the project panel shows it, or by its id when names repeat.",
      "type": "string"
    },
    "file": {
      "description": "Path of the proxy file; the first frame when sequence is true.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "sequence": {
      "description": "Treat the file as the start of an image sequence.",
      "type": "boolean",
      "default": false
    },
    "none": {
      "description": "Remove the proxy.",
      "type": "boolean",
      "default": false
    },
    "use": {
      "description": "Use the proxy (true) or the full item (false); the proxy stays set either way.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "solidColor": {
      "description": "A solid of this colour as the proxy.",
      "default": null
    },
    "placeholder": {
      "description": "A placeholder as the proxy.",
      "type": "boolean",
      "default": false
    },
    "width": {
      "description": "Width of the solid or placeholder proxy.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "height": {
      "description": "Height of the solid or placeholder proxy.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "frameRate": {
      "description": "Frame rate of the placeholder proxy.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Duration of the placeholder proxy in seconds.",
      "type": [
        "number",
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

## <code>ae_<wbr>replace_<wbr>layer_<wbr>source</code>

Put another footage item or composition on a layer, keeping the layer's transform, effects, masks and keyframes; expressions that name the old source are fixed when asked.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `item` | `string` | yes | — | The footage item or composition to put on the layer, by name or id. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `fixExpressions` | `boolean` | — | `true` | Rewrite expressions that name the old source so they keep working; true by default. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "item": {
      "description": "The footage item or composition to put on the layer, by name or id.",
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
    "fixExpressions": {
      "description": "Rewrite expressions that name the old source so they keep working; true by default.",
      "type": "boolean",
      "default": true
    }
  },
  "required": [
    "layer",
    "item"
  ]
}
```

</details>

## <code>ae_<wbr>project_<wbr>footage</code>

The footage of the project with the file each points to, whether it is missing and how many compositions use it; only the missing or only file-backed items when asked. Relink missing files with ae_replace_footage.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `onlyMissing` | `boolean` | — | `false` | Only footage whose file is missing. |
| `onlyFiles` | `boolean` | — | `false` | Only footage backed by a file, leaving out solids and placeholders. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "onlyMissing": {
      "description": "Only footage whose file is missing.",
      "type": "boolean",
      "default": false
    },
    "onlyFiles": {
      "description": "Only footage backed by a file, leaving out solids and placeholders.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>project_<wbr>cleanup</code>

Tidy the project as the File menu does: consolidate duplicate footage, remove footage no composition uses, or reduce the project to the items named and what they use. The answer counts what went.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `removeUnused` | `boolean` | — | `false` | Remove footage no composition uses. |
| `consolidate` | `boolean` | — | `false` | Merge footage items that point to the same file. |
| `reduceTo` | `array?` | — | `null` | Keep only these items (by name or id) and what they use; everything else goes. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "removeUnused": {
      "description": "Remove footage no composition uses.",
      "type": "boolean",
      "default": false
    },
    "consolidate": {
      "description": "Merge footage items that point to the same file.",
      "type": "boolean",
      "default": false
    },
    "reduceTo": {
      "description": "Keep only these items (by name or id) and what they use; everything else goes.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": [
          "string",
          "null"
        ]
      },
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>collect_<wbr>files</code>

Collect the project into a folder: every file the footage points to is copied into a (Footage) folder, the items are relinked to the copies, and the project is saved there under its name; missing footage is listed and skipped. The open project becomes the collected copy.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `folder` | `string` | yes | — | Folder to collect into; created when missing. |
| `name` | `string?` | — | `null` | Name of the collected project file without .aep; the project's own name when empty. |
| `timeoutSeconds` | `integer` | — | `300` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "folder": {
      "description": "Folder to collect into; created when missing.",
      "type": "string"
    },
    "name": {
      "description": "Name of the collected project file without .aep; the project\u0027s own name when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 300
    }
  },
  "required": [
    "folder"
  ]
}
```

</details>

