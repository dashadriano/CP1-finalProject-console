# Computer Programming 1's Final Project (Console Version)
Console-based employee salary management program version for the full project (has a GUI), with admin and employee functionalities. Built via C#.

<em>Console version built by [@dashadriano](https://https://github.com/dashadriano) and [@Ejheyyyyy](https://github.com/Ejheyyyyy)</em>

# Access
Clone this repository using:

```bash
git clone https://github.com/dashadriano/CP1-finalProject-console
```

The `.gitignore` file only caters to Visual Studio and VS Code users. 

# Development
The program only utilizes a single `.cs` file. Other files included are needed for it to run, as well as other build files that will be included in the instantiation of your C# Project locally.

The code flow is regulated through `running`, a boolean attribute of the main class. Comments are included for other segments of the code to aid collaborators in skimming the code before edits. 

A security liability for the project, noticeably, is the addition of the password text files in this repository, as well as storing plain-text passwords. 

Although the addition of the text files for passwords will not be removed due to this being an academic project that needs to include the password text files for demonstration purposes, hashing was added to provide security. Despite this, employee data under the `data` folder is of course in plain text for admin viewing purposes, and as such git commits are required not to contain real sensitive information such as accurate addresses for the employees if the employee logged is based on an actual person. 

Admin passwords include 5 hashes, all of which are surnames of the members for this project–– all in lowercase.
