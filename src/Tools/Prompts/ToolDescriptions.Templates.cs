namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Templates
    {
        public const string Info =
            "A composition as a motion graphics template: its template name, the controllers exposed in Essential Graphics, and the instances of " +
            "templates among its layers (nested compositions with essential properties) with each property's name, type and value.";

        public const string Update = "Name the template a composition exports as, or open it in the Essential Graphics panel.";

        public const string AddProperty =
            "Expose a property of a layer as a controller of the composition's template, under a name of your choosing: a number, colour, " +
            "checkbox, popup, text, 2D point or media property; others are refused with the reason.";

        public const string Fill =
            "Fill an instance of a template: set the essential properties of a nested composition layer by controller name (text as a string, " +
            "colours as #rrggbb or [r, g, b], numbers, points; a media controller takes a footage item by name or id, null to clear), as keyframes " +
            "when time is given. The answer lists the properties afterwards.";

        public const string Substitute =
            "Customise a template composition directly by layer name: new text for text layers, a colour for solids, text, shape fills or a Fill " +
            "effect, and new footage or compositions for footage layers. Use it on compositions that are templates by convention rather than Essential Graphics.";

        public const string Export =
            "Export a composition as a .mogrt motion graphics template, to a file or folder, or into the Essential Graphics library when no path is " +
            "given. The composition needs at least one controller; its template name defaults to its own.";

        public const string Import =
            "Import a .mogrt file into the project as a composition, with its controllers; into a folder when one is named. Where this After Effects " +
            "refuses scripted import the answer says so.";
    }

    public static partial class Parameters
    {
        public const string TemplateValues = "An object of controller names (or indices as ae_template_info numbers them) to values.";
    }
}
