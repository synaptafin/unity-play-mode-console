# LaunchableAttribute Analyzer

## Publish DLL

```bash
dotnet pack Analyzers~.csproj -c Release -o ./output
```

## Copy DLL to Unity Project

## Required Step Before use .dll as Analyzer

In .dll asset Inspector

- Under the Select platforms for plugin heading, disable Any Platform.
- Under the Include Platforms heading, disable Editor and Standalone.
- Under the Asset Labels heading in the bottom-right of Plugin Inspector window, click on the blue label icon to open the Asset Labels sub-menu.
- **Manually exactly add label "RoslynAnalyzer" and assign it to the .dll asset.**

