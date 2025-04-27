using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

class ConsoleDraft {
     // class attributes
    static UserRole role = UserRole.None; // user role variable (holder)
    static bool running = true; // app state bool checker
    static string empDataFilePath = "data/employeeData.txt";
    static string empPasswordsFilePath = "passwords/employeePasswords.txt";
    static string adminFilePath = "passwords/adminPasswords.txt";

    enum UserRole { // user roles via enum
        None,
        Employee,
        Admin
    }

    enum Department { // department roles via enum
        HUMAN_RESOURCES,
        FINANCE,
        OPERATIONS,
        INFORMATION_TECHNOLOGY,
        CUSTOMER_SERVICE
    }

    static void Main() { // app logic
        while (running) { // user opts out on their own through `running` control. if not exiting, attempts to log in
            if (InitMenu() ==  true) {
                if (role == UserRole.Admin) {  
                    AdminMenu();
                } else if (role == UserRole.Employee) {  
                    Header();
                    // showEmpMenu(); -- method to be created, actual app logic
                    Console.WriteLine("Logged in as employee."); // temp console print
                    Space();
                    Buffer();
                    running = false; // temp stop
                } 
            } else {
                Console.Clear();
                Space();
                Console.WriteLine("Program terminated.");
                Space();
                running = false;
            }
        }
    }

    static void Space() { // console spacing method
        Console.WriteLine("");
    }

    static void Header() { // header for every page state
        Console.Clear();
        Console.WriteLine("Employee Salary Computation Management App");
        Space();
    }

    static void Buffer() { // buffer to prevent instant page state flick 
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
   
    static void AdminMenu() { // admin menu view logic
        bool inAdminView = true;

        while (inAdminView) { // actual logic to be added for most parts
            Header();
            Console.WriteLine("Choices:");
            Console.WriteLine("1. View list of employees"); // completed
            Console.WriteLine("2. Edit employee data");
            Console.WriteLine("3. Add an employee"); // completed
            Console.WriteLine("4. Remove an employee"); 
            Console.WriteLine("5. Add an admin"); // completed
            Console.WriteLine("6. Remove an admin"); // completed
            Console.WriteLine("7. Exit");
            Space();
            Console.Write("> ");

            // input checker for int validity
            int adminMenuChoice;
            while (!int.TryParse(Console.ReadLine(), out adminMenuChoice)) {
                Space();
                
                Console.WriteLine("Invalid input. Please enter a valid number:");
                Console.Write("> ");
            }
            
            switch(adminMenuChoice) {
                case 1: 
                    Header();
                    if (new FileInfo(empDataFilePath).Length != 0) {
                        Console.WriteLine("Employee list:");
                        Space();
                        ViewEmployees();
                    } else {
                        Console.WriteLine("No employees in record!");
                    }
                    Space();
                    Buffer();
                    break;
                case 2:
                    Header();
                    Console.WriteLine("Editing list of employees!");
                    // METHOD TO BE CREATED
                    Space();
                    Buffer();
                    break;
                case 3: 
                    Header();
                    Console.WriteLine("Adding an employee!");
                    Space();
                    AddEmployee(); // to add other data such as gross salary and logic for deductions (taxes, etc)
                    Space();
                    Buffer();
                    break;
                case 4:
                    Header();
                    Console.WriteLine("Removing an employee!"); 
                    Space();
                    // METHOD TO BE CREATED
                    Space();
                    Buffer();
                    break;
                case 5:
                    Header();
                    Console.WriteLine("Adding an admin!"); 
                    Space();
                    AskForPasswordAndAdd("Admin", adminFilePath);
                    Space();
                    Buffer();
                    break;
                case 6:
                    Header();
                    Console.WriteLine("Removing an admin!"); 
                    Space();
                    AskForAdminPasswordAndRemove("Admin", adminFilePath);
                    Space();
                    Buffer();
                    break;
                case 7:
                    Header();
                    Console.WriteLine("Exiting as admin.");
                    Space();
                    Buffer();
                    inAdminView = false;
                    break;
            }
        }
    }

    static void ViewEmployees() { // viewing employees method
        string[] employees = File.ReadAllLines(empDataFilePath);
        int empCount = 1;

        foreach (string line in employees) {
            string[] data = line.Split('|');

            if (data.Length < 6) {
                Space();
                Console.WriteLine("Malformed employee entry found. Skipping...");
                Space();
                continue;
            }

            Console.WriteLine($"--- Employee ID {empCount} ---");
            Console.WriteLine($"Name: {data[0]}");
            Console.WriteLine($"Age: {data[1]}");
            Console.WriteLine($"Gender: {data[2]}");
            Console.WriteLine($"Address: {data[3]}");
            Console.WriteLine($"Department: {data[4]}");
            Space();

            empCount++;
        }
    }

    static void AddEmployee() { // method for adding employees
        // to add other data such as gross salary and logic for deductions (taxes, etc)
        string name = AskForName();
        int age = AskForAge();
        string gender = AskForGender();
        string address = AskForAddress();
        string department = AskForDepartment();
        string password = AskForPasswordAndAdd("Employee", empPasswordsFilePath);

        string employeeData = $"{name}|{age}|{gender}|{address}|{department}|{password}";

        File.AppendAllText(empDataFilePath, employeeData + Environment.NewLine);
        Space();
        Console.WriteLine("Employee added successfully!");
    }

    static string AskForName() { // asks for name to feed add employee method
        Console.Write("Name: ");
        string name = Console.ReadLine();

        // capitalize the first letter of each name
        string[] words = name.Split(' ');
        for (int i = 0; i < words.Length; i++) {
            if (words[i].Length > 0) {
                words[i] = Char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }

        return string.Join(" ", words);
    }

    static int AskForAge() { // asks for age to feed to add employee method
        int age;
        while (true) {
            Console.Write("Age: ");
            if (int.TryParse(Console.ReadLine(), out age) && age >= 18 && age <= 60) {
                return age;
            } else {
                Console.WriteLine("Invalid age. Please enter a valid age between 18 and 60.");
                Space();
                Buffer();
                Console.Clear();
                continue;
            }
        }
    }

    static string AskForGender() { // asks for gender to feed to add employee method
        string gender;
        while (true) {
            Console.Write("Gender (male, female, or other): ");
            gender = Console.ReadLine().ToLower();
            if (gender == "male" || gender == "female" || gender == "other") {
                gender = Char.ToUpper(gender[0]) + gender.Substring(1); // capitalizes first letter
                return gender;
            } else {
                Console.WriteLine("Invalid gender. Please enter 'male', 'female', or 'other'.");
                Space();
                Buffer();
                Console.Clear();
            }
        }
    }

    static string AskForAddress() { // asks for address to feed to add employee method
        string address;
        while (true) {
            Console.Write("Address (e.g., 123 Main St, City, State): ");
            address = Console.ReadLine();

            // ensures the address has at least one number and space, and then letters (for street names)
            if (System.Text.RegularExpressions.Regex.IsMatch(address, @"^\d+\s[A-Za-z]+(?:\s[A-Za-z]+)*,?\s?[A-Za-z\s]+(?:,\s?[A-Za-z\s]+)*$")) {
                return address;  

            } else if (string.IsNullOrWhiteSpace(address)) {
                Space();
                Console.WriteLine("Address cannot be empty.");
                Space();
                Buffer();
                Header();
                continue; 
            } else {
                Space();
                Console.WriteLine("Invalid address. Please enter a valid address (e.g., 123 Main St, City, State).");
                Space();
                Buffer();
                Header();
                continue;
            }
        }
    }

    static string AskForDepartment() { // asks for department to feed to add employee method
        while (true) {
            Space();
            Console.WriteLine("Available Departments:");
            foreach (string dept in Enum.GetNames(typeof(Department))) {
                Console.WriteLine($"- {dept.Replace("_", " ")}");
            }
            Space();

            Console.Write("Department: ");
            string input = Console.ReadLine().Replace(" ", "_").ToUpper();

            if (Enum.TryParse(input, out Department selectedDept)) {
                return selectedDept.ToString().Replace("_", " ");
            } else {
                Console.WriteLine("Invalid department. Please try again.");
                Space();
                Buffer();
                Console.Clear();
            }
        }
    }

    static string AskForPasswordAndAdd(string role, string filePath) { // asks for password, then calls add password method
        bool askingForPassword = true;
        while (askingForPassword) {
            Console.Write($"New {role} password: ");
            string pwToBeAdded = Console.ReadLine();

            if (AddPassword(pwToBeAdded, filePath)) {
                return HashPassword(pwToBeAdded);
            } else {
                continue;
            }
        }
        return ""; // failsafe, compilation error fix
    }

    static void AskForAdminPasswordAndRemove(string role, string filePath) { // asks for password to be removed, then calls remove password method
        int passwordCount = File.ReadLines(adminFilePath).Count();
        if (passwordCount > 1) {
            bool askingForPassword = true;
            while (askingForPassword) {
                Console.Write($"{role} password to be removed: ");
                string pwToBeRemoved = Console.ReadLine();

                RemovePassword(pwToBeRemoved, filePath);
                askingForPassword = false;
            }
        } else {
            Console.WriteLine("Cannot remove all instances of admin users. Only one admin user remains.");
            Space();
        }
    }

    static bool AddPassword(string newPassword, string filePath) { // adding password logic
        string hashedPassword = HashPassword(newPassword); 

        // checks if there is an existing user with the same password to prevent same passwords
        // although a security liability, it is okay as the only role with adding privileges is an admin      
        foreach (var existingPw in File.ReadLines(filePath)) {
            if (existingPw.Trim() == hashedPassword) {
                Space();
                Console.WriteLine("A user with this password already exists!");
                return false;
            }
        }

        // adds new user password to textfile
        File.AppendAllText(filePath, hashedPassword + Environment.NewLine);
        return true;
    }

    static void RemovePassword(string passwordToDelete, string filePath) { // removing password logic
        string hashedPassword = HashPassword(passwordToDelete); 

        var updatedLines = new List<string>();
        bool passwordFound = false;

        foreach (var existingPw in File.ReadAllLines(filePath)) {
            if (existingPw.Trim() != hashedPassword) {
                updatedLines.Add(existingPw); 
            } else {
                passwordFound = true; 
            }
        }

        if (passwordFound) {
            File.WriteAllLines(filePath, updatedLines); // rewrites storage text file without the password to be removed
            Space();
            Console.WriteLine("Password removed successfully!");
            Space();
        } else {
            Space();
            Console.WriteLine("No users found with the password entered.");
            Space();
        }
    }

    static string HashPassword(string acceptedPassword) { // SHA-256 hashing logic
        using (SHA256 hash = SHA256.Create()) {
            // creation of hash and final string compiler
            byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(acceptedPassword));
            StringBuilder sb = new StringBuilder();
            
            // appending to StringBuilder obj to store hash as string
            foreach (var b in bytes) {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    static bool InitMenu() { // initial menu method
        bool logLoop = true; // logging in loop checker

        while (logLoop) {
            Header();
            Console.WriteLine("Choices:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Exit");
            Space();
            Console.Write("> ");

            // input checker for int validity
            int initMenuChoice;
            while (!int.TryParse(Console.ReadLine(), out initMenuChoice)) {
                Space();
                
                Console.WriteLine("Invalid input. Please enter a valid number:");
                Console.Write("> ");
            }

            switch (initMenuChoice) {  
                case 1:
                    if (Login()) {
                        logLoop = false;
                        return true;  
                    } else {
                        // to include attempt tracker logic here, increments at the start of this code block 
                        // since being here indicates that the user failed a login attempt
                        Space();
                        Console.WriteLine("Continue attempts? [Y/N]");
                        Console.Write("> ");

                        string userInput = Console.ReadLine().Trim().ToUpperInvariant();
                        if (userInput == "Y") {
                            continue;
                        } else if (userInput == "N") {
                            continue;
                        } else {
                            Space();
                            Console.WriteLine("Invalid input. Please enter 'Y' or 'N'.");
                            Buffer();
                        }
                    }
                    break;
            case 2:
                return false;  
            default:
                Console.WriteLine("Invalid input. Only enter 1 or 2.");
                break;
            }
        }
        return false;
    }

    static bool Login() { // login logic method
        Header();
        Console.Write("Enter password: ");
        string password = ReadPassword();
        Space();

        if (CheckPasswordValidity(password) != UserRole.None) {
            Space();
            Console.WriteLine($"Logged in as {role}!");
            Space();
            Buffer();
            return true;
        } else {
                Console.WriteLine("Test success: Log in prevented.");
                return false;
        }
    }

    static string ReadPassword() { // password masking method
        string password = "";
        ConsoleKeyInfo key;

        while (true) {
            key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter) {
                break;
            }

            if (key.Key == ConsoleKey.Backspace) {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else {
                password += key.KeyChar;
                Console.Write("*");
            }
        }

        return password;
    }

    static UserRole CheckPasswordValidity(string enteredPassword) { // password validity checker method
        string hashedEnteredPassword = HashPassword(enteredPassword);
        
        foreach (var ePw in File.ReadLines(empPasswordsFilePath)) {
            if (ePw.Trim() == hashedEnteredPassword.Trim()) {
                role = UserRole.Employee;
                return UserRole.Employee; 
            }
        }

        foreach (var aPw in File.ReadLines(adminFilePath)) {
            if (aPw.Trim() == hashedEnteredPassword.Trim()) {
                role = UserRole.Admin;
                return UserRole.Admin; 
            }
        }

        return UserRole.None;  
    }
}
