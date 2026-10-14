# Motion graphics templates

Part of the `templates` skill: these tools appear once it is loaded — by `ae_enable_skill` with `templates`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>template_<wbr>info</code>

A composition as a motion graphics template: its template name, the controllers exposed in Essential Graphics, and the instances of templates among its layers (nested compositions with essential properties) with each property's name, type and value.

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

## <code>ae_<wbr>update_<wbr>template</code>

Name the template a composition exports as, or open it in the Essential Graphics panel.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | The template name, shown in Premiere Pro and the Essential Graphics panel. |
| `openPanel` | `boolean` | — | `false` | Open the composition in the Essential Graphics panel. |

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
    "name": {
      "description": "The template name, shown in Premiere Pro and the Essential Graphics panel.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "openPanel": {
      "description": "Open the composition in the Essential Graphics panel.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>add_<wbr>template_<wbr>property</code>

Expose a property of a layer as a controller of the composition's template, under a name of your choosing: a number, colour, checkbox, popup, text, 2D point or media property; others are refused with the reason.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `path` | `string` | yes | — | Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display names in After Effects' language, the English names of the common ones (Transform/Position, Effects/&lt;effect&gt;/&lt;parameter&gt;, Masks/1/Mask Path, Text/Source Text), or 1-based indices. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name of the controller; the property's own name when empty. |

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
    "name": {
      "description": "Name of the controller; the property\u0027s own name when empty.",
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

## <code>ae_<wbr>fill_<wbr>template</code>

Fill an instance of a template: set the essential properties of a nested composition layer by controller name (text as a string, colours as #rrggbb or [r, g, b], numbers, points; a media controller takes a footage item by name or id, null to clear), as keyframes when time is given. The answer lists the properties afterwards.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The nested composition layer that is the template instance, by name or index. |
| `values` | `object` | yes | — | An object of controller names (or indices as ae_template_info numbers them) to values. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the values as keyframes at this time. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The nested composition layer that is the template instance, by name or index.",
      "type": "string"
    },
    "values": {
      "description": "An object of controller names (or indices as ae_template_info numbers them) to values.",
      "type": "object"
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
      "description": "Set the values as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "values"
  ]
}
```

</details>

## <code>ae_<wbr>substitute_<wbr>template</code>

Customise a template composition directly by layer name: new text for text layers, a colour for solids, text, shape fills or a Fill effect, and new footage or compositions for footage layers. Use it on compositions that are templates by convention rather than Essential Graphics.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `text` | `object?` | — | `null` | Layer names to the new text of each text layer. |
| `colors` | `object?` | — | `null` | Layer names to the new colour of each, as #rrggbb or [r, g, b]. |
| `footage` | `object?` | — | `null` | Layer names to the footage item or composition (by name or id) that replaces each layer's source. |

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
    "text": {
      "description": "Layer names to the new text of each text layer.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "colors": {
      "description": "Layer names to the new colour of each, as #rrggbb or [r, g, b].",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "footage": {
      "description": "Layer names to the footage item or composition (by name or id) that replaces each layer\u0027s source.",
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

## <code>ae_<wbr>export_<wbr>template</code>

Export a composition as a .mogrt motion graphics template, to a file or folder, or into the Essential Graphics library when no path is given. The composition needs at least one controller; its template name defaults to its own.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `path` | `string?` | — | `null` | A .mogrt file path, or a folder to take the template's name; the Essential Graphics library when empty. |
| `overwrite` | `boolean` | — | `false` | Replace a file that is already there. |
| `timeoutSeconds` | `integer` | — | `120` | Seconds to wait for After Effects, 1 to 600. |

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
    "path": {
      "description": "A .mogrt file path, or a folder to take the template\u0027s name; the Essential Graphics library when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "overwrite": {
      "description": "Replace a file that is already there.",
      "type": "boolean",
      "default": false
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 120
    }
  }
}
```

</details>

## <code>ae_<wbr>import_<wbr>template</code>

Import a .mogrt file into the project as a composition, with its controllers; into a folder when one is named. Where this After Effects refuses scripted import the answer says so.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string` | yes | — | Path of the .mogrt file. |
| `folder` | `string?` | — | `null` | Project folder to put it in, by name or id; the root when empty. |
| `open` | `boolean` | — | `false` | Open the imported composition in the viewer. |
| `timeoutSeconds` | `integer` | — | `120` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Path of the .mogrt file.",
      "type": "string"
    },
    "folder": {
      "description": "Project folder to put it in, by name or id; the root when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "open": {
      "description": "Open the imported composition in the viewer.",
      "type": "boolean",
      "default": false
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 120
    }
  },
  "required": [
    "path"
  ]
}
```

</details>

