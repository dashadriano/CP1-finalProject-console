using System;
using System.IO;

class ConsoleDraft {
     // class attributes
    static UserRole role = UserRole.None; // user role variable
    static bool running = true; // app state bool checker

    // user roles via enum
    enum UserRole { 
        None,
        Employee,
        Admin
    }

    // app logic
    static void Main() {
        while (running) { // user opts out on their own through `running` control. if not exiting, attempts to log in
            if (InitMenu() ==  true) {
                if (role == UserRole.Admin) {  
                    Header();
                    // showAdminMenu(); -- method to be created, actual app logic
                    Console.WriteLine("Logged in as admin."); // temp console print
                    running = false; // temp stop
                } else if (role == UserRole.Employee) {  
                    Header();
                    // showEmpMenu(); -- method to be created, actual app logic
                    Console.WriteLine("Logged in as employee."); // temp console print
                    running = false; // temp stop
                } 
            } else {
                Console.WriteLine("Program terminated.");
                running = false;
            }
        }
    }

    // header for every page state
    static void Header() {
        Console.Clear();
        Console.WriteLine("Employee Salary Computation Management App");
        Space();
    }

    // buffer to prevent instant page state flick 
    static void Buffer() {
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }

    // initial menu method
    static bool InitMenu() {
        bool logLoop = true; // logging in loop checker

        // app logic
        Header();
        Console.WriteLine("Choices:");
        Console.WriteLine("1. Login");
        Console.WriteLine("2. Exit");
        Space();
        Console.Write("> ");

        // input checker for int validity
        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice)) {
            Space();
            
            Console.WriteLine("Invalid input. Please enter a valid number:");
            Console.Write("> ");
        }

        switch (choice) {  
            case 1:
                while (logLoop) {
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
                            return InitMenu();
                        } else {
                            Space();
                            Console.WriteLine("Invalid input. Please enter 'Y' or 'N'.");
                            Buffer();
                        }
                    }
                }
                break;
            case 2:
                return false;  
            default:
                Console.WriteLine("Invalid input. Only enter 1 or 2.");
                break;
        }
        return false;
    }

    // login logic method
    static bool Login() {
        Header();
        Console.Write("Enter password: ");
        string password = ReadPassword();
        Space();

        if (CheckPasswordValidity(password) != UserRole.None) {
            if (role == UserRole.Employee) {
                Console.WriteLine("Test success: Logged in as Employee.");
                return true;
            }
            else if (role == UserRole.Admin) {
                Console.WriteLine("Test success: Logged in as Admin.");
                return true;
            }
        } else {
                Console.WriteLine("Test success: Log in prevented.");
                return false;
        }

        return false;
    }

    // console spacing method
    static void Space() {
        Console.WriteLine("");
    }

    // password masking method
    static string ReadPassword() {
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

    // password validity checker method
    static UserRole CheckPasswordValidity(string enteredPassword) {
        string empFilePath = "passwords/employeePasswords.txt";
        string adminFilePath = "passwords/adminPasswords.txt";

        if (File.Exists(empFilePath)) {
            foreach (var password in File.ReadLines(empFilePath)) {
                if (password.Trim() == enteredPassword.Trim()) {
                    role = UserRole.Employee;
                    return UserRole.Employee; 
                }
            }
        }

        if (File.Exists(adminFilePath)) {
            foreach (var password in File.ReadLines(adminFilePath)) {
                if (password.Trim() == enteredPassword.Trim()) {
                    role = UserRole.Admin;
                    return UserRole.Admin; 
                }
            }
        }

        return UserRole.None;  
    }
}
