# Programs

## <code>run_<wbr>program</code>

Run a JavaScript program here whose functions are the bridge commands — ae.layer_add({...}): the command find_tool names beside a tool, parameters in snake_case — so a build that decides as it goes costs one call. A failed command throws with the bridge's type.
What the program returns comes back as result, log(…) writes lines that come back with it, and input is the object
passed alongside. A failed or refused command throws { error, message, hint, type, details }, which the program may
catch and act on; a command the server does not have stops the program with the names nearest to it. The program
reaches nothing but the commands: no files, no network, no clock but its own. The reply lists every command it sent.
Limits: 200 commands, 1000000 statements, calls nested 256 deep, 32 MB of memory, and 120 seconds unless timeoutSeconds gives up to 600.

*may overwrite or delete · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `code` | `string` | yes | — | The body of the program: JavaScript in strict mode, with return for whatever should come back as result. |
| `input` | `object?` | — | `null` | Values the program reads as input, so that the code itself can stay the same between calls. |
| `timeoutSeconds` | `integer?` | — | `null` | Seconds the program may run; the description of run_program states the default and the most it may be given. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "code": {
      "description": "The body of the program: JavaScript in strict mode, with return for whatever should come back as result.",
      "type": "string"
    },
    "input": {
      "description": "Values the program reads as input, so that the code itself can stay the same between calls.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "timeoutSeconds": {
      "description": "Seconds the program may run; the description of run_program states the default and the most it may be given.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "code"
  ]
}
```

</details>

