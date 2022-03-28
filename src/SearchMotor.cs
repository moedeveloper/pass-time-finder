using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class SearchMotor
{
    private readonly IWebDriver _driver;
    private readonly int _numberOfPersons;
    public SearchMotor(){
        _numberOfPersons = BookingOptions.NumberOfPersons;
        var options = new ChromeOptions();
        options.AddArgument("--disable-blink-features=AutomationControlled");
        
        _driver = new ChromeDriver(options: options);
        _driver.Navigate().GoToUrl($"https://bokapass.nemoq.se/Booking/Booking/Index/{BookingOptions.Lan}");
        PreSetup();
    } 
    private void PreSetup()
    {
        Console.WriteLine("Getting started...");    
        // click the first button
        // https://bokapass.nemoq.se/Booking/Booking/Index/vastragotaland
        _driver.FindElement(By.XPath("//*[@id=\"Main\"]/div[2]/div[1]/div/form/div[2]/input"))
        .Click();

        Task.Delay(500);

        // fill second page Behandling av personuppgifter
        // accept agreement and number of presons
        _driver.FindElement(By.XPath("//*[@id=\"AcceptInformationStorage\"]"))
        .Click();

        // find dropdown
        var staticDropDown = _driver.FindElement(By.Id("NumberOfPeople"));

        var select = new SelectElement(staticDropDown);
        select.SelectByValue($"{_numberOfPersons}");

        // click for next page
        _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[2]/input"))
        .Click();

        // confirm living in sweden
        for (int i = 0; i < _numberOfPersons; i++)
        {
            
            var name = $"ServiceCategoryCustomers[{i}].ServiceCategoryId";
            var radioButton = _driver.FindElement(By.Name(name));

            var radioButtonValue = radioButton.GetAttribute("value");

            if(radioButtonValue == "2")
            {
                radioButton.Click();
            }
        }

        // next after confirmation 
        _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[2]/input"))
        .Click();
    }

    private void setExpedition()
    {
        var expedictionDropDown = _driver.FindElement(By.Id("SectionId"));
        var selected = new SelectElement(expedictionDropDown);
        selected.SelectByText(BookingOptions.Expedition);
    }

    public bool SearchForFirstAvailableDate()
    {
        // expedition is set
        if(BookingOptions.Expedition != string.Empty)
        {
            setExpedition();
        }

        //default run first available time
        _driver.FindElement(By.Name("TimeSearchFirstAvailableButton"))
        .Click();

        return errorOnPage();
    }

    public bool FoundAndSelectedTimeSlot()
    {
        var firstAvailableDate = _driver.FindElement(By.XPath("//*[@id=\"datepicker\"]")).GetAttribute("value");
        Console.WriteLine($"firstAvailableDate: {firstAvailableDate}");
        DateTime firstAvailableDateTime = Convert.ToDateTime(firstAvailableDate);
        DateTime endDateTime = Convert.ToDateTime(BookingOptions.EndDate);

        if (firstAvailableDateTime > endDateTime)
                return false;

        if (availableSlot())
        {
            // go further
            // klick next to reserve slot and start filling info on next step
            _driver.FindElement(By.XPath("//*[@id=\"booking-next\"]"))
            .Click();

            if (errorOnPage())
                return false;

            return true;
        }
        // error on page or not slot found
        return false;
    }

    private bool errorOnPage()
    {
        try
        {
            return _driver.FindElement(By.ClassName("alert-error")).Enabled;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
    private bool availableSlot()
    {
        try
        {
            // select first time slot
            var cellKey = "//*[contains(@aria-label,\"202\")]";
            _driver.FindElement(By.XPath(cellKey))
            .Click();
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public bool FillPersonInformation(List<Person> persons)
    {
        try
        {
             for (int i = 0; i < persons.Count(); i++)
            {
                var firstnameInputId = $"//*[@id=\"Customers_{i}__BookingFieldValues_0__Value\"]";
                var lastnameInputId = $"//*[@id=\"Customers_{i}__BookingFieldValues_1__Value\"]";
                var passCheckBox = $"//*[@id=\"Customers_{i}__Services_0__IsSelected\"]";

                // var firstnameInputId = $"Customers_{i}__BookingFieldValues_0__Value";
                // var lastnameInputId = $"Customers_{i}__BookingFieldValues_1__Value";
                // var passCheckBox = $"Customers_{i}__Services_0__IsSelected";

                Console.WriteLine(firstnameInputId);

                _driver.FindElement(By.XPath(firstnameInputId))
                .SendKeys(persons[i].FirstName);

                _driver.FindElement(By.XPath(lastnameInputId))
                .SendKeys(persons[i].LastName);

                _driver.FindElement(By.XPath(passCheckBox))
                .Click();
            }
            
            Thread.Sleep(900);
            // click next when done!
            _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[2]/input"))
            .Click();

            // Confirmation Viktig information next
            _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div/input"))
            .Click();
            
            Thread.Sleep(900);

            // Fill personal info
            _driver.FindElement(By.Id("EmailAddress")).SendKeys(BookingOptions.Email);
            _driver.FindElement(By.Id("ConfirmEmailAddress")).SendKeys(BookingOptions.Email);
            _driver.FindElement(By.Id("PhoneNumber")).SendKeys(BookingOptions.PhoneNumber);
            _driver.FindElement(By.Id("ConfirmPhoneNumber")).SendKeys(BookingOptions.PhoneNumber);

            _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[1]/div[5]/div/label[1]")).Click();
            _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[1]/div[5]/div/label[2]")).Click();
            _driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[1]/div[6]/div/label[1]")).Click();
            
            // last steps!!! they can be done manually!
            //_driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[1]/div[6]/div/label[2]")).Click();
            //_driver.FindElement(By.XPath("//*[@id=\"Main\"]/form/div[2]/input")).Click();

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}