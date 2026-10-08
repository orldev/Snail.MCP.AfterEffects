namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>What each skill is for, as ae_skills and find_tool state it; one constant per skill, read by its attribute.</summary>
internal static class SkillDescriptions
{
    public const string Animation =
        "Keyframes and their timing: list, set and remove keys on any property, ease them with the presets of the Animation menu or by influence " +
        "and speed, put expressions on properties with their errors caught, comp and layer markers, and time remapping of footage and nested compositions.";

    public const string Text =
        "Text layers in depth: the fonts installed, the whole document with every attribute (font, size, fill and stroke, tracking, leading, caps, " +
        "scales, margins) on the whole text or a range of characters, text animators with range, wiggly and expression selectors, and text on a mask path.";

    public const string Shapes =
        "Shape layers and masks: the contents tree of groups, rectangles, ellipses, stars and paths with fills, strokes, gradients, trim paths, " +
        "repeaters and the other operators, each settable by plain names; and masks on any layer from a rectangle, an ellipse or vertices.";

    public const string Effects =
        "Effects and looks: the catalog of effects this After Effects can apply, effects on a layer with every parameter settable by name, " +
        "animation presets (.ffx) found by name and applied, and layer styles. Blending modes and track mattes are on ae_update_layer.";

    public const string ThreeD =
        "The third dimension: cameras with their lens and depth of field, lights with cones, falloff and shadows, the material options of 3D layers, " +
        "extrusion and bevels where the renderer allows, and the renderer of the composition. Cameras and lights are added with ae_add_layer.";

    public const string Compositing =
        "Putting a project together: precomposing layers, the tree of nested compositions, replacing footage and layer sources, interpreting footage " +
        "(alpha, frame rate, fields, pulldown, loop, pixel aspect), proxies, the project's files with what is missing, cleanup, and collecting files.";

    public const string Rendering =
        "Rendering inside After Effects: the render queue with its items and output modules, render settings and output module templates, " +
        "adding compositions with a path, span and settings, rendering in place while the tool waits, and handing the queue to Media Encoder.";

    public const string Farm =
        "Renders that outlive a tool call: jobs for aerender, which renders a saved project in its own engine while After Effects stays free; " +
        "a queue by priority, progress by frame, retries, cancellation, logs and cleanup.";

    public const string Templates =
        "Motion graphics templates: the Essential Graphics controllers of a composition, exposing properties as controllers, filling an instance " +
        "of a template through its essential properties, substituting text, colours and footage in a template composition by layer name, and " +
        ".mogrt files exported and imported.";

    public const string Transfer =
        "Files between the client and this server: what the server keeps by area (project files, captures, renders, farm logs), uploads into the " +
        "project folder and downloads from any area, as shell commands with signed links over HTTP and as local paths over stdio.";

    public const string Layout =
        "Measuring and placing: the pixel bounds of a layer in its own space and in the composition, points converted between the two, what is " +
        "selected, guides on compositions and footage, splitting a layer at a time, copying a layer to another composition, placing a layer by " +
        "three corners, and finding a layer anywhere in the project by its id.";

    public const string Settings =
        "The project's settings and the application's: time display and frame counting, colour depth, working space and colour management, " +
        "the expression engine and a search-and-replace over every expression, GPU acceleration, the fonts the project uses and replacing one, " +
        "placeholders, purging caches and memory limits; the machine and the application, preferences and script settings by key, a shell " +
        "command, quitting.";

    public const string Viewer =
        "The viewer panel: which kind is open, whether it is maximized, its views and each view's channels, exposure, fast preview, rulers, " +
        "guides and zoom.";

    public const string Workers =
        "After Effects instances of your own: the server launches a worker, binds this client to it and every command goes there, apart from the " +
        "desk a person works in; workers quit on their own when idle. For several clients at once, or for work that must not touch the open project.";
}
