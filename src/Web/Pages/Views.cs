namespace Snail.MCP.AfterEffects.Web.Pages;

/// <summary>A section of the pages, named in the header.</summary>
internal sealed record Section(string Label, string Href);

/// <summary>One step of the path above a listing.</summary>
public sealed record Crumb(string Label, string Href);

/// <summary>What the last action did, said once at the top of the page it returned to.</summary>
public sealed record Flash(string Text, bool IsError);

/// <summary>A captioned group of status rows.</summary>
public sealed record StatusGroup(string Caption, IReadOnlyList<StatusRow> Rows);

/// <summary>A label, what it says, and whether what it says is a problem.</summary>
public sealed record StatusRow(string Label, string Value, bool IsProblem = false);

/// <summary>A row of a listing: a file, a folder or a render job's log.</summary>
public sealed record EntryView
{
    /// <summary>What the entry is called in its folder.</summary>
    public required string Name { get; init; }

    /// <summary>Where the page opens it: the folder's own page, or the file's.</summary>
    public required string Href { get; init; }

    /// <summary>Where a browser downloads it rather than showing it.</summary>
    public required string DownloadHref { get; init; }

    /// <summary>Whether it holds other entries.</summary>
    public bool IsFolder { get; init; }

    /// <summary>Its size, or everything under it for a folder.</summary>
    public long Bytes { get; init; }

    /// <summary>When it last changed; null when the file system did not say.</summary>
    public DateTimeOffset? Modified { get; init; }

    /// <summary>A job's state, for the jobs area.</summary>
    public string? State { get; init; }

    /// <summary>Frames done, for the jobs area.</summary>
    public string? Detail { get; init; }

    /// <summary>Why it cannot be deleted now: the job is still running.</summary>
    public string? InUse { get; init; }

    /// <summary>The job the log belongs to, which the cancel form names.</summary>
    public string? JobId { get; init; }

    /// <summary>Whether the job behind this log is still going, which is what keeps its log from being deleted.</summary>
    public bool IsRunning => Application.Farm.JobStates.IsActive(State ?? string.Empty);
}

/// <summary>One folder of an area.</summary>
public sealed record BrowseView
{
    /// <summary>The area this folder belongs to: files, captures, renders or jobs.</summary>
    public required string Area { get; init; }

    /// <summary>What the page is called in the header and the browser's tab.</summary>
    public required string Title { get; init; }

    /// <summary>The folder inside the area, empty for the area itself.</summary>
    public required string Path { get; init; }

    /// <summary>The path above this folder, step by step.</summary>
    public required IReadOnlyList<Crumb> Crumbs { get; init; }

    /// <summary>What is in the folder, folders first.</summary>
    public IReadOnlyList<EntryView> Entries { get; init; } = [];

    /// <summary>Whether the listing stopped short because the folder holds more than a page shows.</summary>
    public bool IsTruncated { get; init; }

    /// <summary>Where the whole folder comes down as one archive; null when it is empty.</summary>
    public string? ZipHref { get; init; }

    /// <summary>What the last action did, shown once above the listing.</summary>
    public Flash? Flash { get; init; }

    /// <summary>Why the folder could not be listed.</summary>
    public string? Problem { get; init; }

    /// <summary>The jobs area can forget everything that has ended in one go.</summary>
    public bool CanPrune { get; init; }

    /// <summary>Whether this is the jobs area, whose rows carry a state and a cancel form.</summary>
    public bool HasJobs => Area == Application.Storage.VolumeAreas.Jobs;

    /// <summary>Everything the listing holds together.</summary>
    public long Bytes => Entries.Sum(entry => entry.Bytes);
}

/// <summary>One file of an area, with what can be shown of it.</summary>
public sealed record FileView
{
    /// <summary>The area the file belongs to.</summary>
    public required string Area { get; init; }

    /// <summary>What the page is called in the header and the browser's tab.</summary>
    public required string Title { get; init; }

    /// <summary>The file's own name.</summary>
    public required string Name { get; init; }

    /// <summary>The folder it sits in, inside the area.</summary>
    public required string Folder { get; init; }

    /// <summary>The path above the file, step by step.</summary>
    public required IReadOnlyList<Crumb> Crumbs { get; init; }

    /// <summary>Its size.</summary>
    public long Bytes { get; init; }

    /// <summary>Where the file itself is served, which an image tag and a preview use.</summary>
    public required string RawHref { get; init; }

    /// <summary>Where a browser downloads it rather than showing it.</summary>
    public required string DownloadHref { get; init; }

    /// <summary>Whether the page shows it as a picture.</summary>
    public bool IsImage { get; init; }

    /// <summary>The file's text when it is small text; null otherwise.</summary>
    public string? Text { get; init; }
}
