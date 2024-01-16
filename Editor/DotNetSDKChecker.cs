#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetSDKChecker
{
    [InitializeOnLoad]
    /// <summary>
    /// This Editor script checks if the .NET SDK is correctly installed so that the link between VSCode and Unity works.
    /// It executes the verification once when the Unity editor is loaded.
    /// It also executes when the script is recompiled (messages are shown if PATH is changed).
    /// It shows a message in the console if there is an issue with the SDK.
    /// 
    /// This script is only intended for Windows.
    /// It must be placed in an Editor folder (inside the Assets or Packages folder).
    /// 
    /// It is a work in progress, but it works well enough to be useful for my students.
    /// It assumes that the SDK is installed in the "Program Files" or "Program Files (x86)" folder.
    /// It could also check the SDK version, and the C# extension version.
    ///  
    /// Created by Jonathan Tremblay, teacher at Cegep de Saint-Jerome.
    /// This project is available for distribution and modification under the CC0 License.
    /// https://github.com/JonathanTremblay/UnityDotNetSDKChecker
    /// </summary>
    public class DotNetSDKChecker
    {
        static readonly string currentVersion = "Version 0.1 (2024-01)";

        private static readonly string _sdkFolderName = "dotnet\\";

        private static readonly bool _showPositiveMessages = false; // If true, the messages will be shown when the SDK is found. If false, only the negative messages will be shown.

        private static readonly Dictionary<string, string> _messagesEn = new() // A dictionary for English messages
        {
            {"explanation", $"The DotNetSDKChecker script checks if the .NET SDK required by VSCode is correctly installed.\n** DotNet SDK Checker is free and open source. For updates and feedback, visit https://github.com/JonathanTremblay/UnityDotNetSDKChecker. **\n** {currentVersion} **"},
            {"systemPath", "\nCurrent system PATH where executables are searched for: {0}"},
            {"testSdk64Only", "<color=#90ee90>TEST PASSED</color> → .NET SDK 7 (64-bit) is in the PATH. 64-bit SDK Path: "},
            {"testSdk32Only", "<color=red>TEST FAILED</color> → .NET SDK 7 (32-bit) is in the PATH, but not .NET SDK 7 (64-bit). 32-bit SDK Path: "},
            {"testBothSdkCorrect", "<color=#90ee90>TEST PASSED</color> → .NET SDK 7 (64-bit) is in the PATH before 32-bit version. 64-bit SDK Path: "},
            {"testBothSdkWrongOrder", "<color=yellow>TEST PARTIALLY FAILED</color> → .NET SDK 7 (64-bit) is in the PATH, BUT it is after 32-bit version (this is an issue with pre 2024 versions of C# Dev Kit extension for VSCode). 32-bit SDK Path: "},
            {"testFailedNotFound", "<color=red>TEST FAILED</color> → .NET SDK 7 is not found in the PATH. "}
        };

        private static readonly Dictionary<string, string> _messagesFr = new() // A dictionary for French messages
        {
            {"explanation", $"Le script DotNetSDKChecker vérifie si le .NET SDK requis par VSCode est installé correctement.\n** DotNet SDK Checker est gratuit et open source. Pour les mises à jour et les commentaires, visitez https://github.com/JonathanTremblay/UnityDotNetSDKChecker. **\n** {currentVersion} **"},
            {"systemPath", "\nChemin d'accès système actuel où les exécutables sont recherchés : {0}"},
            {"testSdk64Only", "<color=#90ee90>TEST RÉUSSI</color> → .NET SDK 7 (64-bit) est dans le PATH. Chemin du SDK : "},
            {"testSdk32Only", "<color=red>TEST ÉCHOUÉ</color> → .NET SDK 7 (32-bit) est dans le PATH, mais pas .NET SDK 7 (64-bit). Chemin du SDK 32-bit : "},
            {"testBothSdkCorrect", "<color=#90ee90>TEST RÉUSSI</color> → .NET SDK 7 (64-bit) est dans le PATH avant la version 32-bit. Chemin du SDK 64-bit  : "},
            {"testBothSdkWrongOrder", "<color=yellow>TEST PARTIELLEMENT ÉCHOUÉ</color> → .NET SDK 7 (64-bit) est dans le PATH, MAIS il est après la version 32-bit (c'est un problème avec les versions pré 2024 de l'extension C# Dev Kit pour VSCode). Chemin du SDK 32-bit : "},
            {"testFailedNotFound", "<color=red>TEST ÉCHOUÉ</color> → .NET SDK 7 n'est pas trouvé dans le PATH. "}
        };

        // The dictionary to use for messages, depending on the current language:
        static private Dictionary<string, string> _messages = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr" ? _messagesFr : _messagesEn;

        /// <summary>
        /// Verify the .NET SDK 7 installation when the class is loaded.
        /// </summary>
        static DotNetSDKChecker()
        {
            VerifyDotNetSDK();
            //if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr") VerifyDotNetSDK(true); // To force the messages in English, uncomment this line
        }

        /// <summary>
        /// Verify the .NET SDK 7 installation by checking if the SDK is in the PATH.
        /// It considers every drive when it searches for the SDK.
        /// If both the 32-bit and 64-bit versions are installed, it checks if the 64-bit version is before the 32-bit version in the PATH.
        /// It then displays a message in the console.
        /// </summary>
        /// <param name="forceEnglishMessages">Allows to force all messages in English, even if the user system is in french.</param>
        static void VerifyDotNetSDK(bool forceEnglishMessages = false)
        {
            // This script is only intended for Windows, so we return early if the OS is not Windows:
            if (Application.platform != RuntimePlatform.WindowsEditor) return;

            if (forceEnglishMessages) _messages = _messagesEn;

            string dotnetPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            string systemPath = string.Format(_messages["systemPath"], dotnetPath);


            var drives = DriveInfo.GetDrives();
            bool has32Bit = false;
            bool has64Bit = false;
            bool has64BitFirst = true;
            string path64 = "";
            string path32 = "";

            foreach (var drive in drives)
            {
                if (!has32Bit)
                {
                    path32 = Path.Combine(drive.Name, "Program Files (x86)", _sdkFolderName);
                    has32Bit = dotnetPath.Contains(path32);
                }
                if (!has64Bit)
                {
                    path64 = Path.Combine(drive.Name, "Program Files", _sdkFolderName);
                    has64Bit = dotnetPath.Contains(path64);
                }

                if (has32Bit && has64Bit) break; // If both versions are found, we can stop searching
            }
            if (has64Bit && has32Bit) has64BitFirst = dotnetPath.IndexOf(path64) < dotnetPath.IndexOf(path32);

            // Check if the values are the same as the last time
            if (SessionState.GetInt("has32Bit", 0) == (has32Bit ? 1 : 0) &&
                SessionState.GetInt("has64Bit", 0) == (has64Bit ? 1 : 0) &&
                SessionState.GetInt("has64BitFirst", 0) == (has64BitFirst ? 1 : 0) &&
                SessionState.GetString("path32", "") == path32 &&
                SessionState.GetString("path64", "") == path64)
            {
                // The values are the same, so we return without displaying the messages
                return;
            }

            SaveCurrentResults(has32Bit, has64Bit, has64BitFirst, path32, path64);

            string messageText;

            if (has64Bit && !has32Bit) messageText = _messages["testSdk64Only"] + path64;
            else if (!has64Bit && has32Bit) messageText = _messages["testSdk32Only"] + path32;
            else if (has64Bit && has32Bit && has64BitFirst) messageText = _messages["testBothSdkCorrect"] + path64;
            else if (has64Bit && has32Bit && !has64BitFirst) messageText = _messages["testBothSdkWrongOrder"] + path32;
            else messageText = _messages["testFailedNotFound"];

            if (((has64Bit && !has32Bit) || (has64Bit && has32Bit && has64BitFirst)) && !_showPositiveMessages) return;

            messageText += _messages["explanation"] + systemPath;
            Debug.Log(messageText);
        }

        /// <summary>
        /// Save the current results in the SessionState, to avoid displaying the messages every time the script is loaded.
        /// </summary>
        private static void SaveCurrentResults(bool has32Bit, bool has64Bit, bool has64BitFirst, string path32, string path64)
        {
            // Store the new values
            SessionState.SetInt("has32Bit", has32Bit ? 1 : 0);
            SessionState.SetInt("has64Bit", has64Bit ? 1 : 0);
            SessionState.SetInt("has64BitFirst", has64BitFirst ? 1 : 0);
            SessionState.SetString("path32", path32);
            SessionState.SetString("path64", path64);
        }
    }
    #endif
}