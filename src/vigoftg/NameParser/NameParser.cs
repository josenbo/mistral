using Ardalis.GuardClauses;
using Serilog;
using vigobase;

namespace vigoftg;

internal class NameParser : INameParser
{
    internal NameParser(IEnvironmentDescriptor descriptor)
    {
        _descriptor = descriptor;
    }

    public INameParseResult Parse(string name)
    {
        var sanitizedFileName = Guard.Against.NullOrWhiteSpace(name).Trim();
        return new NameParseResult(_descriptor, sanitizedFileName);
    }

    private readonly IEnvironmentDescriptor _descriptor;
}