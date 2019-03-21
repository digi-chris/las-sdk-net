# .NET SDK for Lucidtech AI Services

## Documentation

[Link to docs](https://docs.lucidtech.ai/dotnet/v1/index.html)

## Installation

### NuGet

```bash
$ nuget install Lucidtech.Las
```

## Usage

### Preconditions

- Documents must be in upright position
- Only one receipt or invoice per document is supported
- Supported file formats are: jpeg, pdf
- Lucidtech.Las requires compatibility with .NET standard 4.7

### Quick start

```C#
using Lucidtech.Las;

ApiClient apiClient = new ApiClient("<api endpoint>");
Prediction response = apiClient.Predict(documentPath: "document.pdf",modelName: "invoice");
```

## Contributing

### Prerequisites
Download the latest and greatest version of [MSBuild](https://github.com/Microsoft/msbuild).

Download NUnit.Console version 3.9.0 or higher to run tests from command line
```bash
$ nuget install NUnit.Console --version 3.9.0
```

### Run tests

```bash
$ msbuild Test/Test.csproj # Build 
$ nunit3-console Test/bin/Debug/Test.dll; # Run
```

