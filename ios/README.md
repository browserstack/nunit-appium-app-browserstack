## Setup
* Clone the repo
* Open the solution `ios.sln` in Visual Studio
* Build the solution
* Update `App.config` file with your BrowserStack Username and Access Key(https://www.browserstack.com/accounts/settings)

## Running your tests from Test Explorer via NUnit Test Adapter
- Upload your Native App (.ipa file) to BrowserStack servers using upload API:

  ```
  curl -u "username:accesskey" -X POST "https://api.browserstack.com/app-automate/upload" -F "file=@/path/to/app/file/Application-debug.ipa"
  ```

- If you do not have an .ipa file and looking to simply try App Automate, [you can download our sample app and upload](https://www.browserstack.com/app-automate/sample-apps/ios/BStackSampleApp.ipa)
to the BrowserStack servers using the above API.
- For running local tests, you can use our [local sample app](https://www.browserstack.com/app-automate/sample-apps/ios/LocalSample.ipa).
- Update the desired capability "app" with the App URL returned from the above API call
- To run a single test, run test with fixture `single`
- To run local tests, run test with fixture `local`
- To run parallel tests, run tests with fixture `parallel`

## Notes
* You can view your test results on the [BrowserStack automate dashboard](https://www.browserstack.com/app-automate)
* You can export the environment variables for the Username and Access Key of your BrowserStack account

  ```
  export BROWSERSTACK_USERNAME=<browserstack-username> &&
  export BROWSERSTACK_ACCESS_KEY=<browserstack-access-key>
  ```

## Additional Resources
* [Getting Started with App Automate](https://www.browserstack.com/app-automate/appium-nunit)
