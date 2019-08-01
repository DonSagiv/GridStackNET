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
* **Children**: The children elements inside the gridstack. Each child must be of type **System.Windows.UIElement** and the Children object itself must be **ObservableCollection\<UIElement\>**. This property can also be binded to a view model object.

This code has an MIT license.
