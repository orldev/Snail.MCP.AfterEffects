# Cameras, lights, materials and the renderer

Part of the `threed` skill: these tools appear once it is loaded — by `ae_enable_skill` with `threed`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>3d_<wbr>info</code>

The 3D state of a composition (renderer, cameras, lights, how many 3D layers) or of one layer: its transform, point of interest, camera lens and depth of field, light settings, material options, geometry options, and the renderers the composition can switch to.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `layer` | `string?` | — | `null` | One layer to describe in full; the composition's 3D overview when empty. |
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
    "layer": {
      "description": "One layer to describe in full; the composition\u0027s 3D overview when empty.",
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

## <code>ae_<wbr>set_<wbr>camera</code>

Set a camera: position and point of interest (or look_at a layer), one or two node, zoom or focal length in millimetres, depth of field with focus distance, aperture and blur level, and the iris and highlight settings; as keyframes when time is given.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The camera layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the values as keyframes at this time. |
| `position` | `array?` | — | `null` | [x, y, z] position of the camera in composition pixels. |
| `pointOfInterest` | `array?` | — | `null` | [x, y, z] the camera or light aims at, in composition pixels. |
| `lookAt` | `string?` | — | `null` | A layer whose position becomes the point of interest. |
| `twoNode` | `boolean?` | — | `null` | Aim at the point of interest (two node) or not (one node, free rotation). |
| `zoom` | `number?` | — | `null` | Zoom in pixels: the distance from the camera to the plane that fills the frame. |
| `focalLengthMm` | `number?` | — | `null` | Focal length in millimetres on a 36 mm film back; sets zoom from the composition width. |
| `depthOfField` | `boolean?` | — | `null` | Switch depth of field on or off. |
| `focusDistance` | `number?` | — | `null` | Focus distance in pixels. |
| `aperture` | `number?` | — | `null` | Aperture in pixels; larger blurs more. |
| `blurLevel` | `number?` | — | `null` | Blur level in percent. |
| `irisShape` | `integer?` | — | `null` | Iris shape as an option number, 1 fast rectangle, then triangle, square, pentagon, hexagon, heptagon, octagon, nonagon, decagon. |
| `irisRotation` | `number?` | — | `null` | Iris rotation in degrees. |
| `irisRoundness` | `number?` | — | `null` | Iris roundness in percent. |
| `irisAspectRatio` | `number?` | — | `null` | Iris aspect ratio. |
| `irisDiffractionFringe` | `number?` | — | `null` | Iris diffraction fringe. |
| `highlightGain` | `number?` | — | `null` | Highlight gain. |
| `highlightThreshold` | `number?` | — | `null` | Highlight threshold. |
| `highlightSaturation` | `number?` | — | `null` | Highlight saturation. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The camera layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the values as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "position": {
      "description": "[x, y, z] position of the camera in composition pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "pointOfInterest": {
      "description": "[x, y, z] the camera or light aims at, in composition pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "lookAt": {
      "description": "A layer whose position becomes the point of interest.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "twoNode": {
      "description": "Aim at the point of interest (two node) or not (one node, free rotation).",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "zoom": {
      "description": "Zoom in pixels: the distance from the camera to the plane that fills the frame.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "focalLengthMm": {
      "description": "Focal length in millimetres on a 36 mm film back; sets zoom from the composition width.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "depthOfField": {
      "description": "Switch depth of field on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "focusDistance": {
      "description": "Focus distance in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "aperture": {
      "description": "Aperture in pixels; larger blurs more.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "blurLevel": {
      "description": "Blur level in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "irisShape": {
      "description": "Iris shape as an option number, 1 fast rectangle, then triangle, square, pentagon, hexagon, heptagon, octagon, nonagon, decagon.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "irisRotation": {
      "description": "Iris rotation in degrees.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "irisRoundness": {
      "description": "Iris roundness in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "irisAspectRatio": {
      "description": "Iris aspect ratio.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "irisDiffractionFringe": {
      "description": "Iris diffraction fringe.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "highlightGain": {
      "description": "Highlight gain.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "highlightThreshold": {
      "description": "Highlight threshold.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "highlightSaturation": {
      "description": "Highlight saturation.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>light</code>

Set a light: type, position and point of interest (or look_at a layer), intensity, colour, cone angle and feather for spots, falloff with its start and distance, and shadows with darkness and diffusion; as keyframes when time is given.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The light layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the values as keyframes at this time. |
| `type` | `string?` | — | `null` | PARALLEL, SPOT, POINT or AMBIENT. |
| `position` | `array?` | — | `null` | [x, y, z] position of the light in composition pixels. |
| `pointOfInterest` | `array?` | — | `null` | [x, y, z] the camera or light aims at, in composition pixels. |
| `lookAt` | `string?` | — | `null` | A layer whose position becomes the point of interest. |
| `twoNode` | `boolean?` | — | `null` | Aim at the point of interest (two node) or not (one node, free rotation). |
| `intensity` | `number?` | — | `null` | Intensity in percent. |
| `color` | `any` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `coneAngle` | `number?` | — | `null` | Cone angle in degrees, for spot lights. |
| `coneFeather` | `number?` | — | `null` | Cone feather in percent, for spot lights. |
| `falloff` | `string?` | — | `null` | none, smooth, or inverse_square_clamped. |
| `falloffStart` | `number?` | — | `null` | Where the falloff starts, in pixels. |
| `falloffDistance` | `number?` | — | `null` | Over how many pixels the light falls off. |
| `castsShadows` | `boolean?` | — | `null` | Cast shadows from this light. |
| `shadowDarkness` | `number?` | — | `null` | Shadow darkness in percent. |
| `shadowDiffusion` | `number?` | — | `null` | Shadow diffusion in pixels. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The light layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the values as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "type": {
      "description": "PARALLEL, SPOT, POINT or AMBIENT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "position": {
      "description": "[x, y, z] position of the light in composition pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "pointOfInterest": {
      "description": "[x, y, z] the camera or light aims at, in composition pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "lookAt": {
      "description": "A layer whose position becomes the point of interest.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "twoNode": {
      "description": "Aim at the point of interest (two node) or not (one node, free rotation).",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "intensity": {
      "description": "Intensity in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "color": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "default": null
    },
    "coneAngle": {
      "description": "Cone angle in degrees, for spot lights.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "coneFeather": {
      "description": "Cone feather in percent, for spot lights.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "falloff": {
      "description": "none, smooth, or inverse_square_clamped.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "falloffStart": {
      "description": "Where the falloff starts, in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "falloffDistance": {
      "description": "Over how many pixels the light falls off.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "castsShadows": {
      "description": "Cast shadows from this light.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "shadowDarkness": {
      "description": "Shadow darkness in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "shadowDiffusion": {
      "description": "Shadow diffusion in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>material</code>

Set the material options of a 3D layer (switching it to 3D if it is not): casting and accepting shadows, accepting lights, ambient, diffuse, specular intensity and shininess, metal, and the reflection, transparency and refraction settings of renderers that have them.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the values as keyframes at this time. |
| `castsShadows` | `string?` | — | `null` | on, off, or only (cast or accept shadows without rendering the layer itself). |
| `lightTransmission` | `number?` | — | `null` | Light transmission in percent: how much light passes through to cast coloured shadows. |
| `acceptsShadows` | `string?` | — | `null` | on, off, or only (cast or accept shadows without rendering the layer itself). |
| `acceptsLights` | `boolean?` | — | `null` | Let lights affect the layer. |
| `appearsInReflections` | `boolean?` | — | `null` | Show the layer in reflections, where the renderer has them. |
| `ambient` | `number?` | — | `null` | Ambient in percent. |
| `diffuse` | `number?` | — | `null` | Diffuse in percent. |
| `specularIntensity` | `number?` | — | `null` | Specular intensity in percent. |
| `specularShininess` | `number?` | — | `null` | Specular shininess in percent. |
| `metal` | `number?` | — | `null` | Metal in percent: how much the specular takes the layer's colour. |
| `reflectionIntensity` | `number?` | — | `null` | Reflection intensity in percent. |
| `reflectionSharpness` | `number?` | — | `null` | Reflection sharpness in percent. |
| `reflectionRolloff` | `number?` | — | `null` | Reflection rolloff in percent. |
| `transparency` | `number?` | — | `null` | Transparency in percent. |
| `transparencyRolloff` | `number?` | — | `null` | Transparency rolloff in percent. |
| `indexOfRefraction` | `number?` | — | `null` | Index of refraction. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the values as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "castsShadows": {
      "description": "on, off, or only (cast or accept shadows without rendering the layer itself).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "lightTransmission": {
      "description": "Light transmission in percent: how much light passes through to cast coloured shadows.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "acceptsShadows": {
      "description": "on, off, or only (cast or accept shadows without rendering the layer itself).",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "acceptsLights": {
      "description": "Let lights affect the layer.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "appearsInReflections": {
      "description": "Show the layer in reflections, where the renderer has them.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "ambient": {
      "description": "Ambient in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "diffuse": {
      "description": "Diffuse in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "specularIntensity": {
      "description": "Specular intensity in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "specularShininess": {
      "description": "Specular shininess in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "metal": {
      "description": "Metal in percent: how much the specular takes the layer\u0027s colour.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "reflectionIntensity": {
      "description": "Reflection intensity in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "reflectionSharpness": {
      "description": "Reflection sharpness in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "reflectionRolloff": {
      "description": "Reflection rolloff in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "transparency": {
      "description": "Transparency in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "transparencyRolloff": {
      "description": "Transparency rolloff in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "indexOfRefraction": {
      "description": "Index of refraction.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>geometry</code>

Set the geometry options of a 3D layer under a renderer that extrudes (Advanced 3D, Cinema 4D): extrusion depth, bevel style, direction and depth, hole bevel depth, curvature and segments. Under the Classic 3D renderer the answer says so and names the renderers to switch to.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The layer by its name or its index in the composition, 1 at the top. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Set the values as keyframes at this time. |
| `bevelStyle` | `integer?` | — | `null` | Bevel style as an option number: 1 none, 2 angular, 3 concave, 4 convex. |
| `bevelDirection` | `integer?` | — | `null` | Bevel direction as an option number, where the renderer has it. |
| `bevelDepth` | `number?` | — | `null` | Bevel depth in pixels. |
| `holeBevelDepth` | `number?` | — | `null` | Hole bevel depth in percent. |
| `extrusionDepth` | `number?` | — | `null` | Extrusion depth in pixels. |
| `curvature` | `number?` | — | `null` | Curvature in percent, for curved layers. |
| `segments` | `integer?` | — | `null` | Segments of a curved layer. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The layer by its name or its index in the composition, 1 at the top.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Set the values as keyframes at this time.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "bevelStyle": {
      "description": "Bevel style as an option number: 1 none, 2 angular, 3 concave, 4 convex.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "bevelDirection": {
      "description": "Bevel direction as an option number, where the renderer has it.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "bevelDepth": {
      "description": "Bevel depth in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "holeBevelDepth": {
      "description": "Hole bevel depth in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "extrusionDepth": {
      "description": "Extrusion depth in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "curvature": {
      "description": "Curvature in percent, for curved layers.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "segments": {
      "description": "Segments of a curved layer.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>set_<wbr>renderer</code>

The renderer of a composition: Classic 3D, Advanced 3D or Cinema 4D by name or a word of it; with no renderer the answer only reports the current one and the choices. The renderer decides which material and geometry options exist.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `renderer` | `string?` | — | `null` | The renderer by its name or match name (ADBE Advanced 3d, ADBE Ernst for Cinema 4D, ADBE Standard 3d for Classic 3D), or a word of it; empty only reports. |

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
    "renderer": {
      "description": "The renderer by its name or match name (ADBE Advanced 3d, ADBE Ernst for Cinema 4D, ADBE Standard 3d for Classic 3D), or a word of it; empty only reports.",
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

