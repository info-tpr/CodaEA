![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# About Accounts in CodaEA

An Account allows you to access CodaEA.  You can do so directly via the REST API, or you can do so via front-end utilities written to utilize the API - like the CodaEA client utility CodaClient.

There are 3 types of Accounts.  Each of these has a private key (used for account recovery), and an API Key (used to access CodaEA).  The API Key is required for all CodaEA access.  Each Account is associated with an email address.

Account Type | Description
---- | ----
Standard | A Standard account, which grants access to be able to report Errors for any Network, and participate in communities via Discussions & Troubleshooting solutions.
Organization | An Organization account belongs to a Company or other Organization.  Some of those Organization accounts are designated as Organization Administrators (or OA's), who can create, deactivate, and manage accounts owned by that Organization.  Like a Standard account, these otherwise have the same privileges and capabilities.
Developer | Like an Organization account, a Developer represents a Developer Organization who develops one (or more) of the Network software applications reported on CodaEA.  As a Developer, they can manage the error codes - providing analysis, and also have access to the Metrics Reporting for their Network(s), as well as normal participation in error discussions and troubleshooting.


# Other Links

[CodaClient Command Line Interface Specifications](CodaClient_CLI.md) | [Community Rules](Community_Rules.md) | [About Subscriptions](Subscriptions.md)
