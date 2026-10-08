namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>Studio recipes served as MCP prompts: the order of tools a motion designer would follow; a contract with the agent, guarded like the descriptions.</summary>
internal static class PromptTexts
{
    public const string TitleCardName = "animated_title_card";

    public const string TitleCardDescription = "Build a composition with an animated title from a brief, check it as a picture, and render it.";

    public const string TitleCard =
        """
        Goal: a {width}x{height} composition at {fps} fps, {duration} s long, with the title "{title}" animating in, delivered as {delivery}.
        Work in this order and read every reply before the next call; stop and ask when a reply is not ok.
        1. ae_diagnose: After Effects must be reachable. ae_enable_skill animation and text (rendering or farm at the end).
        2. ae_create_comp with the size, frame rate, duration and a background colour that suits the brief; it becomes the composition later calls mean.
        3. ae_add_layer kind text with the title; ae_text_fonts to pick a font that exists, then ae_text_style with font, size, fill colour, tracking and justification; ae_update_layer or ae_transform_layer to place it.
        4. Animate the entrance: ae_set_keyframes on opacity (0 to 100 over the first 0.6 s) and position or scale, with preset easy_ease; or ae_add_text_animator with an opacity or position property and a range selector, then ae_set_keyframes on the selector's start so the characters arrive one after another.
        5. ae_capture_frame at a time in the middle of the entrance and at the end; look at the pictures and fix size, placement, colour or timing before going on.
        6. Deliver: ae_project_save, then ae_add_render with an output template and path followed by ae_render for a short composition, or ae_farm_submit and polling ae_farm_jobs when it is long or After Effects must stay free.
        7. Report the composition name, the file path and what you would still improve.
        """;

    public const string TemplateFillName = "fill_motion_graphics_template";

    public const string TemplateFillDescription = "Bring in a motion graphics template, fill its controllers, check the result as a picture and render it.";

    public const string TemplateFill =
        """
        Goal: a finished version of the template {template} with these values: {values}; delivered as {delivery}.
        1. ae_diagnose; ae_enable_skill templates, rendering. ae_import_template with the .mogrt path when the template is a file, else ae_list_items to find the template composition in the project.
        2. ae_template_info on the template composition: read the controllers and any instances. To customise a copy, ae_duplicate_comp first and work on the copy.
        3. When the template is used as a nested layer, ae_fill_template on that layer with values keyed by controller name; when it is the composition itself, ae_substitute_template by layer name for text, colours and footage, and ae_set_property for anything else.
        4. ae_capture_frame at the start, the middle and the end; look at each picture, and fix text that overflows, colours that clash, footage that is missing (ae_project_footage).
        5. Deliver with ae_add_render and ae_render, or ae_farm_submit when After Effects must stay free; ae_export_template when the result should be a new .mogrt.
        6. Report what was filled, the file path, and anything the template would not let you change.
        """;

    public const string DeliveryName = "render_delivery";

    public const string DeliveryDescription = "Check a composition, choose between rendering in place, the farm and Media Encoder, and deliver with the right template.";

    public const string Delivery =
        """
        Goal: deliver the composition {comp} as {delivery} into {directory}.
        1. ae_diagnose; ae_enable_skill rendering and farm. ae_item_info on the composition for size, duration and frame rate; ae_project_footage onlyMissing true, and fix missing files with ae_replace_footage before rendering anything.
        2. ae_render_templates: pick the output module template that matches the delivery (H.264 for review, Lossless or a ProRes template for masters, PNG Sequence for frames) and Best Settings for render settings.
        3. Short composition or a quick check: ae_add_render with the templates, output path under {directory} and the work area or a span, then ae_render with a timeout the render fits in; read the output files in the answer.
        4. Long render, or After Effects must stay free: ae_project_save, ae_farm_submit with the same templates and span, then ae_farm_jobs until the state is done; ae_farm_cancel when something is wrong. With Media Encoder installed, ae_render_in_media_encoder hands the queue over instead.
        5. ae_capture_frame on the rendered composition at a late time to confirm the picture matches the file; report the path, size and the settings used.
        """;
}
