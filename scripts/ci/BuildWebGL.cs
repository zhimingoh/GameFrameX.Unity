using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CI
{
    public static class BuildWebGL
    {
        public static void Build()
        {
            // Optional pre-steps controlled by env vars
            RunOptional("ENABLE_HYBRIDCLR", "HYBRIDCLR_METHOD");
            RunOptional("ENABLE_YOOASSET", "YOOASSET_METHOD");

            var buildPath = "Build/WebGL";
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("WebGL build failed: " + report.summary.result);
            }
        }

        private static void RunOptional(string enableEnv, string methodEnv)
        {
            var enable = GetEnv(enableEnv);
            if (!string.Equals(enable, "true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var methodName = GetEnv(methodEnv);
            if (string.IsNullOrEmpty(methodName))
            {
                throw new Exception($"{methodEnv} is required when {enableEnv}=true");
            }

            InvokeStaticMethod(methodName);
        }

        private static string GetEnv(string key)
        {
            return Environment.GetEnvironmentVariable(key) ?? string.Empty;
        }

        private static void InvokeStaticMethod(string fullMethodName)
        {
            // Supports "Namespace.Type.Method" or "Type.Method"
            var lastDot = fullMethodName.LastIndexOf('.');
            if (lastDot <= 0 || lastDot == fullMethodName.Length - 1)
            {
                throw new Exception("Invalid method name: " + fullMethodName);
            }

            var typeName = fullMethodName.Substring(0, lastDot);
            var methodName = fullMethodName.Substring(lastDot + 1);

            var type = FindType(typeName);
            if (type == null)
            {
                throw new Exception("Type not found: " + typeName);
            }

            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                throw new Exception("Method not found: " + fullMethodName);
            }

            method.Invoke(null, null);
        }

        private static Type FindType(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
