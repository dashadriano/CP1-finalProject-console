using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

class ConsoleDraft {
    // Application state
    static UserRole role = UserRole.None;
    static bool running = true;
    static bool isLoggedIn = false;
    static string loggedInUserPasswordHash = "";

    // Data files paths
    static string empDataFilePath = "data/employeeData.txt";
    static string empPasswordsFilePath = "passwords/employeePasswords.txt";
    static string adminFilePath = "passwords/adminPasswords.txt";

    // In-memory employee list
    static List<Employee> employees = new List<Employee>();

    // User roles enumeration
    enum UserRole {
        None,
        Employee,
        Admin
    }

    // Departments enumeration
    enum Department {
        HUMAN_RESOURCES,
        FINANCE,
        OPERATIONS,
        INFORMATION_TECHNOLOGY,
        CUSTOMER_SERVICE
    }

    static void Main() {
        // Ensure data directories exist
        Directory.CreateDirectory(Path.GetDirectoryName(empDataFilePath) ?? "data");
        Directory.CreateDirectory(Path.GetDirectoryName(empPasswordsFilePath) ?? "passwords");
        Directory.CreateDirectory(Path.GetDirectoryName(adminFilePath) ?? "passwords");

        LoadEmployeeData();

        while (running) {
            if (InitMenu()) {
                if (role == UserRole.Admin) {
                    AdminMenu();
                } else if (role == UserRole.Employee) {
                    ShowEmployeeMenu();
                }
            } else {
                Console.Clear();
                Space();
                Console.WriteLine("Program terminated. Goodbye!");
                Space();
                running = false;
            }
        }
    }

    #region Data Loading and Saving

    static void LoadEmployeeData() {
        employees.Clear();
        if (!File.Exists(empDataFilePath)) {
            return;
        }

        foreach (var line in File.ReadAllLines(empDataFilePath)) {
            var parts = line.Split('|');
            if (parts.Length != 8)
                continue;

            if (!int.TryParse(parts[1], out int age)) age = 0;
            if (!double.TryParse(parts[6], out double wage)) wage = 0;
            if (!int.TryParse(parts[7], out int workSeconds)) workSeconds = 0;

            employees.Add(new Employee {
                Name = parts[0],
                Age = age,
                Gender = parts[2],
                Address = parts[3],
                Department = parts[4],
                PasswordHash = parts[5],
                Wage = wage,
                WorkSeconds = workSeconds
            });
        }
    }

    static void SaveEmployeeData() {
        var lines = employees.Select(e =>
            string.Join("|", new string[] {
                e.Name,
                e.Age.ToString(),
                e.Gender,
                e.Address,
                e.Department,
                e.PasswordHash,
                e.Wage.ToString("F2", CultureInfo.InvariantCulture),
                e.WorkSeconds.ToString()
            }));

        File.WriteAllLines(empDataFilePath, lines);
    }

    #endregion

    #region UI Helpers

    static void Header() {
        Console.Clear();
        Console.WriteLine("PayClock: Salary Management System");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine();
    }

    static void Space() => Console.WriteLine();

    static void Buffer() {
        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey(true);
    }

    #endregion

    #region Menus

    static bool InitMenu() {
        while (true) {
            Header();
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Exit");
            Space();
            Console.Write("> ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) {
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
                Buffer();
                continue;
            }

            switch (choice) {
                case 1:
                    if (Login()) return true;
                    Console.WriteLine("Continue login attempts? (Y/N): ");
                    var cont = Console.ReadLine().Trim().ToUpper();
                    if (cont != "Y") return false;
                    break;
                case 2:
                    return false;
                default:
                    Console.WriteLine("Please only enter 1 or 2.");
                    Buffer();
                    break;
            }
        }
    }

    static void AdminMenu() {
        bool inAdminView = true;

        while (inAdminView) {
            Header();
            Console.WriteLine("Admin Menu:");
            Console.WriteLine("1. View Employees");
            Console.WriteLine("2. Edit Employee");
            Console.WriteLine("3. Add Employee");
            Console.WriteLine("4. Remove Employee");
            Console.WriteLine("5. Add Admin");
            Console.WriteLine("6. Remove Admin");
            Console.WriteLine("7. Logout");
            Space();
            Console.Write("> ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) {
                Console.WriteLine("Invalid input. Please enter a number 1-7.");
                Buffer();
                continue;
            }

            switch (choice) {
                case 1:
                    Header();
                    ViewEmployees();
                    Buffer();
                    break;
                case 2:
                    Header();
                    EditEmployee();
                    break;
                case 3:
                    Header();
                    AddEmployee();
                    Buffer();
                    break;
                case 4:
                    Header();
                    RemoveEmployee();
                    break;
                case 5:
                    Header();
                    AddAdmin();
                    Buffer();
                    break;
                case 6:
                    Header();
                    RemoveAdmin();
                    Buffer();
                    break;
                case 7:
                    isLoggedIn = false;
                    role = UserRole.None;
                    loggedInUserPasswordHash = "";
                    inAdminView = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice, please enter 1-7.");
                    Buffer();
                    break;
            }
        }
    }

    static void ShowEmployeeMenu() {
        if (!isLoggedIn) {
            Console.WriteLine("Access denied. Please login.");
            Buffer();
            return;
        }

        bool inEmployeeView = true;

        while (inEmployeeView && isLoggedIn) {
            Header();
            Console.WriteLine("Employee Menu:");
            Console.WriteLine("1. View Department Tasks");
            Console.WriteLine("2. View Work Hours and Wage");
            Console.WriteLine("3. Start Work (₱200 every 15 seconds)");
            Console.WriteLine("4. Logout");
            Space();
            Console.Write("> ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) {
                Console.WriteLine("Invalid input. Try again.");
                Buffer();
                continue;
            }

            switch (choice) {
                case 1:
                    ShowDepartmentTasks();
                    Buffer();
                    break;
                case 2:
                    ShowEmployeeWageAndHours();
                    Buffer();
                    break;
                case 3:
                    StartWageSession();
                    Buffer();
                    break;
                case 4:
                    isLoggedIn = false;
                    role = UserRole.None;
                    loggedInUserPasswordHash = "";
                    inEmployeeView = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    Buffer();
                    break;
            }
        }
    }

    #endregion

    #region Login / Password Handling

    static bool Login() {
        Header();
        Console.Write("Enter password: ");
        string password = ReadPassword();
        Space();

        UserRole user = ValidatePassword(password);
        if (user != UserRole.None) {
            string hash = HashPassword(password);
            if (user == UserRole.Employee && !employees.Any(e => e.PasswordHash == hash)) {
                Console.WriteLine("No employee record found for this password.");
                Buffer();
                return false;
            }

            role = user;
            loggedInUserPasswordHash = hash;
            isLoggedIn = true;
            Console.WriteLine($"Logged in as {role}!");
            Buffer();
            return true;
        }

        Console.WriteLine("Invalid password, login failed.");
        Buffer();
        return false;
    }

    static UserRole ValidatePassword(string enteredPassword) {
        string hashed = HashPassword(enteredPassword);
        if (File.Exists(empPasswordsFilePath) &&
            File.ReadLines(empPasswordsFilePath).Any(pw => pw.Trim() == hashed))
            return UserRole.Employee;

        if (File.Exists(adminFilePath) &&
            File.ReadLines(adminFilePath).Any(pw => pw.Trim() == hashed))
            return UserRole.Admin;

        return UserRole.None;
    }

    static string HashPassword(string password) {
        using (SHA256 sha = SHA256.Create()) {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sb = new StringBuilder();
            foreach(var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    static string ReadPassword() {
        StringBuilder pass = new StringBuilder();
        ConsoleKeyInfo key;

        while (true) {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;

            if (key.Key == ConsoleKey.Backspace) {
                if (pass.Length > 0) {
                    pass.Remove(pass.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else {
                pass.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pass.ToString();
    }

    #endregion

    #region Admin Functions

    static void ViewEmployees() {
        if (employees.Count == 0) {
            Console.WriteLine("No employees found.");
            return;
        }
        int count = 1;
        foreach (var e in employees) {
            Console.WriteLine($"ID {count}:");
            Console.WriteLine($"  Name: {e.Name}");
            Console.WriteLine($"  Age: {e.Age}");
            Console.WriteLine($"  Gender: {e.Gender}");
            Console.WriteLine($"  Address: {e.Address}");
            Console.WriteLine($"  Department: {e.Department}");
            Console.WriteLine($"  Wage: ₱{e.Wage:F2}");
            Console.WriteLine($"  Work Seconds: {e.WorkSeconds}");
            Console.WriteLine();
            count++;
        }
    }

    static void AddEmployee() {
        string name = AskForName();
        int age = AskForAge();
        string gender = AskForGender();
        string address = AskForAddress();
        string department = AskForDepartment();
        string passwordHash = AskForPasswordAndAdd("Employee", empPasswordsFilePath);

        employees.Add(new Employee {
            Name = name,
            Age = age,
            Gender = gender,
            Address = address,
            Department = department,
            Wage = 0,
            WorkSeconds = 0,
            PasswordHash = passwordHash
        });

        SaveEmployeeData();
        Console.WriteLine("Employee added successfully!");
    }

    static void EditEmployee() {
        if (employees.Count == 0) {
            Console.WriteLine("No employees available to edit.");
            Buffer();
            return;
        }

        ViewEmployees();
        Console.Write("Enter employee ID to edit: ");
        
        if (!int.TryParse(Console.ReadLine(), out int id) || id < 1 || id > employees.Count) {
            Console.WriteLine("Invalid employee ID.");
            Buffer();
            return;
        }

        Employee emp = employees[id - 1];
        Header();
        Console.WriteLine($"Editing Employee ID {id}: {emp.Name}");
        Console.WriteLine("Leave input empty to keep current values.");

        Console.Write($"Name [{emp.Name}]: ");
        string name = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(name))
            emp.Name = char.ToUpper(name[0]) + name.Substring(1);

        Console.Write($"Age [{emp.Age}]: ");
        string ageStr = Console.ReadLine();
        if (int.TryParse(ageStr, out int newAge) && newAge >= 18 && newAge <= 60)
            emp.Age = newAge;

        Console.Write($"Gender [{emp.Gender}]: ");
        string gender = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(gender)) {
            gender = gender.ToLower();
            if (gender == "male" || gender == "female" || gender == "other")
                emp.Gender = char.ToUpper(gender[0]) + gender.Substring(1);
        }

        Console.Write($"Address [{emp.Address}]: ");
        string addr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(addr) && Regex.IsMatch(addr, @"^\d+\s+[\w\s]+(?:,\s*[\w\s]+)*$"))
            emp.Address = addr;

        Console.Write($"Department [{emp.Department}]: ");
        string dept = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(dept) && Enum.TryParse(dept.Replace(" ", "_").ToUpper(), out Department d))
            emp.Department = d.ToString().Replace("_", " ");

        SaveEmployeeData();
        Console.WriteLine("Employee updated successfully.");
        Buffer();
    }

    static void RemoveEmployee() {
        if (employees.Count == 0) {
            Console.WriteLine("No employees available to remove.");
            Buffer();
            return;
        }

        ViewEmployees();
        Console.Write("Enter employee ID to remove: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id < 1 || id > employees.Count) {
            Console.WriteLine("Invalid employee ID.");
            Buffer();
            return;
        }

        Employee emp = employees[id - 1];

        Console.Write($"Confirm removal of employee {emp.Name} (Y/N)? ");
        string confirm = Console.ReadLine().ToUpper();
        if (confirm == "Y") {
            employees.RemoveAt(id - 1);
            RemovePasswordByHash(emp.PasswordHash, empPasswordsFilePath);
            SaveEmployeeData();
            Console.WriteLine("Employee removed successfully.");
        } else {
            Console.WriteLine("Employee removal cancelled.");
        }
        Buffer();
    }

    static void AddAdmin() {
        string passwordHash = AskForPasswordAndAdd("Admin", adminFilePath);
        Console.WriteLine("Admin added successfully!");
    }

    static void RemoveAdmin() {
        if (!File.Exists(adminFilePath)) {
            Console.WriteLine("Admin password file not found.");
            return;
        }
        int count = File.ReadLines(adminFilePath).Count();
        
        if (count <= 1) {
            Console.WriteLine("Cannot remove all admins. Only one admin remains.");
            return;
        }

        Console.Write("Enter Admin password to remove: ");
        string pwToRemove = Console.ReadLine();
        if (RemovePassword(pwToRemove, adminFilePath))
            Console.WriteLine("Admin removed successfully.");
        else
            Console.WriteLine("Password not found.");
    }

    static void RemovePasswordByHash(string passwordHash, string filePath) {
        if (!File.Exists(filePath))
            return;

        var lines = File.ReadAllLines(filePath).Where(line => line.Trim() != passwordHash).ToList();
        File.WriteAllLines(filePath, lines);
    }

    #endregion

    #region Employee Menu Functions

    static void ShowDepartmentTasks() {
        Header();
        string department = GetLoggedInUserDepartment();
        if (string.IsNullOrWhiteSpace(department)) {
            Console.WriteLine("Could not determine your department.");
            return;
        }
        Console.WriteLine($"Your Department: {department}");
        Console.WriteLine("Tasks:");

        switch (department.ToUpper().Replace(" ", "_")) {
            case "HUMAN_RESOURCES":
                Console.WriteLine("- Manage employee records");
                Console.WriteLine("- Conduct interviews");
                break;
            case "FINANCE":
                Console.WriteLine("- Prepare budgets");
                Console.WriteLine("- Track financial reports");
                break;
            case "OPERATIONS":
                Console.WriteLine("- Oversee daily operations");
                Console.WriteLine("- Schedule shifts");
                break;
            case "INFORMATION_TECHNOLOGY":
                Console.WriteLine("- Maintain system infrastructure");
                Console.WriteLine("- Support employees with tech issues");
                break;
            case "CUSTOMER_SERVICE":
                Console.WriteLine("- Respond to client inquiries");
                Console.WriteLine("- Resolve complaints");
                break;
            default:
                Console.WriteLine("No tasks found for your department.");
                break;
        }
    }

    static void ShowEmployeeWageAndHours() {
        var emp = employees.FirstOrDefault(e => e.PasswordHash == loggedInUserPasswordHash);
        if (emp == null) {
            Console.WriteLine("Your record was not found.");
            return;
        }
        double hours = emp.WorkSeconds / 3600.0;
        double netWage = emp.Wage * 0.8; // Calculate net wage after 20% tax
        Console.WriteLine($"Hours Worked: {hours:F2} hours");
        Console.WriteLine($"Total Wage: ₱{emp.Wage:F2}");
        Console.WriteLine($"Net Wage after Tax (20%): ₱{netWage:F2}");
    }

    static void StartWageSession() {
        var emp = employees.FirstOrDefault(e => e.PasswordHash == loggedInUserPasswordHash);
        if (emp == null) {
            Console.WriteLine("Your employee record was not found.");
            return;
        }

        Console.WriteLine("Work session started. ₱200 will be credited every 15 seconds.");
        Console.WriteLine("Press 'Q' to stop work session and return to the menu.");

        bool working = true;
        while (working && isLoggedIn) {
            int sleepInterval = 15000;
            int slept = 0;
            while (working && slept < sleepInterval) {
                if (Console.KeyAvailable) {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q) {
                        working = false;
                        break;
                    }
                }
                Thread.Sleep(200);
                slept += 200;
            }
            if (!working || !isLoggedIn)
                break;

            // Credit wage:
            emp.Wage += 200;
            emp.WorkSeconds += 15;
            double netWage = emp.Wage * 0.8; // Calculate net wage after 20% tax
            Console.WriteLine($"₱200 credited! Total Wage: ₱{emp.Wage:F2} | Net Wage after Tax: ₱{netWage:F2}");
            SaveEmployeeData();
        }
        Console.WriteLine("Work session ended.");
    }

    static string GetLoggedInUserDepartment() {
        var emp = employees.FirstOrDefault(e => e.PasswordHash == loggedInUserPasswordHash);
        return emp?.Department ?? "";
    }

    #endregion

    #region Common Input & Password Helpers

    static string AskForPasswordAndAdd(string role, string filePath) {
        while (true) {
            Console.Write($"Enter new {role} password: ");
            string pwd = Console.ReadLine();
            if (AddPassword(pwd, filePath))
                return HashPassword(pwd);
        }
    }

    static bool AddPassword(string newPassword, string filePath) {
        string hashed = HashPassword(newPassword);
        if (File.Exists(filePath)) {
            if (File.ReadLines(filePath).Any(line => line.Trim() == hashed)) {
                Console.WriteLine("That password already exists. Try a different one.");
                return false;
            }
        }
        File.AppendAllText(filePath, hashed + Environment.NewLine);
        return true;
    }

    static bool RemovePassword(string passwordToDelete, string filePath) {
        string hashed = HashPassword(passwordToDelete);
        if (!File.Exists(filePath))
            return false;

        var lines = File.ReadAllLines(filePath).ToList();
        int beforeCount = lines.Count;
        lines = lines.Where(line => line.Trim() != hashed).ToList();

        if (lines.Count == beforeCount)
            return false;

        File.WriteAllLines(filePath, lines);
        return true;
    }

    static string AskForName() {
        Console.Write("Name: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return AskForName();

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();

        return string.Join(" ", words);
    }

    static int AskForAge() {
        Console.Write("Age (18 - 60): ");
        if (int.TryParse(Console.ReadLine(), out int age) && age >= 18 && age <= 60)
            return age;
        Console.WriteLine("Invalid age. Please try again.");
        return AskForAge();
    }

    static string AskForGender() {
        Console.Write("Gender (male, female, other): ");
        var input = Console.ReadLine()?.Trim().ToLower();
        if (input == "male" || input == "female" || input == "other")
            return char.ToUpper(input[0]) + input.Substring(1);
        Console.WriteLine("Invalid gender. Please try again.");
        return AskForGender();
    }

    static string AskForAddress() {
        Console.Write("Address (e.g., 123 Main St, City, State): ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) {
            Console.WriteLine("Address cannot be empty.");
            return AskForAddress();
        }
        if (Regex.IsMatch(input, @"^\d+\s+[\w\s]+(?:,\s*[\w\s]+)*$"))
            return input;
        Console.WriteLine("Invalid address format.");
        return AskForAddress();
    }

    static string AskForDepartment() {
        Console.WriteLine("Available Departments:");
        foreach (var d in Enum.GetNames(typeof(Department)))
            Console.WriteLine($"- {d.Replace('_', ' ')}");

        Console.Write("Department: ");
        var input = Console.ReadLine()?.Replace(" ", "_").ToUpper();

        if (Enum.TryParse(input, out Department dept))
            return dept.ToString().Replace('_', ' ');

        Console.WriteLine("Invalid department. Please try again.");
        return AskForDepartment();
    }

    #endregion
}

class Employee {
    public string Name { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public string Department { get; set; }
    public string PasswordHash { get; set; }
    public double Wage { get; set; }
    public int WorkSeconds { get; set; }
}