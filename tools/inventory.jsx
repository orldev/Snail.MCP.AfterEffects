// Dumps the scripting object model After Effects exposes: every property and method of each object kind this project can reach,
// read through ExtendScript's reflect interface on live instances. Writes JSON to OUT_PATH.
(function () {
  try {
    var OUT_PATH = (new Folder("~/.s3l-mcp-aftereffects").exists ? "~/.s3l-mcp-aftereffects" : "~/.snail-mcp-aftereffects") + "/state/scripting-api.json";
    var LOG = new File(OUT_PATH.replace(/api\.json$/, "progress.log"));
    function log(line) { LOG.open("a"); LOG.writeln(new Date().toTimeString().substring(0, 8) + " " + line); LOG.close(); }
    function names(list) { var out = []; for (var i = 0; i < list.length; i++) { out.push(list[i].name); } out.sort(); return out; }
    function describe(obj) {
        var out = { properties: [], methods: [] };
        try { out.properties = names(obj.reflect.properties); } catch (e) { out.error = String(e); }
        try { out.methods = names(obj.reflect.methods); } catch (e2) { out.error2 = String(e2); }
        return out;
    }
    function json(value) {
        if (value === null || value === undefined) { return "null"; }
        if (typeof value === "number" || typeof value === "boolean") { return String(value); }
        if (typeof value === "string") { return '"' + value.replace(/\\/g, "\\\\").replace(/"/g, '\\"').replace(/\n/g, "\\n") + '"'; }
        if (value instanceof Array) { var a = []; for (var i = 0; i < value.length; i++) { a.push(json(value[i])); } return "[" + a.join(",") + "]"; }
        var o = []; for (var k in value) { if (value.hasOwnProperty(k)) { o.push(json(k) + ":" + json(value[k])); } } return "{" + o.join(",") + "}";
    }
    log("start"); app.beginSuppressDialogs();
    var kinds = {};
    var proj = app.project;
    var comp = proj.items.addComp("__inventory", 320, 240, 1, 2, 25);
    var solid = comp.layers.addSolid([1, 0, 0], "s", 320, 240, 1);
    var text = comp.layers.addText("t");
    var shape = comp.layers.addShape();
    var cam = comp.layers.addCamera("c", [160, 120]);
    var light = comp.layers.addLight("l", [160, 120]);
    var nullL = comp.layers.addNull();
    var folder = proj.items.addFolder("__inv");
    var rq = proj.renderQueue; var rqItem = rq.items.add(comp); var om = rqItem.outputModule(1);
    var mask = solid.property("ADBE Mask Parade").addProperty("ADBE Mask Atom");
    var fx = solid.property("ADBE Effect Parade").addProperty("ADBE Gaussian Blur 2");
    var anim = text.property("ADBE Text Properties").property("ADBE Text Animators").addProperty("ADBE Text Animator");
    var sel = anim.property("ADBE Text Selectors").addProperty("ADBE Text Selector");
    var group = shape.property("ADBE Root Vectors Group").addProperty("ADBE Vector Group");
    var rect = group.property("ADBE Vectors Group").addProperty("ADBE Vector Shape - Rect");
    var doc = text.property("ADBE Text Properties").property("ADBE Text Document").value;
    var targets = {
        app: app, project: proj, items: proj.items, CompItem: comp, FolderItem: folder, FootageItem: solid.source, AVLayer: solid, TextLayer: text,
        ShapeLayer: shape, CameraLayer: cam, LightLayer: light, NullLayer: nullL, LayerCollection: comp.layers, Property: solid.property("ADBE Transform Group").property("ADBE Position"),
        PropertyGroup: solid.property("ADBE Transform Group"), MaskPropertyGroup: mask, Effect: fx, TextAnimator: anim, TextSelector: sel, VectorGroup: group, VectorRect: rect,
        TextDocument: doc, RenderQueue: rq, RQItem: rqItem, OutputModule: om, Viewer: app.activeViewer, Preferences: app.preferences, Settings: app.settings,
        MarkerValue: new MarkerValue("m"), Shape: new Shape(), KeyframeEase: new KeyframeEase(0, 33), ImportOptions: new ImportOptions(), Fonts: null, System: system
    };
    try { targets.Fonts = app.fonts; } catch (eF) { }
    try { targets.Font = app.fonts.allFonts[0][0]; } catch (eF2) { }
    try { targets.CharacterRange = doc.characterRange(0, 1); } catch (eC) { }
    try { targets.ParagraphRange = doc.paragraphRange(0, 1); } catch (eP) { }
    try { targets.Viewer = app.activeViewer; targets.View = app.activeViewer ? app.activeViewer.views[0] : null; targets.ViewOptions = targets.View ? targets.View.options : null; } catch (eV) { }
    for (var k in targets) { if (targets.hasOwnProperty(k) && targets[k]) { log("describe " + k); kinds[k] = describe(targets[k]); } }
    log("enums");
    var enums = ["AlphaMode","AutoOrientType","BlendingMode","CloseOptions","FieldSeparationType","FrameBlendingType","ImportAsType","KeyframeInterpolationType","Language","LayerQuality","LayerSamplingQuality","LightType","LogType","MaskFeatherFalloff","MaskMode","MaskMotionBlur","ParagraphJustification","PostRenderAction","PREFType","PropertyType","PropertyValueType","PulldownMethod","PulldownPhase","PurgeTarget","ResolveType","RQItemStatus","TimeDisplayType","ToolType","TrackMatteType","ViewerType","FootageTimecodeDisplayStartType","FeetFramesFilmType","FramesCountType","GetSettingsFormat","ChannelType","CTFontTechnology","CTFontType","CTScript","CTStyleProperty","BoxAutoFitPolicy","BoxVerticalAlignment","ComposerEngine","DigitSet","FontBaselineOption","FontCapsOption","LineJoinType","LineOrientation","AutoKernType","GpuAccelType","ShapeDirection","BoxFirstBaselineAlignment","ParagraphDirection","LeadingType","BaselineDirection"];
    var enumsOut = {};
    for (var e = 0; e < enums.length; e++) { try { var en = eval(enums[e]); var ks = []; for (var kk in en) { if (/^[A-Z]/.test(kk)) { ks.push(kk); } } ks.sort(); enumsOut[enums[e]] = ks; } catch (ignored) { } }
    log("effects");
    var effects = []; for (var f = 0; f < app.effects.length; f++) { effects.push([app.effects[f].matchName, app.effects[f].displayName, app.effects[f].category]); }
    log("cleanup");
    rqItem.remove(); comp.remove(); folder.remove();
    var out = { version: app.version, kinds: kinds, enums: enumsOut, effects: effects };
    var file = new File(OUT_PATH); file.encoding = "UTF-8"; file.open("w"); file.write(json(out)); file.close();
    log("written"); app.endSuppressDialogs(false);
    return "written";
  } catch (error) { try { var f = new File((new Folder("~/.s3l-mcp-aftereffects").exists ? "~/.s3l-mcp-aftereffects" : "~/.snail-mcp-aftereffects") + "/state/scripting-api.log"); f.open("a"); f.writeln("ERROR " + error + " line " + error.line); f.close(); } catch (e2) { } }
})();
