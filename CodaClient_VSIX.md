![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# CodaClient Visual Studio Extension

## About

The CodaClient Visual Studio Extension (VSIX) allows developers to look up error messages from Microsoft Visual Studio on Windows platforms directly in CodaEA.  There are 3 components to the extension:

1. The Configuration tool in the Tools menu allows you to configure your CodaEA connection, or to create an account and sign up for Visual Studio access.
1. The dockable view window gives you information and buttons to access various functionality within CodaEA (besides error code lookup)
1. The Error Lookup function is available from the right-click context menu in the Error List window, for any errors or warnings or ideas.

## Getting Started

In order to get started, you will need to configure your CodaEA.  This means you will need an account to access the API and Web servers.

To create an account, simply navigate to the [Register link on the website](https://www.codaea.io/Account/SignUp), fill out the form and answer the captcha to submit.  You will receive a confirmation email - click the verification link, and you will be emailed your confidential API key.

Now, in Visual Studio, with the Extension installed and re-launched to activate it, go to Tools / Configure CodaEA.  Select the TEST server, and paste your API key in, and click OK.

For now, we are in TESTING status, so make sure to use the TEST server.  When we go into PRODUCTION, you will have a new option available to switch to the PRODUCTION server.

# Other Links

[CodaClient Command Line Utility](CodaClient_CLI.md) | [CodaClient Advanced Topics](CodaClient_Advanced.md) | [About CodaEA Accounts](Coda_Accounts.md) | [Community Rules](Community_Rules.md) | [About Subscriptions](Subscriptions.md)
