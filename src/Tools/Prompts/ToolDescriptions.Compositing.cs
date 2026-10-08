namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Compositing
    {
        public const string Precompose =
            "Move layers into a new nested composition in their place: the layers named, the new composition's name, and whether their " +
            "attributes (transform, effects, masks) move with them or stay on the new layer, which works for one layer only.";

        public const string Tree =
            "The compositions nested in a composition, as a tree with the layer each sits on, down to a depth; and the compositions this one is " +
            "used in. Read it before changing a composition several others depend on.";

        public const string ReplaceFootage =
            "Relink or replace a footage item for every layer that uses it: with a file (or an image sequence starting at that file), with a solid " +
            "of a colour, or with a placeholder. Interpretation stays as it was.";

        public const string Interpret =
            "Interpret footage as the Interpret Footage dialog does: alpha as ignored, straight or premultiplied (or guessed), inverted, frame rate " +
            "to conform to, field separation, pulldown removal (or guessed), loop count and pixel aspect ratio. With nothing to change the answer " +
            "reports the interpretation as it stands.";

        public const string Proxy =
            "Set, switch or remove the proxy of a footage item or composition: a file, an image sequence, a solid or a placeholder stands in while " +
            "use is true, and the full item renders when it is false.";

        public const string ReplaceLayerSource =
            "Put another footage item or composition on a layer, keeping the layer's transform, effects, masks and keyframes; expressions that " +
            "name the old source are fixed when asked.";

        public const string ProjectFootage =
            "The footage of the project with the file each points to, whether it is missing and how many compositions use it; only the missing " +
            "or only file-backed items when asked. Relink missing files with ae_replace_footage.";

        public const string Cleanup =
            "Tidy the project as the File menu does: consolidate duplicate footage, remove footage no composition uses, or reduce the project to the " +
            "items named and what they use. The answer counts what went.";

        public const string CollectFiles =
            "Collect the project into a folder: every file the footage points to is copied into a (Footage) folder, the items are relinked to the " +
            "copies, and the project is saved there under its name; missing footage is listed and skipped. The open project becomes the collected copy.";
    }

    public static partial class Parameters
    {
        public const string Alpha = "IGNORE, STRAIGHT or PREMULTIPLIED.";

        public const string Fields = "OFF, UPPER_FIELD_FIRST or LOWER_FIELD_FIRST.";
    }
}
