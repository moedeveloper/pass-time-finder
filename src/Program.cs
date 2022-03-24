#nullable disable

List<Person> _persons = new List<Person>();
init();

void init()
{
    Console.WriteLine("Press 1 for testing or 2 to run the App: ");
    var input = Convert.ToInt32(Console.ReadLine());

    if( input == 1){
        testApp();
    }

    if( input == 2){
        runApp();
    }

    RunProcess();
}

void testApp(){
    BookingOptions.Email = "test@test.com";
    BookingOptions.EndDate = "2022-11-25";
    BookingOptions.Expedition = "";
    BookingOptions.Lan = "vastragotaland";
    BookingOptions.StartDate = DateTime.Today.ToString("yyyy-M-d");
    BookingOptions.NumberOfPersons = 2;
    BookingOptions.PhoneNumber = "0705555562";

    _persons = new List<Person>()
    {
        new Person {
            FirstName = "Sara",
            LastName = "Charafeddin"
        },
        new Person {
            FirstName = "Celine",
            LastName = "mortada"
        }
    };
}

void runApp(){
    Console.WriteLine("Number of Persons Between 1-8, Press Enter for default (1) ");
    var numberOfPersonsInput = Console.ReadLine();
    
    BookingOptions.NumberOfPersons = (numberOfPersonsInput == string.Empty) 
    ? 1
    : Convert.ToInt32(numberOfPersonsInput);
    
    while(BookingOptions.NumberOfPersons > 8 
        || BookingOptions.NumberOfPersons <= 0)
    {
        Console.WriteLine("Not valid number, Only 8 person allowed.");
        BookingOptions.NumberOfPersons = Convert.ToInt32(Console.ReadLine());
    }

    for (int i = 1; i <= BookingOptions.NumberOfPersons; i++)
    {
        var person = new Person();
        
        while(person.FirstName  == string.Empty)
        {
            Console.WriteLine($"Enter First name for person {i}: ");
            person.FirstName = Console.ReadLine();
        }

        while(person.LastName == string.Empty)
        {
            Console.WriteLine($"Enter Last name for person {i}: ");
            person.LastName = Console.ReadLine();
        }

        _persons.Add(person);

        Console.WriteLine(_persons.Count());
    }

    while(BookingOptions.Lan == string.Empty)
    {
        Console.WriteLine($"Enter Swedish Lan, example: vastragotland - default: (vastragotaland)");
        var lanInput = Console.ReadLine();
        BookingOptions.Lan = (lanInput == string.Empty) ? "vastragotaland" : Console.ReadLine();
    }

    // TODO capital the first letter.
    Console.WriteLine($"Enter Expedition, example: Göteborg - Default is all Expeditions.");
    BookingOptions.Expedition = Console.ReadLine();

    // TODO validate email
    while(BookingOptions.Email == string.Empty)
    {
        Console.WriteLine($"Enter Email adress: ");
        BookingOptions.Email = Console.ReadLine();
    }

    // TODO validate phonenumber
    while(BookingOptions.PhoneNumber == string.Empty)
    {
        Console.WriteLine($"Enter PhoneNumber: ");
        BookingOptions.PhoneNumber = Console.ReadLine();
    }

    BookingOptions.StartDate = DateTime.Today.ToString("yyyy-M-d");

    Console.WriteLine(BookingOptions.StartDate);

    while(BookingOptions.EndDate == string.Empty)
    {
        Console.WriteLine($"Enter EndDate ex: 2022-03-25 : ");
        BookingOptions.EndDate = Console.ReadLine();
    }
}

void RunProcess()
{
    // Define an array with two AutoResetEvent WaitHandles.
    WaitHandle[] waitHandles = new WaitHandle[]
    {
        new AutoResetEvent(false)
    };

    // wait until the tasks are completed.
    ThreadPool.QueueUserWorkItem(new WaitCallback(DoTask), waitHandles[0]);
    WaitHandle.WaitAll(waitHandles);
}

void DoTask(Object state)
{
    var signaled = false;
    AutoResetEvent autoResetEvent = (AutoResetEvent) state;
    var searchMotor = new SearchMotor();
    do{
        Console.WriteLine("searching for first available slot...");
        signaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(10));
        
        var enabledAlert = searchMotor.SearchForFirstAvailableDate();

        if(enabledAlert)
        {
            Console.WriteLine("Thread is sleeping...");
            Thread.Sleep(TimeSpan.FromSeconds(30));
        }

        if (searchMotor.FoundAndSelectedTimeSlot())
        {
            var done = searchMotor.FillPersonInformation(_persons);

            if (done){            
                signaled = autoResetEvent.Set();
            }
        }
    } while (!signaled);
}
