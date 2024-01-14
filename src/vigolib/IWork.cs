namespace vigolib;

internal interface IWork
{
    string Name { get; }
    bool Prepare();
    bool Execute();
    bool Finish();
}