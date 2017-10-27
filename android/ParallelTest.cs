using System;
using System.Threading;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;

namespace android
{
  [TestFixture("parallel", "pixel")]
  [TestFixture("parallel", "galaxy-s6")]
  [TestFixture("parallel", "galaxy-s7")]
  [Parallelizable(ParallelScope.Fixtures)]
  public class ParallelTest : BrowserStackNUnitTest
  {
    public ParallelTest(string profile, string device) : base(profile,device){ }

    [Test]
    public void searchWikipedia()
    {
      AndroidElement searchElement = (AndroidElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(By.Id("Search Wikipedia")));
      searchElement.Click();
      AndroidElement insertTextElement = (AndroidElement)new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(By.Id("org.wikipedia.alpha:id/search_src_text")));
      insertTextElement.SendKeys("Browserstack");
      Thread.Sleep(5000);

      ReadOnlyCollection<AndroidElement> allProductsName = driver.FindElements(By.ClassName("android.widget.TextView"));
      Assert.True(allProductsName.Count > 0);
    }
  }
}
