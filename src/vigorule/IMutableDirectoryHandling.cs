﻿namespace vigorule;

public interface IMutableDirectoryHandling : IMutableHandling
{
    DirectoryInfo SourceDirectory { get; }
    bool KeepEmptyDirectory { get; }
    bool IsEmptyDirectory { get; set; }
    IFinalDirectoryHandling CheckAndTransform();
}