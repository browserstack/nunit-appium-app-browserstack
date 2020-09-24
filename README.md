# nunit-appium-app-browserstack

 - This repository demonstrates how to run Appium tests in [NUnit](https://nunit.org/) on BrowserStack App Automate.
 - It contains four Visual Studio projects in all - two for Android and two for iOS.
 - The two projects for Android contain tests for Appium Dotnet Driver 3 and Appium Dotnet Driver 4. Same for iOS.
 - Each project contains a first test that you can run to get started.

![BrowserStack Logo](https://d98b8t1nnulk5.cloudfront.net/production/images/layout/logo-header.png?1469004780)

## Setup

### Requirements

1. Visual Studio and .NET Core

    - Download Visual Studio from [here](https://visualstudio.microsoft.com/)
    - Install .NET Core while installing Visual Studio

## Getting Started

- Getting Started with Appium tests in NUnit on BrowserStack couldn't be easier!

    - These steps below are for Android with Appium DotNet Driver version 4, but similar steps apply for any Appium Driver version and any OS in the repo.
    - Open the Android Appium Dotnet Driver [Project file](https://github.com/gshah30/nunit-appium-app-browserstack/blob/master/android/appium_dotnet_driver_4_examples/android.csproj) in Visual Studio
    - Install all the Nuget packages in Visual Studio at the versions present in [packages config](https://github.com/gshah30/nunit-appium-app-browserstack/blob/master/android/appium_dotnet_driver_4_examples/packages.config). This can be done from   `Projects > Manage NuGet Packages... > Browse tab > Select version` from the botton right and click 'Add Package'
    - Add your Browserstack `username` and `access key` in the [`appSettings` tag in App.config](https://github.com/gshah30/nunit-appium-app-browserstack/blob/master/android/appium_dotnet_driver_4_examples/App.config)
    - Add the url of the app uploaded to Browserstack in the [`first`, `local` and `parallel` tags in App.config](https://github.com/gshah30/nunit-appium-app-browserstack/blob/master/android/appium_dotnet_driver_4_examples/App.config)
    - Build the project using `Build > Build Android`
    - If you are running Local test on Mac or GNU/Linux, you will also have to run the local binary like this `./BrowserStackLocal --key <your access key>`
    - Update `App.config` file with your BrowserStack Username and Access Key(https://www.browserstack.com/accounts/settings)
    - You can optionally export the environment variables BROWSERSTACK_USERNAME with value as username and BROWSERSTACK_ACCESS_KEY with value as access key of your BrowserStack account
    - To run the tests click `View > Test` to open Unit Tests panel and run any/all tests from there
    - You can view your test results on the [BrowserStack automate dashboard](https://www.browserstack.com/app-automate)

## Getting Help

If you are running into any issues or have any queries, please check [Browserstack Support page](https://www.browserstack.com/support/app-automate) or [get in touch with us](https://www.browserstack.com/contact?ref=help).
