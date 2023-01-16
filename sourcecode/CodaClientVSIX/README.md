# CodaClientVSIX Source Code README

### Version 2023.1.1

rev 2023-01-16

## Dependencies
This project requires the following to be installed on the target system:

* Microsoft .NET 6.0 (for Linux, see [instructions from Microsoft](https://learn.microsoft.com/en-us/dotnet/core/install/linux))
* CodaRESTClient library available from this project, at least 2023.1.1 or later


## Code Components

### CodaToolWindow Class

This class is the code that instantiates the Tool Window (the dockable window availabe in the View / Other Windows menu).

### CodaToolWindowCommand Class

This is the menu pick that is added to the View / Other Windows menu.

### CodaToolWindowControl Dialog

This XAML file defines the graphical aspects of the Tool Window.

### CodaOptionsForm Form

This form retrieves user options from the CodaEA API server, and presents a GUI dialog that allows the user to change them, then saves them back to the API server.

### CodaConfigCommand Class

This is the code that is executed by the Tools / Configure CodaEA command, that instantiates the CodaClientConfigForm.

### CodaClientConfigForm Form

This is the dialog presented that allows users to enter configuration for CodaClient.  Configuration is saved in the User Registry.

### CodaAnalyzeCommand Class

This is the code that is attached to the Error List window right-click contextual menu, and allows users to look up the message on CodaEA.

### CodaClientVSIXPackage.vsct

This XML defines the menus and commands deployed in the package.

