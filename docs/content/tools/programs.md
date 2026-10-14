# Programs

## <code>run_<wbr>program</code>

Run a small JavaScript program here on the server that calls the bridge commands itself — ae.layer_add({...}), ae.property_set({...}) — with loops and the results in hand, so a build that decides as it goes costs one call and one reply. Only what the program returns and logs comes back. The program reaches the commands only: no files, no network, no After Effects API beyond them.

*may overwrite or delete · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `code` | `string` | yes | — | JavaScript. Call commands as ae.&lt;command&gt;({...}) — the tool name without ae_, parameters in snake_case — read input, return a value, log(...) to add a line. A failed command throws {type, message, details}. Up to 200 commands, no files, no network. |
| `input` | `object?` | — | `null` | Values the program reads as input, e.g. {"count": 8}. |
| `timeoutSeconds` | `integer` | — | `120` | Seconds to wait for After Effects, 1 to 600. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "code": {
      "description": "JavaScript. Call commands as ae.\u003Ccommand\u003E({...}) \u2014 the tool name without ae_, parameters in snake_case \u2014 read input, return a value, log(...) to add a line. A failed command throws {type, message, details}. Up to 200 commands, no files, no network.",
      "type": "string"
    },
    "input": {
      "description": "Values the program reads as input, e.g. {\u0022count\u0022: 8}.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "timeoutSeconds": {
      "description": "Seconds to wait for After Effects, 1 to 600.",
      "type": "integer",
      "default": 120
    }
  },
  "required": [
    "code"
  ]
}
```

</details>

