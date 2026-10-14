# Pictures

## <code>ae_<wbr>capture_<wbr>frame</code>

A frame of a composition as a picture, scaled to width and laid over the composition's background colour (After Effects renders the background as transparency), returned as image content next to the path of the PNG. Look at it before judging layout, colour or timing.

*only adds · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Time in seconds within the composition; the composition's current time when empty. |
| `width` | `integer` | — | `1280` | Width in pixels, scaled down from the composition; 1280 by default. |
| `path` | `string?` | — | `null` | Where to write the PNG on After Effects' machine; the captures folder otherwise. |
| `transparent` | `boolean` | — | `false` | Keep the background transparent rather than the composition's colour. |
| `timeoutSeconds` | `integer` | — | `60` | Seconds to wait for After Effects, 1 to 600. |

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
    },
    "width": {
      "description": "Width in pixels, scaled down from the composition; 1280 by default.",
      "type": "integer",
      "default": 1280
    },
    "path": {
      "description": "Where to write the PNG on After Effects\u0027 machine; the captures folder otherwise.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "transparent": {
      "description": "Keep the background transparent rather than the composition\u0027s colour.",
      "type": "boolean",
      "default": false
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 60
    }
  }
}
```

</details>

