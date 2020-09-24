using System;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Support.UI;

namespace ios.first
{
    [TestFixture("first", "iphone-11-pro")]
    public class SingleTest : BrowserStackNUnitTest
    {
        public SingleTest(string profile, string environment) : base(profile, environment) { }

        [Test]
        public void textVerificationTest()
        {
            IOSElement textButton = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("Text Button")));
            textButton.Click();

            IOSElement textInput = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("Text Input")));
            textInput.SendKeys("hello@browserstack.com"+"\n");

            IOSElement textOutput = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("Text Output")));
            Assert.AreEqual(textOutput.Text,"hello@browserstack.com");
        }
    }
}
