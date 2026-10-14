# Measuring, guides and placing

Part of the `layout` skill: these tools appear once it is loaded — by `ae_enable_skill` with `layout`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>layer_<wbr>bounds</code>

The pixel bounds of a layer at a time: the rectangle in the layer's own space (what sourceRectAtTime measures, text and shapes included) and, for a 2D layer, its four corners, centre and bounding box in composition pixels after transform. Use it to align and fit layers instead of guessing sizes.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |
| `extents` | `boolean` | — | `false` | Measure only the pixels with content rather than the layer's full extent; false by default. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
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
    "time": {
      "description": "Time in seconds within the composition; the composition\u0027s current time when empty.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "extents": {
      "description": "Measure only the pixels with content rather than the layer\u0027s full extent; false by default.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>convert_<wbr>point</code>

Convert a point between a layer's own pixels and the composition's, at a time: where a corner of a text lands on screen, or which pixel of a footage sits under a composition point.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `point` | `array` | yes | — | [x, y] to convert. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `to` | `string` | — | `comp` | comp converts from the layer's pixels into the composition (the default); source converts the other way. |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "point": {
      "description": "[x, y] to convert.",
      "type": "array",
      "items": {
        "type": "number"
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
    "to": {
      "description": "comp converts from the layer\u0027s pixels into the composition (the default); source converts the other way.",
      "type": "string",
      "default": "comp"
    },
    "time": {
      "description": "Time in seconds within the composition; the composition\u0027s current time when empty.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "point"
  ]
}
```

</details>

## <code>ae_<wbr>selection</code>

What the user has selected in a composition: the layers, and the properties with the layer and path of each; the composition's current time comes along.

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

## <code>ae_<wbr>guides</code>

The guides of a composition or a footage item: index, orientation and position in its pixels.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string?` | — | `null` | The composition or footage item by name or id; the active composition when empty. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The composition or footage item by name or id; the active composition when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
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

## <code>ae_<wbr>set_<wbr>guides</code>

Change the guides of a composition or footage item in one call: add guides by orientation and position, move or remove them by index, or clear them all. The answer lists the guides afterwards.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `item` | `string?` | — | `null` | The composition or footage item by name or id; the active composition when empty. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `add` | `array?` | — | `null` | Guides to add, each {orientation: horizontal or vertical, position} in the item's pixels. |
| `move` | `array?` | — | `null` | Guides to move, each {index, position}. |
| `remove` | `array?` | — | `null` | Indices of guides to remove, 0 first, as ae_guides numbers them. |
| `clear` | `boolean` | — | `false` | Remove every guide first. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "item": {
      "description": "The composition or footage item by name or id; the active composition when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "add": {
      "description": "Guides to add, each {orientation: horizontal or vertical, position} in the item\u0027s pixels.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "move": {
      "description": "Guides to move, each {index, position}.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "remove": {
      "description": "Indices of guides to remove, 0 first, as ae_guides numbers them.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "clear": {
      "description": "Remove every guide first.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>split_<wbr>layer</code>

Split a layer at a time into two, as Edit &gt; Split Layer does: the part before keeps the name and goes below, the part after sits above. The answer carries both with their in and out points.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
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
    "time": {
      "description": "Time in seconds within the composition; the composition\u0027s current time when empty.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>copy_<wbr>layer_<wbr>to_<wbr>comp</code>

Copy a layer with everything on it into another composition, at the top of its stack; the original stays.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `target` | `string` | yes | — | The composition the copy goes to, by name or id. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "target": {
      "description": "The composition the copy goes to, by name or id.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "target"
  ]
}
```

</details>

## <code>ae_<wbr>place_<wbr>layer_<wbr>corners</code>

Place a 2D or 3D layer so that its top-left, top-right and bottom-left corners land on three points of the composition, as calculateTransformFromPoints solves it; the transform is applied unless apply is false, and returned either way.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `topLeft` | `array` | yes | — | [x, y] or [x, y, z] in composition pixels. |
| `topRight` | `array` | yes | — | [x, y] or [x, y, z] in composition pixels. |
| `bottomLeft` | `array` | yes | — | [x, y] or [x, y, z] in composition pixels. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `apply` | `boolean` | — | `true` | Apply the transform to the layer (true, the default) or only compute it. |
| `time` | `number?` | — | `null` | Set the transform as keyframes at this time. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "topLeft": {
      "description": "[x, y] or [x, y, z] in composition pixels.",
      "type": "array",
      "items": {
        "type": "number"
      }
    },
    "topRight": {
      "description": "[x, y] or [x, y, z] in composition pixels.",
      "type": "array",
      "items": {
        "type": "number"
      }
    },
    "bottomLeft": {
      "description": "[x, y] or [x, y, z] in composition pixels.",
      "type": "array",
      "items": {
        "type": "number"
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
    "apply": {
      "description": "Apply the transform to the layer (true, the default) or only compute it.",
      "type": "boolean",
      "default": true
    },
    "time": {
      "description": "Set the transform as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "topLeft",
    "topRight",
    "bottomLeft"
  ]
}
```

</details>

## <code>ae_<wbr>find_<wbr>layer</code>

Find a layer anywhere in the project by its id, which ae_layer_info and ae_list_layers report, and answer with its composition and its full description; ids survive renames and reordering.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `id` | `integer` | yes | — | The layer's id as ae_layer_info reports it. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "id": {
      "description": "The layer\u0027s id as ae_layer_info reports it.",
      "type": "integer"
    }
  },
  "required": [
    "id"
  ]
}
```

</details>

