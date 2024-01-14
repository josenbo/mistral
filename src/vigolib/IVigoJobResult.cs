namespace vigolib;

public interface IVigoJobResult
{
    // todo: file statistics (number read, number not in scope, number in scope)
    bool Success { get; }
}