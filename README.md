# .NET SDK for Lucidtech AI Services API

## Usage
Instantiate the Lucidtech.LAS.Client with your API key:

``` C#
using Lucidtech.LAS;

// ...

string apiKey = "...";
var client = new Client(apiKey);
```

To scan a receipt, call the API either with a URL to an image file:
```C#
string receiptUrl = "...";
List<Detection> detections = client.ScanReceipt(receiptUrl);
```

or with a System.IO.Stream object:

```C#
FileStream stream = new FileStream(/* ... */); 
List<Detection> detections = client.ScanReceipt(stream);
```

The detection class contains a label (e.g. total or date), a confidence estimation and the extracted value:
```C#
foreach (var detection in detections)
{
    Console.WriteLine(detection.Label);
    Console.WriteLine(detection.Confidence);
    Console.WriteLine(detection.Value);
}
```

Supported image file types: PNG, JPEG and GIF.
