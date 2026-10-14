# Prompts

The server offers three MCP prompts. A client lists them with `prompts/list` and expands one with `prompts/get`; the text puts the
studio order of tools in front of the model before it starts.

| Prompt | Arguments | What it walks through |
|--------|-----------|-----------------------|
| `animated_title_card` | `title`, `width`, `height`, `fps`, `duration`, `delivery` | A composition from the brief, a text layer with a font that exists, the entrance animated with eased keyframes or a text animator, frames looked at before rendering, the render in place or on the farm. |
| `fill_motion_graphics_template` | `template`, `values`, `delivery` | Importing or finding the template, reading its controllers, filling an instance or substituting by layer name, frames checked, the render or a new `.mogrt`. |
| `render_delivery` | `comp`, `directory`, `delivery` | Missing footage first, the output module template for the delivery, rendering in place for a short composition or on the farm for a long one, Media Encoder when installed, a frame to confirm. |

Every tool a prompt names exists: a guard test checks the texts against the tool catalog.
