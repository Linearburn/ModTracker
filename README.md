## License

This project is licensed under a proprietary license. You may view the source code for personal use only. Redistribution, modification, and commercial use are prohibited without explicit permission from the author. See the [LICENSE](./LICENSE) file for more details.

For inquiries regarding permissions, please contact linearburn1@gmail.com.



# ModTracker

ModTracker is a Windows Forms application that allows users to manage and track mods for Conan Exiles. The application supports adding, removing, and enabling/disabling mods, and it can save and load the mod list from a file.

## Features

- **Add Mods**: Add mods to the list by entering the mod URL.
- **Remove Mods**: Remove selected mods from the list.
- **Enable/Disable Mods**: Enable or disable mods and update the mod list accordingly.
- **Move Mods**: Move selected mods up or down in the list.
- **Build Mod List**: Generate a comma-separated list of enabled mod IDs.
- **Copy to Clipboard**: Copy the generated mod list to the clipboard.
- **Save/Load Mod List**: Save the current mod list to a file and load it on application start.

## Installation

1. Download the ZIP file containing the ModTracker application.
2. Extract the contents of the ZIP file to a desired location on your computer.
3. Run the `ModTracker.exe` file to start the application.

## Usage

### Adding Mods

1. Enter the mod URL in the `Mod URL` text box.
2. Click the `Add Mod` button.
3. The application will fetch the mod details using the Steam Web API and add the mod to the list.

### Removing Mods

1. Select the mod(s) you want to remove from the list.
2. Click the `Remove Mod` button.

### Enabling/Disabling Mods

1. Check or uncheck the `Enabled?` checkbox for the desired mod.
2. The mod list in the `Mod List` text box will be updated accordingly.

### Moving Mods

1. Select the mod you want to move.
2. Click the `Up` or `Down` button to move the mod up or down in the list.
3. The buttons will be disabled when the selected mod is at the top or bottom of the list, respectively.

### Building and Copying the Mod List


1. Click the `Copy to Clipboard` button to copy the mod list to the clipboard.

### Saving and Loading the Mod List

- **Save**: Click the `Save` menu item to save the current mod list to `list.txt` in the root directory.
- **Load**: Click the `Load` menu item to load the mod list from `list.txt` in the root directory.

### Menu Items


- **Save**: Saves the current mod list to `list.txt`.
- **Load**: Loads the mod list from `list.txt`.

## Dependencies

The application requires the following dependencies, which are included in the ZIP file:

- [HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/)
- [System.Text.Json](https://www.nuget.org/packages/System.Text.Json/)

## Notes

- Ensure that the `list.txt` file is in the same directory as `ModTracker.exe` for the Save and Load functionalities to work correctly.
- If you encounter any issues or have questions, please refer to this README file for instructions on how to use the application.
