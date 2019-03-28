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
- Lucidtech.Las requires compatibility with .NET standard 2.0

### Quick start

Run inference and create prediction on document 
```C#
using Lucidtech.Las;

ApiClient apiClient = new ApiClient("<api endpoint>");
Prediction response = apiClient.Predict(documentPath: "document.pdf", modelName: "invoice");
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

Do a prediction of type document split.
```C#
using Lucidtech.Las;
string contentType = "application/pdf";
string modelType = "documentSplit";

ApiClient apiClient = new ApiClient("<endpoint>");
var res = JsonSerialPublisher.ObjectToDict<Dictionary<string, string>>(
    apiClient.PostDocuments(contentType, "<consentId>");

apiClient.PutDocument(documentPath: "document.pdf", contentType, res["uploadUrl"]);

var predictionResponse = apiClient.PostPredictions(res["documentId"], modelType);

JObject jsonResponse = JObject.Parse(predictionResponse.ToString());
var preds = JsonSerialPublisher.ObjectToDict<List<Dictionary<string, object>>>(jsonResponse["predictions"]);
// preds will now be a list with dictionaries with the following structure: List<Dictionary<string, object>>
// type: invoice
// start: 1
// end: 3
// confidence: 0.9912641852
    
```

## Contributing

### Prerequisites
Download version 14.1.0.0 of [MSBuild](https://aur.archlinux.org/msbuild-bin.git) (arch linux),
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
$ nuget install -OutputDirectory packages
$ msbuild Test/Test.csproj # Build 
$ nunit3-console Test/bin/Debug/Test.dll # Run
```

