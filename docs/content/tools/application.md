# Undo, menu commands, sequences and scripts

## <code>ae_<wbr>undo</code>

Undo the last steps in After Effects; every tool call is one step, run_tools is one step for its whole list. The answer says the undo was asked for; ae_list_layers or ae_project_info show the result.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `steps` | `integer` | — | `1` | How many steps to undo. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "steps": {
      "description": "How many steps to undo.",
      "type": "integer",
      "default": 1
    }
  }
}
```

</details>

## <code>ae_<wbr>redo</code>

Redo steps undone with ae_undo; the answer says it was asked for.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `steps` | `integer` | — | `1` | How many steps to redo. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "steps": {
      "description": "How many steps to redo.",
      "type": "integer",
      "default": 1
    }
  }
}
```

</details>

## <code>ae_<wbr>run_<wbr>menu_<wbr>command</code>

Run any command of After Effects' menus, by its name in After Effects' language or by its numeric id, on the current selection: ae_select_layers chooses the layers first. For what no tool covers, such as pre-composing or fitting a layer to the composition. Never Undo or Redo this way: use ae_undo and ae_redo, which run them safely.

*may overwrite or delete · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string?` | — | `null` | The command as the menu shows it, in After Effects' own language. |
| `id` | `integer?` | — | `null` | The command's numeric id, which is the same in every language. |
| `timeoutSeconds` | `integer` | — | `60` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "The command as the menu shows it, in After Effects\u0027 own language.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "id": {
      "description": "The command\u0027s numeric id, which is the same in every language.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
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

## <code>run_<wbr>tools</code>

Run a list of bridge commands in one call and one undo step: each step names a command — the tool name without ae_, parameters in snake_case as the bridge reads them, e.g. {"command": "layer_add", "params": {"kind": "text", "text": "Hi"}} — and gets its own result or error. A build of twenty layers costs one round trip instead of twenty.

*may overwrite or delete · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `steps` | `array` | yes | — | The steps as a JSON array of {"command": &lt;bridge command&gt;, "params": {...}}; commands and parameters are the tools' in snake_case without the ae_ prefix (layer_add, property_set, comp_create). |
| `stopOnError` | `boolean` | — | `true` | Stop at the first step that fails; false runs every step and reports each. |
| `timeoutSeconds` | `integer` | — | `120` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "steps": {
      "description": "The steps as a JSON array of {\u0022command\u0022: \u003Cbridge command\u003E, \u0022params\u0022: {...}}; commands and parameters are the tools\u0027 in snake_case without the ae_ prefix (layer_add, property_set, comp_create).",
      "type": "array"
    },
    "stopOnError": {
      "description": "Stop at the first step that fails; false runs every step and reports each.",
      "type": "boolean",
      "default": true
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 120
    }
  },
  "required": [
    "steps"
  ]
}
```

</details>

## <code>ae_<wbr>script</code>

Run ExtendScript inside After Effects for what no tool reaches. A script skips the tools' validation and comes back as text only; prefer the tools and run_tools, and use this for the API they do not cover. Do not call app.executeCommand for Undo or Redo in a script: it stalls the bridge; ae_undo and ae_redo exist for that.

*may overwrite or delete · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `code` | `string` | yes | — | ExtendScript to run inside After Effects; the value of its last expression comes back as text. |
| `timeoutSeconds` | `integer` | — | `60` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "code": {
      "description": "ExtendScript to run inside After Effects; the value of its last expression comes back as text.",
      "type": "string"
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 60
    }
  },
  "required": [
    "code"
  ]
}
```

</details>

