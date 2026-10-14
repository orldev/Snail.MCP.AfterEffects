# Effects, presets and layer styles

Part of the `effects` skill: these tools appear once it is loaded — by `ae_enable_skill` with `effects`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>effects_<wbr>catalog</code>

The effects this After Effects can apply: display name, match name, category and version, narrowed by a query or a category. Use the match name in ae_add_effect; display names are in the application's language.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `query` | `string?` | — | `null` | Keep only effects whose name or match name contains this, case-insensitive. |
| `category` | `string?` | — | `null` | Keep only effects of this category, as the answer lists them. |
| `limit` | `integer` | — | `60` | Most effects to list; 60 by default. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "query": {
      "description": "Keep only effects whose name or match name contains this, case-insensitive.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "category": {
      "description": "Keep only effects of this category, as the answer lists them.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "limit": {
      "description": "Most effects to list; 60 by default.",
      "type": "integer",
      "default": 60
    }
  }
}
```

</details>

## <code>ae_<wbr>add_<wbr>effect</code>

Apply an effect to a layer by match name or display name, with its parameters set in the same call by their names or indices. The answer lists every parameter with its value, limits and path, which ae_set_keyframes and ae_set_expression take.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `effect` | `string` | yes | — | The effect to apply, by match name (ADBE Gaussian Blur 2) or display name as ae_effects_catalog lists them. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name for the effect on the layer; the effect's own name when empty. |
| `settings` | `object?` | — | `null` | Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters. |
| `at` | `integer?` | — | `null` | Position in the effect stack, 1 first; at the end when empty. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "effect": {
      "description": "The effect to apply, by match name (ADBE Gaussian Blur 2) or display name as ae_effects_catalog lists them.",
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
      "description": "Name for the effect on the layer; the effect\u0027s own name when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "settings": {
      "description": "Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "at": {
      "description": "Position in the effect stack, 1 first; at the end when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "effect"
  ]
}
```

</details>

## <code>ae_<wbr>effect_<wbr>info</code>

The effects on a layer, or one effect in full: every parameter with its type, value at a time, limits, keyframes and expression. Popup parameters take a number, 1 for the first option.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `effect` | `string?` | — | `null` | The effect to describe in full; all effects in brief when empty. |
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
    "effect": {
      "description": "The effect to describe in full; all effects in brief when empty.",
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

## <code>ae_<wbr>update_<wbr>effect</code>

Change an effect on a layer: rename, switch on or off, set parameters by name or index (as keyframes when time is given; a layer parameter takes a layer name or index, null for none), or move it in the stack.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `effect` | `string` | yes | — | The effect by its name on the layer, its match name, or its index in the effects, 1 first. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | New name. |
| `enabled` | `boolean?` | — | `null` | Switch the effect on or off. |
| `settings` | `object?` | — | `null` | Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters. |
| `time` | `number?` | — | `null` | Set the parameters as keyframes at this time. |
| `at` | `integer?` | — | `null` | Move the effect to this position in the stack, 1 first. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "effect": {
      "description": "The effect by its name on the layer, its match name, or its index in the effects, 1 first.",
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
      "description": "Switch the effect on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "settings": {
      "description": "Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the parameters as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "at": {
      "description": "Move the effect to this position in the stack, 1 first.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "effect"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>effect</code>

Remove an effect from a layer.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `effect` | `string` | yes | — | The effect by its name on the layer, its match name, or its index in the effects, 1 first. |
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
    "effect": {
      "description": "The effect by its name on the layer, its match name, or its index in the effects, 1 first.",
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
    "effect"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>dropdown_<wbr>items</code>

Write the entries of a Dropdown Menu Control effect's menu, which expressions and templates then read by index; the only popup whose items a script may change. ae_effect_info shows a dropdown's current items.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the menu property of a Dropdown Menu Control effect, as ae_effect_info lists it. |
| `items` | `array` | yes | — | The entries of the menu, in order. |
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
      "description": "Path of the menu property of a Dropdown Menu Control effect, as ae_effect_info lists it.",
      "type": "string"
    },
    "items": {
      "description": "The entries of the menu, in order.",
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
    "layer",
    "path",
    "items"
  ]
}
```

</details>

## <code>ae_<wbr>find_<wbr>presets</code>

Animation presets (.ffx) installed with After Effects and in the user's presets, found by a word in their name or folder: Text, Transitions, Backgrounds, Behaviors, Shapes and the rest. The name is what ae_apply_preset takes.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `query` | `string?` | — | `null` | A word of the preset's name or folder, case-insensitive; everything when empty. |
| `limit` | `integer` | — | `60` | Most presets to list; 60 by default. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "query": {
      "description": "A word of the preset\u0027s name or folder, case-insensitive; everything when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "limit": {
      "description": "Most presets to list; 60 by default.",
      "type": "integer",
      "default": 60
    }
  }
}
```

</details>

## <code>ae_<wbr>apply_<wbr>preset</code>

Apply an animation preset to layers, by the name ae_find_presets gives or a path to an .ffx file. Presets add effects, keyframes, expressions or text animators; the answer says how many effects each layer gained, and ae_layer_info shows the rest.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layers` | `array` | yes | — | Layers to apply the preset to, each by name or index. |
| `preset` | `string` | yes | — | The preset by name as ae_find_presets gives it, or a path to an .ffx file. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layers": {
      "description": "Layers to apply the preset to, each by name or index.",
      "type": "array",
      "items": {
        "type": "string"
      }
    },
    "preset": {
      "description": "The preset by name as ae_find_presets gives it, or a path to an .ffx file.",
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
    "layers",
    "preset"
  ]
}
```

</details>

## <code>ae_<wbr>layer_<wbr>styles</code>

The layer styles on a layer (drop shadow, glows, bevel, satin, overlays, stroke) with every parameter.

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

## <code>ae_<wbr>set_<wbr>layer_<wbr>style</code>

Add a layer style to a layer, or change one it has: switch it on or off and set its parameters by name (as keyframes when time is given). A new style is added through the Layer menu in After Effects' own language, with the layer selected for a moment; when that fails the answer says so and the style can be added from Layer &gt; Layer Styles by hand.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `style` | `string` | yes | — | drop_shadow, inner_shadow, outer_glow, inner_glow, bevel_emboss, satin, color_overlay, gradient_overlay or stroke. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `enabled` | `boolean?` | — | `null` | Switch the style on or off. |
| `settings` | `object?` | — | `null` | Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters. |
| `time` | `number?` | — | `null` | Set the parameters as keyframes at this time. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "style": {
      "description": "drop_shadow, inner_shadow, outer_glow, inner_glow, bevel_emboss, satin, color_overlay, gradient_overlay or stroke.",
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
      "description": "Switch the style on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "settings": {
      "description": "Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the parameters as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "style"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>layer_<wbr>style</code>

Remove a layer style from a layer.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `style` | `string` | yes | — | drop_shadow, inner_shadow, outer_glow, inner_glow, bevel_emboss, satin, color_overlay, gradient_overlay or stroke. |
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
    "style": {
      "description": "drop_shadow, inner_shadow, outer_glow, inner_glow, bevel_emboss, satin, color_overlay, gradient_overlay or stroke.",
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
    "style"
  ]
}
```

</details>

