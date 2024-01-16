# Unity DotNetSDKChecker

This Editor script checks if the .NET SDK is correctly installed so that the link between VSCode and Unity works.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Technical Details](#technical-details)
3. [Compatibility](#compatibility)
4. [Known Issues](#known-issues)
5. [About the Project](#about-the-project)
6. [Contact](#contact)
7. [Version History](#version-history)
8. [License](#license)

## Getting Started

Import this lightweight package to your project (or manually add the scripts to an Editor folder in the Assets folder).

To use it, simply open your project or recompile a script, the test will execute itself. It will show a message in the console if there is an issue with the SDK.

That's it!

## Technical Details

* This script is only intended for Windows.
* It verifies the presence of the .NET SDK by checking Windows' PATH.
* It assumes that the SDK is installed in the "Program Files" or "Program Files (x86)" folder.
* A future version could also check the SDK version, and the C# extension version.

## Compatibility

Tested on Windows with Unity version 2022.3.17 (LTS).

## Known Issues

* The script assumes that the SDK is installed in the default folders ("Program Files" or "Program Files (x86)" ).

## About the Project

I created this tool to help my students to identify .NET SDK problems.

## Contact

**Jonathan Tremblay**  
Teacher, Cegep de Saint-Jerome  
jtrembla@cstj.qc.ca

Project Repository: https://github.com/JonathanTremblay/UnityDotNetSDKChecker 

## Version History

* 0.1.0
    * First public version.

## License

This tool is available for distribution and modification under the CC0 License, which allows for free use and modification.  
https://creativecommons.org/share-your-work/public-domain/cc0/