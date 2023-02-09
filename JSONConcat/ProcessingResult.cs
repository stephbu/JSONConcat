namespace JSONConcat;

using System;

/// <summary>ProcessingResult contains telemetry from processing of a file</summary>
internal struct ProcessingResult
{
    internal string Filename;
    internal int ElementCount;
    internal TimeSpan Duration;

    public override string ToString()
    {
        return $"{Filename} Elements:{ElementCount} Parsing:{Duration.TotalSeconds}s";
    }
}