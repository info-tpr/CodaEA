![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# CodaClient Command Line Utility - Advanced Topics

## Email Customization

Notification emails sent by CodaClient using your SMTP account are built using files in the `/templates/` folder located in your client deployment.  These emails are built using the following logic:

1. The email message is built up as a table using the tablehead, tablesectionhead, tablerow, and tablefoot files (Mail-notification-{portion}.txt).
1. The message is embedded in the body of an HTML-formatted mail file, simply called Mail-body.txt.
1. Data fields are identified with square brackets [ and ], and are hard-coded.

You can customize the stylesheets, layout, and HTML around those fields as desired, to customize the look and feel of the emails you get from your CodaClient.

## Custom Plugins

Coming soon, we will support 2 types of [Plugins](Creating%20A%20Plugin.md) for processing error logs.  We will release a .NET 6 Class code that you can use to create a Class Library project.  You will subclass the CodaEA Class from our repository, and add the methods as required.

Plugin Type | Description
---- | ----
Line Processor | Used for line-by-line text file processing, it will be passed a line of text and the configuration section from the config file, and return a single error log report entry.
File Processor | Used to process a whole file, it will be passed the file path and configuration section from the config file, and will have methods to return the next available report entry from the file.  It doesn't have to parse the whole file at once, it can do it one log entry at a time.

Public Plugins will be available for installation right within the CodaClient CLI.  You will be able to search, browse, select and install the plugin, and it will be available for Analyze jobs using the JSON configuration you specify for the `inputSpecs` object in the config file.

To be clear, standard text processing (delimited, CSV, fixed-width, and Regular Expression pattern matching) are supported out-of-the-box, as well as special formats like Cardano or Stellar logs.  You will only need Plugins for formats that cannot easily be read by those processors, such as binary, structured (e.g. Json/XML), or complex formats.

### Plugin Submission

In order to make your Plugin available to the CodaEA community in the CLI for browsing and download, there will be a submission process.  You will have to have a Git repository with your source code, and an API call (and corresponding CodaClient CLI function) will allow you to submit your processor for code review.  Our team of programmers will review the code, and if approved, will make it available for installation in the wider community via the CodaClient CLI.

We will be compiling the source code, and storing the binaries on our server.  We do this manual review process to ensure the security of running Plugins.  We need to make sure that there is no malicious code, and that standard and approved libraries are used.

If you use proprietary libraries or code, or you do not wish your Plugin to be publicly available, we will have instructions for configuring CodaClient for your custom Plugin.  The above process is only if you wish to make your Plugin available to the broader CodaEA community.

We would add that, if your submission is approved, a large Reputation bonus will be added for each approved submission, as well as a Badge for the first submission on each Network.

# Other Links

[CodaClient CLI](CodaClient_CLI.md) | [About CodaEA Accounts](Coda_Accounts.md) | [Community Rules](Community_Rules.md) | [CodaClient Visual Studio Extension](CodaClient_VSIX.md) | [About Subscriptions](Subscriptions.md)
