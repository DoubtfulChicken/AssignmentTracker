# Assignment Tracker

## Description

Assignment Tracker is a desktop application designed to help manage assignments effectively. It allows users to add, edit, and delete assignments, set reminders, and visualize their tasks using a Gantt chart. The application provides notifications for upcoming deadlines and supports importing data from Excel files.

## Features

- Add, edit, and delete assignments
- Set reminders for assignments
- Notifications for upcoming tasks
- Visualize tasks using a Gantt chart
- Import assignments from Excel files
- Customizable unit code and task grade colors

## Installation

### Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) (version 3.1 or later)

### Building the Project

1. Clone the repository:

    ```sh
    git clone https://github.com/yourusername/your-repo-name.git
    cd your-repo-name
    ```

2. Restore NuGet packages:

    ```sh
    dotnet restore
    ```

3. Build the project:

    ```sh
    dotnet build --configuration Release
    ```

4. Run the application:

    ```sh
    dotnet run --project AssignmentTracker
    ```

## Usage

### Adding an Assignment

1. Click the "Add Assignment" button.
2. Fill in the required fields:
    - Unit Code
    - Task Name
    - Task Grade
    - Start Date
    - Due Date
3. Optionally, add notes and set a reminder date.
4. Click "Save" to add the assignment.

### Editing an Assignment

1. Right-click on the assignment in the list.
2. Select "Edit".
3. Modify the assignment details.
4. Click "Save" to update the assignment.

### Deleting an Assignment

1. Right-click on the assignment in the list.
2. Select "Delete".
3. Confirm the deletion.

### Importing Assignments from Excel

1. Click the "Import Data" button.
2. Select an Excel file with the following columns:
    - Unit Code
    - Task Name
    - Task Grade
    - Start Date
    - Due Date
    - Notes (optional)
3. Click "Open" to import the data.

## Customizing Colors

### Unit Code Colors

1. Right-click on an assignment.
2. Select "Set Unit Code Colour".
3. Choose a color from the color picker.
4. Click "OK" to apply the color.

### Task Grade Colors

1. Right-click on an assignment.
2. Select "Set Task Grade Colour".
3. Choose a color from the color picker.
4. Click "OK" to apply the color.

## Contributing

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes and push to your branch.
4. Open a pull request with a detailed description of your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [EPPlus](https://github.com/EPPlusSoftware/EPPlus) for Excel file handling
- [MaterialDesignInXAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) for UI components

## Contact

For any questions or suggestions, please open an issue.

