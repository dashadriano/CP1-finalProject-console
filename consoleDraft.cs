using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class ConsoleDraft {
     // class attributes
    static UserRole role = UserRole.None; // user role variable
    static bool running = true; // app state bool checker
    static string empFilePath = "passwords/employeePasswords.txt";
    static string adminFilePath = "passwords/adminPasswords.txt";

    enum UserRole { // user roles via enum
        None,
        Employee,
        Admin
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
            Console.WriteLine("1. View list of employees");
            Console.WriteLine("2. Edit employee data");
            Console.WriteLine("3. Add an employee");
            Console.WriteLine("4. Remove an employee");
            Console.WriteLine("5. Add an admin");
            Console.WriteLine("6. Remove an admin");
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
                    Console.WriteLine("Viewing list of employees!");
                    Space();
                    Buffer();
                    break;
                case 2:
                    Header();
                    Console.WriteLine("Editing list of employees!");
                    Space();
                    Buffer();
                    break;
                case 3: // to add other data such as gross salary and logic for deductions (taxes, etc)
                    Header();
                    Console.WriteLine("Adding an employee!");
                    Space();
                    
                    // regulates password asking loop until proper password is inputted
                    bool askingForEmpPassword = true;

                    while (askingForEmpPassword) {
                        Console.Write("New employee password: ");
                        string newEmpPw = Console.ReadLine();

                        if (AddPassword(newEmpPw, empFilePath)) {// should include other info creation about employee after
                            askingForEmpPassword = false;
                        } else {
                            askingForEmpPassword = true;
                        }
                    }
    
                    Space();
                    Buffer();
                    break;
                case 4:
                    Header();
                    Console.WriteLine("Removing an employee!");
                    Space();
                    Buffer();
                    break;
                case 5:
                    Header();
                    Console.WriteLine("Adding an admin!"); 
                    Space();
                    
                     // regulates password asking loop until proper password is inputted
                    bool askingForAdminPassword = true;

                    while (askingForAdminPassword) {
                        Console.Write("New admin password: ");
                        string newAdminPw = Console.ReadLine();

                        if (AddPassword(newAdminPw, adminFilePath)) {// should include other info creation about employee after
                            askingForAdminPassword = false;
                        } else {
                            askingForAdminPassword = true;
                        }
                    }

                    Space();
                    Buffer();
                    break;
                case 6:
                    Header();
                    Console.WriteLine("Removing an admin!"); 
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

    static bool AddPassword(string newPassword, string filePath) {
        string hashedPassword = HashPassword(newPassword); 

        // checks if there is an existing user with the same password to prevent same passwords
        // although a security liability, it is okay as the only role with adding privileges is an admin      
        if (File.Exists(filePath)) {
            foreach (var existingPw in File.ReadLines(filePath)) {
                if (existingPw.Trim() == hashedPassword) {
                    Console.WriteLine("A user with this password already exists!");
                    return false; 
                }
            }
        }

        // adds new user password to textfile
        File.AppendAllText(filePath, hashedPassword + Environment.NewLine);
        Space();
        Console.WriteLine("Password added successfully!");
        return true;
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
        
        if (File.Exists(empFilePath)) {
            foreach (var ePw in File.ReadLines(empFilePath)) {
                if (ePw.Trim() == hashedEnteredPassword.Trim()) {
                    role = UserRole.Employee;
                    return UserRole.Employee; 
                }
            }
        }

        if (File.Exists(adminFilePath)) {
            foreach (var aPw in File.ReadLines(adminFilePath)) {
                if (aPw.Trim() == hashedEnteredPassword.Trim()) {
                    role = UserRole.Admin;
                    return UserRole.Admin; 
                }
            }
        }

        return UserRole.None;  
    }
}
