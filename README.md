# OnDayApp

### Travel & Daily Life Management Application

OnDayApp is a Windows desktop application developed as a university programming course project using **C, C#, and Windows Forms**.

The application is designed to help users organize travel plans and daily activities through a unified management system.

## Project Overview

The main objective of this project was to design and implement a desktop application that integrates multiple scheduling and personal management features.

The project consists of two applications:

- **C Console Application:** Implements core management features using procedural programming.
- **C# Windows Forms Application:** Provides a graphical user interface for managing travel plans and daily activities.

## Tech Stack

| Category | Technology |
|---|---|
| Languages | C, C# |
| IDE | Visual Studio |
| GUI Framework | Windows Forms |
| Framework | .NET Framework 4.0 |
| Data Storage | Local Text Files |
| Platform | Windows |

## Key Features

### Travel Management
- Travel schedule management
- Reservation management
- Travel route planning
- Travel expense tracking
- Travel checklists

### Daily Life Management
- Daily schedule management
- To-do list management
- Personal activity tracking
- Alarm and reminder features

### Additional Features
- User registration and login
- Friend management
- Schedule sharing
- Weather information management
- Place reviews

## Project Structure

```text
OnDayApp/
├── OnDay/
│   ├── main.c
│   ├── OnDay.vcxproj
│   └── data/
├── OnDayGui/
│   ├── Program.cs
│   ├── MainForm.cs
│   ├── Models.cs
│   ├── DataStore.cs
│   ├── AlarmForm.cs
│   ├── TripDetailForm.cs
│   ├── RouteSelectForm.cs
│   └── ...
└── OnDay.sln
```

## Implementation Details

**1. C-Based Console Application**

Implemented a console-based application using C to manage schedules, reservations, and other user activities.

**2. Graphical User Interface**

Developed a Windows Forms application using C# to provide a more accessible and interactive user experience.

**3. File-Based Data Management**

Implemented local text-file storage to manage application records without requiring an external database server.

**4. Modular Application Structure**

Organized the application into separate components for user interaction, data models, data storage, and feature-specific interfaces.

## Getting Started

### Requirements

- Windows operating system
- Visual Studio
- C++ Desktop Development tools
- .NET Desktop Development tools

### How to Run

1. Clone or download this repository.
2. Open `OnDay.sln` in Visual Studio.
3. Select `OnDayGui` as the startup project to run the graphical application.
4. Build the project.
5. Run the application.

## Future Improvements

- Replace text-file storage with a relational database.
- Improve user authentication and data security.
- Refactor the codebase for better maintainability.
- Add automated testing.
- Improve the graphical user interface.

## Project Background

This application was developed as part of a university programming course.

Through this project, I gained practical experience in:

- C programming and procedural application design
- C# event-driven GUI development
- File input/output and data management
- Desktop application development
- Organizing and maintaining a multi-feature software project

## Notes

This repository contains fictional test data used for development and demonstration purposes.

The project was developed for educational purposes.
