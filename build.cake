#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"

using Cake.Common.Tools.GitVersion;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
string projectName = "MultiFileStream";
string buildDir = Directory("./src/" + projectName + "/bin") + Directory(configuration);
string outputDir = Directory("./output");
string solutionPath = "./src/" + projectName + ".sln";

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(outputDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() => NuGetRestore(solutionPath));

Task("Update-Assembly-Info")
    .Does(() => GitVersion(new GitVersionSettings { UpdateAssemblyInfo = true }));

Task("Build")
    .IsDependentOn("Update-Assembly-Info")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() => MSBuild(solutionPath, settings => settings.SetConfiguration(configuration)));

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() => XUnit2(GetFiles("./src/**/bin/Release/*.Specs.dll")));

Task("Create-Nuget-Package")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
     NuGetPack(GetFiles("./**/" + projectName + ".nuspec"), new NuGetPackSettings {
        Version           = GitVersion(new GitVersionSettings()).AssemblySemVer,
        NoPackageAnalysis = true,
        Files             = new [] {
                                new NuSpecContent {Source = Directory(buildDir) + File(projectName +".dll"), Target = "lib/net45" },
                            },
        BasePath          = ".",
        OutputDirectory   = outputDir
    });
});

Task("Push-Nuget-Package")
    .IsDependentOn("Create-Nuget-Package")
    .Does(() =>
{
    var package = Directory(outputDir) + File(projectName + "." + GitVersion(new GitVersionSettings()).SemVer + ".nupkg");
    NuGetPush(package, new NuGetPushSettings {
        Source = "https://api.nuget.org/v3/index.json",
        ApiKey = EnvironmentVariable("NugetApiKey")
    });
});

Task("Default").IsDependentOn("Run-Unit-Tests");
Task("Package").IsDependentOn("Create-Nuget-Package");
Task("Publish").IsDependentOn("Push-Nuget-Package");
RunTarget(target);