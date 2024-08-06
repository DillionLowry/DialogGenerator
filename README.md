# DialogGenerator [![NuGet Version](https://img.shields.io/nuget/v/Rhizine.DialogGenerator.svg?style=flat)](https://www.nuget.org/packages/Rhizine.DialogGenerator/)

DialogGenerator is a source generator and attribute that simplifies the process of creating dialog popups for ICommand results.

## Features

- Easy-to-use attribute-based dialog generation
- Support for multiple dialog types (Bubble, Toast, Modal, Snackbar)
- Customizable success and failure messages
- Options for dialog duration, position, and theme
- Compile-time generation for improved performance

## Usage

1. Add the necessary using statement in your ViewModel:

```csharp
using DialogAttributes;
```
2. Decorate your command properties with the ShowDialogResult attribute:

```csharp
public class MainViewModel : ViewModelBase
{
    [ShowDialogResult(Type = DialogType.Toast, SuccessMessage = "Save successful!", FailureMessage = "Save failed", Duration = 3000, Position = "TopRight", Theme = "Dark")]
    public ICommand SaveCommand => new RelayCommand(SaveData);

    [ShowDialogResult(Type = DialogType.Modal, SuccessMessage = "Item deleted", FailureMessage = "Delete operation failed")]
    public ICommand DeleteCommand => new RelayCommand(DeleteItem);

    private bool SaveData()
    {
        // Your save logic here
        return true; // or false if the operation failed
    }

    private bool DeleteItem()
    {
        // Your delete logic here
        return true; // or false if the operation failed
    }
}
```
The source generator will automatically create decorated versions of your commands that show the appropriate dialog based on the command's result.

## Attribute Options
The ShowDialogResult attribute accepts the following parameters:

- Type: The type of dialog to show (Bubble, Toast, Modal, Snackbar)
- SuccessMessage: The message to display when the command succeeds
- FailureMessage: The message to display when the command fails
- Duration: The duration (in milliseconds) for which the dialog should be displayed (for non-modal dialogs)
- Position: The position of the dialog (e.g., "TopRight", "Center", "BottomLeft")
- Theme: The theme of the dialog (e.g., "Light", "Dark")

The full list of types is defined by this enumeration:

```csharp
public enum DialogType
{
    Bubble,
    Toast,
    Modal,
    MessageBox,
    Input,
    Confirmation,
    YesNo,
    OpenFile,
    SaveFile,
    OpenFolder,
    Snackbar
}
```

## Notes

Ensure that your project is using C# 8.0 or later.
The generated code assumes the existence of an IDialogService in your application. Implement this interface to handle the actual display of dialogs.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## License
This project is licensed under the MIT License - see the LICENSE.md file for details.
