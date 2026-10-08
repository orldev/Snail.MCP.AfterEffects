namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Project
    {
        public const string Info =
            "The open project: its file, whether it has unsaved changes, how many compositions, footage items and folders it holds, the active " +
            "item and the selection. Read it first to learn what is there.";

        public const string New =
            "Start a new empty project. Refused while the open project has unsaved changes unless discard is true, so a user's work is never thrown away in silence.";

        public const string Open =
            "Open a project file, replacing the open one. Refused while the open project has unsaved changes unless discard is true. Dialogs about " +
            "missing footage are suppressed; ae_list_items with kind footage then shows what is missing.";

        public const string Save = "Save the project in place, or under the path given; the folder is created when it does not exist.";

        public const string Close =
            "Close the project and leave After Effects with a new empty one. Refused while it has unsaved changes unless discard is true.";

        public const string ListItems =
            "The items of the project panel — compositions, footage and folders — with ids, sizes, durations and, for footage, the file and whether it is " +
            "missing. Filter by kind, folder or name; the id is what later calls take when names repeat.";

        public const string ItemInfo =
            "Everything about one item: a composition's settings and its layer list, a footage item's source, a folder's children, and the compositions " +
            "the item is used in.";

        public const string ItemUpdate = "Rename an item, change its comment or label colour, or move it into a folder.";

        public const string ItemRemove =
            "Delete an item from the project. Layers that use it are deleted with it, and so is everything inside a folder; ae_item_info shows where it is used first.";

        public const string FolderCreate = "Create a folder in the project panel, at the root or inside another folder.";

        public const string ImportFile =
            "Import a file into the project: video, audio, a still, an image sequence, a layered .psd or .ai as a composition, or another .aep merged in. " +
            "The imported item comes back with its id; use ae_add_layer to put it into a composition.";
    }
}
