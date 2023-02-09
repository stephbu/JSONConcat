namespace JSONConcat;

/// <summary>State is a representation of the FSM for JSON parsing</summary>
internal enum State
{
    ArrayStart,
    Element,
    ArrayEnd,
}