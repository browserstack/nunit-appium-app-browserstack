using System;
using System.Threading;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace android.local
{
    [TestFixture("local", "pixel-3")]
    public class LocalTest : BrowserStackNUnitTest
    {
        public LocalTest(string profile, string device) : base(profile,device) {}

        [Test]
        public void testLocal()
        {
            AndroidElement searchElement = (AndroidElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(By.Id("com.example.android.basicnetworking:id/test_action")));
            searchElement.Click();
            AndroidElement testElement = (AndroidElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(By.ClassName("android.widget.TextView")));

            ReadOnlyCollection<AndroidElement> allTextViewElements = driver.FindElements(By.ClassName("android.widget.TextView"));

            Thread.Sleep(5000);

            foreach (AndroidElement textElement in allTextViewElements)
            {
                if (textElement.Text.Contains("The active connection is"))
                {
                    Assert.True(textElement.Text.Contains("The active connection is wifi"),"Incorrect Text");        
                }
            }
        }
    }
}
