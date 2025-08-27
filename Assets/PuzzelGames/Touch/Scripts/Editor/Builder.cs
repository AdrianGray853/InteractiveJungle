using System.Collections;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Interactive.Touch
{
using System.Collections;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

    public static class Builder
    {
        [MenuItem("Build/Build iOS")]
        public static void BuildiOS()
        {
            string[] scenes = GetScenePaths();
            string outputPath = "Build";
            string buildName = "iOS";
            BuildOptions buildOptions = BuildOptions.None;
            BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath + "/" + buildName, BuildTarget.iOS, buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        [MenuItem("Build/Build Amazon")]
        public static void BuildAmazon()
        {
            string[] scenes = GetScenePaths();
            string outputPath = "Build";
            string buildName = "Amazon.apk";
            BuildOptions buildOptions = BuildOptions.None;
            EditorUserBuildSettings.development = false;
            BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath + "/" + buildName, BuildTarget.Android, buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        [MenuItem("Build/Build Android")]
        public static void BuildAndroid()
        {
            string[] scenes = GetScenePaths();
            string outputPath = "Build";
            string buildName = "Android.aab";
            BuildOptions buildOptions = BuildOptions.None;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            //EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = System.IO.Path.GetFullPath("GooglePlay/user.keystore");
            PlayerSettings.Android.keyaliasName = "appstorepublish";
            PlayerSettings.Android.keystorePass = "123123AGrai";
            PlayerSettings.Android.keyaliasPass = "123123AGrai";
            PlayerSettings.Android.minifyWithR8 = true;
            PlayerSettings.Android.minifyRelease = true;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.Android.splitApplicationBinary = true;        
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath + "/" + buildName, BuildTarget.Android, buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("Build failed");
            }
        }

        private static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }
    }

}