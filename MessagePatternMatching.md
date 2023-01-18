# About Message Pattern Matching

CodaEA allows you to assign message severity (e.g. Error, Info, Warning, etc.), and a unique code to message patterns in a given Network (or application).  To accomplish this, we define a series of message pattern rules.  The rules are processed in sequence, such that the first rule that matches to a line is used, and the rest are not used for that line.

This is extremely useful for many purposes.  First of all, a uniquely idenfied code can help users identify solutions, and CodaEA can coordinate all activities around it.  Second, that unique code and severity can also be used in our Log Aggregator to standardize log format and analysis of the logs.

This document specifies how the Rules Engine is used, and how to manage rules effectively.

# Rules Lifecycle
![Rules Lifecycle](https://raw.githubusercontent.com/info-tpr/CodaEA/main/images/PatternRuleLifecycle.jpg)

Each Message Pattern Rule has a lifecycle it goes through.  When it is first created, it starts its life in Development mode.

While in Development mode, it can only be accessed by the Account that created it.  If you wish to allow others to use your rules, promote it to Testing, where others can use it for testing purposes.

A rule that is in Published state cannot be changed or reordered, it can only be demoted.  Obsolete state is a holding pattern where rules can be decommissioned, and perhaps restored later if needed.  The only exception is that if a new rule is Published in between the existing Published rule list, the following rules will be reordered to occur after the new rule.

## Rule Creation

You create, promote, demote, test, vote, report, and manage rules using the [Rule Editor on the website](https://www.codaea.io/CodaEA/RuleEditor).  The testing process consists of:

1. Creating or editing the Rules list
1. Uploading a log file
1. Selecting your mode and test option
1. Run the test
1. Make adjustments as needed - repeat from 1
1. Promote new rule

If you find an issue with a current Published rule, you can vote it down and enter a comment as to why.  This will flag Moderators to evaluate its effectiveness.  If a Moderator deems necessary, one can demote it to either Testing, Development, or Obsolete.

IMPORTANT NOTE:  Once a rule is demoted to Development, it becomes private again to the account that created it.  If that account is no longer active, then that demotion will not be allowed.  If an account goes inactive, its Development rules are no longer accessible by anyone.


## Rules Promotion

When you are satisfied with a rule you created, you should promote it to Testing.  This allows others to test your rules.  A notification will be sent out to all members of the Network community that a new rule has been proposed.

Similarly, if the rule is in Testing state and you are satisfied, you should promote it to Published.  In this case, it will receive a Vote Up.  However, if you feel that the rule was detrimental, or did not work for you, you should vote it down.

Once a rule receives a Votes rating of 5 (at least 5 people vote it up, more if some voted it down), then it will be promoted to Published, and become part of the active Rule Set.

Alternatively, you can request that a community moderator can promote it to Published, if for example there are not enough votes, but it definitely works.

If there are problems with a Published rule, you can report it to the Moderators.  It can be investigated, and either demoted or obsoleted as necessary.

# Rules Processing

Each rule has a Sort Order.  This is the order in which it will be processed, in numeric order.  Because rule processing is ordered, you must put more specific rules first, and less specific rules after.  This guarantees that lines that contain the same elements will fall through to the right rule.

![Rules Lifecycle](https://raw.githubusercontent.com/info-tpr/CodaEA/main/images/PatternRuleRunning.jpg)


When a log file is processed, it can be done so in one of 3 modes corresponding to the Life Cycle States:  Development, Testing, or Published.  When CodaClient scans logs, it will run in Published mode, meaning only Published rules will apply.

For the other 2 modes, you have a parsing option of rules from which other modes to include.

# Rules Testing

![Rule Processing Order](https://raw.githubusercontent.com/info-tpr/CodaEA/main/images/PatternRuleOrder.jpg)

Rule Testing is done with the [Rule Editor on the website](https://www.codaea.io/CodaEA/RuleEditor).

When testing Message Parsing Rules, as mentioned above, rules are processed in order.  If the rules you are testing are in a different mode from Published, they will have their own sort order - but will be inserted into a combined list if you use the ParseAllButTesting or ParseAllModes options.  The "lower" the lifecycle state, the sooner the Rule at the same index will be processed - meaning, if you have 8 rules in Published sorted from 1 to 8, and you add a rule in Testing sorted at 5, it will take place after Rule 4 in Published, and before Rule 5 in Published.  Similarly, another Rule set to 5 in Development will take place after Rule 4 in Published, and before Rule 5 in Testing.

This ensures you can use more mature rules in a rule set when testing new rules, and the ultimate effect it will make in the stack of rules when promoted.

The CodaEA Message Parsing Rule Engine is available via the `Coda.FunctionLibrary` library, which is the same engine that is used for CodaClient and the Rule Editor for testing purposes.

# Coda.FunctionLibrary.MessagePatterns

If you wish to incorporate the CodaEA Rule Engine into your own code, simply retrieve rules using the `GET /api/patterns/{network}` and instantiate the MessagePatterns class, add the rules, and parse away.

Contact [The Parallel Revolution](https://www.theparallelrevolution.com) if you need help, or have any feedback or ideas.

# Other Links

[CodaClient Advanced Topics](CodaClient_Advanced.md) | [About CodaEA Accounts](Coda_Accounts.md) | [Community Rules](Community_Rules.md) | [CodaClient Visual Studio Extension](CodaClient_VSIX.md) | [About Subscriptions](Subscriptions.md) | [About Message Pattern Matching](MessagePatternMatching.md)
