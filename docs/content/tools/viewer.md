# The viewer

Part of the `viewer` skill: these tools appear once it is loaded — by `ae_enable_skill` with `viewer`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>viewer</code>

The active viewer: composition, layer or footage viewer, whether it is maximized, its views with the active one, and each view's channels, checkerboards, exposure, fast preview mode, rulers, guides and zoom.

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

## <code>ae_<wbr>set_<wbr>viewer</code>

Change a view of the active viewer: the channel shown, transparency checkerboards, exposure in stops, fast preview mode, rulers, guides visibility, snapping and locking, zoom; maximize the viewer or make the view active.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `view` | `integer?` | — | `null` | Which view of the viewer, 1 first; the active view when empty. |
| `channels` | `string?` | — | `null` | CHANNEL_RGB, CHANNEL_RGB_STRAIGHT, CHANNEL_RED, CHANNEL_GREEN, CHANNEL_BLUE, CHANNEL_ALPHA, CHANNEL_ALPHA_BOUNDARY, CHANNEL_ALPHA_OVERLAY, CHANNEL_RED_COLORIZE, CHANNEL_GREEN_COLORIZE or CHANNEL_BLUE_COLORIZE. |
| `checkerboards` | `boolean?` | — | `null` | Show a checkerboard behind transparency. |
| `exposure` | `number?` | — | `null` | Exposure in stops. |
| `fastPreview` | `string?` | — | `null` | FP_OFF, FP_ADAPTIVE_RESOLUTION, FP_DRAFT, FP_FAST_DRAFT or FP_WIREFRAME. |
| `guidesLocked` | `boolean?` | — | `null` | Lock the guides. |
| `guidesSnap` | `boolean?` | — | `null` | Snap to the guides. |
| `guidesVisibility` | `boolean?` | — | `null` | Show the guides. |
| `rulers` | `boolean?` | — | `null` | Show the rulers. |
| `zoom` | `number?` | — | `null` | Zoom factor, 1 for 100 percent. |
| `maximized` | `boolean?` | — | `null` | Maximize the viewer panel, or restore it. |
| `activate` | `boolean` | — | `false` | Make this view and its viewer active. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "view": {
      "description": "Which view of the viewer, 1 first; the active view when empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "channels": {
      "description": "CHANNEL_RGB, CHANNEL_RGB_STRAIGHT, CHANNEL_RED, CHANNEL_GREEN, CHANNEL_BLUE, CHANNEL_ALPHA, CHANNEL_ALPHA_BOUNDARY, CHANNEL_ALPHA_OVERLAY, CHANNEL_RED_COLORIZE, CHANNEL_GREEN_COLORIZE or CHANNEL_BLUE_COLORIZE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "checkerboards": {
      "description": "Show a checkerboard behind transparency.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "exposure": {
      "description": "Exposure in stops.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "fastPreview": {
      "description": "FP_OFF, FP_ADAPTIVE_RESOLUTION, FP_DRAFT, FP_FAST_DRAFT or FP_WIREFRAME.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "guidesLocked": {
      "description": "Lock the guides.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "guidesSnap": {
      "description": "Snap to the guides.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "guidesVisibility": {
      "description": "Show the guides.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "rulers": {
      "description": "Show the rulers.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "zoom": {
      "description": "Zoom factor, 1 for 100 percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "maximized": {
      "description": "Maximize the viewer panel, or restore it.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "activate": {
      "description": "Make this view and its viewer active.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

