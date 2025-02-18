#pragma warning disable
using System.Runtime.CompilerServices;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Net;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using NLog;
using Serilog;
using Serilog;
using NLog.Config;
using NLog.Targets;
using BrowserStackSDK.TestObservability.Serilog.Sink;
using BrowserStackSDK.TestObservability.NLog.Appender;
using BrowserStackSDK.TestObservability.ConsoleAppender;
using BrowserStackSDK.TestObservability.TestCase.Model;
using BrowserStackSDK.TestObservability.Models.BDD;
using BrowserStackSDK.Utils;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;






using NUnit.Framework;


using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;


internal static class Initializer
{
    public static List<MethodBase> patchMethods = new List<MethodBase>();
    [ModuleInitializer]
    internal static void Run() {
        try
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(VanilaErrorHandler);
            BrowserStackSDK.Utils.ProcessHandler.HandleAdapterProcess();
        }
        catch {}

        try
        {
            int index = int.Parse(Environment.GetEnvironmentVariable("index") ?? "0");
            string jsonText = File.ReadAllText(Environment.GetEnvironmentVariable("capabilitiesPath"));

            if (jsonText != null)
            {
                JArray json = JArray.Parse(jsonText);
                if(json.Count > 0)
                {
                    JObject jsonIndexed = (JObject)json[index];
                    BrowserStackSDK.Automation.Context.capabilitiesJson = jsonIndexed;
                }
            }
        }
        catch{}

        Assembly assembly = Assembly.GetExecutingAssembly();
        BrowserStackSDK.Automation.Context.executingAssembly = assembly;
        string[] attributes = { "NUnit.Framework.TestAttribute", "NUnit.Framework.TestCaseAttribute", "NUnit.Framework.TestCaseSourceAttribute", "NUnit.Framework.TheoryAttribute" };
        var allTypes = assembly.GetTypes();
        foreach (var type in allTypes)
        {
            if (type.IsClass)
            {
                foreach (var method in type.GetMethods())
                {
                    foreach (var att in method.CustomAttributes)
                    {
                       if (attributes.Contains(att.Constructor.DeclaringType.ToString()) && (method.DeclaringType == method.ReflectedType))
                        {
                            patchMethods.Add(method);
                        }
                    }
                }
            }
        }
        if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_TOKEN")))
            Console.SetOut(new ConsoleAppender());
        BrowserstackPatcher.DoPatching();
    }

    static void VanilaErrorHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        string filePath = Path.Join(Path.GetTempPath(), ".browserstack", "vanilaErrorFile_" + Environment.GetEnvironmentVariable("index"));
        var platformDetails = Environment.GetEnvironmentVariable("browserName") + " " + Environment.GetEnvironmentVariable("osVersion") + " " + Environment.GetEnvironmentVariable("os") + " " + Environment.GetEnvironmentVariable("browserVersion") ;
        string[] fileContents = { platformDetails + "\n------------\n" + e.Message + "\n" + e.GetBaseException() + "\n"};
        File.WriteAllLines(filePath, fileContents);
    }
}

public class BrowserstackPatcher
{
    static string logLevel = Environment.GetEnvironmentVariable("BROWSERSTACK_LOG_LEVEL");
    //public static Configuration configs;
    // make sure DoPatching() is called at start either by
    // the mod loader or by your injector
    public static void DoPatching()
    {
        var harmony = new Harmony("com.browserstack.patch");
        harmony.PatchAllUncategorized(Assembly.GetExecutingAssembly());
        if (BrowserStackSDK.Accessibility.Injector.IsAccessibility()) {
            harmony.PatchCategory(Assembly.GetExecutingAssembly(), "accessibility");
            try
            {
                harmony.PatchCategory(Assembly.GetExecutingAssembly(), "accessibility-generic");
            }
            catch (System.Exception ex)
            {
                BrowserstackPatcher.BrowserStackLog("Error in wrapping A11y generic commands " + ex);
            }
            try
            {
                harmony.PatchCategory(Assembly.GetExecutingAssembly(), "accessibility-non-generic");
            }
            catch (System.Exception ex)
            {
                BrowserstackPatcher.BrowserStackLog("Error in wrapping A11y non-generic commands " + ex);
            }
        }

        try
        {
            if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_TOKEN"))) {
                harmony.PatchCategory(Assembly.GetExecutingAssembly(), "testObservability");
            }
        }
        catch (Exception ex)
        {
            BrowserstackPatcher.BrowserStackLog("Exception while patch connection to retrieve integrations data : "+ ex.ToString());
        }

        if(Environment.GetEnvironmentVariable("VSTEST_HOST_DEBUG") != "1")
        {
            harmony.Patch(typeof(WebDriver).GetMethod(nameof(WebDriver.Dispose), new Type[] {}), prefix: new HarmonyMethod(typeof(DisposePatch).GetMethod(nameof(DisposePatch.Prefix))));
            foreach (var method in Initializer.patchMethods)
            {
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(PatchTest).GetMethod(nameof(PatchTest.Prefix))), finalizer: new HarmonyMethod(typeof(PatchTest).GetMethod(nameof(PatchTest.Finalizer))));
            }
        }

    }

    public static void BrowserStackLog(String message)
    {
        if (logLevel == "debug")
        {
            Console.WriteLine(message);
        }
    }
}

class BrowserStackException : Exception
{
    private string oldStackTrace;

    public BrowserStackException(string message, string stackTrace) : base(message)
    {
        this.oldStackTrace = stackTrace;
    }


    public override string StackTrace
    {
        get
        {
            return this.oldStackTrace;
        }
    }
}

class Store {
    public static void PersistHubUrl(string optimalHubUrl) {
        try {
            string filePath = Path.Combine(Path.GetTempPath(), ".browserstack", "hubUrlList.json");
            if (File.Exists(filePath)) {
                string hubUrls = File.ReadAllText(filePath);
                hubUrls += $"; {optimalHubUrl}";
                File.WriteAllText(filePath, hubUrls);
                return;
            }
            File.WriteAllText(filePath, optimalHubUrl);
            return;
        } catch {}
    }
}


class BrowserStackOptions : DriverOptions
{
    public BrowserStackOptions(String browser_name, String browser_version = "latest")
    {
        if (browser_name != null){
            this.BrowserName = browser_name;
        }
        if (browser_version != null){
            this.BrowserVersion = browser_version;
        }
    }

    public void AddBrowserName(Object browser_name)
    {
        if(this.BrowserName == null)
            this.BrowserName = browser_name.ToString();
    }

    public void AddBrowserVersion(Object browser_version)
    {
        if(this.BrowserVersion == null)
            this.BrowserVersion = browser_version.ToString();
    }

    

    public override void AddAdditionalOption(string optionName, object optionValue)
    {
      if (optionName == "platformName") {
        if (this.PlatformName == null) {
          this.PlatformName = optionValue.ToString();
        }
        return;
      }
      else if (optionName == "unhandledPromptBehavior")
      {
        string stringValue = optionValue?.ToString();
        // This enum doesn't exsist below 3.7.0 version of Selenium WebDriver.
        if (Enum.TryParse<UnhandledPromptBehavior>(stringValue, true, out var behavior))
        {
            this.UnhandledPromptBehavior = behavior;
            return;
        }
        else
        {
            BrowserstackPatcher.BrowserStackLog("Exception in adding cap " + optionName);
        }
      }
      base.AddAdditionalOption(optionName, optionValue);
    }

    public override ICapabilities ToCapabilities()
    {
        IWritableCapabilities capabilities = this.GenerateDesiredCapabilities(true);

        return capabilities.AsReadOnly();
    }
}

[HarmonyPatch(typeof(HttpCommandExecutor))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(Uri), typeof(TimeSpan), typeof(bool) })]
class ExecutorPatch
{
    static void Prefix(ref Uri addressOfRemoteServer, ref TimeSpan timeout)
    {
        var url = Environment.GetEnvironmentVariable("hubUrl") ?? "https://hub.browserstack.com/wd/hub";
        addressOfRemoteServer = new Uri(url);
        timeout = timeout.Add(TimeSpan.FromSeconds(900));
    }

    static void Postfix(HttpCommandExecutor __instance)
    {
        if (Environment.GetEnvironmentVariable("proxy") != null){
            __instance.Proxy = new WebProxy(Environment.GetEnvironmentVariable("proxy"), false);
        }
    }
}

[HarmonyPatch(typeof(Response))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(Dictionary<string, object>) })]
class ResponsePatch
{
    static void Postfix(Response __instance)
    {
        try
        {
            if (__instance.Value != null)
            {
                Type responseType = __instance.Value.GetType();
                if (responseType.Equals(typeof(Dictionary<string, object>)))
                {
                    Dictionary<string, object> value = (Dictionary<string, object>) __instance.Value;
                    if (value.ContainsKey("optimalHubUrl"))
                    {
                        var hubUrl = "https://" + value["optimalHubUrl"] + "/wd/hub/";
                        Environment.SetEnvironmentVariable("hubUrl", hubUrl);
                        Environment.SetEnvironmentVariable("optimalHubFlag", "true");
                        Store.PersistHubUrl((string)value["optimalHubUrl"]);
                    }
                }
            }
        } catch {}
    }
}

[HarmonyPatch]
class CommandExecutorPatch
{
    public static MethodBase TargetMethod()
    {
        MethodBase executeAsync = typeof(HttpCommandExecutor).GetMethod("ExecuteAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (executeAsync != null) return executeAsync;
        MethodBase execute = typeof(HttpCommandExecutor).GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        return execute;
    }
    static void Prefix(Command commandToExecute, ref HttpClient ___client, ref Uri ___remoteServerUri, ref TimeSpan ___serverResponseTimeout)
    {
        if (Environment.GetEnvironmentVariable("optimalHubFlag") == "true" && commandToExecute.Name != "newSession")
        {
            ___client = null;
            ___serverResponseTimeout = ___serverResponseTimeout.Add(TimeSpan.FromSeconds(900));
            ___remoteServerUri = new Uri(Environment.GetEnvironmentVariable("hubUrl"));
            Environment.SetEnvironmentVariable("optimalHubFlag", "false");
        }
        BrowserStackSDK.Utils.MiscUtils.AddLog("Starting a11y PerformScan");
        BrowserStackSDK.Utils.MiscUtils.AddLog($"Command to execute: {commandToExecute.Name}");
        BrowserStackSDK.Accessibility.Injector.PerformScan(commandToExecute.Name);
        BrowserStackSDK.Utils.MiscUtils.AddLog("Completed a11y PerformScan");
        BrowserStackSDK.Percy.PercyEventHandler.BeforeExecute(commandToExecute.Name);
   }
}

[HarmonyPatch(typeof(DriverService))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(string) })]
class ServicePatch
{
    static void Prefix(ref string servicePath, ref string driverServiceExecutableName)
    {
        try
        {
            File.Create(Path.Join(Path.GetTempPath(), "WebDriver.exe"));
        }catch{}


        servicePath = Path.GetTempPath();
        driverServiceExecutableName = "WebDriver.exe";
    }
}


class FindDriverServiceExecutablePatch
{
    static MethodBase TargetMethod()
    {
        // refer to C# reflection documentation:
        return typeof(DriverService).GetMethod("FindDriverServiceExecutable", BindingFlags.NonPublic |  BindingFlags.Static);
    }
    static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(DriverService), nameof(DriverService.Start))]
class StartPatch
{
    static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(WebDriver))]
[HarmonyPatch(MethodType.Constructor)]
[HarmonyPatch(new Type[] { typeof(ICommandExecutor), typeof(ICapabilities) })]
class WebDriverPatch
{
    public static Dictionary<int, RemoteWebDriver> drivers__ = new Dictionary<int, RemoteWebDriver>();
    public static Dictionary<int, bool> quitFromDrivers = new Dictionary<int, bool>();
    public static Dictionary<int, bool> insideTestMethods = new Dictionary<int, bool>();
    public static Dictionary<string, List<string>> errorMessagesList = new Dictionary<string, List<string>>();
    public static bool localNotSetError = false;
    public static string urlForExceptionInResp = "";
    static bool Prefix(ref dynamic executor, ref ICapabilities capabilities)
    {
        Dictionary<string, object> browserstackOptions = new Dictionary<string, object>();

        int index = int.Parse(Environment.GetEnvironmentVariable("index"));
        var browserName = Environment.GetEnvironmentVariable("browserName");
        var browserVersion = Environment.GetEnvironmentVariable("browserVersion");
        var isLocal = Environment.GetEnvironmentVariable("isLocal");
        var localIdentifier = Environment.GetEnvironmentVariable("localIdentifier");
        var proxy = Environment.GetEnvironmentVariable("proxy");

        var browserstackAutomationEnv = true;
        if (Environment.GetEnvironmentVariable("BROWSERSTACK_AUTOMATION") == "False")
        {
            browserstackAutomationEnv = false;
        }

        BrowserStackOptions finalOptions = new BrowserStackOptions(browserName, browserVersion);

        var capsType = capabilities.GetType();
        Dictionary<string, Object> existingKeys = new Dictionary<string, object>();
        if(capsType.ToString() == "OpenQA.Selenium.Remote.DesiredCapabilities")
        {
            IWritableCapabilities appiumCapabilities = (IWritableCapabilities)capabilities;
            ReadOnlyDesiredCapabilities rc = (ReadOnlyDesiredCapabilities)appiumCapabilities.AsReadOnly();
var dic = rc.ToDictionary();
            foreach (var cap in dic)
            {
                try
                {
                    if (cap.Key == "browserName")
                    {
                        finalOptions.AddBrowserName(cap.Value);
                    }
                    else if (cap.Key == "browserVersion")
                    {
                        finalOptions.AddBrowserVersion(cap.Value);
                    }
                    else{
                        finalOptions.AddAdditionalOption(cap.Key, cap.Value);
                        existingKeys.TryAdd(cap.Key, cap.Value);
                    }

                    if (!browserstackAutomationEnv && cap.Value != null)
                    {
                        if(cap.Key == "bstack:options")
                        {
                            try
                            {
                                string jsonValue = JsonConvert.SerializeObject(cap.Value);
                                browserstackOptions = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonValue);
                            }
                            catch(Exception e) {
                                BrowserstackPatcher.BrowserStackLog("Error in processing bstack:options for SDK caps " + e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    BrowserstackPatcher.BrowserStackLog("Exception in adding cap: " + cap.Key + " Value: " + cap.Value);
                }
            }
        }
        else
        {
            ReadOnlyDesiredCapabilities dc = (ReadOnlyDesiredCapabilities)capabilities;
            var dic = dc.ToDictionary();
            foreach (var cap in dic)
            {
                try
                {
                    if(cap.Key == "browserName")
                    {
                        finalOptions.AddBrowserName(cap.Value);
                    }
                    else if(cap.Key == "browserVersion")
                    {
                        finalOptions.AddBrowserVersion(cap.Value);
                    }
                    else
                    {
                        finalOptions.AddAdditionalOption(cap.Key, cap.Value);
                        existingKeys.TryAdd(cap.Key, cap.Value);
                    }

                    if (!browserstackAutomationEnv && cap.Value != null)
                    {
                        try
                        {
                            if(cap.Key == "bstack:options")
                            {
                                string jsonValue = JsonConvert.SerializeObject(cap.Value);
                                browserstackOptions = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonValue);
                            }
                        }
                        catch(Exception e) {
                            BrowserstackPatcher.BrowserStackLog("Error in processing bstack:options for SDK caps " + e);
                        }
                    }
                }
                catch (Exception e)
                {
                    BrowserstackPatcher.BrowserStackLog("Exception while adding cap: " + e.ToString());
                }
            }
        }

        String jsonText = null;
        try
        {
            jsonText = File.ReadAllText(Environment.GetEnvironmentVariable("capabilitiesPath"));
        }
        catch
        { }

        if (jsonText != null)
        {
            JArray json = JArray.Parse(jsonText);
            JObject options = null;
            if (!browserstackAutomationEnv)
            {
                try{
                    var remoteServerUri = executor.GetType().GetField("remoteServerUri", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(executor);
                    if(remoteServerUri != null && remoteServerUri.ToString().Contains("browserstack.com") && json.Count > 0)
                    {
                        JObject jsonIndexed = (JObject)json[0];
                        JObject sdkCaps = (JObject)jsonIndexed.GetValue("w3c");
                        JObject bstackOptions = (JObject)sdkCaps.GetValue("bstack:options");
                        if (bstackOptions != null)
                        {
                            foreach (var cap in bstackOptions)
                            {
                                if (browserstackOptions.ContainsKey(cap.Key))
                                {
                                    browserstackOptions[cap.Key] = cap.Value;
                                }
                                else
                                {
                                    browserstackOptions.Add(cap.Key, cap.Value);
                                }
                            }
                        }
                    }
                    finalOptions.AddAdditionalOption("bstack:options", browserstackOptions);
                }
                catch(Exception e) {
                    BrowserstackPatcher.BrowserStackLog("Error in processing SDK caps " + e);
                }
            }
            else
            {
                if(json.Count > 0)
                {
                    JObject jsonIndexed = (JObject)json[index];
                    options = (JObject)jsonIndexed.GetValue("bstack:options");
                    if (options != null)
                    {
                        foreach (var item in options)
                        {
                            browserstackOptions.Add(item.Key, item.Value);
                        }

                        if (isLocal == "true")
                        {
                            browserstackOptions.Add("local", true);
                            if (localIdentifier != "")
                                browserstackOptions.Add("localIdentifier", localIdentifier);
                        }

                        bool addedCaps = false;
                        try {
                            bool bstackOptionsPresent = existingKeys.ContainsKey("bstack:options");
                            bool mergeCaps = bstackOptionsPresent;
                            if (bstackOptionsPresent) {
                                Object existingBstackOptions = existingKeys["bstack:options"];
                                Type baseType = BrowserStackSDK.Utils.MiscUtils.GetBasestNonObjectType(existingBstackOptions.GetType());

                                BrowserstackPatcher.BrowserStackLog("Base type of bstack:options: " + baseType);

                                /*
                                Check if root caps are added in bstack:options. This is to handle a bug in sample repos where root capabilities are added in bstack:options
                                If root caps are present then skip merging
                                */
                                if (baseType != null && baseType.ToString() == "OpenQA.Selenium.DriverOptions") {
                                    ICapabilities caps = ((DriverOptions) existingBstackOptions).ToCapabilities();
                                    if (caps.GetCapability("bstack:options") != null) {
                                        BrowserstackPatcher.BrowserStackLog("Detected bstack:options inside bstack:options, skipping merging");
                                        mergeCaps = false;
                                    }
                                }
                            }

                            if (mergeCaps)
                            {
                                JObject existingBStackOptions = (JObject)JToken.FromObject(existingKeys["bstack:options"]);
                                JObject values = (JObject)JToken.FromObject(browserstackOptions);
                                existingBStackOptions.Merge(values);
                                finalOptions.AddAdditionalOption("bstack:options", existingBStackOptions);
                                addedCaps = true;
                            }
                        } catch (Exception e) {
                            BrowserstackPatcher.BrowserStackLog("Exception in merging bstack:options " + e.ToString());
                        }

                        if (!addedCaps) finalOptions.AddAdditionalOption("bstack:options", browserstackOptions);
                        jsonIndexed.Remove("bstack:options");
                    }
                    foreach (var item in jsonIndexed)
                    {
                        try
                        {
                            if (item.Value != null && existingKeys.ContainsKey(item.Key))
                            {
                                try {
                                    JToken existingToken = JToken.FromObject(existingKeys[item.Key]);
                                    JToken newToken = item.Value;

                                    if (existingToken.Type == JTokenType.Object && newToken.Type == JTokenType.Object)
                                    {
                                        JObject ex = (JObject)existingToken;
                                        JObject values = (JObject)newToken;
                                        ex.Merge(values);
                                        finalOptions.AddAdditionalOption(item.Key, ex);
                                    }
                                    else if (existingToken.Type == JTokenType.Array && newToken.Type == JTokenType.Array)
                                    {
                                        JArray exArray = (JArray)existingToken;
                                        JArray newArray = (JArray)newToken;
                                        foreach (var element in newArray)
                                        {
                                            exArray.Add(element);
                                        }
                                        finalOptions.AddAdditionalOption(item.Key, exArray);
                                    }
                                    else
                                    {
                                        finalOptions.AddAdditionalOption(item.Key, newToken);
                                    }
                                } catch (Exception e)
                                {
                                    BrowserstackPatcher.BrowserStackLog("Exception in merging " + item.Key + " exception: " + e.ToString());
                                    finalOptions.AddAdditionalOption(item.Key, item.Value);
                                }
                            }
                            else
                            {
                                finalOptions.AddAdditionalOption(item.Key, item.Value);
                            }
                        }
                        catch (Exception e)
                        {
                            BrowserstackPatcher.BrowserStackLog("Exception in adding cap " + item.Key + " with error: " + e.ToString());
                        }
                    }
                }
                if (options == null && isLocal == "true")
                {
                    finalOptions.AddAdditionalOption("browserstack.local", true);
                    if (localIdentifier != "")
                        finalOptions.AddAdditionalOption("local_identifier", localIdentifier);
                }
            }
        }

        try
        {
            capabilities = finalOptions.ToCapabilities();
        }
        catch
        {
        }


        return true;
    }

    static void Postfix(RemoteWebDriver __instance)
    {
        drivers__[Thread.CurrentThread.ManagedThreadId] = __instance;
        BrowserStackSDK.Automation.Context automationContext = BrowserStackSDK.Automation.Context.AddOrGet();
        BrowserStackSDK.TestObservability.EventsHandler.LogEventsHandler.ReceiveDriver(__instance);

        if (Environment.GetEnvironmentVariable("BROWSERSTACK_AUTOMATION") == "False")
        {
            automationContext.BrowserstackAutomation = false;
        }
        automationContext.AddDriver(__instance);
        BrowserStackSDK.Accessibility.Injector.InsideWebDriver();
    }
}

class TestObservabilityReflector {
    private static Type? assemblyType;

    private static MethodInfo? LoadMethodInfo(string methodName, bool isTestContext = false) {
        string listenerDll = System.IO.Path.Join(System.AppDomain.CurrentDomain.BaseDirectory, "BrowserstackListener.dll");
        if (string.IsNullOrEmpty(listenerDll)) return null;

        Assembly assembly = Assembly.LoadFrom(listenerDll);
        string className = isTestContext ? "TestEventsHandler" : "LogEventsHandler";
        assemblyType = assembly.GetType($"TestObservability.EventsHandler.{className}");
        MethodInfo? method = assemblyType.GetMethod(methodName);
        return method;
    }

    public static void SendDriver(object __instance) {
        object[] webDriver = new object[1] {__instance};
        MethodInfo? receiveDriver = LoadMethodInfo("ReceiveDriver");
        if (receiveDriver != null)
            receiveDriver.Invoke(null!, webDriver);
    }

    public static void SendScreenshot(Screenshot __screenshot, Type screenShotType = null) {
        string[] screenshot = new string[1] {__screenshot.AsBase64EncodedString};
        MethodInfo? receiveScreenshot = LoadMethodInfo("ReceiveScreenshot");
        if (receiveScreenshot != null)
            receiveScreenshot.Invoke(null!, screenshot);
    }

    public static void SendTestContext(object testContext, string eventType) {
        object[] testContexts = new object[2] {testContext, eventType};
        MethodInfo? receiveTestContext = LoadMethodInfo("ReceiveTestContext", true);
        if (receiveTestContext != null)
            receiveTestContext.Invoke(null!, testContexts);
    }
}

class DisposePatch {
    public static bool Prefix(RemoteWebDriver __instance)
    {
        try {
            TestObservabilityReflector.SendDriver(__instance);
            Console.Out.Flush();
            Console.Out.Close();
            

            if (WebDriverPatch.insideTestMethods.GetValueOrDefault(Thread.CurrentThread.ManagedThreadId, false)) {
                WebDriverPatch.quitFromDrivers[Thread.CurrentThread.ManagedThreadId] = true;
                return false;
            } else {
                if (Environment.GetEnvironmentVariable("BROWSERSTACK_SKIP_SESSION_STATUS").ToLower() != "true"  && __instance != null && __instance.SessionId != null) {
                    // Final session marking.
                    if (WebDriverPatch.errorMessagesList.GetValueOrDefault(__instance.SessionId.ToString(), new List<string>()).Count > 0)
                        ((IJavaScriptExecutor)__instance).ExecuteScript("browserstack_executor: {\"action\": \"setSessionStatus\", \"arguments\": {\"status\":\"failed\", \"reason\": " + JsonConvert.SerializeObject(String.Join(", ", WebDriverPatch.errorMessagesList.GetValueOrDefault(__instance.SessionId.ToString(), new List<string>()))) + "}}");
                    else
                        ((IJavaScriptExecutor)__instance).ExecuteScript("browserstack_executor: {\"action\": \"setSessionStatus\", \"arguments\": {\"status\":\"passed\", \"reason\": \"Passed\"}}");
                }
            }
        } catch(Exception e) {
            BrowserstackPatcher.BrowserStackLog("Error in marking session status " + e);
        }
        return true;
    }
}

[HarmonyPatch]
[HarmonyPatch(typeof(OpenQA.Selenium.WebDriver))]
[HarmonyPatch("GetScreenshot")]
class ScreenshotPatch {
    static void Postfix(ref Screenshot __result) {
        TestObservabilityReflector.SendScreenshot(__result, typeof(Screenshot));
        BrowserStackSDK.Percy.PercyEventHandler.OnEvent("driver.screenshot");
    }
}

[HarmonyPatch]
[HarmonyPatch(typeof(OpenQA.Selenium.WebElement))]
[HarmonyPatch("SendKeys")]
class SendKeysPatch {
    static void Postfix() {
        BrowserStackSDK.Percy.PercyEventHandler.OnEvent("element.sendKeys");
    }
}


[HarmonyPatch]
[HarmonyPatch(typeof(Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.EventHandlers.TestRunEventsHandler))]
[HarmonyPatch("HandleTestRunComplete")]

class TestRunCompletePatch {
    static void Prefix() {
        try {
            BrowserStackSDK.Automation.Context.PerformCleanup();
        } catch (Exception e)
        {
        }
    }
}

[HarmonyPatch]
[HarmonyPatch(typeof(Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.PlatformEnvironment))]
[HarmonyPatch("Exit")]

class PlatformEnvironmentPatch {
    static void Prefix() {
        try {
            BrowserStackSDK.Automation.Context.PerformCleanup();
        } catch (Exception e)
        {
        }
    }
}

[HarmonyPatch]
[HarmonyPatch(typeof(OpenQA.Selenium.WebElement))]
[HarmonyPatch("Click")]
class ClickPatch {
    static void Postfix() {
        BrowserStackSDK.Percy.PercyEventHandler.OnEvent("element.click");
    }
}

[HarmonyPatch]
class GoToUrlPatch
{
    public static List<MethodBase> accessMethods = new List<MethodBase>();
    public static List<string> validDomainOrIPs = new List<string>() { "^localhost$", "^bs-local.com$", "^127\\.", "^10\\.", "^172\\.1[6-9]\\.", "^172\\.2[0-9]\\.", "^172\\.3[0-1]\\.", "^192\\.168\\." };
    static List<MethodBase> TargetMethods()
    {
        MethodBase patchStringGoToUrl = AccessTools.Method(AccessTools.TypeByName("OpenQA.Selenium.Navigator"), "GoToUrl", new Type[] { typeof(String) });
        MethodBase patchUriGoToUrl = AccessTools.Method(AccessTools.TypeByName("OpenQA.Selenium.Navigator"), "GoToUrl", new Type[] { typeof(Uri) });
        accessMethods.Add(patchStringGoToUrl);
        accessMethods.Add(patchUriGoToUrl);
        return accessMethods;
    }
    static void Prefix(ref dynamic url)
    {
        String goToUrl = Convert.ToString(url);
        WebDriverPatch.urlForExceptionInResp = goToUrl;
        getNudgeLocalNotSetError(goToUrl);
    }

    static void getNudgeLocalNotSetError(String url)
    {
        try {
            var isLocal = Environment.GetEnvironmentVariable("isLocal");
            if (isLocal == "true" || WebDriverPatch.localNotSetError.Equals(true))
            {
                return;
            }
            string hostname = GetHostName(url);
            bool isPrivate = IsPrivateDomainOrIP(hostname);
                if (isPrivate)
            {
                string browserstackFolderPath = GetTempDir();
                if (!Directory.Exists(browserstackFolderPath))
                {
                    Directory.CreateDirectory(browserstackFolderPath);
                }
                string filePath = Path.Join(browserstackFolderPath, ".local-not-set.json");
                if (File.Exists(browserstackFolderPath))
                {
                    WebDriverPatch.localNotSetError = true;
                    return;
                }

                WebDriverPatch.localNotSetError = true;
                JObject j = new JObject();
                j.Add("hostname", hostname);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(j));
            }
        } catch (Exception ex)
        {
        }
    }

    static String GetHostName(String url)
    {
        String hostName = "";
        try
        {
            var uriObject = new Uri(url);
            hostName = uriObject.Host;

        }
        catch (Exception ex)
        {
        }

        return hostName;
    }

    static bool IsPrivateDomainOrIP(String hostName)
    {
        bool isPrivate = false;
        if (!String.IsNullOrEmpty(hostName))
        {
            try
            {
                foreach (String reg in validDomainOrIPs)
                {
                    Regex regex = new Regex(reg);
                    if (regex.IsMatch(hostName))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        return isPrivate;
    }

    public static string GetTempDir()
    {
        string browserstackFolderPath = Path.Join(Path.GetTempPath(), ".browserstack");
        try
        {
            if (!Directory.Exists(browserstackFolderPath))
            {
                Directory.CreateDirectory(browserstackFolderPath);
            }
            return browserstackFolderPath;
        }
        catch (Exception ex)
        {
        }
        return browserstackFolderPath;
    }

    static Exception Finalizer(Exception __exception)
    {
        var driver = WebDriverPatch.drivers__.GetValueOrDefault(Thread.CurrentThread.ManagedThreadId, null);
        if (driver != null)
        {
            var sessionName = TestContext.CurrentContext.Test.FullName;
var status = TestContext.CurrentContext.Result.Outcome.Status;
var message = TestContext.CurrentContext.Result.Message;
 
            if (message != null) message = message.ToString();
            if (__exception != null)
            {
                message = __exception.Message;
            }
            if (message != null && (message.Contains("ERR_FAILED") || message.Contains("ERR_TIMED_OUT") || message.Contains("ERR_BLOCKED_BY_CLIENT") || message.Contains("ERR_NETWORK_CHANGED") || message.Contains("ERR_SOCKET_NOT_CONNECTED") || message.Contains("ERR_CONNECTION_CLOSED") || message.Contains("ERR_CONNECTION_RESET") || message.Contains("ERR_CONNECTION_REFUSED") || message.Contains("ERR_CONNECTION_ABORTED") || message.Contains("ERR_CONNECTION_FAILED") || message.Contains("ERR_NAME_NOT_RESOLVED") || message.Contains("ERR_ADDRESS_INVALID") || message.Contains("ERR_ADDRESS_UNREACHABLE") || message.Contains("ERR_TUNNEL_CONNECTION_FAILED") || message.Contains("ERR_CONNECTION_TIMED_OUT") || message.Contains("ERR_SOCKS_CONNECTION_FAILED") || message.Contains("ERR_SOCKS_CONNECTION_HOST_UNREACHABLE") || message.Contains("ERR_PROXY_CONNECTION_FAILED") || message.Contains("ERR_NAME_RESOLUTION_FAILED") || message.Contains("ERR_MANDATORY_PROXY_CONFIGURATION_FAILED")))
            {
                try {
                    string hostName = GetHostName(WebDriverPatch.urlForExceptionInResp);
                    var isLocal = Environment.GetEnvironmentVariable("isLocal");
                    if (!(isLocal == "true" || WebDriverPatch.localNotSetError.Equals(true)))
                    {
                        string browserstackFolderPath = Path.Join(Path.GetTempPath(), ".browserstack");
                        try
                        {
                            if (!Directory.Exists(browserstackFolderPath))
                            {
                                Directory.CreateDirectory(browserstackFolderPath);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        if (!Directory.Exists(browserstackFolderPath))
                        {
                            Directory.CreateDirectory(browserstackFolderPath);
                        }
                        string filePath = Path.Join(browserstackFolderPath, ".local-not-set.json");
                        if (File.Exists(browserstackFolderPath))
                        {
                            WebDriverPatch.localNotSetError = true;
                        }
                        else
                        {
                            WebDriverPatch.localNotSetError = true;
                            JObject j = new JObject();
                            j.Add("hostname", hostName);
                            File.WriteAllText(filePath, JsonConvert.SerializeObject(j));
                        }
                    }
                } catch (Exception ex)
                {
                }
            }
            
        }
        return __exception;
    }
}



[HarmonyPatch(typeof(NUnit.Framework.Internal.TestResult), nameof(NUnit.Framework.Internal.TestResult.SetResult))]
[HarmonyPatch(new Type[] { typeof(NUnit.Framework.Interfaces.ResultState), typeof(string), typeof(string) })]
class Patch07
{
static bool Prefix(ref string stackTrace)
{
if (stackTrace != null)
stackTrace = Regex.Replace(stackTrace, @"(_Patch)(\d+)(?!.*\d)", "");
return true;
}
}


class PatchTest
{
    

    public static void Prefix(MethodBase __originalMethod, params object[] __args)
    {
        BrowserStackSDK.Automation.Context.ThreadNameAsync.Value = Thread.CurrentThread.ManagedThreadId;
        BrowserStackSDK.Automation.Context automationContext = BrowserStackSDK.Automation.Context.AddOrGet();
        // Observability specific handling, JWT_TOKEN = O11Y JWT Token
        if(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_TOKEN"))) {
            Console.SetOut(new ConsoleAppender());
        }
        TestObservabilityReflector.SendTestContext(TestContext.CurrentContext, "TestRunStarted");
        var sessionName = TestContext.CurrentContext.Test.FullName;
var status = TestContext.CurrentContext.Result.Outcome.Status;
var message = TestContext.CurrentContext.Result.Message;
 

        WebDriverPatch.insideTestMethods[Thread.CurrentThread.ManagedThreadId] = true;
        var driver = WebDriverPatch.drivers__.GetValueOrDefault(Thread.CurrentThread.ManagedThreadId, null);
        
        automationContext.sessionName = sessionName;
        automationContext.insideTestMethods = true;

        BrowserStackSDK.Accessibility.Injector.BeforeTest();
    }

    public static Exception Finalizer(Exception __exception, MethodBase __originalMethod, params object[] __args)
    {
        try
        {
            TestObservabilityReflector.SendTestContext(TestContext.CurrentContext, "TestRunFinished");
            BrowserStackSDK.Automation.Context automationContext = BrowserStackSDK.Automation.Context.AddOrGet();
            var driver = WebDriverPatch.drivers__.GetValueOrDefault(Thread.CurrentThread.ManagedThreadId, null);
            if (driver != null)
            {
                var sessionName = TestContext.CurrentContext.Test.FullName;
var status = TestContext.CurrentContext.Result.Outcome.Status;
var message = TestContext.CurrentContext.Result.Message;
 
                

                automationContext.sessionName = sessionName;
                if (message != null) message = message.ToString();
                if (__exception != null)
                {
                    status = NUnit.Framework.Interfaces.TestStatus.Failed;
                    message = __exception.Message;
                    automationContext.status = "failed";
                }
                else
                {
                    automationContext.status = "passed";
                }

                if (automationContext.BrowserstackAutomation) {
                    if (Environment.GetEnvironmentVariable("BROWSERSTACK_SKIP_SESSION_NAME").ToLower() != "true")
                        ((IJavaScriptExecutor)driver).ExecuteScript("browserstack_executor: {\"action\": \"setSessionName\", \"arguments\": {\"name\": " + JsonConvert.SerializeObject(sessionName) + "}}");

                    if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
                    {
                        if (WebDriverPatch.errorMessagesList.ContainsKey(driver.SessionId.ToString()))
                        {
                            WebDriverPatch.errorMessagesList[driver.SessionId.ToString()].Add(message);
                        }
                        else
                        {
                            WebDriverPatch.errorMessagesList.Add(driver.SessionId.ToString(), new List<string> { message });
                        }
                        ((IJavaScriptExecutor)driver).ExecuteScript("browserstack_executor: {\"action\": \"annotate\", \"arguments\": {\"data\": " + JsonConvert.SerializeObject("Failed - " + message) + ", \"level\": \"error\"}}");
                    }
                    else
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript("browserstack_executor: {\"action\": \"annotate\", \"arguments\": {\"data\": \"Passed\", \"level\": \"info\"}}");
                    }
                }
                BrowserStackSDK.Accessibility.Injector.AfterTest();
                BrowserStackSDK.Percy.PercyEventHandler.OnEvent("test.end");
                WebDriverPatch.insideTestMethods[Thread.CurrentThread.ManagedThreadId] = false;
                if (WebDriverPatch.quitFromDrivers.GetValueOrDefault(Thread.CurrentThread.ManagedThreadId, false))
                {
                    WebDriverPatch.quitFromDrivers[Thread.CurrentThread.ManagedThreadId] = false;
                    driver.Quit();
                }

                

            }
        }
        catch(Exception ex)
        {
            BrowserstackPatcher.BrowserStackLog("Exception while running finalizer: "+ ex.ToString());
        }

        return __exception;
    }
}

[HarmonyPatch(typeof(LoggerConfiguration))]
[HarmonyPatch(MethodType.Constructor)]
class SeriLogPatch {
    static void Postfix(ref LoggerConfiguration __instance) {
        __instance.WriteTo.TestObservabilitySerilogSink();

    }
}

[HarmonyPatch(typeof(LoggingConfiguration))]
[HarmonyPatch(MethodType.Constructor)]
class LoggingConfigurationPatch {
    static void Postfix(ref LoggingConfiguration __instance) {
      TestObservabilityNLogAppender nlogAppender = new TestObservabilityNLogAppender();
      __instance.AddTarget("custom", nlogAppender);
      LoggingRule rule = new("*", NLog.LogLevel.Debug, nlogAppender);
      __instance.LoggingRules.Add(rule);
    }
}







#pragma warning restore
