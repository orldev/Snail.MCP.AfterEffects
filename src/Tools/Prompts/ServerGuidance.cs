namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>How to work with this server, delivered once at initialize; a wire contract, edited with caution.</summary>
internal static class ServerGuidance
{
    public const string Instructions =
        """
        This server drives a running Adobe After Effects through an ExtendScript bridge inside it. Every tool is named ae_*.
        Start with ae_diagnose: it tells whether the bridge inside After Effects has connected to this server. When it has not,
        ae_install_bridge puts the bridge into After Effects' scripts folder and starts it; the user switches on
        'Allow Scripts to Write Files and Access Network' in Preferences → Scripting & Expressions once.
        Read before writing: ae_project_info, then ae_list_items for the project panel and ae_list_layers for a composition;
        ae_item_info and ae_layer_info go deeper. Items and layers are named by name or by id and index, which the answers carry.
        Build with ae_create_comp, ae_add_layer, ae_transform_layer and ae_update_layer; ae_import_file brings footage in.
        Everything else a layer has is a property: ae_property_tree lists them with match names, ae_get_property reads one and
        ae_set_property sets any of them — effect parameters, text, masks, expressions — with a keyframe when a time is given.
        Times are seconds, positions are composition pixels, colours are 0 to 1 or #rrggbb, rotations are degrees, scale and opacity are percent.
        ae_capture_frame returns a picture of a composition at a time: look at it before judging the result.
        Deeper work lives in skills that load on demand: animation (keyframes, easing, expressions, markers, time remap), text (fonts, styling, animators, text on a path), shapes (shape layer contents, masks), effects (the effects catalog, effects and their parameters, presets, layer styles), threed (cameras, lights, materials, geometry, renderer), compositing (precomps, footage, proxies, project files), rendering (the render queue, templates, rendering in place, Media Encoder), farm (background renders through aerender with progress and retries), templates (Essential Graphics, .mogrt, filling and substituting template compositions), transfer (the server's files by area, uploads and downloads), layout (pixel bounds, point conversion, selection, guides, splitting, placing by corners), settings (project settings, colour, expression engine, fonts, caches), viewer (views, channels, exposure, zoom), workers (an After Effects of your own, leased from a pool the server launches); ae_skills lists them, ae_enable_skill loads one and its tools join the list;
        find_tool searches the whole catalog by what you want to do, loaded or not, so ask it before concluding something has no tool.
        Four layers, from most to least specific: the dedicated tools, which find_tool locates; run_tools, which sends a list of their commands in one call
        and one undo step; run_program, a small JavaScript program that calls the commands itself with loops and results in hand;
        ae_run_menu_command for any command of the menus; ae_script last, for the API none of those reaches.
        ae_undo takes back the last tool call; After Effects' own language is Russian or English or another, so prefer match names and ids to display names.
        Every answer is JSON with an ok flag, and a failed call is also flagged isError. When ok is false, read error
        and hint and fix the call instead of retrying it verbatim; data.type names the failure.
        """;
}
