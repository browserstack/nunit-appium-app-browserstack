# nunit-appium-app-browserstack

This repository demonstrates how to run Appium tests in [NUnit](https://nunit.org/) on BrowserStack App Automate.

## Setup

### Requirements

1. Visual Studio and .NET Core

    - Download Visual Studio from [here](https://visualstudio.microsoft.com/)
    - Install .NET Core while installing Visual Studio


## Getting Started

Getting Started with Appium tests in NUnit on BrowserStack devices couldn't be easier!

### ** Run first test :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation to run your first test - [Get Started with your first test on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit)

### **Speed up test execution with parallel testing :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation - [Get Started with Parallel testing on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit/parallelize-tests)

### **Use Local testing for apps that access resources hosted in development or testing environments :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation - [Get Started with Local testing on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit/local-testing)

**Note**: If you are running Local test on Mac, you need to download the   steps:

 1. Download BrowserStack Local binary for mac: [OS X (10.7 and above)](https://www.browserstack.com/browserstack-local/BrowserStackLocal-darwin-x64.zip)
 2. Run the binary using `./BrowserStackLocal --key <YOUR_ACCESS_KEY>` before the starting the test

## Integration with other CSharp frameworks

For other CSharp frameworks samples, refer to following repositories :

- [C# tests](https://github.com/browserstack/csharp-appium-app-browserstack)

Note: For other test frameworks supported by App-Automate refer our [Developer documentation](https://www.browserstack.com/docs/)

## Troubleshooting

- In case `Unhandled Exception: System.IO.FileLoadException: Could not load file or assembly 'Newtonsoft.Json'` issue is encountered while running Android or iOS Local test on Windows, please follow the following steps to resolve it :

    - Go to Tools > NuGet Package Manager > Package Manager Console

    - Run the command `update-package Newtonsoft.Json -reinstall` in the opened console

    - Then run `update-package Newtonsoft.Json` in the same console

## Getting Help

If you are running into any issues or have any queries, please check [Browserstack Support page](https://www.browserstack.com/support/app-automate) or [get in touch with us](https://www.browserstack.com/contact?ref=help).
