using JetBrains.Annotations;

namespace vigobase;

[PublicAPI]
public interface IFlow
{
    void Initialize();
    bool IsFileProcessingRequired(DirectoryInfo directoryInfo);
    void ProcessFile(FileInfo fileInfo);
    bool IsFolderProcessingRequired(DirectoryInfo directoryInfo);
    void Build();
}
