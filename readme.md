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
{K1:V1},
{K2:V2}
]
```

## Overview
- .NET 6.0/C#
- Streaming JSON Reader - supports very large files with low memory footprint
- Very Fast
- Supports serializer quirks such as trailing commas