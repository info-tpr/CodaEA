![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# Creating A CodaClient PlugIn

## About PlugIns

As of right now, PlugIns have one or two possible connection points and function types.  The purpose of a PlugIn for CodaClient is to provide custom error log processing that cannot otherwise be handled by the built-in format handlers.

A PlugIn can perform either a Line Processing function, or a File Processing function, or both.

## Line Processing

Line Processing means that the CodaClient will load the text file, and call the PlugIn line by line.  This means that the lines are delimited by CR, LF, or CR/LF pairs - as long as the log file conforms to this format, a Line Processor is a good choice.  If not, you will want to do a File Processor.

## File Processing

The File Processor is a little more complex - it has a method to open the file, and another method to read log entries one at a time.  This is done so that your PlugIn can efficiently process a stream, instead of loading the whole stream into memory at once.

# Getting Started

To get started, you will need Visual Studio 2022 or later.  Visual Studio Community is perfect.

1. Download the CodaClient.Plugin project from our GitHub repository. This provides starter code you will need, and an example of a project.
1. Create a new project, and select Class Library / C#.  Right now, that is the only language supported.
1. Give your Class Library a name.  This will become your own namespace.  For example, you may want to call it `AcmeCorp.CodaPlugin`.
1. Target the .NET 6 framework, or whatever is currently matching your version of CodaClient.
1. You will need to go to the NuGet Package manager, and add Newtonsoft.Json to your project.
1. Add a Project Reference from your project to the CodaClient.Plugin project.
1. The Visual Studio Class Library template creates a class `Class1.cs` - you may want to rename this, or leave it as you desire.  Your custom class must implement the `ICodaPlugin` interface, like this:

```
using Newtonsoft.Json.Linq;
using CodaClient.Plugin;  // <-- Very important

namespace AcmeCorp.CodaPlugin
{
    public class Class1 : ICodaPlugIn // <-- Very important
    {

    }
}
```

8. You will get the following error: CS0535 '{your class}' does not implement interface member...  You can use the CodeLens suggestion to "Implement interface", now you will see:

```
using Newtonsoft.Json.Linq;
using CodaClient.Plugin;

namespace AcmeCorp.CodaPlugin
{
    public class Class1 : ICodaPlugin
    {
        // Allows your code to set end of stream
        private bool _EndOfStream;
        public bool EndOfStream { get { return _EndOfStream; } }
        // Whether or not your PlugIn supports the full File Processor members
        public bool FileProcessor { get { return true; } }
        // Whether or not your PlugIn supports the Line Processor member
        public bool LineProcessor { get { return true; } }

        public string Version { get { return "2022.1"; } }

        public string Name { get { return "AcmeCorp Log Processor Plugin"; } }

        public string Description { get { return "This plugin processes log files from AcmeCorp software, Copyright © 2022, All Rights Reserved"; } }

        public JObject? NextLogItem()
        {
            throw new NotImplementedException();
        }

        public bool OpenLogFile(string FilePath, JObject ConfigOptions)
        {
            throw new NotImplementedException();
        }

        public JObject ProcessLineItem(string LogLine, JObject ConfigOptions)
        {
            throw new NotImplementedException();
        }
    }
}
```

9. As is shown above, remove all the throw calls and replace them as desired.  If your PlugIn supports the Line Processor, return `true`; if it supports the File Processor, return`true`.
9. Don't forget to add your reference to `Newtonsoft.Json.Linq` and `CodaClient.Plugin`.
9. Finally, you will get an error that you must implement the Interface members - you can use CodeSense to insert the appropriate code for you.


## Programming a Line Processor

If you set `LineProcessor = true`, you will need to override the `ProcessLineItem()` method.  To do so, start typing `public override Process`... and you will be prompted to complete it.

Then, you can write your code as you need, for example:

```
    public override JObject ProcessLineItem(string LogLine, JObject ConfigOptions)
    {
        var logItem = new JObject()
        {
            ["TimeOccurredUTC"] = GetLogTime(LogLine, ConfigOptions),
            ["Severity"] = GetSeverity(LogLine, ConfigOptions),
            ["Network"] = "my-app",
            ["ReportingProcess"] = GetProcess(LogLine, ConfigOptions),
            ["ErrorCode"] = GetHashCode(LogLine, ConfigOptions),
            ["ErrorMessage"] = GetMessage(LogLine, ConfigOptions),
            ["OtherData"] = LogLine,
        };
        return logItem;
    }
```

## Programming a File Processor

A File Processor, as noted earlier, is responsible for reading the file using whatever method you deem.  It has 2 methods and a property that CodaClient will call.

First, CodaClient will call `OpenLogFile()`, which you will override to establish a stream reader and get things started.  Next, CodaClient will check the `EndOfStream` property, and if `false`, call the `NextLogItem()` method to return the next log entry.  You will also need to override that to do the work of parsing individual log entries.

Note in the example below, we also create private members to capture the config options JObject and the StreamReader to read the log file.

```
    private StreamReader _LogStream;
    private JObject _Options;

    public bool OpenLogFile(string FilePath, JObject ConfigOptions)
    {
        // Open file stream reader
        _LogStream = new StreamReader(FilePath);
        _Options = ConfigOptions; // NOTE - We capture the ConfigOptions for use in other functions
    }

    public JObject? NextLogItem()
    {
        if (_LogStream.EndOfStream)
        {
            _LogStream.Close();
            _EndOfStream = true;
            return null;
        }
        return GetNextLogEntry();
    }
```

Finally, CodaClient provides to you the date this analysis was last run, in UTC adjusted time zone in the `UTCLastRunDate` property.  You can use this to filter your log parsing results.

# Deploying a PlugIn in your CodaClient

If you wish to deploy a PlugIn, you must configure the CodaClient so that it knows about the PlugIn.  Then, you configure your job config file to run analyze jobs using that PlugIn.

<a name="plugin_config">

## Configuring CodaClient Plugins

In order for CodaClient to know about your PlugIn, you have to do the following:

1. Deploy the build files (e.g. from the Release/net6.0 folder) to the CodaClient "plugins" folder.  The build files include your project name in them, for example `AcmeCorp.CodaPlugin.dll`, `AcmeCorp.CodaPlugin.deps.json`, `AcmeCorp.CodaPlugin.pdb` (useful for debugging), and `AcmeCorp.CodaPlugin.runtimeconfig.json`.
1. Edit the CodaClient.Plugins.json file in your CodaClient directory.  This is a JObject, where each field is an object defining the PlugIn to CodaClient.  For example, if you wanted to name your PlugIn `acmecorp` you would do:


```
{
  "acmecorp": {
    "description": "Acme Corp Log Processor",
    "github": "https://github.com/acmecorp",
    "pluginId": null,
    "className": "AcmeCorp.CodaPlugin.Class1",
    "version": "2022.1",
    "publisher": "The Acme Manufacturing Corporation, makers of the Portable Hole and Rocket Roller Skates",
    "publishDate": "05/18/2022T15:53:25Z"
  }
}
```
The name of the plugin is given as its field name.  This name must match what is used in the analyze job after the `text/` or `file/` specifier in the [input specs](CodaClient_CLI#inputspecs).

The `description` and `className` fields are required in order to operate; `github` is optional, and `pluginId` MUST be `null` if you add the entry yourself.  If the PlugIn is downloaded via the CodaClient from the CodaEA PlugIn Repository, then a unique ID will be assigned and the other fields filled out.

NOTE that ALL fields must exist, but only `description` and `className` are required for operation on self-published Plugins.

Then, in order to use your PlugIn, you would configure it in your job configuration file:

```
...
  },
  "analyze": [
    {
      "name": "acme-log",
      "input": "text/acmecorp",
      "messageType": {
        "values": [ "Error" ]
      },
      "inputSpecs": {
        "inputFile": "C:\\Users\\JayImerman\\Downloads\\cnodelog.tar\\cardano-node-20220405183200.log",
        "your-field-1": "error",
        "your-field-2": 1,
        ...
      }
    }
  ]
}

```

So, in the above portion of the job config file, the `analyze` section you give your source a name, in this case `acme-log` (this is irrelevant - whatever you want to call it).  The `input` is defined as `text/acmecorp`.  This is key - `text` indicates a line processor, and `acmecorp` is the PlugIn name from the Plugins json file.

The `messageType` field still provides a filter list for application-reported message severity that you want to focus on.

Finally, note that the `inputSpecs` field requires an `inputFile` field; other than that, all the other fields are up to you.  The entire `inputSpecs` object will be passed into the PlugIn methods for use within your code.
