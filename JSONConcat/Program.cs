namespace JSONConcat;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class Program
{
    
    private const int BufferSize = 2 << 18;

    private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
    private static readonly JsonReaderOptions ReaderOptions = new JsonReaderOptions{AllowTrailingCommas = true};
    private static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions {Indented = false, SkipValidation = false};
    
    public static void Main(string[] args)
    {
        string? path = null;
        string? outputFilename = null;

        // args[0] == directory to scan, current working directory if unspecified
        // args[1] == output file name, STDOUT if unspecified
        if (args.Length == 0)
        {
            path = ".";
        }
        else
        {
            if (args.Length == 1)
            {
                path = args[0];
            }
            else if (args.Length == 2)
            {
                path = args[0];
                outputFilename = args[1];
            }
            else
            {
                throw new ArgumentException("Invalid argument count {args}");
            }
        }

        if (outputFilename != null)
        {
            List<ProcessingResult> result;
            using var fs = new FileStream(
                outputFilename, 
                FileMode.Create, 
                FileAccess.Write, 
                FileShare.None,
                BufferSize, 
                FileOptions.Asynchronous
                );
            using var writer = new Utf8JsonWriter(fs, WriterOptions);
            {
                result = JsonConcatenateDirectory(writer, path);
            }
            Console.WriteLine(result);
        }
        else
        {
            var stream = Console.OpenStandardOutput();
            using var writer = new Utf8JsonWriter(stream, WriterOptions);
            _ = JsonConcatenateDirectory(writer, path);
        }
    }

    private static List<ProcessingResult> JsonConcatenateDirectory(Utf8JsonWriter writer, string path)
    {
        writer.WriteStartArray();

        void WriteElement(JsonElement e)
        {
            JsonSerializer.Serialize(writer, e);
        }

        var result = EnumDirectory(path, WriteElement);

        writer.WriteEndArray();

        return result;
    }
    
    private static List<ProcessingResult> EnumDirectory(string path, Action<JsonElement> elementWriter)
    {
        var result = new List<ProcessingResult>();
        
        foreach (var f in Directory.EnumerateFiles(path))
        {
            var fileResult = EnumerateFile(f, elementWriter);
            result.Add(fileResult);
        }

        return result;
    }
    

    private static ProcessingResult EnumerateFile(string filepath, Action<JsonElement> writer)
    {

        var start = DateTime.UtcNow;
        
        ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(filepath);
        // Read past the UTF-8 BOM bytes if a BOM exists.
        if (jsonReadOnlySpan.StartsWith(Utf8Bom))
        {
            jsonReadOnlySpan = jsonReadOnlySpan.Slice(Utf8Bom.Length);
        }
        
        var totalElements = 0;
        var reader = new Utf8JsonReader(jsonReadOnlySpan, ReaderOptions);
        var expectedState = State.ArrayStart;
        
        while (reader.Read())
        {
            switch (expectedState)
            {
                case State.ArrayStart:
                {
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        expectedState = State.Element;
                        break;
                    }

                    throw new InvalidDataException($"Unexpected token {reader.TokenType} for state {expectedState.ToString()}");
                }

                case State.Element:
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                            var obj = JsonElement.ParseValue(ref reader);
                            totalElements = totalElements + 1;
                            // write obj to stdout
                            writer(obj);
                            break;
                        case JsonTokenType.EndArray:
                            expectedState = State.ArrayEnd;
                            break;
                        default:
                            throw new InvalidDataException($"Unexpected token {reader.TokenType} for state {expectedState.ToString()}");
                    }

                    break;
                }
                
                default:
                    throw new InvalidDataException($"Unexpected token {reader.TokenType} for state {expectedState.ToString()}");
            }
        }

        var result = new ProcessingResult{ Filename = filepath, ElementCount = totalElements, Duration = DateTime.UtcNow - start};
        return result;
    }
}