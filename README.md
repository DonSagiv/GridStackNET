# GridStackNET
 A layout manager inspired by GRIDSTACK.JS (http://gridstackjs.com/ for more information). Credit goes to Pavel Reznikov for inspriation. Uses .NET 4.6.1 using C# 6. Forking and modifying encouraged.
 
![GridStackNET image](https://raw.githubusercontent.com/DonSagiv/GridStackNET/master/Screenshots/Animation.gif)

## How to use GridStackNET in your WPF project

    <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:gridStackNet="clr-namespace:GridStackNET"
        x:Class="GridStackNET.MainWindow">

        <gridStackNet:GridStack Margin="5" x:Name="gridStack"
                                NumColumns="12"
                                MinRows="8"
                                MinRowHeight="50"
                                ItemMargin="3"
                                DefaultColumnSpan="3"
                                DefaultRowSpan="1"
                                Children="{Binding gridElements}"/>

    </Window>

* **NumColumns**: Number of columns in the grid stack
* **MinRows**: Minimum number of rows in the grid stack (this expands based on how many items you have or how you manipulate the items)
* **MinRowHeight**: Minimum row height of each item. Before the scrollbar appears, these items will be stretched
* **ItemMargin**: The margin between the grid border and the element placed in it.
* **DefaultColumnSpan**: When adding a new item, this will be how many columns it spans.
* **DefaultRowSpan**: When adding a new item, this will be how many rows it spans.
* **AutoAssignGridSpace**: When adding a new item to the grid stack, it will use the default column and row span dependency properties. Should be set to "False" if any grid stack layouts will be saved and re-rendered.
* **Children**: The children elements inside the gridstack. Each child must be of type **System.Windows.UIElement** and the Children object itself must be **ObservableCollection\<UIElement\>**. This property can also be binded to a view model object.

This code has an MIT license.

# Changelog

## Version 0.0.0.4

- Bug Fix: infinite loop, errors when adding grid items that have a larger column span than the number of columns.

## Version 0.0.0.3

- Add "AutoAssignGridSpace" dependency property with avoids defaulting any elements to the DefaultColumnSpan and DefaultRowSpan dependency properties.
- Allows users to custom-define grid-sizes of the UI elements to add by setting the attached Grid properties to the UI elements.
- Bug Fix: Re-sizing windows doesn't re-size elements in grid stack.

## Version 0.0.0.2

- Prevent re-sizing of rows when content is larger than row definition's actual height.

## Version 0.0.0.1

- Initial Release