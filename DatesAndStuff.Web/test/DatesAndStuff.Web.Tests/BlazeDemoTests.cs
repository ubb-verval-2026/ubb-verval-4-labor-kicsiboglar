using FluentAssertions;
using OpenQA.Selenium;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace DatesAndStuff.Web.Tests
{
    public class BlazeDemoTests
    {
        private IWebDriver driver;
        private StringBuilder verificationErrors;
        private const string BaseURL = "https://blazedemo.com/";
        private bool acceptNextAlert = true;
        
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
        [TestCase("Mexico City", "Dublin", 3)]
        public void SearchFlight_BetweenTwoCities_ShouldBeAtLeastNr(String firstCity, String secondCity, int nr)
        {
            // Arrange
            driver.Navigate().GoToUrl(BaseURL);
            driver.Navigate().Refresh();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            //var selector = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@name='fromPort']")));
            var firstCitySelector = new SelectElement(driver.FindElement(By.Name("fromPort")));
            firstCitySelector.SelectByValue(firstCity);
            
            var secondCitySelector = new SelectElement(driver.FindElement(By.Name("toPort")));
            secondCitySelector.SelectByValue(secondCity);
            
            // Act
            driver.FindElement(By.XPath("//*[@type='submit']")).Click();
            
            // Assert
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("table")));
            
            var flightList = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath("//*[@type='submit']"))).ToList();
            flightList.Should().HaveCountGreaterThanOrEqualTo(nr);
        }
    }
    
}