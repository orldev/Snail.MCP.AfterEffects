# Finding a tool

## <code>find_<wbr>tool</code>

Find the tool for a task across every skill, loaded or not, by saying what you want in English words. Each match carries the tool's command and parameters as a program sends them (snake_case; the tool takes the same names in camelCase), which is enough to call it through run_program without loading anything; pass enable to load the skills the matches live in so the tools can be called by name. Ask here before concluding that something has no tool.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `intent` | `string` | yes | — | What you are trying to do, in English words: "ease the keyframes of a position", "add a drop shadow", "render the composition to a file". |
| `limit` | `integer` | — | `8` | Most tools to return, 1 to 25; 8 by default. |
| `enable` | `boolean` | — | `false` | Load the skills the matches live in, so their tools join the tool list; false by default, because the answer already carries what it takes to call them through run_program. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "intent": {
      "description": "What you are trying to do, in English words: \u0022ease the keyframes of a position\u0022, \u0022add a drop shadow\u0022, \u0022render the composition to a file\u0022.",
      "type": "string"
    },
    "limit": {
      "description": "Most tools to return, 1 to 25; 8 by default.",
      "type": "integer",
      "default": 8
    },
    "enable": {
      "description": "Load the skills the matches live in, so their tools join the tool list; false by default, because the answer already carries what it takes to call them through run_program.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "intent"
  ]
}
```

</details>

