# CodaEA Release Notes

Current Version:  2022.1(beta)

## About this Document

The purpose of this document is to capture features and changes for the CodaEA Ecosystem.  It is updated at each release.

# CodaEA Features

## CodaClient Command Line Utility

Feature | Command | Introduced | Description
---- | ---- | ---- | ----
Text CSV Parsing | Analyze | 2022.1(beta) | Parses single-line CSV files, support for quote qualifiers; supports post-processing operations like trim whitespace and specific characters
Text Delimiter Parsing | Analyze | 2022.1(beta) | Parses single-line files with character delimiters; supports post-processing operations like trim whitespace and specific characters
Text Fixed-Width Parsing | Analyze | 2022.1(beta) | Parses single-line files with columns in specific character positions; supports post-processing operations like trim whitespace and specific characters
Analyze Email Notification | Analyze | 2022.1(beta) | Sends email notices after Analyze if errors were found at Unanalyzed (Severity 0) or Critical (Severity 1) severities
Node Exporter stats | Analyze | 2022.1(beta) | Creates Prometheus Node Exporter file for analysis runs reporting number of errors found in each Severity
Analysis Report | Analyze | 2022.1(beta) | Analysis report in JSON format
Account Query | Account Query | 2022.1(beta) | Basic Account Query options - query your own, or other Accounts by Account ID.  You can also list all accounts owned by your Organization if you are an Organizational/Developer Account.  Also, Options are available from this menu.
Org Account Admin | Account Query | 2022.1(beta) | Basic Organizational Account administration functions:  Create account, Generate API key, Update account, Make Admin/Remove Admin badge.  Works for "Organization" and "Developer" type of Accounts.
Plugin Support | N/A | 2022.1(beta) | Support for Visual Studio 2022 .NET 6 Plugins for Line and File processors
SQL Server Plugin | N/A | 2022.1(beta) | SQL Server table / stored procedure query plugin for SQL logs
Error Query | Error Query | 2022.1(beta) | CodaClient allows querying of a specific error code, or all errors that are Unanalized or Unresolved.  Functions include: Editing (Error Editor badge), Discussion reading & posting, Troubleshooting reading & posting & commenting, Voting & Reporting.
Error Update | Error Update | 2022.1(beta) | CodaClient allows you to update an error from the command line, setting either Meaning or Severity or both if your Account has EE badge.

## CodaRESTClient Client Library

Feature | Introduced | Description
---- | ---- | ----
Full Endpoint Support | 2022.1(beta) | All REST endpoints have corresponding functions
URL Encoding | 2022.1(beta) | All string route values are URL encoded to eliminate value-related calling issues
Simple Request & Response Wrappers | 2022.1(beta) | New REST call functions can be done in just a few lines because of helper functions that handle the complexity of setting up calls and responses
Robust Error Handling | 2022.1(beta) | Handle all errors from REST server and return predictably-formatted responses

## CodaRESTServer API Server

Feature | Area | Introduced | Description
---- | ---- | ---- | ----
Endpoint Documentation | Docs | 2022.1(beta) | All endpoints are documented via OpenAPI 3.0 specs.  To view documentation, simply navigate browser to the root of the API server URL (e.g. https://test.codaea.io)
Error Reporting | Errors | 2022.1(beta) | Ability to check error information, report error occurrences, and retrieve related discussion posts.
Error Metrics | Errors | 2022.1(beta) | Metrics captured on error occurrences.
Reputation Points | Reputation | 2022.1(beta) | All badges and reputation points assigned automatically through posting and participation as indicated in Badge prerequisites.
Subscriptions | Subscriptions | 2022.1(beta) | Errors reported by CodaClient are subscribed to for updates.  Votes and other activities on your Posts are also subscribed to.
Database Caching | Database | 2022.1(beta) | Implemented enterprise-level database record caching to reduce load on Database Server.

## Job Processor Server

Feature | Area | Introduced | Description
---- | ---- | ---- | ----
Account Validation Emails | Emails | 2022.1(beta) | System sends Account Validation emails with links to enable new Accounts
Account Expiration Emails | Emails | 2022.1(beta) | Sends email notices on expiring Accounts.
Subscription Emails | Emails | 2022.1(beta) | System sends consolidated Subscription emails with all activity since last Subscription email in a table
Purge Old Data | Cleanup | 2022.1(beta) | Old emails, abandoned Accounts, and such are removed after a reasonable period of time.

# Change Log

## 2022.2(beta2)

This section will be coming soon.
