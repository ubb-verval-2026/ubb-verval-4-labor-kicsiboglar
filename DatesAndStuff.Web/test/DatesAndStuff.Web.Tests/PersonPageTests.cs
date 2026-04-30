using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class PersonPageTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private const string BaseURL = "http://localhost:5091";
    private bool acceptNextAlert = true;

    private Process? _blazorProcess;

    [OneTimeSetUp]
    public void StartBlazorServer()
    {
        var webProjectPath = Path.GetFullPath(Path.Combine(
            Assembly.GetExecutingAssembly().Location,
            "../../../../../../src/DatesAndStuff.Web/DatesAndStuff.Web.csproj"
            ));

        var webProjFolderPath = Path.GetDirectoryName(webProjectPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            //Arguments = $"run --project \"{webProjectPath}\"",
            Arguments = "dotnet run --no-build",
            WorkingDirectory = webProjFolderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        _blazorProcess = Process.Start(startInfo);

        // Wait for the app to become available
        var client = new HttpClient();
        var timeout = TimeSpan.FromSeconds(30);
        var start = DateTime.Now;

        while (DateTime.Now - start < timeout)
        {
            try
            {
                var result = client.GetAsync(BaseURL).Result;
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }
        }
    }

    [OneTimeTearDown]
    public void StopBlazorServer()
    {
        if (_blazorProcess != null && !_blazorProcess.HasExited)
        {
            _blazorProcess.Kill(true);
            _blazorProcess.Dispose();
        }
    }

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    [TestCase(0)]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    public void Person_SalaryIncrease_ShouldIncrease(int percentage)
    {
        // Arrange
        driver.Navigate().GoToUrl(BaseURL);
        driver.Navigate().Refresh();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        try
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='PersonPageNavigation']")))
                .Click();

            wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            var input = wait.Until(
                ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            input.Clear();

            input = wait.Until(
                ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            input.SendKeys(percentage.ToString());
        }
        catch (StaleElementReferenceException e)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='PersonPageNavigation']")))
                .Click();

            wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            var input = wait.Until(
                ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            input.Clear();

            input = wait.Until(
                ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
            input.SendKeys(percentage.ToString());
        }
        finally
        {
            var salaryLabel =
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='DisplayedSalary']")));
            var originalSalary = double.Parse(salaryLabel.Text);
            var expectedSalary = originalSalary + (originalSalary * (percentage / 100.0));

            // Act
            var submitButton =
                wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']")));
            submitButton.Click();

            // Assert
            var salaryAfterSubmissionLabel =
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='DisplayedSalary']")));
            var salaryAfterSubmission = double.Parse(salaryAfterSubmissionLabel.Text);
            salaryAfterSubmission.Should().BeApproximately(expectedSalary, 0.001);
        }
    }
        
    [Test]
    public void Person_InvalidSalaryDecrease_ShouldAlert()
    {
        // Arrange
        driver.Navigate().GoToUrl(BaseURL);
        driver.Navigate().Refresh();
        driver.FindElement(By.XPath("//*[@data-test='PersonPageNavigation']")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var input = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
        input.Clear();
        
        input.SendKeys("-20");

        // Act
        var submitButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']")));
        submitButton.Click();


        // Assert

        var errorMessages = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[@class='validation-message']"))).ToList();
        errorMessages.Should().HaveCount(2);
    }
    
    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (acceptNextAlert)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
            return alertText;
        }
        finally
        {
            acceptNextAlert = true;
        }
    }
}