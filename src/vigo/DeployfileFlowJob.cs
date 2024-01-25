using vigobase;
using vigoflow_deployfile;

namespace vigo;

public class DeployfileFlowJob : IJob
{
    public bool Run()
    {
        const string baseDirPath = @"I:\INES-Github\_workspaces_\vigo\ines_hub_sample";
        const string archiveFilePath = @"I:\INES-Github\_workspaces_\vigo\archive_ines_hub_sample.tar.gz";

        var flow = DeployfileFlowFactory.Create(new DirectoryInfo(baseDirPath), new FileInfo(archiveFilePath));
        
        flow.Initialize();
        ProcessDirectory(flow, new DirectoryInfo(baseDirPath));
        flow.Build();
    }

    private void ProcessDirectory(IFlow flow, DirectoryInfo currentDirectory)
    {
        // Check if the file needs to be processed
        if (flow.IsFileProcessingRequired(currentDirectory))
        {
        }

        if (flow.IsFolderProcessingRequired(currentDirectory))
        {
            
        }

        // Enumerate all files in the current directory
        foreach (var file in currentDirectory.GetFiles())
        {
            // Check if the file needs to be processed
            if (flow.IsFileProcessingRequired(file.Directory))
            {
                // Process the file
                flow.ProcessFile(file);
            }
        }

        // Enumerate all subfolders in the current directory
        foreach (var subfolder in currentDirectory.GetDirectories())
        {
            // Check if the subfolder needs to be processed
            if (flow.IsFolderProcessingRequired(subfolder))
            {
                // Process the subfolder
                flow.ProcessSubfolder(subfolder);

                // Recursively process the subdirectory
                ProcessDirectory(flow, subfolder);
            }
        }
    }
}