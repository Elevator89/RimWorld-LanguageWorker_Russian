// Note:
// The first steps are with this tutorial already done:
// - The target .Net version is already switched to 3.5
// - The references are already set to UnityEngine and Assembly-CSharp
//   (Can all be done in the Projectsolution-Explorer)
//     => right-click on RimWorld_ExampleProject => Settings => ...
// So on we go...

// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Reflection;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
//using Verse.AI;          // Needed when you do something with the AI
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

// Note: If the usings are not found, (red line under it,) look into the folder '/Source-DLLs' and follow the instructions in the text files


// Now the program starts:
// Here I've provided the source code of the Dark Matter Generator. A small power generating structure based of the Plasma Generator from mrofa


// This is the namespace of your mod. Change it to your liking, but don't forget to change it in your XML too.
namespace LanguageWorkerRussian_Test
{
    // Now follows the class with the actual programm of the building. 

    // But first a few descriptions:

    // /// <summary>
    // /// This is an XML-Tag. It is good practice to make a short description of the function in these.
    // /// If you call the function from somewhere else, you'll get this summary text as a quick tooltip info.
    // /// Try the ToolTipSample in the class...
    // /// </summary>


    // class (This is a class)
    // Building_DarkMatterGenerator (This is the name of your new class) 
    // : Building (This is the base class 'Building'. Your class takes all the functions from the base class, but can override some of them to make it work how you like it to work.)
    //             This is, so that you don't need to write every function needed again and again. You write it once and take this as a base, so that the other classes have it already.
    //             The same goes for the error correction. If you find an error, you only have to correct it once and all your classes, that use this as a base, have the correction.


    // Note: The following code will be partially created, if you write /// in the line before a class, function, ...
    /// <summary>
    /// This is the main class for the Dark Matter Generator.
    /// </summary>
    /// <author>mrofa, Haplo</author>
    /// <permission>Usage of this code is .....</permission>
    /// 
    [StaticConstructorOnStartup]
    public static class LanguageWorkerRussian_Mod
    {
        static LanguageWorkerRussian_Mod()
        {
            try
            {
                FieldInfo loadedLanguageField = typeof (LoadedLanguage)
                    .GetField("workerInt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                Log.MessageFormat("Field loaded: {0}, type: {1}", loadedLanguageField.Name, loadedLanguageField.FieldType);

				loadedLanguageField.SetValue(LanguageDatabase.activeLanguage, new LanguageWorker_Russian_Modified());

				Log.Message("Field is set");
            }
            catch (Exception ex)
            {
                Log.Message(ex.Message);
            }
        }
    }
}
