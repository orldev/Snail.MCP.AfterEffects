# Skills

## <code>ae_<wbr>skills</code>

The skills this server can load — groups of tools for animation, text, shapes, effects, 3D, compositing, rendering — with which are loaded and the tools each brings.

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

## <code>ae_<wbr>enable_<wbr>skill</code>

Load a skill: its tools join the tool list at once, and the client is told the list changed. Skills stay loaded until ae_disable_skill, unless the server unloads idle ones.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | yes | — | Skill name as ae_skills lists it. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Skill name as ae_skills lists it.",
      "type": "string"
    }
  },
  "required": [
    "name"
  ]
}
```

</details>

## <code>ae_<wbr>disable_<wbr>skill</code>

Unload a skill, taking its tools out of the list.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `name` | `string` | yes | — | Skill name as ae_skills lists it. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "name": {
      "description": "Skill name as ae_skills lists it.",
      "type": "string"
    }
  },
  "required": [
    "name"
  ]
}
```

</details>

