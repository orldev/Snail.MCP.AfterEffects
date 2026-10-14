# Project and application settings

Part of the `settings` skill: these tools appear once it is loaded — by `ae_enable_skill` with `settings`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>project_<wbr>settings</code>

The project settings as Project Settings shows them, read and set in one call: time display (timecode or frames, the start frame), colour depth, linear blending, working space and gamma, colour management and OCIO config, LUT interpolation, the expression engine, GPU acceleration (with what the machine offers), transparency grid thumbnails, the active tool. With colorProfiles the answer lists the colour profiles a working space can be.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `timeDisplay` | `string?` | — | `null` | TIMECODE or FRAMES. |
| `displayStartFrame` | `integer?` | — | `null` | The frame numbers start at 0 or 1. |
| `framesCount` | `string?` | — | `null` | FC_START_0, FC_START_1 or FC_TIMECODE_CONVERSION. |
| `framesUseFeetFrames` | `boolean?` | — | `null` | Count frames in feet and frames. |
| `feetFramesFilm` | `string?` | — | `null` | MM16 or MM35, for feet and frames. |
| `footageTimecodeStart` | `string?` | — | `null` | FTCS_START_0 or FTCS_USE_SOURCE_MEDIA: where footage timecode starts. |
| `bitsPerChannel` | `integer?` | — | `null` | Colour depth: 8, 16 or 32 bits per channel. |
| `linearBlending` | `boolean?` | — | `null` | Blend colours in linear light. |
| `workingSpace` | `string?` | — | `null` | The working colour space by profile name, as the colour profiles list spells it; empty for none. |
| `workingGamma` | `number?` | — | `null` | Working gamma, 2.2 or 2.4, where the colour management system has it. |
| `linearizeWorkingSpace` | `boolean?` | — | `null` | Linearize the working space. |
| `compensateForSceneReferredProfiles` | `boolean?` | — | `null` | Compensate for scene-referred profiles. |
| `colorManagementSystem` | `integer?` | — | `null` | Colour management system as After Effects numbers them, where the version has the setting. |
| `ocioConfigurationFile` | `string?` | — | `null` | Path of an OCIO configuration file; an empty string returns to the built-in one. |
| `lutInterpolationMethod` | `integer?` | — | `null` | LUT interpolation method as After Effects numbers them. |
| `expressionEngine` | `string?` | — | `null` | javascript-1.0 or extendscript. |
| `gpuAccel` | `string?` | — | `null` | METAL, CUDA, OPENCL, DIRECTX, VULKAN or SOFTWARE; the answer lists what this machine offers. |
| `transparencyGridThumbnails` | `boolean?` | — | `null` | Show thumbnails over a transparency grid. |
| `tool` | `string?` | — | `null` | The active tool, as ToolType names it (Tool_Arrow, Tool_Pen, Tool_TextH and so on). |
| `colorProfiles` | `boolean` | — | `false` | List the colour profiles the working space can be set to. |
| `defaultImportFolder` | `string?` | — | `null` | Folder the Import dialog opens in, on After Effects' machine. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "timeDisplay": {
      "description": "TIMECODE or FRAMES.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "displayStartFrame": {
      "description": "The frame numbers start at 0 or 1.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "framesCount": {
      "description": "FC_START_0, FC_START_1 or FC_TIMECODE_CONVERSION.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "framesUseFeetFrames": {
      "description": "Count frames in feet and frames.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "feetFramesFilm": {
      "description": "MM16 or MM35, for feet and frames.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "footageTimecodeStart": {
      "description": "FTCS_START_0 or FTCS_USE_SOURCE_MEDIA: where footage timecode starts.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "bitsPerChannel": {
      "description": "Colour depth: 8, 16 or 32 bits per channel.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "linearBlending": {
      "description": "Blend colours in linear light.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "workingSpace": {
      "description": "The working colour space by profile name, as the colour profiles list spells it; empty for none.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "workingGamma": {
      "description": "Working gamma, 2.2 or 2.4, where the colour management system has it.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "linearizeWorkingSpace": {
      "description": "Linearize the working space.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "compensateForSceneReferredProfiles": {
      "description": "Compensate for scene-referred profiles.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "colorManagementSystem": {
      "description": "Colour management system as After Effects numbers them, where the version has the setting.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "ocioConfigurationFile": {
      "description": "Path of an OCIO configuration file; an empty string returns to the built-in one.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "lutInterpolationMethod": {
      "description": "LUT interpolation method as After Effects numbers them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "expressionEngine": {
      "description": "javascript-1.0 or extendscript.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "gpuAccel": {
      "description": "METAL, CUDA, OPENCL, DIRECTX, VULKAN or SOFTWARE; the answer lists what this machine offers.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "transparencyGridThumbnails": {
      "description": "Show thumbnails over a transparency grid.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "tool": {
      "description": "The active tool, as ToolType names it (Tool_Arrow, Tool_Pen, Tool_TextH and so on).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "colorProfiles": {
      "description": "List the colour profiles the working space can be set to.",
      "type": "boolean",
      "default": false
    },
    "defaultImportFolder": {
      "description": "Folder the Import dialog opens in, on After Effects\u0027 machine.",
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

## <code>ae_<wbr>replace_<wbr>in_<wbr>expressions</code>

Replace text in every expression of the project, as After Effects does when it renames something: a layer name, a property name, a comp name that expressions refer to.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `find` | `string` | yes | — | Text to find in the expressions. |
| `replace` | `string` | — | `` | Text to put in its place; empty removes the text. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "find": {
      "description": "Text to find in the expressions.",
      "type": "string"
    },
    "replace": {
      "description": "Text to put in its place; empty removes the text.",
      "type": "string",
      "default": ""
    }
  },
  "required": [
    "find"
  ]
}
```

</details>

## <code>ae_<wbr>project_<wbr>fonts</code>

The fonts the project uses, each with the layers that use it, and the fonts that are missing or substituted on this machine.

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

## <code>ae_<wbr>replace_<wbr>font</code>

Replace one font with another across the project, by PostScript names as ae_text_fonts gives them.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `from` | `string` | yes | — | PostScript name of the font to replace. |
| `to` | `string` | yes | — | PostScript name of the font to use instead. |
| `reloadMissing` | `boolean` | — | `false` | Also reload footage whose fonts were missing. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "from": {
      "description": "PostScript name of the font to replace.",
      "type": "string"
    },
    "to": {
      "description": "PostScript name of the font to use instead.",
      "type": "string"
    },
    "reloadMissing": {
      "description": "Also reload footage whose fonts were missing.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "from",
    "to"
  ]
}
```

</details>

## <code>ae_<wbr>import_<wbr>placeholder</code>

Add a placeholder footage item of a size, frame rate and duration, to lay out a composition before the footage exists; ae_replace_footage swaps the file in later.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string?` | — | `null` | Name of the placeholder. |
| `width` | `integer?` | — | `null` | Width in pixels; 1920 by default. |
| `height` | `integer?` | — | `null` | Height in pixels; 1080 by default. |
| `frameRate` | `number?` | — | `null` | Frame rate; 30 by default. |
| `duration` | `number?` | — | `null` | Duration in seconds; 10 by default. |
| `folder` | `string?` | — | `null` | Project folder to put it in, by name or id. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Name of the placeholder.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "width": {
      "description": "Width in pixels; 1920 by default.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "height": {
      "description": "Height in pixels; 1080 by default.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "frameRate": {
      "description": "Frame rate; 30 by default.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Duration in seconds; 10 by default.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "folder": {
      "description": "Project folder to put it in, by name or id.",
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

## <code>ae_<wbr>purge</code>

Purge After Effects' caches: all, memory, images, snapshots or undo, as the Edit &gt; Purge menu offers.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `target` | `string` | — | `ALL_CACHES` | ALL_CACHES, ALL_MEMORY_CACHES, IMAGE_CACHES, SNAPSHOT_CACHES or UNDO_CACHES. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "target": {
      "description": "ALL_CACHES, ALL_MEMORY_CACHES, IMAGE_CACHES, SNAPSHOT_CACHES or UNDO_CACHES.",
      "type": "string",
      "default": "ALL_CACHES"
    }
  }
}
```

</details>

## <code>ae_<wbr>performance</code>

Memory and multi-frame rendering of After Effects: the image cache and total memory percentages, and multi-frame rendering on or off with a CPU ceiling; the preferences the Performance pane holds.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `imageCachePercent` | `integer?` | — | `null` | Percent of memory for the image cache; goes with maxMemoryPercent. |
| `maxMemoryPercent` | `integer?` | — | `null` | Percent of memory After Effects may use in all. |
| `multiFrameRendering` | `boolean?` | — | `null` | Multi-frame rendering on or off. |
| `maxCpuPercent` | `integer?` | — | `null` | CPU percent multi-frame rendering may take, 1 to 100. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "imageCachePercent": {
      "description": "Percent of memory for the image cache; goes with maxMemoryPercent.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "maxMemoryPercent": {
      "description": "Percent of memory After Effects may use in all.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "multiFrameRendering": {
      "description": "Multi-frame rendering on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "maxCpuPercent": {
      "description": "CPU percent multi-frame rendering may take, 1 to 100.",
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

## <code>ae_<wbr>system_<wbr>info</code>

The machine After Effects runs on and the application itself: machine and user name, operating system and version, After Effects' version, build and language, memory in use, whether it is a render engine or a beta, the rendering and crash-saving flags, where its preferences live, and environment variables by name.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `environment` | `array?` | — | `null` | Environment variables to read on After Effects' machine, by name. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "environment": {
      "description": "Environment variables to read on After Effects\u0027 machine, by name.",
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

## <code>ae_<wbr>set_<wbr>application</code>

Application-wide switches: rendering of previews off while scripts work, an error on missing footage frames, saving the project on a crash, writing the preferences on quit.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `disableRendering` | `boolean?` | — | `null` | Stop After Effects rendering previews while scripts run; speeds long batches. |
| `reportErrorOnMissingFrame` | `boolean?` | — | `null` | Stop and report when a frame of footage is missing, rather than render on. |
| `saveProjectOnCrash` | `boolean?` | — | `null` | Save the project when After Effects crashes. |
| `savePreferencesOnQuit` | `boolean?` | — | `null` | Write the preferences when After Effects quits. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "disableRendering": {
      "description": "Stop After Effects rendering previews while scripts run; speeds long batches.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "reportErrorOnMissingFrame": {
      "description": "Stop and report when a frame of footage is missing, rather than render on.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "saveProjectOnCrash": {
      "description": "Save the project when After Effects crashes.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "savePreferencesOnQuit": {
      "description": "Write the preferences when After Effects quits.",
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

## <code>ae_<wbr>read_<wbr>swatches</code>

Read an Adobe swatch file (.ase or .aco) through After Effects: each swatch's name, colour space and values.

*read-only · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `path` | `string` | yes | — | Path of the .ase or .aco swatch file on After Effects' machine. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "path": {
      "description": "Path of the .ase or .aco swatch file on After Effects\u0027 machine.",
      "type": "string"
    }
  },
  "required": [
    "path"
  ]
}
```

</details>

## <code>ae_<wbr>system_<wbr>call</code>

Run a shell command on After Effects' machine through After Effects and return its output; it waits for the command to finish. Script-scope work: a key without that scope is refused.

*may overwrite or delete · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `command` | `string` | yes | — | The shell line to run. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "command": {
      "description": "The shell line to run.",
      "type": "string"
    }
  },
  "required": [
    "command"
  ]
}
```

</details>

## <code>ae_<wbr>preference</code>

Read one After Effects preference by section and key, as the preferences file spells them (ae_system_info says where it is), read as a string, long, float or bool.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `section` | `string` | yes | — | The section in the preferences file, such as "Main Pref Section v2". |
| `key` | `string` | yes | — | The key within the section, such as "Pref_SCRIPTING_FILE_NETWORK_SECURITY". |
| `kind` | `string` | — | `string` | How the value is read or written: string, long, float or bool. |
| `file` | `string?` | — | `null` | Which preferences file: MACHINE_INDEPENDENT (the default), MACHINE_SPECIFIC, MACHINE_INDEPENDENT_RENDER, MACHINE_INDEPENDENT_OUTPUT, MACHINE_INDEPENDENT_COMPOSITION, MACHINE_SPECIFIC_TEXT, MACHINE_SPECIFIC_PAINT, MACHINE_SPECIFIC_EFFECTS, MACHINE_SPECIFIC_EXPRESSION_SNIPPETS or MACHINE_SPECIFIC_SCRIPT_SNIPPETS. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "section": {
      "description": "The section in the preferences file, such as \u0022Main Pref Section v2\u0022.",
      "type": "string"
    },
    "key": {
      "description": "The key within the section, such as \u0022Pref_SCRIPTING_FILE_NETWORK_SECURITY\u0022.",
      "type": "string"
    },
    "kind": {
      "description": "How the value is read or written: string, long, float or bool.",
      "type": "string",
      "default": "string"
    },
    "file": {
      "description": "Which preferences file: MACHINE_INDEPENDENT (the default), MACHINE_SPECIFIC, MACHINE_INDEPENDENT_RENDER, MACHINE_INDEPENDENT_OUTPUT, MACHINE_INDEPENDENT_COMPOSITION, MACHINE_SPECIFIC_TEXT, MACHINE_SPECIFIC_PAINT, MACHINE_SPECIFIC_EFFECTS, MACHINE_SPECIFIC_EXPRESSION_SNIPPETS or MACHINE_SPECIFIC_SCRIPT_SNIPPETS.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "section",
    "key"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>preference</code>

Write, remove, reload or save to disk After Effects' preferences by section and key; a value takes effect when the preference is read next, some only after a restart. Wrong keys create new entries, so read with ae_preference first.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `section` | `string?` | — | `null` | The section in the preferences file, such as "Main Pref Section v2". |
| `key` | `string?` | — | `null` | The key within the section, such as "Pref_SCRIPTING_FILE_NETWORK_SECURITY". |
| `value` | `any` | — | `null` | The value to store, read as kind says. |
| `kind` | `string` | — | `string` | How the value is read or written: string, long, float or bool. |
| `file` | `string?` | — | `null` | Which preferences file: MACHINE_INDEPENDENT (the default), MACHINE_SPECIFIC, MACHINE_INDEPENDENT_RENDER, MACHINE_INDEPENDENT_OUTPUT, MACHINE_INDEPENDENT_COMPOSITION, MACHINE_SPECIFIC_TEXT, MACHINE_SPECIFIC_PAINT, MACHINE_SPECIFIC_EFFECTS, MACHINE_SPECIFIC_EXPRESSION_SNIPPETS or MACHINE_SPECIFIC_SCRIPT_SNIPPETS. |
| `remove` | `boolean` | — | `false` | Remove the preference instead of setting it. |
| `saveToDisk` | `boolean` | — | `false` | Write the preferences file to disk afterwards. |
| `reload` | `boolean` | — | `false` | Re-read the preferences file first, dropping unsaved changes. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "section": {
      "description": "The section in the preferences file, such as \u0022Main Pref Section v2\u0022.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "key": {
      "description": "The key within the section, such as \u0022Pref_SCRIPTING_FILE_NETWORK_SECURITY\u0022.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "value": {
      "description": "The value to store, read as kind says.",
      "default": null
    },
    "kind": {
      "description": "How the value is read or written: string, long, float or bool.",
      "type": "string",
      "default": "string"
    },
    "file": {
      "description": "Which preferences file: MACHINE_INDEPENDENT (the default), MACHINE_SPECIFIC, MACHINE_INDEPENDENT_RENDER, MACHINE_INDEPENDENT_OUTPUT, MACHINE_INDEPENDENT_COMPOSITION, MACHINE_SPECIFIC_TEXT, MACHINE_SPECIFIC_PAINT, MACHINE_SPECIFIC_EFFECTS, MACHINE_SPECIFIC_EXPRESSION_SNIPPETS or MACHINE_SPECIFIC_SCRIPT_SNIPPETS.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "remove": {
      "description": "Remove the preference instead of setting it.",
      "type": "boolean",
      "default": false
    },
    "saveToDisk": {
      "description": "Write the preferences file to disk afterwards.",
      "type": "boolean",
      "default": false
    },
    "reload": {
      "description": "Re-read the preferences file first, dropping unsaved changes.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>script_<wbr>setting</code>

Read a setting scripts keep between sessions in After Effects, by section and key: the key-value store of app.settings.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `section` | `string` | yes | — | The section, usually the script's or tool's name. |
| `key` | `string` | yes | — | The key within the section. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "section": {
      "description": "The section, usually the script\u0027s or tool\u0027s name.",
      "type": "string"
    },
    "key": {
      "description": "The key within the section.",
      "type": "string"
    }
  },
  "required": [
    "section",
    "key"
  ]
}
```

</details>

## <code>ae_<wbr>save_<wbr>script_<wbr>setting</code>

Keep a text under a section and key in After Effects' script settings, where it survives restarts.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `section` | `string` | yes | — | The section, usually the script's or tool's name. |
| `key` | `string` | yes | — | The key within the section. |
| `value` | `string` | yes | — | The text to keep. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "section": {
      "description": "The section, usually the script\u0027s or tool\u0027s name.",
      "type": "string"
    },
    "key": {
      "description": "The key within the section.",
      "type": "string"
    },
    "value": {
      "description": "The text to keep.",
      "type": "string"
    }
  },
  "required": [
    "section",
    "key",
    "value"
  ]
}
```

</details>

## <code>ae_<wbr>quit</code>

Quit After Effects: with unsaved changes it saves first or drops them as save says, and refuses when save is not given. The bridge returns once it is running again.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `save` | `boolean?` | — | `null` | With unsaved changes: true saves the project first, false drops the changes; empty refuses to quit. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "save": {
      "description": "With unsaved changes: true saves the project first, false drops the changes; empty refuses to quit.",
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

