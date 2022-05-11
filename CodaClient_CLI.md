![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# CodaClient Command Line Utility

## Purpose of CodaEA
CodaEA is an Error Analysis ecosystem designed to link communities around networks or applications for mutual support.  
Network Operators typically have the job of running server software, which emit execution logs.  Often, the error messages 
are cryptic or otherwise difficult to determine a) whether they are siginificant or not, and b) what you should do to fix it - 
especially if you are new to operating that software.

CodaEA is the solution.  It provides a public REST API to report the errors, and query to determine what the community has
analyzed as the appropriate meaning and severity of the error.  It facilitates discussion and troubleshooting.

Community Members earn reputation for their participation, and that reputation will be tied to the CODA Token cryptocurrency, which
are awarded to the Members' Cardano wallet by the end of every epoch.

CODA Tokens can also be offered as bounties to help incentivize and prioritize focus on the solution of specific error messages.

The CODA Token payouts and features will be coming later this year.

In order to be able to access the Coda database, you must create and validate an Operator Account.  To do this, please navigate
to [The Parallel Revolution](https://www.theparallelrevolution.com/Coda) and follow the process to request access.

There are 3 types of Accounts:  Operator, Organizational Administrator, and Organizational User.  CodaClient is designed for
use by any of these Accounts.

## CodaClient
The CodaClient is a command line utility that allows you, as a Network Operator, to check the output of your logs against the
CodaEA database of community input on a scheduled or ad-hoc basis, and to query and participate interactively in the analysis,
troubleshooting and discussion of the Error Codes.

The purpose of the CodaClient is to make it easy to interact with the CodaEA system.  CodaEA is a RESTful API, and as such,
the use of CodaClient is provided as a convenience.  You certainly can utilize the curl command, or write your own code to
make the API calls.  However, since all the data is in JSON format, it may be more tedious to deal with.

For full specifications on the CodaEA REST API endpoints, you can simply navigate by browser to the servers:

Mainnet:  https://prod.codaea.io

Testnet:  https://test.codaea.io


## Command Line Parameters
Syntax:

`    codaclient.<os> <path-to-config-file> <command> <command-options>`

Example:
`    codaclient.linux ./cardano-config.json az`

    <os>:  Supported Operating Systems are Linux (Ubuntu 20.04 LTS)

    <path-to-config-file>:  See the section on Main Configuration File for file schema.

    <command>:  One of the following commands.  Each command will have a section below that will detail options.  Note that
                each command has an alias you can use as a shorthand form.

Command | Alias | Purpose
------- | ----- | -----
[--accountquery](#accountquery) | aq | Allows you to query or update your Account on CodaEA.  IMPORTANT NOTE:  By registering, you will receive both a Private Key, and an API Key.  These keys are used to operate and recover your Account, and must be kept safe.  Retrieval of Account data will obfuscate these keys.
[--analyze](#analyze) | az | This command will query a log, and submit the entries against the CodaEA database, and produce a report.  The report is in XML format, with XSLT stylesheet to make it viewable and interactive on a web browser.
[--errorquery](#errorquery) | eq | Use this to query a specific error message, retrieve its community input, and post to it.  Please see [Community Rules](Community_Rules.md) for the rules and rewards for participation.
[--errorupdate](#errorupdate) | eu | Once you have earned the Moderator badge, you can post updates to an Error Message.  The updates you can post are Accepted Meaning, and Accepted Severity.
[--prometheusmerge](#prometheusmerge) | pm | In addition to the above commands, you can add a `prometheusmerge` command at the end to merge multiple Prometheus Node Exporter outputs into a single file.


<a name="accountquery">

### accountquery Command

An Account is used to offer access to CodaEA.  In order to access  the functions and data of CodaEA, you must have an Account,
and that Account must be active.  See [About Accounts](Coda_Accounts.md) for more information.

Using `accountquery` you can retrieve or update your account info.  Specify what it is you want it to do using options following
the `accountquery` command.

Example:

`codaclient.linux ./myconfig.json aq 7548 --full`

#### Options

Parameters | Description
---- | ----
\{accountId} | (Optional) the Account ID to query; if not specified, your own account is retrieved.  Example:  `codaclient.linux -aq 5345`
--full | Use this option to specify retrieving Reputation History on the account


<a name="analyze">

### analyze Command

Analyze causes CodaClient to check logs in specific formats.  If your logs are in a format that is not supported, you may
either request support on our [Github project page](https://github.com/info-tpr/CodaEA), or investigate whether you can
configure the application to output in one of the supported formats, or write some code that will transform your application 
logs to that format.

If you run the Analyze command, all analyses in the array will be run in order.  If you want to change the order or specify specific analyses, use the --analysis= option, like so:

`codaclient.linux ./cardano-config.json az --analysis=node-log,prometheus-log`

<a name="errorquery">

### errorquery Command

The Error Query command allows you to browse specific error messages, or list messages needing attention from the community.  It is also the main entrypoint to interacting with the errors - you can see discussions and troubleshooting steps, vote on how useful you found them, and post your own responses.  Don't forget, every vote and post gives you a boost to your Reputation score!

Once you execute the errorquery command, the results will be displayed to you and a command prompt will let you enter interactive commands to work with it.

Each Parameter is to be used by itself, with no other parameters.  For example, if you use `--code=`, don't use any of the other ones.  Parameters to the errorquery command are as follows:

Parameter | Description
---- | ----
--code= | Specifies the error code to retrieve.  For example: `codaclient.linux eq --code="5310"`
--unanalyzed --uz | Use either the long or short version of this option to retrieve a list of Error Codes for your selected Network that have not been analyzed yet for Severity or Meaning.  Check out [Community Rules](Community_Rules.md) for a list of privileges that allow you to update Error Codes and how you earn them.
--unanswered --ua | Use either the long or short version of this option to retrieve a list of Error Codes for your selected Network that have not yet had Troubleshooting Steps submitted.  Submitting a Troubleshooting Step to an unanswered Error, and having it voted up by the Community, will earn you a big boost to your reputation.

Once you are reviewing an Error Code and its associated Discussions and Troubleshooting suggestions, you will have an opportunity to post a response, or vote on the responses from other members.  You may enter `?` on a command input prompt to display a list of available commands at the level you are at.

<a name="errorupdate">

### errorupdate Command

As a Moderator, you have the priviledge to update an Error Code's analysis fields.  Doing so will achieve reputation the first time you update an error code - but subsequent updates to the same code will not award any more reputation.

To update a code, add the following parameters:

Parameter | Description
---- | ----
--code= | Specify the error code to be updated, for example `--code="5310"`
--severity= | Specify the severity:  1=Critical, 2=Important, 3=Nominal.
--meaning= | Enter the plain-English meaning of the code.  For example `--meaning="The log file format is improperly specified - perhaps an invalid character in the path."`

<a name="prometheusmerge">

### prometheusmerge Command

Prometheus Node Exporter is free and open-source metric reporting system that is widely adopted ([Windows](https://www.devopsschool.com/blog/how-to-install-windows-exporter-for-prometheus/)) ([Linux](https://prometheus.io/docs/guides/node-exporter/)).  With CodaClient, each configuration file can handle one Prometheus text file.  If you have multiple analysis jobs generating multiple Node Exporter stats, and want to not overwrite them, you should use this command in each of your analysis executions.

To specify a Prometheus Merge operation, you simply specify a file with a JSON array of file paths, where the last file path is the one that it will be merged with.

On the command line, you put:

`--prometheusmerge={path-to-config-file}`

For example:

`codaclient.linux ./oracle-scan.json az pm=./prometheus-merge.json`

or

`codaclient.linux ./oracle-scan.json az --prometheusmerge=./prometheus-merge.json`

Let's take a hypothetical situation in which you run Oracle and Cardano, report stats for both apps.  So, you create 2 CodaClient config files. The contents of the `prometheus-merge.json` file in the above example may look like this:

'''
[
  "/var/reports/prometheus/oracle_alertlog.txt",
  "/var/reports/prometheus/cardano_report.txt",
  "/var/reports/prometheus/prometheus.txt"
]
'''

In this case, the first 2 files would be Prometheus outputs from other configs - an Oracle config file that may specify 1 or more alert log files to scan, and a Cardano scan file.  When you run each of these jobs, you specify the Prometheus Merge file:

`codaclient.linux ./oracle-scan.json --analyze --prometheusmerge=./prometheus-merge.json`

`codaclient.linux ./cardano.json az pm=./prometheus-merge.json`

Then, when you run Node Exporter, specify the merged file `/var/reports/prometheus/prometheus.txt` for exporting via http.

<a name="mainconfig">

## Main Configuration File
The Main Configuration File contains configurations pertaining to executing CodaClient under a specific context.  This must be specified in JSON format as the following example:

```
{
  "network": "cardano",
  "currentVersion": "1.33.0",
  "apiserver": "https://prod.codaea.io",
  "apikey": "a5147f83b4ef4b2d9cc4faa898d0fa39795ee99ebe4a4c8884317a12bc53a632",
  "maximumSeverity": 1,
  "reportPath": "/home/stakepool/reports/coda",
  "prometheusFile": "/home/stakepool/cnode/prometheus/coda.txt",
  "uiOptions": {
    "menuType": "short",
    "textEditor": "nano"
  },
  "analysis": {
    "lastRunDate": null,
    "notification": true,
    "smtpFromEmail": "coda-donotreply@tpr.org",
    "smtpServer": "smtp.gmail.com",
    "smtpPort": 25,
    "smtpAccount": "cardano@gmail.com",
    "smtpPassword": "68cr6t%T25ti!",
    "smtpUseSSL": true,
    "sendToAddress": "notify@myorg.com"
  },
  "logging": {
    "logLevel": "Debug",
    "logPath": "codaclient.log"
  },
  "analyze": [
    {
       // analysis input specs
    }
  ]
}
```

Field | Meaning
----- | -----
network | The network or app to which all your operations will pertain.  Note that you can work with multiple networks by simply having multiple config files.
currentVersion | The version of the network/application generating the log.
lastRunDate | The date/time stamp (UTC) that Analyze was last run, leave null to initialize
apiserver | Always use https, and use prod.codaea.io for Production (Mainnet), or test.codaea.io for Test (Testnet).  Note that you will have to request an account separately on Mainnet and Testnet.
apikey | The API Key you received after registering for API access.
reportPath | The analysis reports will be output to this folder.
prometheusFile | If a file path is specified, CodaClient will write `analyze` statistics to the text file using Prometheus Node Exporter format ([Linux](https://prometheus.io/download/#node_exporter) or [Windows](https://github.com/prometheus-community/windows_exporter)).  Simply include this file in your launch parameters for Node Exporter using the `--collector.textfile.directory` parameter.
analysis | When you run an error log analysis, you can enable email notifications of Severity 1 messages in your logs.  If `notification` is set to `true`, the email settings will be used as shown above.  Emails will be sent if any Severity 1 errors are found in your logs, with details on the message(s).
logging | This object specifies how CodaClient will log its actions
logging-logLevel | One of `Off` (no logging), `Error` (Errors only), `Warning` (Errors and Warnings), or `Debug` (All messages)
logging-logPath | Logs will be saved to the file in the specified path; if left blank, logs will not be saved and only output to stdout
analyze | This is an array of Analysis inpt specifications, [see below](#inputspecs).  Each one of these will be processed in order, or if you specify the names on the command line, only those names will be processed.
uiOptions | Represents options for the CodaClient user interface.
uiOptions-menuType | Either `full` or `short` - whether to display shortened menus.
uiOptions-textEditor | Path to editor bin.


IMPORTANT NOTE:  The API Key you receive is private to you, and must not be given out.  It expires after 1 year, or whenever 
you want to generate a new one (which will invalidate any prior keys).  If you somehow lose access to your account, you can
generate a new API Key and recover it by providing the Private Key via the Account Recovery option on [The Parallel Revolution
website](https://www.theparallelrevolution.com/Coda).

IMPORTANT NOTE:  Sending emails requires CodaClient to run under root privileges.  If scheduled as a cron job, it should be
done in the root account.

<a name="inputspecs" />

### Analyze Input Specs

The Analyze section is an array of source specifications using the following format:

```
{
  "name": "node-log"
  "input": "journal",
  "inputSpecs": {
      "process": "cardano",
      "type": "error",
      "maximumSeverity": 1
  }
}
```

Type | Meaning
---- | ----
name | The analysis name you wish to give this analysis.  You can specify which analyses to run on the command line using the `az --analysis=` option.
input | This specifies the input format processor, as indicated below.
input: eventlog | Reads Windows event log per specifications
input: journal | Linux system journal (i.e. use `journalctl` to query)
input: text/csv | Text, CSV format
input: text/fixed | Text, fixed width format
input: text/other | Text, Other delimiter format
input: text/cardano | Special processor for Cardano-Node JSON text log files
input: text/regex | Regular Expression pattern matching for line-based text files
input: &lt;other> | Any other processor represents a plug-in.  Plug-in support will be coming soon.
inputSpecs | This is an object that contains the specs for the type indicated for `input`.  For example, if `input` is `journal`, then you would use the Journal Specs for `inputSpecs`.

Based on the type specified, the `inputSpecs` will use one of the following formats:

IMPORTANT NOTE:  Accessing the System Journal requires root privileges.  If scheduled as a cron job, it should be
done in the root account.

#### Windows Event Log specs

```
{
  "eventLog": "Application",
  "source": "DockerService",
  "severity": ["critical","error"]
}
```

The `eventlog` source allows you to scan entries from the Windows Event Log and check them against CodaEA.  Since the Event Log is a predefined format, you only need to specify 3 things:

Field | Description
---- | ----
eventLog | The event log to query, either `Application`, `Security`, `Setup`, or `System`.
source | The application source reporting in the Source field.
severity | Which severities to check - options are `critical`, `error`, `warning`, `information` and `verbose` - although only `critical` and `error` are typically considered worth monitoring.

#### Journal Specs

```
{
  "process": "cardano"
}
```

Field | Description
---- | ----
process | The process reporting to the Journal.  In the example for Cardano network cardano-node is the process for running the blockchain network node.

NOTE:  All error messages of severity `Emergency`, `Alert`, `Critical` and `Error` are scanned from the indicated process.



#### Text/CSV Specs

CSV, or Comma-Separated Values, is an international standard in text file specifications.  CodaClient supports quote 
delimiters.  For `text/csv` input type, the `inputSpecs` object must be of the following format:

```
{
  "inputFile": "/var/tmp/myapp/currentlog",
  "skipLines": 1,
  "columnSpec": [
    {
      "columnName": "Date"
    },
    {
      "columnName": "Source"
    },
    {
      "columnName": "Code"
    },
    {
      "columnName": "Severity"
    },
    {
      "columnName": "Message"
    }
  ],
  "messageType": {
    "columnName": "Severity",
    "values": ["Error","Exception"],
    "codeColumn": "Code",
    "messageColumn": "Message",
    "timeColumn": "Date"
  }
}
```

Field | Description
---- | ----
inputFile | The path to the log file to read
skipLines | The number of lines to skip at the head of the file, in case there are header lines.
columnSpec | An array of column specificatoin objects, one for each column in order they appear in the file.  It is not necessary to have column names on the first line.
messageType | This object determines the filter of entries to read from the log file and analyze.  Using the column name from the columnSpec array, you can specify how the log file is read.
messageType-columnName | This determines the column to use for which message types to analyze.
messageType-values | A list of values in the above column to analyze.
messageType-codeColumn | The column used to identify the error code.  This is usually a number, but could also be some kind of alphanumeric identifier.
messageType-messageColumn | The column used for the message reported from the application.
messageType-timeColumn | The column used for date/time the error was reported.

#### Text/fixed Specs

Fixed-width text files are files that align columns at various character positions in the line.  For example:

```
Date        Identifier        Type             Message
----------------------------------------------------------------------------------
02/01/2022  ACH-5545          Message          Starting up...
02/01/2022  ACH-6535          Warning          No polling period is defined, polling is disabled
```

For `text/fixed` input type, the `inputSpecs` object must be of the following format:

```
{
  "inputFile": "/var/tmp/myapp/currentlog",
  "skipLines": 2,
  "columnSpec": [
    {
      "columnName": "Date",
      "startColumn": 1,
      "length": 10,
      "trim": true
    },
    {
      "columnName": "Code",
      "startColumn": 13,
      "length": 15,
      "trim": true,
      "trimChars": "[]()"
    },
    {
      "columnName": "Severity",
      "startColumn": 32,
      "length": 15,
      "trim": true
    },
    {
      "columnName": "Message",
      "startColumn": 49,
      "length": 200,
      "trim": true
    }
  ],
  "messageType": {
    "columnName": "Severity",
    "values": ["Error"],
    "codeColumn": "Code",
    "messageColumn": "Message",
    "timeColumn": "Date"
  }
}
```

Field | Description
---- | ----
inputFile | The path to the log file to read
skipLines | The number of lines to skip at the head of the file, in case there are header lines.
columnSpec | An array of column specificatoin objects, one for each column in order they appear in the file.  It is not necessary to have column names on the first line.
columnSpec-columnName | The name you want to give to the column; all names must be unique in the list.
columnSpec-startColumn | The character position in the line where the column starts.  First character position is 1.
columnSpec-length | The number of characters to include in the data.
columnSpec-trim | Whether or not to trim leading and trailing whitespace (tabs, spaces, etc.).
columnSpec-trimChars | If `trim` is `true`, then this optional parameter specifies additional characters beyond whitespace to remove from beginning/end of the column value.
messageType | This object determines the filter of entries to read from the log file and analyze.  Using the column name from the columnSpec array, you can specify how the log file is read.
messageType-columnName | This determines the column to use for which message types to analyze.
messageType-values | A list of values in the above column to analyze.
messageType-codeColumn | The column used to identify the error code.  This is usually a number, but could also be some kind of alphanumeric identifier.
messageType-messageColumn | The column used for the message reported from the application.
messageType-timeColumn | The column used for date/time the error was reported.

#### Text/other Specs

Use this for text files that have a character (or characters) as a delimiter.  Note that lines that have more fields than you specify
will ignore the latter fields, while lines that have fewer fields will be ignored.  For example:

```
02/01/2022|ACH-5545|Message|Starting up...
02/01/2022|ACH-6535|Warning|No polling period is defined, polling is disabled
02/01/2022|AGG-5100|Error|A problem was detected.|Problem: Could not write to cache folder
----- Following is output from received message:
```

For `text/other` input type, the `inputSpecs` object must be of the following format:

```
{
  "inputFile": "/var/tmp/myapp/currentlog",
  "delimiter": "|",
  "skipLines": 2,
  "columnSpec": [
    {
      "columnName": "Date",
      "trim": true
    },
    {
      "columnName": "Code",
      "trim": true,
      "trimChars": "[]"
    },
    {
      "columnName": "Severity",
      "trim": true
    },
    {
      "columnName": "Message",
      "trim": true
    }
  ],
  "messageType": {
    "columnName": "Severity",
    "values": ["Error"],
    "codeColumn": "Code",
    "messageColumn": "Message",
    "timeColumn": "Date"
  }
}
```

For the above specs and example, lines 1 and 2 will be processed as specified, line 3 will ignore the last field, and line 4 will
be skipped.

Field | Description
---- | ----
inputFile | The path to the log file to read
delimiter | The character or string used to delimit fields in the line
skipLines | The number of lines to skip at the head of the file, in case there are header lines.
columnSpec | An array of column specificatoin objects, one for each column in order they appear in the file.  It is not necessary to have column names on the first line.
columnSpec-columnName | The name you want to give to the column; all names must be unique in the list.
columnSpec-trim | Whether or not to trim leading and trailing whitespace (tabs, spaces, etc.).
columnSpec-trimChars | If `trim` is `true`, then this optional parameter specifies additional characters beyond whitespace to remove from beginning/end of the column value.
messageType | This object determines the filter of entries to read from the log file and analyze.  Using the column name from the columnSpec array, you can specify how the log file is read.
messageType-columnName | This determines the column to use for which message types to analyze.
messageType-values | A list of values in the above column to analyze.
messageType-codeColumn | The column used to identify the error code.  This is usually a number, but could also be some kind of alphanumeric identifier.
messageType-messageColumn | The column used for the message reported from the application.
messageType-timeColumn | The date/time the log entry was reported

#### Text/cardano Specs

When running a node on the Cardano network, you can scan the logs on your Block Producer and Relay servers.  CodaClient supports text logging to a text file using the Cardano-Node text scribe (see below for instructions to configure).  The `inputSpecs` object for this type of log uses the following simplified format:

```
{
  "inputFile": "/var/tmp/myapp/currentlog",
  "messageType": {
    "values": ["Error"]
  }
}
```

Field | Description
---- | ----
inputFile | The path to the log file to read
messageType | This object determines the filter of entries to read from the log file and analyze.
messageType-values | A list of values in the above column to analyze.


IMPORTANT NOTE:  For `text/cardano` processing, you must configure your cardano-node instance to scribe logs to a file.  In order to do so, edit your `mainnet-config.json` with the following sections:

```
...
  "defaultScribes": [
    [
      "FileSK",
      "{path-to-log-file}"
    ]
  ]
...

...
  ],
  "setupScribes": [
    {
      "scFormat": "ScJson",
      "scKind": "FileSK",
      "scName": "{path-to-log-file}",
      "scRotation": null
    }
  ]
```

For example, if you wish your log file to be /home/stakepool/cnode/logs/cardano-node.log then place that in the 2 entries where it says `{path-to-log-file}`.  Also, it is *very important* to use `ScJson` as the format, and *not* `ScText`.

Restart your node after making changes to the mainnet-config.  It is recommended that you run your node as a systemd unit that autostarts on system startup, and not as a process spawned from a shell.

## Important Notes and Considerations

In order to optimize performance, the CodaEA API server caches database items for up to a minute.  If you make any changes to an existing item, and immediately re-run CodaClient, it is possible that you may retrieve the cached data and see the pre-change results.  Just wait another minute and try again.


# Other Links

[CodaClient Advanced Topics](CodaClient_Advanced.md) | [About CodaEA Accounts](Coda_Accounts.md) | [Community Rules](Community_Rules.md) | [About Subscriptions](Subscriptions.md)
