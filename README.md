
# AutoCADLookup

![Autocad API](https://img.shields.io/badge/Revit%20API-2021-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET-4.7.2-blue.svg)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)

Interactive Autocad BIM database exploration tool to view and navigate element properties and relationships.

### Setup

Open the SnoopAutoCADCSharp.sln on Visual Studio 2017. All references should be ready for AutoCAD 2022 default install path, otherwise go to project properties >> References, then click on Reference Paths and adjust. Build the project in Release, the DLL should be placed at same folder. Copy the entire .bundle folder to C:\Program Files\Autodesk\Autodesk\ApplicationPlugins folder and launch Autocad.

### Version

The most up-to-date version provided <a href="[link](https://github.com/chuongmep/SnoopAutoCADCSharp/releases/)">here</a>  is for Autocad 2022.
- [2022](link) for Autocad 2022
- [2021](link) for Autocad 2021
- [2020](link) for Autocad 2020
- [2019](link) for Autocad 2019

# Usage

- On AutoCAD , command the "SnoopAutocad" option, the main form should appear. At the left side is a list of the main collections on the active document. On the right side, the properties of the item selected on the left.

- Can continue Snoop Database to check.

![](Documents/_Image_bfad9808-fa10-4857-8d77-f1f0b161433f.png)

### Author 

First Project write sypport Civi3D with language VB.NET by <a href="https://github.com/augustogoncalves">Augusto Goncalves</a> <a href="https://twitter.com/augustomaia">@augustomaia</a> , member of the Autodesk Developer Technical Services team.

Now project update in .NET C# by <a href="https://github.com/htlcnn">htlcnn</a> and <a href="https://github.com/chuongmep">Hồ Văn Chương</a> 

### Known Issues

The tool may stop working on some properties that cannot be reflected (using .NET).

### Demo

![](Documents/SnoopCad.gif)

### Release History

1.0.0 : First Release


