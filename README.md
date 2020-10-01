# nunit-appium-app-browserstack

This repository demonstrates how to run Appium tests in [NUnit](https://nunit.org/) on BrowserStack App Automate.

## Setup

### Requirements

1. Visual Studio and .NET Core

    - Download Visual Studio from [here](https://visualstudio.microsoft.com/)
    - Install .NET Core while installing Visual Studio


## Getting Started

Getting Started with Appium tests in NUnit on BrowserStack devices couldn't be easier!

### **Run first test :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation to run your first test - [Get Started with your first test on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit)

### **Speed up test execution with parallel testing :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation to run parallel tests - [Get Started with Parallel testing on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit/parallelize-tests)

### **Use Local testing for apps that access resources hosted in development or testing environments :**

- Switch to `appium_dotnet_driver_4_examples`(or `appium_dotnet_driver_3_examples`) under `android` or `ios` directory

- Open the project in Visual Studio by opening `android.sln` or `ios.sln` file

- Follow the steps outlined in the documentation to run local tests - [Get Started with Local testing on App Automate](https://www.browserstack.com/docs/app-automate/appium/getting-started/c-sharp/nunit/local-testing)

**Note**: If you are running Local test on Mac, you need to download and run the BrowserStack Local binary before starting the test:

 1. Download BrowserStack Local binary for mac: [OS X (10.7 and above)](https://www.browserstack.com/browserstack-local/BrowserStackLocal-darwin-x64.zip)
 2. Switch to the directory where binary is downloaded and run using following command `./BrowserStackLocal --key <YOUR_ACCESS_KEY>`

## Integration with other CSharp frameworks

For other CSharp frameworks samples, refer to following repositories :

- [Appium CSharp tests](https://github.com/browserstack/csharp-appium-app-browserstack)

Note: For other test frameworks supported by App-Automate refer our [Developer documentation](https://www.browserstack.com/docs/)

## Troubleshooting

- In case `Unhandled Exception: System.IO.FileLoadException: Could not load file or assembly 'Newtonsoft.Json'` issue is encountered while running a Local test on Windows, please follow the these steps to resolve it :

    - Go to Tools > NuGet Package Manager > Package Manager Console

    - Run the command `update-package Newtonsoft.Json -reinstall` in the opened console

    - Then run `update-package Newtonsoft.Json` in the same console

## Getting Help

If you are running into any issues or have any queries, please check [Browserstack Support page](https://www.browserstack.com/support/app-automate) or [get in touch with us](https://www.browserstack.com/contact?ref=help).
