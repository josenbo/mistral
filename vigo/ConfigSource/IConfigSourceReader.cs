namespace vigo;

internal interface IConfigSourceReader
{
    AppArguments Read(AppArguments initial);
}