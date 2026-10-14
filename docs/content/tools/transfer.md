# Files between the client and the server

Part of the `transfer` skill: these tools appear once it is loaded — by `ae_enable_skill` with `transfer`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>files</code>

The files this server keeps, by area: files (the project folder: footage, uploads), captures, renders, jobs (farm logs with each job's state). Lists a folder with sizes; over HTTP each entry can carry a signed download link.

*read-only · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `area` | `string` | — | `files` | files, captures, renders or jobs. |
| `path` | `string?` | — | `null` | Path inside the area, forward slashes; empty for the area itself. |
| `withLinks` | `boolean` | — | `false` | Add a signed download link per entry (HTTP only). |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "area": {
      "description": "files, captures, renders or jobs.",
      "type": "string",
      "default": "files"
    },
    "path": {
      "description": "Path inside the area, forward slashes; empty for the area itself.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "withLinks": {
      "description": "Add a signed download link per entry (HTTP only).",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>upload</code>

Bring a file or folder from the client's machine into the files area. Over HTTP the answer is a shell command with a short-lived signed link to run on the client; over stdio both are the same machine and the answer says where the file goes.

*only adds · every call adds · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `localPath` | `string` | yes | — | File or folder on the client's machine. |
| `remotePath` | `string?` | — | `null` | Where it lands in the files area; its own name when empty. |
| `overwrite` | `boolean` | — | `false` | Replace files already there (delete work). |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "localPath": {
      "description": "File or folder on the client\u0027s machine.",
      "type": "string"
    },
    "remotePath": {
      "description": "Where it lands in the files area; its own name when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "overwrite": {
      "description": "Replace files already there (delete work).",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "localPath"
  ]
}
```

</details>

## <code>ae_<wbr>download</code>

Take a file or folder of an area to the client's machine: over HTTP a shell command with a signed link (a folder as one tar), over stdio the local path, since the file is already here.

*read-only · repeat-safe · reaches files or programs outside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `remotePath` | `string` | yes | — | Path inside the area, forward slashes; empty for the area itself. |
| `area` | `string` | — | `files` | files, captures, renders or jobs. |
| `localPath` | `string?` | — | `null` | Where it lands on the client's machine; its own name when empty. |
| `overwrite` | `boolean` | — | `false` | Replace files already on the client's machine. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "remotePath": {
      "description": "Path inside the area, forward slashes; empty for the area itself.",
      "type": "string"
    },
    "area": {
      "description": "files, captures, renders or jobs.",
      "type": "string",
      "default": "files"
    },
    "localPath": {
      "description": "Where it lands on the client\u0027s machine; its own name when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "overwrite": {
      "description": "Replace files already on the client\u0027s machine.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "remotePath"
  ]
}
```

</details>

