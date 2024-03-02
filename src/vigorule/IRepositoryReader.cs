﻿namespace vigorule;

public interface IRepositoryReader
{
    delegate void BeforeApplyFileHandling(IMutableFileHandling transformation);
    delegate void BeforeApplyDirectoryHandling(IMutableDirectoryHandling transformation);
    delegate void AfterApplyFileHandling(IFinalFileHandling transformation);
    delegate void AfterApplyDirectoryHandling(IFinalDirectoryHandling transformation);
    
    event BeforeApplyFileHandling? BeforeApplyFileHandlingEvent;
    event BeforeApplyDirectoryHandling? BeforeApplyDirectoryHandlingEvent;
    event AfterApplyFileHandling? AfterApplyFileHandlingEvent;
    event AfterApplyDirectoryHandling? AfterApplyDirectoryHandlingEvent;

    IEnumerable<T> FinalItems<T>(bool canDeployOnly) where T : IFinalHandling;

    void Read();
}


