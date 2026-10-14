# Properties

## <code>ae_<wbr>property_<wbr>tree</code>

The property tree of a layer, which is where everything After Effects animates lives: Transform, Effects and their parameters, Masks, Text, Material Options, Audio. Each node carries its display name, its match name — stable across languages, use it in paths — its type, value, and whether it is animated or driven by an expression. Start here before reading or setting a property.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `path` | `string?` | — | `null` | Group to start from, as a path; the whole layer when empty. |
| `depth` | `integer` | — | `2` | How many levels below the start to list; 2 shows the groups and what is directly in them. |
| `values` | `boolean` | — | `true` | Include the current value of every property listed; false lists names and types only. |

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
    "path": {
      "description": "Group to start from, as a path; the whole layer when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "depth": {
      "description": "How many levels below the start to list; 2 shows the groups and what is directly in them.",
      "type": "integer",
      "default": 2
    },
    "values": {
      "description": "Include the current value of every property listed; false lists names and types only.",
      "type": "boolean",
      "default": true
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>get_<wbr>property</code>

One property by path: its value at a time, type, limits, keyframe count, expression and expression error. Any property of any effect is reachable this way; ae_property_tree lists the paths.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
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
    "path": {
      "description": "Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects\u0027 language, the English names of the common ones (Transform/Position, Effects/\u003Ceffect\u003E/\u003Cparameter\u003E, Masks/1/Mask Path, Text/Source Text), or 1-based indices.",
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
    "layer",
    "path"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>property</code>

Set any property of a layer by path: transform values, effect parameters, mask settings, text (a string or an object with font, size and colours), shapes, markers, or switch a group such as an effect on and off. With time the value becomes a keyframe; an expression can be put on or taken off at the same time. The answer is the property as it stands afterwards.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `value` | `any` | — | `null` | The new value in the property's shape: a number; [x, y] or [x, y, z]; a colour as [r, g, b, a] 0 to 1 or #rrggbb; true/false for a checkbox; a string or an object {text, font, font_size, fill_color, stroke_color, stroke_width, justification, tracking, leading} for text; {vertices, in_tangents, out_tangents, closed} for a mask or shape path; {comment, duration} for a marker. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the value as a keyframe at this time; without it an unanimated property changes everywhere and an animated one is keyed at the current time. |
| `expression` | `string?` | — | `null` | An expression to put on the property; an empty string removes it. |
| `enabled` | `boolean?` | — | `null` | For a group such as an effect or a mask: switch it on or off; for a property with an expression: enable or disable the expression. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "path": {
      "description": "Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects\u0027 language, the English names of the common ones (Transform/Position, Effects/\u003Ceffect\u003E/\u003Cparameter\u003E, Masks/1/Mask Path, Text/Source Text), or 1-based indices.",
      "type": "string"
    },
    "value": {
      "description": "The new value in the property\u0027s shape: a number; [x, y] or [x, y, z]; a colour as [r, g, b, a] 0 to 1 or #rrggbb; true/false for a checkbox; a string or an object {text, font, font_size, fill_color, stroke_color, stroke_width, justification, tracking, leading} for text; {vertices, in_tangents, out_tangents, closed} for a mask or shape path; {comment, duration} for a marker.",
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
    "time": {
      "description": "Set the value as a keyframe at this time; without it an unanimated property changes everywhere and an animated one is keyed at the current time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "expression": {
      "description": "An expression to put on the property; an empty string removes it.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "enabled": {
      "description": "For a group such as an effect or a mask: switch it on or off; for a property with an expression: enable or disable the expression.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "path"
  ]
}
```

</details>

