# Shape layers and masks

Part of the `shapes` skill: these tools appear once it is loaded — by `ae_enable_skill` with `shapes`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>shape_<wbr>contents</code>

The contents of a shape layer as a tree: groups with their transform, the shapes (rectangle, ellipse, star, path), the paint (fill, stroke, gradients) and the operators (trim paths, repeater, merge, offset, round corners, pucker and bloat, twist, wiggle, zig zag), each with its settings by plain name and the path the other tools take.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The shape layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `path` | `string?` | — | `null` | Group to start from, by path; the whole contents when empty. |
| `depth` | `integer` | — | `3` | How many levels of groups to open; 3 by default. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The shape layer by name or index.",
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
      "description": "Group to start from, by path; the whole contents when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "depth": {
      "description": "How many levels of groups to open; 3 by default.",
      "type": "integer",
      "default": 3
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>add_<wbr>shape</code>

Add an item to a shape layer: a group, a rectangle, ellipse, star or path, a fill or stroke, or an operator, into the layer's contents or a group, with its settings. A shape added at the top level with fill or stroke gets its own group with the paint, which is how After Effects draws it; the answer carries the paths to animate it with the keyframe tools.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The shape layer by name or index. |
| `kind` | `string` | yes | — | group, rect, ellipse, star, path, fill, stroke, gradient_fill, gradient_stroke, trim, repeater, merge, offset, round_corners, pucker_bloat, twist, wiggle_paths, wiggle_transform, or zigzag. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `into` | `string?` | — | `null` | The group to add into, by path; the layer's contents when empty. |
| `name` | `string?` | — | `null` | Name of the new item, or of the group made for it. |
| `settings` | `object?` | — | `null` | Settings as an object of plain names: size, position, roundness for rect and ellipse; points, inner_radius, outer_radius, rotation, type (1 star, 2 polygon) for star; path as {vertices, in_tangents, out_tangents, closed} for path; color, opacity, fill_rule for fill; color, width, opacity, line_cap, line_join, miter_limit, dashes [dash, gap, ...], dash_offset for stroke; start, end, offset, type (1 simultaneously, 2 individually) for trim; copies, offset, anchor, position, scale, rotation, start_opacity, end_opacity for repeater; amount, radius, angle, center, size, detail, ridges, points, frequency, seed for the other operators; anchor, position, scale, skew, skew_axis, rotation, opacity for a group. A match name works too. |
| `fill` | `any` | — | `null` | Add a fill of this colour beside the shape. |
| `stroke` | `any` | — | `null` | Add a stroke of this colour beside the shape. |
| `strokeWidth` | `number?` | — | `null` | Width of that stroke in pixels. |
| `at` | `integer?` | — | `null` | Position in the group, 1 first; at the end when empty. Paint and operators act on what is above them. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The shape layer by name or index.",
      "type": "string"
    },
    "kind": {
      "description": "group, rect, ellipse, star, path, fill, stroke, gradient_fill, gradient_stroke, trim, repeater, merge, offset, round_corners, pucker_bloat, twist, wiggle_paths, wiggle_transform, or zigzag.",
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
    "into": {
      "description": "The group to add into, by path; the layer\u0027s contents when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "name": {
      "description": "Name of the new item, or of the group made for it.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "settings": {
      "description": "Settings as an object of plain names: size, position, roundness for rect and ellipse; points, inner_radius, outer_radius, rotation, type (1 star, 2 polygon) for star; path as {vertices, in_tangents, out_tangents, closed} for path; color, opacity, fill_rule for fill; color, width, opacity, line_cap, line_join, miter_limit, dashes [dash, gap, ...], dash_offset for stroke; start, end, offset, type (1 simultaneously, 2 individually) for trim; copies, offset, anchor, position, scale, rotation, start_opacity, end_opacity for repeater; amount, radius, angle, center, size, detail, ridges, points, frequency, seed for the other operators; anchor, position, scale, skew, skew_axis, rotation, opacity for a group. A match name works too.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "fill": {
      "description": "Add a fill of this colour beside the shape.",
      "default": null
    },
    "stroke": {
      "description": "Add a stroke of this colour beside the shape.",
      "default": null
    },
    "strokeWidth": {
      "description": "Width of that stroke in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "at": {
      "description": "Position in the group, 1 first; at the end when empty. Paint and operators act on what is above them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "kind"
  ]
}
```

</details>

## <code>ae_<wbr>update_<wbr>shape</code>

Change an item of a shape layer by path: rename, switch on or off, set any of its settings (a group's transform when it is a group), or move it to another position in its group. With time a setting becomes a keyframe.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The shape layer by name or index. |
| `path` | `string` | yes | — | The item by path as ae_shape_contents gives it. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | New name. |
| `enabled` | `boolean?` | — | `null` | Switch the item on or off. |
| `settings` | `object?` | — | `null` | Settings as an object of plain names: size, position, roundness for rect and ellipse; points, inner_radius, outer_radius, rotation, type (1 star, 2 polygon) for star; path as {vertices, in_tangents, out_tangents, closed} for path; color, opacity, fill_rule for fill; color, width, opacity, line_cap, line_join, miter_limit, dashes [dash, gap, ...], dash_offset for stroke; start, end, offset, type (1 simultaneously, 2 individually) for trim; copies, offset, anchor, position, scale, rotation, start_opacity, end_opacity for repeater; amount, radius, angle, center, size, detail, ridges, points, frequency, seed for the other operators; anchor, position, scale, skew, skew_axis, rotation, opacity for a group. A match name works too. |
| `time` | `number?` | — | `null` | Set the settings as keyframes at this time. |
| `at` | `integer?` | — | `null` | Move the item to this position in its group, 1 first. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The shape layer by name or index.",
      "type": "string"
    },
    "path": {
      "description": "The item by path as ae_shape_contents gives it.",
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
    "name": {
      "description": "New name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "enabled": {
      "description": "Switch the item on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "settings": {
      "description": "Settings as an object of plain names: size, position, roundness for rect and ellipse; points, inner_radius, outer_radius, rotation, type (1 star, 2 polygon) for star; path as {vertices, in_tangents, out_tangents, closed} for path; color, opacity, fill_rule for fill; color, width, opacity, line_cap, line_join, miter_limit, dashes [dash, gap, ...], dash_offset for stroke; start, end, offset, type (1 simultaneously, 2 individually) for trim; copies, offset, anchor, position, scale, rotation, start_opacity, end_opacity for repeater; amount, radius, angle, center, size, detail, ridges, points, frequency, seed for the other operators; anchor, position, scale, skew, skew_axis, rotation, opacity for a group. A match name works too.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the settings as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "at": {
      "description": "Move the item to this position in its group, 1 first.",
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

## <code>ae_<wbr>remove_<wbr>shape</code>

Remove an item of a shape layer by path, with everything inside it.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The shape layer by name or index. |
| `path` | `string` | yes | — | The item by path as ae_shape_contents gives it. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The shape layer by name or index.",
      "type": "string"
    },
    "path": {
      "description": "The item by path as ae_shape_contents gives it.",
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

## <code>ae_<wbr>list_<wbr>masks</code>

The masks of a layer: name, mode, inverted, feather, opacity, expansion, colour, how many vertices the path has and whether it is animated. Any layer with pixels can carry masks; text runs along one with ae_text_path.

*read-only · repeat-safe · stays inside After Effects.*

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

## <code>ae_<wbr>add_<wbr>mask</code>

Add a mask to a layer: a rectangle or an ellipse from [x, y, width, height] in layer pixels, or a path from vertices with tangents; with mode, inverted, feather, opacity and expansion. The answer names the mask for later changes.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name of the mask. |
| `rectangle` | `array?` | — | `null` | A rectangle as [x, y, width, height] in layer pixels. |
| `ellipse` | `array?` | — | `null` | An ellipse inside [x, y, width, height] in layer pixels. |
| `vertices` | `array?` | — | `null` | Vertices of the path as [[x, y], ...] in layer pixels. |
| `inTangents` | `array?` | — | `null` | In tangents, one [x, y] per vertex relative to it; straight when empty. |
| `outTangents` | `array?` | — | `null` | Out tangents, one [x, y] per vertex relative to it; straight when empty. |
| `closed` | `boolean?` | — | `null` | Close the path; true by default. |
| `mode` | `string?` | — | `null` | NONE, ADD, SUBTRACT, INTERSECT, LIGHTEN, DARKEN or DIFFERENCE. |
| `inverted` | `boolean?` | — | `null` | Invert the mask. |
| `feather` | `any` | — | `null` | Feather in pixels, one number or [x, y]. |
| `opacity` | `number?` | — | `null` | Mask opacity in percent. |
| `expansion` | `number?` | — | `null` | Mask expansion in pixels; negative contracts. |
| `color` | `any` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `featherPoints` | `array?` | — | `null` | Variable-width feather points along the mask path, as an array of objects: segment (1 first), position along it (0 to 1, 0.5 by default), radius in pixels, inner (true feathers inward), tension (0 to 1), hold, corner_angle; an empty array removes them all. |
| `featherFalloff` | `string?` | — | `null` | How the feather falls off: FFO_SMOOTH or FFO_LINEAR. |

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
    "name": {
      "description": "Name of the mask.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "rectangle": {
      "description": "A rectangle as [x, y, width, height] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "ellipse": {
      "description": "An ellipse inside [x, y, width, height] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "vertices": {
      "description": "Vertices of the path as [[x, y], ...] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "inTangents": {
      "description": "In tangents, one [x, y] per vertex relative to it; straight when empty.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "outTangents": {
      "description": "Out tangents, one [x, y] per vertex relative to it; straight when empty.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "closed": {
      "description": "Close the path; true by default.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "mode": {
      "description": "NONE, ADD, SUBTRACT, INTERSECT, LIGHTEN, DARKEN or DIFFERENCE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "inverted": {
      "description": "Invert the mask.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "feather": {
      "description": "Feather in pixels, one number or [x, y].",
      "default": null
    },
    "opacity": {
      "description": "Mask opacity in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "expansion": {
      "description": "Mask expansion in pixels; negative contracts.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "color": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "default": null
    },
    "featherPoints": {
      "description": "Variable-width feather points along the mask path, as an array of objects: segment (1 first), position along it (0 to 1, 0.5 by default), radius in pixels, inner (true feathers inward), tension (0 to 1), hold, corner_angle; an empty array removes them all.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "featherFalloff": {
      "description": "How the feather falls off: FFO_SMOOTH or FFO_LINEAR.",
      "type": [
        "string",
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

## <code>ae_<wbr>update_<wbr>mask</code>

Change a mask: a new shape (rectangle, ellipse or vertices, as a keyframe when time is given), name, mode, inverted, feather, opacity, expansion, colour, locked, roto bezier.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `mask` | `string` | yes | — | The mask by name or index, 1 first. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | New name. |
| `rectangle` | `array?` | — | `null` | A rectangle as [x, y, width, height] in layer pixels. |
| `ellipse` | `array?` | — | `null` | An ellipse inside [x, y, width, height] in layer pixels. |
| `vertices` | `array?` | — | `null` | Vertices of the path as [[x, y], ...] in layer pixels. |
| `inTangents` | `array?` | — | `null` | In tangents, one [x, y] per vertex relative to it. |
| `outTangents` | `array?` | — | `null` | Out tangents, one [x, y] per vertex relative to it. |
| `closed` | `boolean?` | — | `null` | Close the path. |
| `mode` | `string?` | — | `null` | NONE, ADD, SUBTRACT, INTERSECT, LIGHTEN, DARKEN or DIFFERENCE. |
| `inverted` | `boolean?` | — | `null` | Invert the mask. |
| `feather` | `any` | — | `null` | Feather in pixels, one number or [x, y]. |
| `opacity` | `number?` | — | `null` | Mask opacity in percent. |
| `expansion` | `number?` | — | `null` | Mask expansion in pixels; negative contracts. |
| `color` | `any` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `locked` | `boolean?` | — | `null` | Lock the mask against edits in the interface. |
| `rotoBezier` | `boolean?` | — | `null` | Smooth the path as a roto bezier. |
| `time` | `number?` | — | `null` | Set the shape and the numbers as keyframes at this time. |
| `featherPoints` | `array?` | — | `null` | Variable-width feather points along the mask path, as an array of objects: segment (1 first), position along it (0 to 1, 0.5 by default), radius in pixels, inner (true feathers inward), tension (0 to 1), hold, corner_angle; an empty array removes them all. |
| `featherFalloff` | `string?` | — | `null` | How the feather falls off: FFO_SMOOTH or FFO_LINEAR. |
| `motionBlur` | `string?` | — | `null` | Motion blur of the mask: SAME_AS_LAYER, ON or OFF. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "mask": {
      "description": "The mask by name or index, 1 first.",
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
    "name": {
      "description": "New name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "rectangle": {
      "description": "A rectangle as [x, y, width, height] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "ellipse": {
      "description": "An ellipse inside [x, y, width, height] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "vertices": {
      "description": "Vertices of the path as [[x, y], ...] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "inTangents": {
      "description": "In tangents, one [x, y] per vertex relative to it.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "outTangents": {
      "description": "Out tangents, one [x, y] per vertex relative to it.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "closed": {
      "description": "Close the path.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "mode": {
      "description": "NONE, ADD, SUBTRACT, INTERSECT, LIGHTEN, DARKEN or DIFFERENCE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "inverted": {
      "description": "Invert the mask.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "feather": {
      "description": "Feather in pixels, one number or [x, y].",
      "default": null
    },
    "opacity": {
      "description": "Mask opacity in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "expansion": {
      "description": "Mask expansion in pixels; negative contracts.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "color": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "default": null
    },
    "locked": {
      "description": "Lock the mask against edits in the interface.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "rotoBezier": {
      "description": "Smooth the path as a roto bezier.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the shape and the numbers as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "featherPoints": {
      "description": "Variable-width feather points along the mask path, as an array of objects: segment (1 first), position along it (0 to 1, 0.5 by default), radius in pixels, inner (true feathers inward), tension (0 to 1), hold, corner_angle; an empty array removes them all.",
      "type": [
        "array",
        "null"
      ],
      "default": null
    },
    "featherFalloff": {
      "description": "How the feather falls off: FFO_SMOOTH or FFO_LINEAR.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "motionBlur": {
      "description": "Motion blur of the mask: SAME_AS_LAYER, ON or OFF.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "mask"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>mask</code>

Remove a mask from a layer.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `mask` | `string` | yes | — | The mask by name or index, 1 first. |
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
    "mask": {
      "description": "The mask by name or index, 1 first.",
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
    "mask"
  ]
}
```

</details>

