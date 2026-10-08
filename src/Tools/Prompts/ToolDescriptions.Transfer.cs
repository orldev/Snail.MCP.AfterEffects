namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Transfer
    {
        public const string Files =
            "The files this server keeps, by area: files (the project folder: footage, uploads), captures, renders, jobs (farm logs with each job's state). " +
            "Lists a folder with sizes; over HTTP each entry can carry a signed download link.";

        public const string Upload =
            "Bring a file or folder from the client's machine into the files area. Over HTTP the answer is a shell command with a short-lived signed link " +
            "to run on the client; over stdio both are the same machine and the answer says where the file goes.";

        public const string Download =
            "Take a file or folder of an area to the client's machine: over HTTP a shell command with a signed link (a folder as one tar), over stdio " +
            "the local path, since the file is already here.";
    }

    public static partial class Parameters
    {
        public const string Area = "files, captures, renders or jobs.";

        public const string RemotePath = "Path inside the area, forward slashes; empty for the area itself.";
    }
}
