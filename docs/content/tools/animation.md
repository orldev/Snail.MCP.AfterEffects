# Keyframes, expressions, markers and time remapping

Part of the `animation` skill: these tools appear once it is loaded — by `ae_enable_skill` with `animation`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>list_<wbr>keyframes</code>

Every keyframe of a property: time, value, interpolation on each side, ease (speed and influence), and for spatial properties the tangents and roving. Read this before easing or editing keys; the indices it gives are what the other keyframe tools take.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
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
    }
  },
  "required": [
    "layer",
    "path"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>keyframes</code>

Set keyframes on a property in one call: a list of {time, value} pairs, each optionally with its own easing, plus a preset or interpolation, influence and speed that apply to all of them. Existing keys at other times stay unless replace is true. A key at a time that already has one replaces it. Values take the same forms as ae_set_property.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `keys` | `array` | yes | — | Keyframes as a list of objects with time (seconds) and value (as ae_set_property takes it); any may also carry its own preset, in_interpolation, out_interpolation, influence_in, influence_out, speed_in, speed_out, in_tangent, out_tangent or label. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `replace` | `boolean` | — | `false` | Remove the property's existing keyframes first. |
| `preset` | `string?` | — | `null` | easy_ease (both sides), ease_in, ease_out, linear, or hold; the explicit settings below override a preset side by side. |
| `inInterpolation` | `string?` | — | `null` | Interpolation into the key: LINEAR, BEZIER or HOLD. |
| `outInterpolation` | `string?` | — | `null` | Interpolation out of the key: LINEAR, BEZIER or HOLD. |
| `influenceIn` | `number?` | — | `null` | Ease influence into the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier. |
| `influenceOut` | `number?` | — | `null` | Ease influence out of the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier. |
| `speedIn` | `number?` | — | `null` | Speed into the key in units per second; 0 comes to a rest, as Easy Ease does. |
| `speedOut` | `number?` | — | `null` | Speed out of the key in units per second; 0 starts from rest, as Easy Ease does. |
| `label` | `integer?` | — | `null` | Keyframe label colour as a number 0 to 16 as After Effects numbers them; 0 is none. |

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
    "keys": {
      "description": "Keyframes as a list of objects with time (seconds) and value (as ae_set_property takes it); any may also carry its own preset, in_interpolation, out_interpolation, influence_in, influence_out, speed_in, speed_out, in_tangent, out_tangent or label.",
      "type": "array"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "replace": {
      "description": "Remove the property\u0027s existing keyframes first.",
      "type": "boolean",
      "default": false
    },
    "preset": {
      "description": "easy_ease (both sides), ease_in, ease_out, linear, or hold; the explicit settings below override a preset side by side.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "inInterpolation": {
      "description": "Interpolation into the key: LINEAR, BEZIER or HOLD.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outInterpolation": {
      "description": "Interpolation out of the key: LINEAR, BEZIER or HOLD.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "influenceIn": {
      "description": "Ease influence into the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "influenceOut": {
      "description": "Ease influence out of the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "speedIn": {
      "description": "Speed into the key in units per second; 0 comes to a rest, as Easy Ease does.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "speedOut": {
      "description": "Speed out of the key in units per second; 0 starts from rest, as Easy Ease does.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "label": {
      "description": "Keyframe label colour as a number 0 to 16 as After Effects numbers them; 0 is none.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "path",
    "keys"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>keyframes</code>

Remove keyframes of a property: the ones at the indices or times given, or all of them when neither is. A property with no keys left keeps its last value.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `indices` | `array?` | — | `null` | Keyframe indices as ae_list_keyframes numbers them, 1 first; the keys at times, or all keys, when empty. |
| `times` | `array?` | — | `null` | Times in seconds of the keyframes meant, each within half a frame of a key; the keys at indices, or all keys, when empty. |

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
    "indices": {
      "description": "Keyframe indices as ae_list_keyframes numbers them, 1 first; the keys at times, or all keys, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "times": {
      "description": "Times in seconds of the keyframes meant, each within half a frame of a key; the keys at indices, or all keys, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
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

## <code>ae_<wbr>ease_<wbr>keyframes</code>

Change the interpolation and ease of existing keyframes: a preset as the Animation menu offers them, or interpolation per side with influence (percent) and speed, continuous and auto bezier tangents, roving for spatial keys. Applies to the keys at the indices or times given, or all of them.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `indices` | `array?` | — | `null` | Keyframe indices as ae_list_keyframes numbers them, 1 first; the keys at times, or all keys, when empty. |
| `times` | `array?` | — | `null` | Times in seconds of the keyframes meant, each within half a frame of a key; the keys at indices, or all keys, when empty. |
| `preset` | `string?` | — | `null` | easy_ease (both sides), ease_in, ease_out, linear, or hold; the explicit settings below override a preset side by side. |
| `inInterpolation` | `string?` | — | `null` | Interpolation into the key: LINEAR, BEZIER or HOLD. |
| `outInterpolation` | `string?` | — | `null` | Interpolation out of the key: LINEAR, BEZIER or HOLD. |
| `influenceIn` | `number?` | — | `null` | Ease influence into the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier. |
| `influenceOut` | `number?` | — | `null` | Ease influence out of the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier. |
| `speedIn` | `number?` | — | `null` | Speed into the key in units per second; 0 comes to a rest, as Easy Ease does. |
| `speedOut` | `number?` | — | `null` | Speed out of the key in units per second; 0 starts from rest, as Easy Ease does. |
| `continuous` | `boolean?` | — | `null` | Keep the tangents continuous through the key. |
| `autoBezier` | `boolean?` | — | `null` | Let After Effects pick the tangents (auto bezier). |
| `roving` | `boolean?` | — | `null` | For spatial properties: let the key rove in time to keep the speed even along the path. |
| `inTangent` | `array?` | — | `null` | For spatial properties: the tangent into the key as [x, y] or [x, y, z] relative to it. |
| `outTangent` | `array?` | — | `null` | For spatial properties: the tangent out of the key. |
| `spatialContinuous` | `boolean?` | — | `null` | For spatial properties: keep the path tangents continuous through the key. |
| `spatialAutoBezier` | `boolean?` | — | `null` | For spatial properties: let After Effects pick the path tangents. |
| `label` | `integer?` | — | `null` | Keyframe label colour as a number 0 to 16 as After Effects numbers them; 0 is none. |

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
    "indices": {
      "description": "Keyframe indices as ae_list_keyframes numbers them, 1 first; the keys at times, or all keys, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "times": {
      "description": "Times in seconds of the keyframes meant, each within half a frame of a key; the keys at indices, or all keys, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "preset": {
      "description": "easy_ease (both sides), ease_in, ease_out, linear, or hold; the explicit settings below override a preset side by side.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "inInterpolation": {
      "description": "Interpolation into the key: LINEAR, BEZIER or HOLD.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "outInterpolation": {
      "description": "Interpolation out of the key: LINEAR, BEZIER or HOLD.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "influenceIn": {
      "description": "Ease influence into the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "influenceOut": {
      "description": "Ease influence out of the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "speedIn": {
      "description": "Speed into the key in units per second; 0 comes to a rest, as Easy Ease does.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "speedOut": {
      "description": "Speed out of the key in units per second; 0 starts from rest, as Easy Ease does.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "continuous": {
      "description": "Keep the tangents continuous through the key.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "autoBezier": {
      "description": "Let After Effects pick the tangents (auto bezier).",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "roving": {
      "description": "For spatial properties: let the key rove in time to keep the speed even along the path.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "inTangent": {
      "description": "For spatial properties: the tangent into the key as [x, y] or [x, y, z] relative to it.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "outTangent": {
      "description": "For spatial properties: the tangent out of the key.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "spatialContinuous": {
      "description": "For spatial properties: keep the path tangents continuous through the key.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "spatialAutoBezier": {
      "description": "For spatial properties: let After Effects pick the path tangents.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "label": {
      "description": "Keyframe label colour as a number 0 to 16 as After Effects numbers them; 0 is none.",
      "type": [
        "integer",
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

## <code>ae_<wbr>separate_<wbr>dimensions</code>

Separate the dimensions of a position or another multi-dimensional property, so each axis has keyframes of its own, or join them again; the answer names the per-axis properties (the followers) the keyframe tools then take by path.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `separate` | `boolean?` | — | `null` | Separate the dimensions (true) or join them again (false); empty only reports. |

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
    "separate": {
      "description": "Separate the dimensions (true) or join them again (false); empty only reports.",
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

## <code>ae_<wbr>set_<wbr>expression</code>

Put an expression on a property and check it compiles: a broken expression is reported with After Effects' own error message and not left on the property, so the answer can be trusted. An empty expression removes the one there; check_only tries the expression and restores whatever was there before. The answer carries the value the expression yields at the current time.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `expression` | `string` | yes | — | The expression, in After Effects' JavaScript; empty removes the expression on the property. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `enabled` | `boolean` | — | `true` | Leave the expression switched on; false puts it on the property disabled. |
| `checkOnly` | `boolean` | — | `false` | Only check that the expression compiles and what it yields, restoring the property afterwards. |

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
    "expression": {
      "description": "The expression, in After Effects\u0027 JavaScript; empty removes the expression on the property.",
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
    "enabled": {
      "description": "Leave the expression switched on; false puts it on the property disabled.",
      "type": "boolean",
      "default": true
    },
    "checkOnly": {
      "description": "Only check that the expression compiles and what it yields, restoring the property afterwards.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "layer",
    "path",
    "expression"
  ]
}
```

</details>

## <code>ae_<wbr>list_<wbr>markers</code>

The markers of a composition, or of a layer when one is named: time, comment, duration, label, chapter, URL, cue point name and, for composition markers, whether the region is protected.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `layer` | `string?` | — | `null` | A layer by name or index for layer markers; the composition's own markers when empty. |

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
    "layer": {
      "description": "A layer by name or index for layer markers; the composition\u0027s own markers when empty.",
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

## <code>ae_<wbr>set_<wbr>marker</code>

Add a marker on the composition, or on a layer when one is named, at a time with a comment and optional duration, label, chapter, URL, frame target and cue point name. A marker already at that time is replaced.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |
| `comment` | `string?` | — | `null` | The marker's comment, shown on it in the timeline. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `layer` | `string?` | — | `null` | A layer by name or index for layer markers; the composition's own markers when empty. |
| `duration` | `number?` | — | `null` | Duration of the marker in seconds; 0 for a point marker. |
| `label` | `integer?` | — | `null` | Label colour as a number 0 to 16 as After Effects numbers them; 0 is none. |
| `chapter` | `string?` | — | `null` | Chapter name, for formats that carry chapters. |
| `url` | `string?` | — | `null` | Web link the marker carries. |
| `frameTarget` | `string?` | — | `null` | Frame target of the web link. |
| `cuePointName` | `string?` | — | `null` | Cue point name, for Flash and streaming cue points. |
| `protectedRegion` | `boolean?` | — | `null` | For composition markers: protect the region the marker spans from being retimed. |
| `eventCuePoint` | `boolean?` | — | `null` | Make the cue point an event rather than navigation. |
| `parameters` | `object?` | — | `null` | Key and value pairs the marker carries, as an object of strings; for cue points and scripts that read them. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "time": {
      "description": "Time in seconds within the composition; the composition\u0027s current time when empty.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "comment": {
      "description": "The marker\u0027s comment, shown on it in the timeline.",
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
    "layer": {
      "description": "A layer by name or index for layer markers; the composition\u0027s own markers when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Duration of the marker in seconds; 0 for a point marker.",
      "type": [
        "number",
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
    "chapter": {
      "description": "Chapter name, for formats that carry chapters.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "url": {
      "description": "Web link the marker carries.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "frameTarget": {
      "description": "Frame target of the web link.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "cuePointName": {
      "description": "Cue point name, for Flash and streaming cue points.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "protectedRegion": {
      "description": "For composition markers: protect the region the marker spans from being retimed.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "eventCuePoint": {
      "description": "Make the cue point an event rather than navigation.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "parameters": {
      "description": "Key and value pairs the marker carries, as an object of strings; for cue points and scripts that read them.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>remove_<wbr>markers</code>

Remove markers of the composition, or of a layer when one is named: those at the indices or times given, or all of them.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `layer` | `string?` | — | `null` | A layer by name or index for layer markers; the composition's own markers when empty. |
| `indices` | `array?` | — | `null` | Marker indices as ae_list_markers numbers them, 1 first; the markers at times, or all markers, when empty. |
| `times` | `array?` | — | `null` | Times in seconds of the markers meant; the markers at indices, or all markers, when empty. |

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
    "layer": {
      "description": "A layer by name or index for layer markers; the composition\u0027s own markers when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "indices": {
      "description": "Marker indices as ae_list_markers numbers them, 1 first; the markers at times, or all markers, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "times": {
      "description": "Times in seconds of the markers meant; the markers at indices, or all markers, when empty.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>time_<wbr>remap</code>

Time remapping of a footage or nested composition layer: switch it on, which puts keys at the layer's start and end, and set the keys that map composition time to source time (both in seconds) for freeze frames, slow motion and reversal. Ease the keys with ae_ease_keyframes on the path ADBE Time Remapping; enabled false switches it off.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `enabled` | `boolean` | — | `true` | Switch time remapping on (the default) or off. |
| `keys` | `array?` | — | `null` | Keyframes as {time, value}: the composition time and the source time it shows, both in seconds. |
| `replace` | `boolean` | — | `false` | Remove the existing time remap keys before setting the new ones. |

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
    "enabled": {
      "description": "Switch time remapping on (the default) or off.",
      "type": "boolean",
      "default": true
    },
    "keys": {
      "description": "Keyframes as {time, value}: the composition time and the source time it shows, both in seconds.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "replace": {
      "description": "Remove the existing time remap keys before setting the new ones.",
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

