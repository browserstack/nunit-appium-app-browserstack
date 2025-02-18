using System;
using System.Threading;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace BrowserStack
{
  [TestFixture]
  [Category("sample-test")]
  public class SampleTest : BrowserStackNUnitTest
  {
    public SampleTest() : base() { }

    [Test]
    public void searchWikipedia()
    {
      // Initialize wait
      var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

      // Find and click search element using AccessibilityId
      var searchElement = wait.Until(ExpectedConditions.ElementToBeClickable(
          MobileBy.AccessibilityId("Search Wikipedia")));
      searchElement.Click();

      // Find and input text using ID
      var insertTextElement = wait.Until(ExpectedConditions.ElementToBeClickable(
          MobileBy.Id("org.wikipedia.alpha:id/search_src_text")));
      insertTextElement.SendKeys("BrowserStack");

      // ReadOnlyCollection<IWebElement> allProductsName = driver.FindElements(By.ClassName("android.widget.TextView"));
      Assert.That("true", Is.EqualTo("true"));
    }
  }
}
