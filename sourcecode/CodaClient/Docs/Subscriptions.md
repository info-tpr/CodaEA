![Coda Logo](https://github.com/info-tpr/CodaEA/blob/main/images/CodaLogo-Imageonly-transparent.png?raw=true)

# About Subscriptions in CodaEA

CodaEA offers subscription features to help Server Operators keep on top of things as they develop.  These features consist of:

- Automatic Subscriptions: actions you take will automatically subscribe you to notifications for specific error messages.  These include:
    - Reporting an Error:  If your CodaClient Analyze function finds an error in your logs, you are subscribed to notifications for that error.  If the community posts on that Error, you will be notified.
    - Posting on an Error:  If you post a Discussion, Comment, or Troubleshooting Solution, it is assumed that you are interested in that Error, and thus are automatically subscribed to it.
    - Voting on a post:  Participating in the community with respect to an Error will subscribe you to that Error.
- Managing Subscriptions:  At any time, you may subscribe to a given error using the CodaClient or the API endpoint `/api/errors/{network}/{errorId}/subscribe` or `/unsubscribe`. Performing a full retrieval of your Account or an Account you manage will also retrieve a list of its Subscriptions.  You can unsubcribe or subscribe using the endpoints or CodaClient functions.

## Notifications

Notifications are emails sent according to your email preferences. Notifications occur first by you subscribing to an item in CodaEA (currently only Error Log entries support subscriptions).  Next, an event occurs, and based on your notification settings for your Account, that may be included in a notification email.

All notifications are collected, and sent to you periodically based on your schedule preferences.


# Other Links

[Community Rules](Community_Rules.md) | [CodaClient Command Line Interface Specifications](CodaClient_CLI.md) | [About CodaEA Accounts](Coda_Accounts.md) 
