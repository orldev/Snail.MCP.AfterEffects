# Compositions

## <code>ae_<wbr>create_<wbr>comp</code>

Create a composition with a size, duration and frame rate; 1920×1080 at 30 fps for 10 seconds unless told otherwise. It opens in the viewer, and later calls that name no composition work on it.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | — | `Comp` | Name of the composition. |
| `width` | `integer` | — | `1920` | Width in pixels. |
| `height` | `integer` | — | `1080` | Height in pixels. |
| `duration` | `number` | — | `10` | Duration in seconds. |
| `frameRate` | `number` | — | `30` | Frames per second. |
| `pixelAspect` | `number` | — | `1` | Pixel aspect ratio; 1 is square. |
| `background` | `string?` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `folder` | `string?` | — | `null` | Folder to create it in, by name or id. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Name of the composition.",
      "type": "string",
      "default": "Comp"
    },
    "width": {
      "description": "Width in pixels.",
      "type": "integer",
      "default": 1920
    },
    "height": {
      "description": "Height in pixels.",
      "type": "integer",
      "default": 1080
    },
    "duration": {
      "description": "Duration in seconds.",
      "type": "number",
      "default": 10
    },
    "frameRate": {
      "description": "Frames per second.",
      "type": "number",
      "default": 30
    },
    "pixelAspect": {
      "description": "Pixel aspect ratio; 1 is square.",
      "type": "number",
      "default": 1
    },
    "background": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "folder": {
      "description": "Folder to create it in, by name or id.",
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

## <code>ae_<wbr>update_<wbr>comp</code>

Change a composition's settings: name, size, duration, frame rate, pixel aspect, background colour, current time, work area, motion blur and shutter angle, shy layers, or its folder. Only what is passed changes.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | The new name. |
| `width` | `integer?` | — | `null` | Width in pixels. |
| `height` | `integer?` | — | `null` | Height in pixels. |
| `duration` | `number?` | — | `null` | Duration in seconds. |
| `frameRate` | `number?` | — | `null` | Frames per second. |
| `pixelAspect` | `number?` | — | `null` | Pixel aspect ratio; 1 is square. |
| `background` | `string?` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `time` | `number?` | — | `null` | The current time of the composition, in seconds. |
| `workAreaStart` | `number?` | — | `null` | Start of the work area, in seconds. |
| `workAreaDuration` | `number?` | — | `null` | Length of the work area, in seconds. |
| `motionBlur` | `boolean?` | — | `null` | Switch motion blur on or off for the composition. |
| `shutterAngle` | `number?` | — | `null` | Shutter angle in degrees, 0 to 720. |
| `hideShy` | `boolean?` | — | `null` | Hide the layers marked shy. |
| `folder` | `string?` | — | `null` | Folder to move it into, by name or id. |
| `shutterPhase` | `number?` | — | `null` | Shutter phase in degrees. |
| `motionBlurSamples` | `integer?` | — | `null` | Motion blur samples per frame. |
| `motionBlurAdaptiveLimit` | `integer?` | — | `null` | Motion blur adaptive sample limit. |
| `preserveNestedFrameRate` | `boolean?` | — | `null` | Preserve nested frame rate. |
| `preserveNestedResolution` | `boolean?` | — | `null` | Preserve nested resolution. |
| `draft3d` | `boolean?` | — | `null` | Draft 3D in the viewer. |
| `frameBlending` | `boolean?` | — | `null` | Frame blending for layers that ask for it. |
| `resolutionFactor` | `array?` | — | `null` | Resolution divisors [x, y]; [2, 2] is half. |
| `displayStartFrame` | `integer?` | — | `null` | First frame number of the timeline. |
| `displayStartTime` | `number?` | — | `null` | Start time of the timeline in seconds. |

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
      "description": "The new name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "width": {
      "description": "Width in pixels.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "height": {
      "description": "Height in pixels.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "duration": {
      "description": "Duration in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "frameRate": {
      "description": "Frames per second.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "pixelAspect": {
      "description": "Pixel aspect ratio; 1 is square.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "background": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "The current time of the composition, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "workAreaStart": {
      "description": "Start of the work area, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "workAreaDuration": {
      "description": "Length of the work area, in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "motionBlur": {
      "description": "Switch motion blur on or off for the composition.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "shutterAngle": {
      "description": "Shutter angle in degrees, 0 to 720.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "hideShy": {
      "description": "Hide the layers marked shy.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "folder": {
      "description": "Folder to move it into, by name or id.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "shutterPhase": {
      "description": "Shutter phase in degrees.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "motionBlurSamples": {
      "description": "Motion blur samples per frame.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "motionBlurAdaptiveLimit": {
      "description": "Motion blur adaptive sample limit.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "preserveNestedFrameRate": {
      "description": "Preserve nested frame rate.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "preserveNestedResolution": {
      "description": "Preserve nested resolution.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "draft3d": {
      "description": "Draft 3D in the viewer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "frameBlending": {
      "description": "Frame blending for layers that ask for it.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "resolutionFactor": {
      "description": "Resolution divisors [x, y]; [2, 2] is half.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "integer"
      },
      "default": null
    },
    "displayStartFrame": {
      "description": "First frame number of the timeline.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "displayStartTime": {
      "description": "Start time of the timeline in seconds.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

## <code>ae_<wbr>duplicate_<wbr>comp</code>

Duplicate a composition, optionally under a new name; the layers keep pointing at the same footage and nested compositions.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name for the copy. |

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
      "description": "Name for the copy.",
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

## <code>ae_<wbr>open_<wbr>comp</code>

Open a composition in the viewer so the user sees it, and set its current time; later calls that name no composition work on it.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |

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
    "time": {
      "description": "Time in seconds within the composition; the composition\u0027s current time when empty.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  }
}
```

</details>

