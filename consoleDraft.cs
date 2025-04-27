using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

class ConsoleDraft
{
    static UserRole role = UserRole.None;
    static bool running = true;

    static string adminPassword = "admin123";
    static string employeePassword = "employee123";

    static string employeeDepartment = ""; // Track employee's department
    static DateTime loginTime;
    static DateTime logoutTime;

    static double employeeWage = 0.0;
    static Timer wageTimer; // Timer to handle wage credits

    static Dictionary<string, (DateTime loginTime, DateTime logoutTime, double hoursWorked)> employeeWorkHistory = new();

    static Dictionary<string, int> departmentCounts = new Dictionary<string, int>()


    {
        {"HUMAN RESOURCES DEPARTMENT (HR)", 0},
        {"FINANCE AND ACCOUNTING DEPARTMENT", 0},
        {"OPERATIONS MANAGEMENT", 0},
        {"INFORMATION TECHNOLOGY DEPARTMENT", 0},
        {"CUSTOMER SERVICE DEPARTMENT", 0}
    };

    class Employee
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
    }

    static List<Employee> employees = new List<Employee>(); // List to hold employees

    const int maxPerDepartment = 20;

    enum UserRole
    {
        None,
        Employee,
        Admin
    }

    static void Main()
    {
        while (running)
        {
            if (InitMenu())
            {
                if (role == UserRole.Admin)
                {
                    AdminMenu();
                }
                else if (role == UserRole.Employee)
                {
                    EmployeeMenu();
                }
            }
            else
            {
                Console.Clear();
                Space();
                Console.WriteLine("Program terminated.");
                Space();
                running = false;
            }
        }
    }

    static void Space() => Console.WriteLine("");

    static void Header()
    {
        Console.Clear();
        Console.WriteLine("Employee Salary Computation Management App");
        Space();
    }

    static void Buffer()
    {
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    static void AdminMenu()
    {
        bool inAdminView = true;

        while (inAdminView)
        {
            Header();
            Console.WriteLine("Choices:");
            Console.WriteLine("1. View list of employees");
            Console.WriteLine("2. View employee work history");
            Console.WriteLine("3. Edit employee data");
            Console.WriteLine("4. Add an employee");
            Console.WriteLine("5. Remove an employee");
            Console.WriteLine("6. Add an admin");
            Console.WriteLine("7. Remove an admin");
            Console.WriteLine("8. Move employee to a different department");
            Console.WriteLine("9. Log out");
            Space();
            Console.Write("> ");

            int adminMenuChoice;
            while (!int.TryParse(Console.ReadLine(), out adminMenuChoice))
            {
                Space();
                Console.WriteLine("Invalid input. Please enter a valid number:");
                Console.Write("> ");
            }

            switch (adminMenuChoice)
            {
                case 1:
                    ViewEmployees();
                    break;
                case 2:
                    ViewEmployeeWorkHistory();
                    break;
                case 3:
                    EditEmployeeData();
                    break;
                case 4:
                    AddEmployee(); // Admin can freely add employees
                    break;
                case 5:
                    RemoveEmployee(); // Admin can remove employees
                    break;
                case 6:
                    AddAdmin();
                    break;
                case 7:
                    RemoveAdmin();
                    break;
                case 9:
                    LogOut();
                    inAdminView = false; // Exit the admin menu after logout
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    Buffer();
                    break;
            }
        }
    }

    static void AddAdmin()
    {
        Header();
        Console.WriteLine("Adding a new admin.");
        Console.Write("Enter admin password: ");
        string newAdminPassword = Console.ReadLine().Trim();
        if (string.IsNullOrEmpty(newAdminPassword))
        {
            Console.WriteLine("Password cannot be empty.");
            Buffer();
            return;
        }

        // For simplicity, this is just adding the new admin's password to the list.
        // In real applications, you would store these in a database or encrypted storage.
        adminPassword = newAdminPassword;
        Console.WriteLine("✅ Admin added successfully.");
        Buffer();
    }

    static void RemoveAdmin()
    {
        Header();
        Console.WriteLine("Removing admin.");
        Console.Write("Enter admin password to remove: ");
        string password = Console.ReadLine().Trim();
        if (password == adminPassword)
        {
            Console.WriteLine("✅ Admin removed successfully.");
            adminPassword = string.Empty;  // Resetting password for removal
        }
        else
        {
            Console.WriteLine("Incorrect password. Admin not removed.");
        }
        Buffer();
    }

    static void ViewEmployees()
    {
        Header();
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees found.");
        }
        else
        {
            Console.WriteLine("Employee List:");
            foreach (var employee in employees)
            {
                Console.WriteLine($"Name: {employee.Name}, Age: {employee.Age}, Gender: {employee.Gender}, Address: {employee.Address}");
            }
        }
        Buffer();
    }

    static bool InitMenu()
    {
        bool logLoop = true;

        while (logLoop)
        {
            Header();
            Console.WriteLine("Choices:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Exit");
            Space();
            Console.Write("> ");

            int initMenuChoice;
            while (!int.TryParse(Console.ReadLine(), out initMenuChoice))
            {
                Space();
                Console.WriteLine("Invalid input. Please enter a valid number:");
                Console.Write("> ");
            }

            switch (initMenuChoice)
            {
                case 1:
                    if (Login())
                    {
                        logLoop = false;
                        return true;
                    }
                    else
                    {
                        Space();
                        Console.WriteLine("Continue attempts? [Y/N]");
                        Console.Write("> ");
                        string userInput = Console.ReadLine().Trim().ToUpperInvariant();

                        if (userInput == "N")
                            return false;
                    }
                    break;
                case 2:
                    return false;
                default:
                    Console.WriteLine("Invalid input. Only enter 1 or 2.");
                    Buffer();
                    break;
            }
        }

        return false;
    }

    static void AddEmployee()
    {
        Header();
        Console.WriteLine("Please enter the details of the new employee.");

        // Get employee details from the admin
        Console.Write("Enter Employee Name: ");
        string name = Console.ReadLine().Trim();

        Console.Write("Enter Employee Age: ");
        int age;
        while (!int.TryParse(Console.ReadLine(), out age))
        {
            Console.WriteLine("Invalid age. Please enter a valid number.");
            Console.Write("Enter Employee Age: ");
        }

        Console.Write("Enter Employee Gender (e.g., Male/Female): ");
        string gender = Console.ReadLine().Trim();

        Console.Write("Enter Employee Address: ");
        string address = Console.ReadLine().Trim();

        // Add new employee to the list
        employees.Add(new Employee { Name = name, Age = age, Gender = gender, Address = address });

        Console.WriteLine($"✅ Employee {name} added successfully.");
        Buffer();
    }

    static void EditEmployeeData()
    {
        Header();
        Console.WriteLine("Enter the name of the employee to edit:");

        string employeeName = Console.ReadLine().Trim();
        Employee employee = employees.Find(e => e.Name.Equals(employeeName, StringComparison.OrdinalIgnoreCase));

        if (employee == null)
        {
            Console.WriteLine("Employee not found.");
            Buffer();
            return;
        }

        Console.WriteLine($"Editing details for {employee.Name}.");
        
        Console.Write("Enter new Age (or press Enter to keep current): ");
        string ageInput = Console.ReadLine().Trim();
        if (!string.IsNullOrEmpty(ageInput) && int.TryParse(ageInput, out int age))
        {
            employee.Age = age;
        }

        Console.Write("Enter new Gender (or press Enter to keep current): ");
        string genderInput = Console.ReadLine().Trim();
        if (!string.IsNullOrEmpty(genderInput))
        {
            employee.Gender = genderInput;
        }

        Console.Write("Enter new Address (or press Enter to keep current): ");
        string addressInput = Console.ReadLine().Trim();
        if (!string.IsNullOrEmpty(addressInput))
        {
            employee.Address = addressInput;
        }

        Console.WriteLine($"✅ Employee {employee.Name}'s details have been updated.");
        Buffer();
    }

    static void RemoveEmployee()
    {
        Header();
        Console.WriteLine("Enter the name of the employee to remove:");

        string employeeName = Console.ReadLine().Trim();
        Employee employee = employees.Find(e => e.Name.Equals(employeeName, StringComparison.OrdinalIgnoreCase));

        if (employee == null)
        {
            Console.WriteLine("Employee not found.");
            Buffer();
            return;
        }

        employees.Remove(employee);
        Console.WriteLine($"✅ Employee {employee.Name} has been removed.");
        Buffer();
    }

    static bool Login()
    {
        Header();
        Console.Write("Enter password: ");
        string password = ReadPassword();
        Space();

        if (CheckPasswordValidity(password) != UserRole.None)
        {
            Space();
            Console.WriteLine($"Logged in as {role}!");
            Space();

            if (role == UserRole.Employee)
            {
                loginTime = DateTime.Now;
                Console.WriteLine($"You logged in at {loginTime}");

                // Start the wage timer to add 200 pesos every 15 seconds
                wageTimer = new Timer(AddWage, null, 0, 15000);  // 0ms delay to start immediately, and 15000ms (15 seconds) interval
            }

            Buffer();
            return true;
        }
        else
        {
            Console.WriteLine("Login failed.");
            Buffer();
            return false;
        }
    }

    static void LogOut()
    {
        if (role == UserRole.Employee)
        {
            logoutTime = DateTime.Now;
            TimeSpan workedDuration = logoutTime - loginTime;
            int totalWorkedSeconds = (int)(workedDuration.TotalSeconds + 0.5);

            employeeWorkHistory["Employee"] = (loginTime, logoutTime, totalWorkedSeconds);

            Console.WriteLine($"You logged out at {logoutTime}.");
            int totalSecondsRounded = (int)Math.Round((double)totalWorkedSeconds);
            int hoursWorked = totalSecondsRounded / 3600;
            int minutesWorked = (totalWorkedSeconds % 3600) / 60;
            int secondsWorked = totalWorkedSeconds % 60;

            Console.WriteLine($"You worked for {hoursWorked:D2}:{minutesWorked:D2}:{secondsWorked:D2}.");
            Console.WriteLine($"Your total wage for this period is {employeeWage:0.00} pesos.");

            // Stop the wage timer
            wageTimer?.Dispose();
        }

        role = UserRole.None;
        Console.WriteLine("Logged out successfully.");
        Buffer();
    }

    static string ReadPassword()
    {
        StringBuilder input = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace && input.Length > 0)
            {
                input.Length--; // Remove last character
                Console.Write("\b \b"); // Erase the '*' on the console
            }
            else
            {
                input.Append(key.KeyChar);
                Console.Write("*"); // Display '*' to mask input
            }
        }
        Console.WriteLine();
        return input.ToString();
    }

    static UserRole CheckPasswordValidity(string password)
    {
        if (password == adminPassword)
        {
            role = UserRole.Admin;
            return UserRole.Admin;
        }
        else if (password == employeePassword)
        {
            role = UserRole.Employee;
            return UserRole.Employee;
        }
        return UserRole.None;
    }

    static void AddWage(object? state)
    {
        // Add 200 pesos to the employee's wage every 15 seconds
        employeeWage += 200;
    }

    static void ViewEmployeeWorkHistory()
{
    Header();
    Console.WriteLine("Employee Work History:");

    if (employeeWorkHistory.Count == 0)
    {
        Console.WriteLine("No work history data found.");
    }
    else
    {
        foreach (var entry in employeeWorkHistory)
        {
            string employeeName = entry.Key;
            var (login, logout, hoursWorked) = entry.Value;

            int totalSeconds = (int)hoursWorked;
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            Console.WriteLine($"Name: {employeeName}");
            Console.WriteLine($"Login Time: {login}");
            Console.WriteLine($"Logout Time: {logout}");
            Console.WriteLine($"Total Worked Time: {hours:D2}:{minutes:D2}:{seconds:D2}");
            Console.WriteLine("----------------------------------------");
        }
    }

    Buffer();
}



    static void EmployeeMenu()
    {
        bool inEmployeeView = true;

        while (inEmployeeView)
        {
            Header();
            Console.WriteLine("Employee Menu:");
            Console.WriteLine("1. View department");
            Console.WriteLine("2. View wage");
            Console.WriteLine("3. Assign department");
            Console.WriteLine("4. Log out");
            Console.Write("> ");

            int employeeMenuChoice;
            while (!int.TryParse(Console.ReadLine(), out employeeMenuChoice))
            {
                Space();
                Console.WriteLine("Invalid input. Please enter a valid number:");
                Console.Write("> ");
            }

            switch (employeeMenuChoice)
            {
                case 1:
                    Console.WriteLine($"Your department: {employeeDepartment}");
                    Buffer();
                    break;
                case 2:
                    ViewEmployeeWage();
                    break;
                case 3:
                    AssignDepartment();
                    break;
                case 4:
                    LogOut();
                    inEmployeeView = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    Buffer();
                    break;
            }
        }
    }

  static void AssignDepartment()
{
    if (!string.IsNullOrEmpty(employeeDepartment))
    {
        Console.WriteLine($"❌ You are already assigned to {employeeDepartment}. You cannot change departments.");
        Buffer();
        return;
    }

    Console.WriteLine("Choose your department from the following options:");

    int index = 1;
    Dictionary<int, string> availableDepartments = new();

    foreach (var dept in departmentCounts)
    {
        if (dept.Value < 10)
        {
            availableDepartments[index] = dept.Key;
            Console.WriteLine($"{index}. {dept.Key}");
            index++;
        }
    }

    if (availableDepartments.Count == 0)
    {
        Console.WriteLine("⚠️ All departments are full. No departments available for assignment.");
        Buffer();
        return;
    }

    Console.Write("Enter the number of your department: ");
    int departmentChoice;
    while (!int.TryParse(Console.ReadLine(), out departmentChoice) || !availableDepartments.ContainsKey(departmentChoice))
    {
        Console.WriteLine("Invalid choice. Please try again.");
    }

    string chosenDepartment = availableDepartments[departmentChoice];
    employeeDepartment = chosenDepartment;
    departmentCounts[chosenDepartment]++;
    Console.WriteLine($"✅ You have been assigned to {employeeDepartment}.");
    Buffer();
}


    static void ViewEmployeeWage()
    {
        Console.WriteLine($"Your current wage is: {employeeWage:0.00} pesos.");
        Buffer();
    }
}
