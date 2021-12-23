---
uid: a_develop_project_health
---

# Project health

Here you can see "health" of the project in terms of test pipelines are passed or not. First of all, we need to define two subsets of the library API:

* **Core API** - it's all API except Multimedia one; in other words it's the API that is supported by .NET itself, so it can be run on any platform .NET Core / .NET supported.
* **Multimedia API** - it's platform-specific API that includes following key classes:  
  * [InputDevice](xref:Melanchall.DryWetMidi.Multimedia.InputDevice) (more details in the [InputDevice](zref:a_dev_input) article);
  * [OutputDevice](xref:Melanchall.DryWetMidi.Multimedia.OutputDevice) (more details in the [OutputDevice](xref:a_dev_output) article);
  * [VirtualDevice](xref:Melanchall.DryWetMidi.Multimedia.VirtualDevice) (more details in the [VirtualDevice](xref:a_dev_virtual) article);
  * [DevicesWatcher](xref:Melanchall.DryWetMidi.Multimedia.DevicesWatcher) (more details in the [DevicesWatcher](xref:a_dev_watcher) article);
  * [HighPrecisionTickGenerator](xref:Melanchall.DryWetMidi.Multimedia.HighPrecisionTickGenerator).

## Windows

**master**

[![Test Core API - Windows - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET%20Core?branchName=master&label=Test%20Core%20API%20-%20Windows%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=57&branchName=master)  
[![Test Core API - Windows - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET?branchName=master&label=Test%20Core%20API%20-%20Windows%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=59&branchName=master)  
[![Test Core API - Windows - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET%20Framework?branchName=master&label=Test%20Core%20API%20-%20Windows%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=67&branchName=master)  
[![Test Multimedia API - Windows - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Core?branchName=master&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=63&branchName=master)  
[![Test Multimedia API - Windows - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET?branchName=master&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=64&branchName=master)  
[![Test Multimedia API - Windows - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Framework?branchName=master&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=68&branchName=master)  

**develop**

[![Test Core API - Windows - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET%20Core?branchName=develop&label=Test%20Core%20API%20-%20Windows%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=57&branchName=develop)  
[![Test Core API - Windows - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET?branchName=develop&label=Test%20Core%20API%20-%20Windows%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=59&branchName=develop)  
[![Test Core API - Windows - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Windows%20-%20.NET%20Framework?branchName=develop&label=Test%20Core%20API%20-%20Windows%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=67&branchName=develop)  
[![Test Multimedia API - Windows - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Core?branchName=develop&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=63&branchName=develop)  
[![Test Multimedia API - Windows - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET?branchName=develop&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=64&branchName=develop)  
[![Test Multimedia API - Windows - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Framework?branchName=develop&label=Test%20Multimedia%20API%20-%20Windows%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=68&branchName=develop)  

## macOS

**master**

[![Test Core API - macOS - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20macOS%20-%20.NET%20Core?branchName=master&label=Test%20Core%20API%20-%20macOS%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=58&branchName=master)  
[![Test Core API - macOS - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20macOS%20-%20.NET?branchName=master&label=Test%20Core%20API%20-%20macOS%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=60&branchName=master)  
[![Test Multimedia API - macOS - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20macOS%20-%20.NET%20Core?branchName=master&label=Test%20Multimedia%20API%20-%20macOS%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=65&branchName=master)  
[![Test Multimedia API - macOS - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20macOS%20-%20.NET?branchName=master&label=Test%20Multimedia%20API%20-%20macOS%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=66&branchName=master)  

**develop**

[![Test Core API - macOS - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20macOS%20-%20.NET%20Core?branchName=develop&label=Test%20Core%20API%20-%20macOS%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=58&branchName=develop)  
[![Test Core API - macOS - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20macOS%20-%20.NET?branchName=develop&label=Test%20Core%20API%20-%20macOS%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=60&branchName=develop)  
[![Test Multimedia API - macOS - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20macOS%20-%20.NET%20Core?branchName=develop&label=Test%20Multimedia%20API%20-%20macOS%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=65&branchName=develop)  
[![Test Multimedia API - macOS - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API%20-%20macOS%20-%20.NET?branchName=develop&label=Test%20Multimedia%20API%20-%20macOS%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=66&branchName=develop)  

## Linux

_Multimedia API is not available for Linux_.

**master**

[![Test Core API - Ubuntu - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Ubuntu%20-%20.NET%20Core?branchName=master&label=Test%20Core%20API%20-%20Ubuntu%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=61&branchName=master)  
[![Test Core API - Ubuntu - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Ubuntu%20-%20.NET?branchName=master&label=Test%20Core%20API%20-%20Ubuntu%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=62&branchName=master)  

**develop**

[![Test Core API - Ubuntu - .NET Core](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Ubuntu%20-%20.NET%20Core?branchName=develop&label=Test%20Core%20API%20-%20Ubuntu%20-%20.NET%20Core)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=61&branchName=develop)  
[![Test Core API - Ubuntu - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API%20-%20Ubuntu%20-%20.NET?branchName=develop&label=Test%20Core%20API%20-%20Ubuntu%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=62&branchName=develop)  

## Package integration

Package integration tests check that NuGet package installed in .NET applications of different types works as expected.

**master**

[![Test package integration - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Framework?branchName=master&label=Test%20package%20integration%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=50&branchName=master)  
[![Test package integration - .NET Core & .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Core%20%26%20.NET?branchName=master&label=Test%20package%20integration%20-%20.NET%20Core%20%26%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=49&branchName=master)  
[![Test package integration - Self-contained - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20Self-contained%20-%20.NET?branchName=master&label=Test%20package%20integration%20-%20Self-contained%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=51&branchName=master)

**develop**

[![Test package integration - .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Framework?branchName=develop&label=Test%20package%20integration%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=50&branchName=develop)  
[![Test package integration - .NET Core & .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Core%20%26%20.NET?branchName=develop&label=Test%20package%20integration%20-%20.NET%20Core%20%26%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=49&branchName=develop)  
[![Test package integration - Self-contained - .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20Self-contained%20-%20.NET?branchName=develop&label=Test%20package%20integration%20-%20Self-contained%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=51&branchName=develop)