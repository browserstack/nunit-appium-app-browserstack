using NUnit.Framework;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

namespace BrowserStack
{
    public class BrowserStackNUnitTest : IDisposable
    {
        protected ThreadLocal<AppiumDriver> _threadLocal = new ThreadLocal<AppiumDriver>();
        protected AppiumDriver driver
        {
            set => _threadLocal.Value = value;
            get => _threadLocal.Value;
        }

        public BrowserStackNUnitTest() { }

        [SetUp]
        public void Init()
        {
            try
            {
                var appiumOptions = new AppiumOptions();
                
                // Set required capabilities
                appiumOptions.DeviceName = "Samsung Galaxy S20";  // Use the DeviceName property
                appiumOptions.PlatformName = "Android";           // Use the PlatformName property
				appiumOptions.PlatformVersion = "10";
                
                // Initialize the driver
                driver = new AndroidDriver(
                    new Uri("http://hub-cloud.browserstack.com/wd/hub/"), 
                    appiumOptions, 
                    TimeSpan.FromSeconds(180)
                );

				if (driver == null)
                {
                    throw new Exception("Driver initialization failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing driver: {ex.Message}");
                throw;
            }
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                if (driver != null)
                {
                    driver.Quit();
                    driver.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        [OneTimeTearDown]
        public void FinalCleanup()
        {
            _threadLocal?.Dispose();
        }

        public void Dispose()
        {
            _threadLocal?.Dispose();
        }
    }
}