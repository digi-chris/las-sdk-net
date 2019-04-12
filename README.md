# .NET SDK for Lucidtech AI Services

## Documentation

[Link to docs](https://docs.lucidtech.ai/dotnet/v1/index.html)

Create documents by using doxygen.
Download the latest and greatest version of [doxygen](https://github.com/doxygen/doxygen.git).
```bash
$ doxygen documentation.conf
```

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
- Necessary keys and credentials to an endpoint on the form: "https://<your prefix>.api.lucidtech.ai/<version>".

### Quick start

Run inference and create prediction on document 
```C#
using Lucidtech.Las;

ApiClient apiClient = new ApiClient("<endpoint>");
Prediction response = apiClient.Predict(documentPath: "document.pdf", modelName: "invoice|receipt|documentSplit");
Console.WriteLine(response.ToJsonString(Formatting.Indented));
```

Send feedback to the model.
```C#
using Lucidtech.Las;

var feedback = new List<Dictionary<string, string>>()
{ 
    new Dictionary<string, string>(){{"label", "total_amount"},{"value", "54.50"}},
    new Dictionary<string, string>(){{"label", "purchase_date"},{"value", "2007-07-30"}}
};
FeedbackResponse response = apiClient.SendFeedback(documentId: "<documentId>", feedback: feedback);
Console.WriteLine(response.ToJsonString(Formatting.Indented));
```

Revoke consent and deleting all documents associated with consentId.
```C#
using Lucidtech.Las;

ApiClient apiClient = new ApiClient("<endpoint>");
RevokeResponse response = apiClient.RevokeConsent(consentId: "<consentId>");
Console.WriteLine(response.ToJsonString(Formatting.Indented));
```

## Contributing

### Prerequisites
Download The latest and greatest stable version of [MSBuild](https://aur.archlinux.org/msbuild-stable.git) (arch linux),
and the latest and greatest version of [NuGet](https://github.com/NuGet/Home).
Download NUnit.Console version 3.9.0 or higher to run tests from command line.
```bash
$ nuget install NUnit.Console 
```


### Build and run tests
Clone repo and install the necessary packages manually for the las-sdk-net.
```bash
$ git clone git@github.com:LucidtechAI/las-sdk-net.git
$ cd las-sdk-net
$ msbuild Lucidtech/Lucidtech.csproj # Build Las library
$ msbuild Test/Test.csproj # Build Tests
$ nunit3-console Test/bin/<framework-version>/Debug/Test.dll # Run
```

