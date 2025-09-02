using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

namespace Synaptafin.PlayModeConsole {

  public class ElementUxmlAssets {

    private static readonly Lazy<VisualTreeAsset> _commandItemAsset = new Lazy<VisualTreeAsset>(() => {
        // Get the directory of this script
        string scriptPath = AssetDatabase.FindAssets("t:Script ElementUxmlAssets")
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(path => path.EndsWith("ElementUxmlAssets.cs"));
            
        if (string.IsNullOrEmpty(scriptPath)) 
            return null;
            
        // Get the directory of the script
        string scriptDirectory = Path.GetDirectoryName(scriptPath);
        
        // Navigate to the UI folder (one level up, then into UI)
        string uiDirectory = Path.GetFullPath(Path.Combine(scriptDirectory, "..", "UI"));
        
        // Make the path relative to the Assets folder
        string relativePath = uiDirectory.Substring(uiDirectory.IndexOf("Assets"));
        relativePath = relativePath.Replace('\\', '/');
        
        // Find the UXML asset
        string uiAssetPath = Path.Combine(relativePath, "CommandItem.uxml").Replace('\\', '/');
        
        // Load the asset
        return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uiAssetPath);
    });

    public static VisualTreeAsset CommandItemAsset => _commandItemAsset.Value;
  }
}