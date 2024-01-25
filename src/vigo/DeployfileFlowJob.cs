using vigobase;

namespace vigo;

public class DeployfileFlowJob : IJob
{
    public bool Run()
    {
        const string baseDirPath = @"I:\INES-Github\_workspaces_\vigo\ines_hub_sample";
        const string archiveFilePath = @"I:\INES-Github\_workspaces_\vigo\archive_ines_hub_sample.tar.gz";

        return true;
    }
}