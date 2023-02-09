# JSONConcat

## Overview
Tool to workaround Excel's PowerQuery data source interface, concatenating 
JSON objects stored arrays across multiple files into a single array of JSON Objects.

Example:

file1.json
```
[
{K1:V1},
{K2:V2},
]
```

file2.json
```
[
{K1:V1},
{K2:V2},
]
```
Concatenates to:
```
[
{K1:V1},
{K2:V2},
{K3:V3},
{K4:V4}
]
```

## Overview
- .NET 6.0/C#
- Streaming JSON Reader - supports very large files with low memory footprint
- Very Fast
- Supports serializer quirks such as trailing commas

## Usage
JSONConcat is a .NET CLI application, it takes two optional parameters as follows:
```
JSONConcat [<directory> [<outputfilename>]]
```

`directory` - path to a folder containing JSON files.  If omitted, will use shell current working directory.

`outputfilename` - path to a file for the concatenated data.  If omitted will output results to `STDOUT`.