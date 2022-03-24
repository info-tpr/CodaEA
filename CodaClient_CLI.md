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
use by an Operator Account.

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
`    codaclient.linux ./cardano-config.json jq --query=./cardano-journal-query.json`

    <os>:  Supported Operating Systems are Linux (Ubuntu 20.04 LTS)

    <path-to-config-file>:  See the section on [Main Configuration File](#mainconfig) for file schema.

    <command>:  One of the following commands.  Each command will have a section below that will detail options.  Note that
                each command has an alias you can use as a shorthand form.

Command | Alias | Purpose
------- | ----- | -----
[accountquery](#accountquery) | aq | Allows you to query or update your Account on CodaEA.  IMPORTANT NOTE:  By registering, you will receive both a Private Key, and an API Key.  These keys are used to operate and recover your Account, and must be kept safe.  Retrieval of Account data will obfuscate these keys.
[analyze](#analyze) | az | This command will query a log, and submit the entries against the CodaEA database, and produce a report.  The report is in XML format, with XSLT stylesheet to make it viewable and interactive on a web browser.
[errorquery](#errorquery) | eq | Use this to query a specific error message, retrieve its community input, and post to it.  Please see [Community Rules](Community_Rules.md) for the rules and rewards for participation.
[errorupdate](#errorupdate) | eu | Once you have earned the Moderator badge, you can post updates to an Error Message.  The updates you can post are Accepted Meaning, and Accepted Severity.


<a name="accountquery">

### accountquery Command

An Account is used to offer access to CodaEA.  In order to access  the functions and data of CodaEA, you must have an Account,
and that Account must be active.  See [About Accounts](Coda_Accounts.md) for more information.

Using `accountquery` you can retrieve or update your account info.  Specify what it is you want it to do using options following
the `accountquery` command.

#### Options

Option | Description
---- | ----
--query | Use this to retrieve your Account info.  You can add another option `--full` to retrieve all associated data.
--update | Use this to update your info.  Then you can specify which fields to update:  `name="new name"`, `email=new@email`, `wallet=new-wallet-address`


<a name="analyze">

### analyze Command

Analyze causes CodaClient to check logs in specific formats.  If your logs are in a format that is not supported, you may
either request support on our [Github project page](https://github.com/info-tpr/CodaEA), or investigate whether you can
configure the application to output in one of the supported formats, or write some code that will transform your application 
logs to that format.

To specify an Analysis, use the option:  `--analysis-file=<path-to-analysis-configuration>`.  This file must be JSON format,
and follow these guidelines:

```
{
  "input": "journal",
  "inputSpecs": {
      "process": "cardano-node",
      "type": "error",
      "maximumSeverity": 1
  }
}
```

The Analysis Config file specifies the input type, one of the following:

Type | Meaning
---- | ----
journal | Linux system journal (i.e. use `journalctl` to query)
text/csv | Text, CSV format
text/fixed | Text, fixed width format
text/other | Text, Other delimiter format

Based on the type specified, the `inputSpecs` will use one of the following formats:

IMPORTANT NOTE:  Accessing the System Journal requires root privileges.  If scheduled as a cron job, it should be
done in the root account.

#### Journal Specs

```
{
  "process": "cardano-node",
  "type": "error"
}
```

Field | Description
---- | ----
process | The process reporting to the Journal.  In the example for Cardano network cardano-node is the process for running the blockchain network node.
type | The type of message to query from the Journal; this could be "error", "warning" or "debug".

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
    "messageColumn": "Message"
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
      "trim": true
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
    "messageColumn": "Message"
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
messageType | This object determines the filter of entries to read from the log file and analyze.  Using the column name from the columnSpec array, you can specify how the log file is read.
messageType-columnName | This determines the column to use for which message types to analyze.
messageType-values | A list of values in the above column to analyze.
messageType-codeColumn | The column used to identify the error code.  This is usually a number, but could also be some kind of alphanumeric identifier.
messageType-messageColumn | The column used for the message reported from the application.

#### Text/other Specs

Use this for text files that have a character (or characters) as a delimiter.  Note that lines that have more fields than you specify
will ignore the latter fields, while lines that have fewer fields will be ignored.  For example:

```
02/01/2022|ACH-5545|Message|Starting up...
02/01/2022|ACH-6535|Warning|No polling period is defined, polling is disabled
02/01/2022|AGG-5100|Error|A problem was detected.|Problem: Could not write to cache folder
----- Following is output from received message:
```

For `text/fixed` input type, the `inputSpecs` object must be of the following format:

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
      "trim": true
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
    "messageColumn": "Message"
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
messageType | This object determines the filter of entries to read from the log file and analyze.  Using the column name from the columnSpec array, you can specify how the log file is read.
messageType-columnName | This determines the column to use for which message types to analyze.
messageType-values | A list of values in the above column to analyze.
messageType-codeColumn | The column used to identify the error code.  This is usually a number, but could also be some kind of alphanumeric identifier.
messageType-messageColumn | The column used for the message reported from the application.

<a name="errorquery">

### errorquery Command

The Error Query command allows you to browse specific error messages, or list messages needing attention from the community.  It
is also the main entrypoint to interacting with the errors - you can see discussions and troubleshooting steps, vote on how useful
you found them, and post your own responses.  Don't forget, every vote and post gives you a boost to your Reputation score!



<a name="errorupdate">

### errorupdate Command


<a name="mainconfig">

## Main Configuration File
The Main Configuration File contains configurations pertaining to executing CodaClient under a specific context.  This must be
specified in JSON format as the following example:

```
{
  "network": "cardano",
  "apiserver": "https://prod.codaea.io",
  "apikey": "a5147f83b4ef4b2d9cc4faa898d0fa39795ee99ebe4a4c8884317a12bc53a632",
  "maximumSeverity": 1,
  "reportPath": "/home/stakepool/reports/coda",
  "analysis": {
    "notification": true,
    "smtpServer": "smtp.gmail.com",
    "smtpPort": 25,
    "smtpAccount": "cardano@gmail.com",
    "smtpPassword": "68cr6t%T25ti!",
    "smtpUseSSL": true,
    "sendToAddress": "notify@myorg.com"
  }
}
```

Field | Meaning
----- | -----
network | The network or app to which all your operations will pertain.  Note that you can work with multiple networks by simply having multiple config files.
apiserver | Always use https, and use prod.codaea.io for Production (Mainnet), or test.codaea.io for Test (Testnet).  Note that you will have to request an account separately on Mainnet and Testnet.
apikey | The API Key you received after registering for API access.
analysis | When you run an error log analysis, you can enable email notifications of Severity 1 messages in your logs.  If `notification` is set to `true`, the email settings will be used as shown above.  Emails will be sent if any Severity 1 errors are found in your logs, with details on the message(s).
maximumSeverity | Each Error Code when analyzed by the CodaEA community is assigned a severity of 1 (critical), 2 (important) or 3 (nominal).  Whatever you set here, the analysis will be cached, and subsequent analyses can skip messages deemed unimportant by you and the other members of the community.  Only error codes with an unassigned severity, or severity of this setting and below, will be analyzed.
reportPath | The analysis reports will be output to this folder.

IMPORTANT NOTE:  The API Key you receive is private to you, and must not be given out.  It expires after 1 year, or whenever 
you want to generate a new one (which will invalidate any prior keys).  If you somehow lose access to your account, you can
generate a new API Key and recover it by providing the Private Key via the Account Recovery option on [The Parallel Revolution
website](https://www.theparallelrevolution.com/Coda).

IMPORTANT NOTE:  Sending emails requires CodaClient to run under root privileges.  If scheduled as a cron job, it should be
done in the root account.
