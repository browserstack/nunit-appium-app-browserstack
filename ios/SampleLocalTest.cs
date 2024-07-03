using System;
using System.Threading;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;

namespace BrowserStack
{
    [TestFixture]
    [Category("sample-local-test")]
    public class LocalTest : BrowserStackNUnitTest
    {
        public LocalTest() : base() {}

        [Test]
        public void testLocal()
        {
            IOSElement testButton = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("TestBrowserStackLocal")));
            testButton.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.TextToBePresentInElementValue(driver.FindElement(MobileBy.AccessibilityId("ResultBrowserStackLocal")), "Up and running"));
            IOSElement resultElement = (IOSElement)driver.FindElement(MobileBy.AccessibilityId("ResultBrowserStackLocal"));

            String resultString = resultElement.Text.ToLower();
            if (resultString.Contains("not working"))
            {
                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                string screenshot = ss.AsBase64EncodedString;
                byte[] screenshotAsByteArray = ss.AsByteArray;
                ss.SaveAsFile("screenshot", ScreenshotImageFormat.Png);
                ss.ToString();
            }

            String expectedString = "Up and running";

            Assert.True(resultString.Contains(expectedString.ToLower()));
        }
    }
}
