---
uid: a_develop_project_health
---

# Project health

Here you can see the "health" of the project in terms of whether test pipelines are passed or not. First of all, we need to define two subsets of the library API:

* **Core API** – it's all API except Multimedia one; in other words, it's the API that is supported by .NET itself, so it can be run on any platform .NET Core / .NET supported.
* **Multimedia API** – it's platform-specific API. The list of the key classes can be found in the [Supported OS](xref:a_develop_supported_os) article.

## Main tests

**master**

[![Test Core API](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API?branchName=master&label=Test%20Core%20API)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=93&branchName=master)  
[![Test Multimedia API](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API?branchName=master&label=Test%20Multimedia%20API)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=92&branchName=master)  

**develop**

[![Test Core API](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Core%20API?branchName=develop&label=Test%20Core%20API)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=93&branchName=develop)  
[![Test Multimedia API](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Test/Test%20Multimedia%20API?branchName=develop&label=Test%20Multimedia%20API)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=92&branchName=develop)  

## Package integration

Package integration tests check that NuGet package installed in .NET applications of different types works as expected.

**master**

[![Test package integration – .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Framework?branchName=master&label=Test%20package%20integration%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=50&branchName=master)  
[![Test package integration – .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET?branchName=master&label=Test%20package%20integration%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=49&branchName=master)  
[![Test package integration – Self-contained – .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20Self-contained%20-%20.NET?branchName=master&label=Test%20package%20integration%20-%20Self-contained%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=51&branchName=master)

**develop**

[![Test package integration – .NET Framework](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET%20Framework?branchName=develop&label=Test%20package%20integration%20-%20.NET%20Framework)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=50&branchName=develop)  
[![Test package integration – .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20.NET?branchName=develop&label=Test%20package%20integration%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=49&branchName=develop)  
[![Test package integration – Self-contained – .NET](https://dev.azure.com/Melanchall/DryWetMIDI/_apis/build/status/Package%20integration/Test%20package%20integration%20-%20Self-contained%20-%20.NET?branchName=develop&label=Test%20package%20integration%20-%20Self-contained%20-%20.NET)](https://dev.azure.com/Melanchall/DryWetMIDI/_build/latest?definitionId=51&branchName=develop)
