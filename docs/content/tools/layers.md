# Layers

## <code>ae_<wbr>list_<wbr>layers</code>

The layers of a composition from top to bottom with index, kind, switches, in and out points, parent and source; filter by kind or name. The index is what later calls take, and it changes when layers are added, removed or moved.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `kind` | `string?` | — | `null` | solid, null, adjustment, text, shape, camera, light, or item (footage or a composition from the project). |
| `nameContains` | `string?` | — | `null` | Only layers whose name contains this, case-insensitive. |
| `limit` | `integer` | — | `200` | How many layers to return at most; the total is reported regardless. |

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
    "kind": {
      "description": "solid, null, adjustment, text, shape, camera, light, or item (footage or a composition from the project).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "nameContains": {
      "description": "Only layers whose name contains this, case-insensitive.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "limit": {
      "description": "How many layers to return at most; the total is reported regardless.",
      "type": "integer",
      "default": 200
    }
  }
}
```

</details>

## <code>ae_<wbr>layer_<wbr>info</code>

Everything about one layer: kind, switches, timing, parent, blending mode, transform values with which of them are animated, effects, mask count, and the text of a text layer.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
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
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>add_<wbr>layer</code>

Add a layer to a composition: a solid, null or adjustment layer, a text layer (a box when width and height are given), an empty shape layer, a camera or light, or a footage item or nested composition from the project. New layers go to the top unless at says otherwise; the answer carries the index later calls use.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `kind` | `string` | — | `solid` | solid, null, adjustment, text, shape, camera, light, or item (footage or a composition from the project). |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name of the new layer. |
| `item` | `string?` | — | `null` | For kind item: the footage or composition, by name or id. |
| `text` | `string?` | — | `null` | For kind text: the text. |
| `vertical` | `boolean` | — | `false` | For kind text: vertical text. |
| `color` | `string?` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `width` | `integer?` | — | `null` | For a solid: its width; for text: the width of a text box. |
| `height` | `integer?` | — | `null` | For a solid: its height; for text: the height of a text box. |
| `duration` | `number?` | — | `null` | Length of the layer in seconds; the composition's otherwise. |
| `position` | `array?` | — | `null` | [x, y] or [x, y, z] in composition pixels; the centre otherwise. |
| `lightType` | `string?` | — | `null` | For kind light: PARALLEL, SPOT, POINT or AMBIENT. |
| `startTime` | `number?` | — | `null` | When the layer starts, in composition seconds. |
| `inPoint` | `number?` | — | `null` | In point in composition seconds. |
| `outPoint` | `number?` | — | `null` | Out point in composition seconds. |
| `at` | `integer?` | — | `null` | Index for the layer, 1 the top; the top otherwise. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "kind": {
      "description": "solid, null, adjustment, text, shape, camera, light, or item (footage or a composition from the project).",
      "type": "string",
      "default": "solid"
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
      "description": "Name of the new layer.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "item": {
      "description": "For kind item: the footage or composition, by name or id.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "text": {
      "description": "For kind text: the text.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "vertical": {
      "description": "For kind text: vertical text.",
      "type": "boolean",
      "default": false
    },
    "color": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "width": {
      "description": "For a solid: its width; for text: the width of a text box.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "height": {
      "description": "For a solid: its height; for text: the height of a text box.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Length of the layer in seconds; the composition\u0027s otherwise.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "position": {
      "description": "[x, y] or [x, y, z] in composition pixels; the centre otherwise.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "lightType": {
      "description": "For kind light: PARALLEL, SPOT, POINT or AMBIENT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "startTime": {
      "description": "When the layer starts, in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "inPoint": {
      "description": "In point in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "outPoint": {
      "description": "Out point in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "at": {
      "description": "Index for the layer, 1 the top; the top otherwise.",
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

## <code>ae_<wbr>update_<wbr>layer</code>

Change a layer's switches and timing: name, video, solo, shy, lock, in and out points, start time, stretch, parent, blending mode, 3D, motion blur, adjustment, guide, label, comment, quality, audio, collapse, time remapping, track matte, auto-orient, preserve transparency, effects and frame blending. Only what is passed changes.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | The new name. |
| `enabled` | `boolean?` | — | `null` | Video switch: whether the layer renders. |
| `solo` | `boolean?` | — | `null` | Solo the layer. |
| `shy` | `boolean?` | — | `null` | Mark the layer shy. |
| `locked` | `boolean?` | — | `null` | Lock the layer. |
| `inPoint` | `number?` | — | `null` | In point in composition seconds. |
| `outPoint` | `number?` | — | `null` | Out point in composition seconds. |
| `startTime` | `number?` | — | `null` | When the layer starts, in composition seconds. |
| `stretch` | `number?` | — | `null` | Time stretch in percent; 100 is normal speed, -100 reversed. |
| `parent` | `string?` | — | `null` | Parent by name or index; empty removes it. |
| `parentJump` | `boolean?` | — | `null` | When parenting, keep the transform values rather than the place on screen. |
| `blendingMode` | `string?` | — | `null` | NORMAL, DISSOLVE, DANCING_DISSOLVE, DARKEN, MULTIPLY, COLOR_BURN, CLASSIC_COLOR_BURN, LINEAR_BURN, DARKER_COLOR, ADD, LIGHTEN, SCREEN, COLOR_DODGE, CLASSIC_COLOR_DODGE, LINEAR_DODGE, LIGHTER_COLOR, OVERLAY, SOFT_LIGHT, HARD_LIGHT, LINEAR_LIGHT, VIVID_LIGHT, PIN_LIGHT, HARD_MIX, DIFFERENCE, CLASSIC_DIFFERENCE, EXCLUSION, SUBTRACT, DIVIDE, HUE, SATURATION, COLOR, LUMINOSITY, STENCIL_ALPHA, STENCIL_LUMA, SILHOUETTE_ALPHA, SILHOUETTE_LUMA, ALPHA_ADD or LUMINESCENT_PREMUL. |
| `threeD` | `boolean?` | — | `null` | Make the layer 3D or flat. |
| `motionBlur` | `boolean?` | — | `null` | Motion blur switch of the layer. |
| `adjustment` | `boolean?` | — | `null` | Make a solid an adjustment layer or back. |
| `guide` | `boolean?` | — | `null` | Guide layer: visible while working, left out of renders. |
| `label` | `integer?` | — | `null` | Label colour as a number 0 to 16 as After Effects numbers them; 0 is none. |
| `comment` | `string?` | — | `null` | Comment shown in the timeline. |
| `quality` | `string?` | — | `null` | Rendering quality: BEST, DRAFT or WIREFRAME. |
| `audioEnabled` | `boolean?` | — | `null` | Audio switch of a layer that has sound. |
| `collapse` | `boolean?` | — | `null` | Collapse transformations (continuous rasterisation for vectors). |
| `timeRemap` | `boolean?` | — | `null` | Switch time remapping on, which adds the Time Remap property. |
| `trackMatte` | `string?` | — | `null` | Track matte layer by name or index; pair with trackMatteType. |
| `trackMatteType` | `string?` | — | `null` | ALPHA (the default when a matte layer is named), ALPHA_INVERTED, LUMA, LUMA_INVERTED, or NONE to remove the matte. |
| `autoOrient` | `string?` | — | `null` | NO_AUTO_ORIENT, ALONG_PATH, CAMERA_OR_POINT_OF_INTEREST or CHARACTERS_TOWARD_CAMERA. |
| `preserveTransparency` | `boolean?` | — | `null` | Preserve underlying transparency. |
| `effectsActive` | `boolean?` | — | `null` | Effects switch of the layer. |
| `frameBlending` | `string?` | — | `null` | NO_FRAME_BLEND, FRAME_MIX or PIXEL_MOTION. |

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
      "description": "The new name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "enabled": {
      "description": "Video switch: whether the layer renders.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "solo": {
      "description": "Solo the layer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "shy": {
      "description": "Mark the layer shy.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "locked": {
      "description": "Lock the layer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "inPoint": {
      "description": "In point in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "outPoint": {
      "description": "Out point in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "startTime": {
      "description": "When the layer starts, in composition seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "stretch": {
      "description": "Time stretch in percent; 100 is normal speed, -100 reversed.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "parent": {
      "description": "Parent by name or index; empty removes it.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "parentJump": {
      "description": "When parenting, keep the transform values rather than the place on screen.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "blendingMode": {
      "description": "NORMAL, DISSOLVE, DANCING_DISSOLVE, DARKEN, MULTIPLY, COLOR_BURN, CLASSIC_COLOR_BURN, LINEAR_BURN, DARKER_COLOR, ADD, LIGHTEN, SCREEN, COLOR_DODGE, CLASSIC_COLOR_DODGE, LINEAR_DODGE, LIGHTER_COLOR, OVERLAY, SOFT_LIGHT, HARD_LIGHT, LINEAR_LIGHT, VIVID_LIGHT, PIN_LIGHT, HARD_MIX, DIFFERENCE, CLASSIC_DIFFERENCE, EXCLUSION, SUBTRACT, DIVIDE, HUE, SATURATION, COLOR, LUMINOSITY, STENCIL_ALPHA, STENCIL_LUMA, SILHOUETTE_ALPHA, SILHOUETTE_LUMA, ALPHA_ADD or LUMINESCENT_PREMUL.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "threeD": {
      "description": "Make the layer 3D or flat.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "motionBlur": {
      "description": "Motion blur switch of the layer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "adjustment": {
      "description": "Make a solid an adjustment layer or back.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "guide": {
      "description": "Guide layer: visible while working, left out of renders.",
      "type": [
        "boolean",
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
    "comment": {
      "description": "Comment shown in the timeline.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "quality": {
      "description": "Rendering quality: BEST, DRAFT or WIREFRAME.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "audioEnabled": {
      "description": "Audio switch of a layer that has sound.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "collapse": {
      "description": "Collapse transformations (continuous rasterisation for vectors).",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "timeRemap": {
      "description": "Switch time remapping on, which adds the Time Remap property.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "trackMatte": {
      "description": "Track matte layer by name or index; pair with trackMatteType.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "trackMatteType": {
      "description": "ALPHA (the default when a matte layer is named), ALPHA_INVERTED, LUMA, LUMA_INVERTED, or NONE to remove the matte.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "autoOrient": {
      "description": "NO_AUTO_ORIENT, ALONG_PATH, CAMERA_OR_POINT_OF_INTEREST or CHARACTERS_TOWARD_CAMERA.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "preserveTransparency": {
      "description": "Preserve underlying transparency.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "effectsActive": {
      "description": "Effects switch of the layer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "frameBlending": {
      "description": "NO_FRAME_BLEND, FRAME_MIX or PIXEL_MOTION.",
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

## <code>ae_<wbr>transform_<wbr>layer</code>

Set a layer's anchor point, position, scale, rotation, opacity and, for 3D layers, orientation and X/Y rotation. A property that already has keyframes gets a keyframe at time instead of a value everywhere; the answer shows the transform and which of it is animated.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `anchor` | `array?` | — | `null` | Anchor point [x, y] or [x, y, z] in layer pixels. |
| `position` | `array?` | — | `null` | Position [x, y] or [x, y, z] in composition pixels. |
| `scale` | `array?` | — | `null` | Scale [x, y] or [x, y, z] in percent; 100 keeps the size. |
| `rotation` | `number?` | — | `null` | Rotation in degrees; the Z rotation of a 3D layer. |
| `opacity` | `number?` | — | `null` | Opacity in percent, 0 to 100. |
| `orientation` | `array?` | — | `null` | 3D layers: orientation [x, y, z] in degrees. |
| `xRotation` | `number?` | — | `null` | 3D layers: X rotation in degrees. |
| `yRotation` | `number?` | — | `null` | 3D layers: Y rotation in degrees. |
| `time` | `number?` | — | `null` | Keyed at this time when the property has keyframes; the composition's current time when empty. |

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
    "anchor": {
      "description": "Anchor point [x, y] or [x, y, z] in layer pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "position": {
      "description": "Position [x, y] or [x, y, z] in composition pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "scale": {
      "description": "Scale [x, y] or [x, y, z] in percent; 100 keeps the size.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "rotation": {
      "description": "Rotation in degrees; the Z rotation of a 3D layer.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "opacity": {
      "description": "Opacity in percent, 0 to 100.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "orientation": {
      "description": "3D layers: orientation [x, y, z] in degrees.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "xRotation": {
      "description": "3D layers: X rotation in degrees.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "yRotation": {
      "description": "3D layers: Y rotation in degrees.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Keyed at this time when the property has keyframes; the composition\u0027s current time when empty.",
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

## <code>ae_<wbr>remove_<wbr>layers</code>

Delete one or more layers from a composition; the footage they used stays in the project.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layers` | `array` | yes | — | Layers to delete, each by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layers": {
      "description": "Layers to delete, each by name or index.",
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
    }
  },
  "required": [
    "layers"
  ]
}
```

</details>

## <code>ae_<wbr>duplicate_<wbr>layer</code>

Duplicate a layer right above the original, with its effects, masks and keyframes, optionally under a new name.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name for the copy. |

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
      "description": "Name for the copy.",
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

## <code>ae_<wbr>move_<wbr>layer</code>

Move a layer to an index, to the top or bottom, or right above or below another layer.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `to` | `string?` | — | `null` | Where to: an index (1 is the top), top or bottom. |
| `before` | `string?` | — | `null` | Put it right above this layer, by name or index. |
| `after` | `string?` | — | `null` | Put it right below this layer, by name or index. |

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
    "to": {
      "description": "Where to: an index (1 is the top), top or bottom.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "before": {
      "description": "Put it right above this layer, by name or index.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "after": {
      "description": "Put it right below this layer, by name or index.",
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

## <code>ae_<wbr>select_<wbr>layers</code>

Select layers in the timeline, which is what menu commands and the user's next action act on.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layers` | `array?` | — | `null` | Layers to select, each by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `mode` | `string` | — | `replace` | replace (default), add (keep the selection), none (deselect all) or all. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layers": {
      "description": "Layers to select, each by name or index.",
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
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "mode": {
      "description": "replace (default), add (keep the selection), none (deselect all) or all.",
      "type": "string",
      "default": "replace"
    }
  }
}
```

</details>

