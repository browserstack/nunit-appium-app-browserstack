using NUnit.Framework;
using OpenQA.Selenium;

namespace BrowserStack
{
  [TestFixture]
  [Category("ai-authoring-test")]
  public class AIAuthoringTest : BrowserStackNUnitTest
  {
    public AIAuthoringTest() : base(){}

    [Test]
    public void testWithAIAgent()
    {
      // Search using AI Agent commands
      ((IJavaScriptExecutor)driver).ExecuteScript(
        "browserstack_executor: {\"action\": \"ai\", \"arguments\": [\"Tap on Search Wikipedia\"]}"
      );
      ((IJavaScriptExecutor)driver).ExecuteScript(
        "browserstack_executor: {\"action\": \"ai\", \"arguments\": [\"Type India in the search field\"]}"
      );

      // Verify results
      ((IJavaScriptExecutor)driver).ExecuteScript(
        "browserstack_executor: {\"action\": \"ai\", \"arguments\": [\"Verify search results are displayed\"]}"
      );
    }
  }
}
