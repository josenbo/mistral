using System.Data.SqlTypes;

namespace vigo;

internal interface IJobRunner
{
    bool Success { get; }
    bool Prepare();
    bool Run();
    void CleanUp();
}