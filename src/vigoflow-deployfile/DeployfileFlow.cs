using vigobase;

namespace vigoflow_deployfile;

public class DeployfileFlow : IFlow
{
    public void Initialize()
    {
    }

    public bool IsFileProcessingRequired(DirectoryInfo directoryInfo)
    {
        return true;
    }

    public void ProcessFile(FileInfo fileInfo)
    {
    }

    public bool IsFolderProcessingRequired(DirectoryInfo directoryInfo)
    {
        return true;
    }

    public void ProcessSubfolder(DirectoryInfo directoryInfo)
    {
    }

    public void Build()
    {
    }
}