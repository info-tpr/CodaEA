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

![CodaEA Config](https://github.com/info-tpr/CodaEA/blob/main/images/VSIX_Config1.png?raw=true)

Now, in Visual Studio, with the Extension installed and re-launched to activate it, go to Tools / Configure CodaEA.  Select the TEST server, and paste your API key in, and click OK.

![CodaEA Config](https://github.com/info-tpr/CodaEA/blob/main/images/VSIX_Config2.png?raw=true)

For now, we are in TESTING status, so make sure to use the TEST server.  When we go into PRODUCTION, you will have a new option available to switch to the PRODUCTION server.

## Features of VSIX Extension

Feature | Functions
---- | ----
CodaEA Tool Window | Allows you to control your experience of CodaEA through Options, view/manage your account, and obtain help on VSIX extension <br/> Available via View / Other Windows.
CodaEA Configuration Dialog | Allows you to configure your VSIX client to connect with CodaEA, as well as link to create an account. <br/> Available via Tools menu.
CodaEA Lookup Command | Allows you to look up all available information on an error code from the Error List window. <br/> Available on Right-Click on an error/warning/idea.

## How does it work?

When you right-click and look up an error, if the code doesn't exist in our Knowledge Base, it is added.  As an early adopter, we are looking for people to help build the database and the process.  True, you can use our easy one-click links at the bottom to reference other sites for solutions, but the true power will come in when you post a solution in CodaEA.  So please post a Troubleshooting Solution, if you find it, or if you come across it at another site - copy and paste, and include a reference link to the original source.  That way, we will be keeping attribution for the original source, and will be building a database of solutions to allow people to find it instantly.

The more you contribute, the better it will be for you, and for all.  Thank you for your contributions to help your fellow developers!

## Wait, is this free?

For now, we have at least a 6 month free trial access.  This means, for at least 6 months, it will be free.  Once we move into PRODUCTION status, on or after your 6 month anniversary, we plan to charge a small fee to help pay for the cost of development and operations.  Here are our access plans for Visual Studio:

Plan | Details
---- | ----
Basic Access | Unlimited access, for free, for 6 months.
Monthly | If you pay month-to-month, USD $2 per month
Annually | If you pay year-to-year, USD $20 per year (save $4 over monthly)
Long-Term | If you pay 6 years at a time, USD $100 per 6 years (save $20 over annually)

All plans are payable via card for now, Crypto coming up.

# Can I earn money for this?

Coming next year, we will be issuing a Cryptocurrency native token on the Cardano network, and will be integrating a money system whereby the more reputation you earn, the more coin you earn.  And we will also be posting money in a liquidity pool so you can trade CodaCoins for real currencies.  You will need a Cardano wallet to participate, and earn free NFTs!  Stay tuned!

# What Else Can I Do?

If you have any feedback or ideas, please let us know!  Contact us via our [Discord server](https://discord.gg/ecaz5C4mCv), or via [email](mailto:info@theparallelrevolution.com).

# Other Links

[CodaClient Command Line Utility](CodaClient_CLI.md) | [CodaClient Advanced Topics](CodaClient_Advanced.md) | [About CodaEA Accounts](Coda_Accounts.md) | [Community Rules](Community_Rules.md) | [About Subscriptions](Subscriptions.md)
