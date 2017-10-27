using System;
using System.Threading;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Support.UI;

namespace ios
{
    [TestFixture("single", "iphone-7")]
    public class SingleTest : BrowserStackNUnitTest
    {
        public SingleTest(string profile, string environment) : base(profile, environment) { }

        [Test]
        public void loginTest()
        {
            IOSElement loginButton = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("Log In")));
            loginButton.Click();

            IOSElement emailTextField = (IOSElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(MobileBy.AccessibilityId("Email address")));
            emailTextField.SendKeys("hello@browserstack.com");

            driver.FindElementByAccessibilityId("Next").Click();
            Thread.Sleep(5000);

            ReadOnlyCollection<IOSElement> textElements = driver.FindElementsByXPath("//XCUIElementTypeStaticText");
            Assert.True(textElements.Count > 0);
            String matchedString = "";
            foreach (IOSElement textElement in textElements)
            {
                String textContent = textElement.Text;
                if (textContent.Contains("not registered"))
                {
                    matchedString = textContent;
                }
            }

            Console.WriteLine(matchedString);
        }
    }
}
