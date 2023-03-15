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

- Go to `NUnit-Appium-App-BrowserStack` under `android` or `ios` directory

- Open the project in Visual Studio by opening `NUnit-Appium-App-BrowserStack.sln` file

- If you have uploaded your app then add the app id to the `browserstack.yml` config file, or you can directly specify the path to your app in the `browserstack.yml` file.

- Run `dotnet test --filter "Category=sample-test"`

- You can access the test execution results, and debugging information such as video recording, network logs on [App Automate dashboard](https://app-automate.browserstack.com/dashboard)

### **Use Local testing for apps that access resources hosted in development or testing environments :**

- Go to `NUnit-Appium-App-BrowserStack` under `android` or `ios` directory

- Open the project in Visual Studio by opening `NUnit-Appium-App-BrowserStack.sln` file

- Ensure that `browserstackLocal` capability is set to `true` in the `browserstack.yml` file

- If you have uploaded your app then add the app id to the `browserstack.yml` config file, or you can directly specify the path to your app in the `browserstack.yml` file.

- Run `dotnet test --filter "Category=sample-local-test"`

- You can access the test execution results, and debugging information such as video recording, network logs on [App Automate dashboard](https://app-automate.browserstack.com/dashboard)

### **Integrate your test suite**
This repository uses the BrowserStack SDK to run tests on BrowserStack. Follow the steps below to install the SDK in your test suite and run tests on BrowserStack:

- Create sample browserstack.yml file with the browserstack related capabilities with your BrowserStack Username and Access Key and place it in your root folder.
- Add nuget library BrowserStack.TestAdapter
```sh
dotnet add BrowserStack.TestAdapter
```
- Build project dotnet build

--

## Integration with other CSharp frameworks

For other CSharp frameworks samples, refer to following repositories :

- [Appium CSharp tests](https://github.com/browserstack/csharp-appium-app-browserstack)

Note: For other test frameworks supported by App-Automate refer our [Developer documentation](https://www.browserstack.com/docs/)

--

## Troubleshooting

- In case `Unhandled Exception: System.IO.FileLoadException: Could not load file or assembly 'Newtonsoft.Json'` issue is encountered while running a Local test on Windows, please follow the these steps to resolve it :

    - Go to Tools > NuGet Package Manager > Package Manager Console

    - Run the command `update-package Newtonsoft.Json -reinstall` in the opened console

    - Then run `update-package Newtonsoft.Json` in the same console

--

## Getting Help

If you are running into any issues or have any queries, please check [Browserstack Support page](https://www.browserstack.com/support/app-automate) or [get in touch with us](https://www.browserstack.com/contact?ref=help).
