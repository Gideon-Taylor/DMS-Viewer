using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace DMS_Cli
{
    // Shared workflow for direct CLI users and agents. No interpreter is required.
    internal sealed class Workflow
    {
        internal readonly Dictionary<string, object> Result = new Dictionary<string, object>
        {
            { "success", false }, { "stage", "setup" }, { "written", false }
        };
        private readonly Dictionary<string, string> inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string reportPath;

        internal void Initialize(IEnumerable<string> inputPaths, string output, string report)
        {
            var paths = inputPaths.Where(p => p != null).Select(Path.GetFullPath).ToList();
            if (output != null && (paths.Contains(output, StringComparer.OrdinalIgnoreCase) || File.Exists(output) || Directory.Exists(output)))
                throw new ArgumentException("Output must be a new file, different from every input.");
            if (report != null && (paths.Contains(report, StringComparer.OrdinalIgnoreCase) ||
                string.Equals(report, output, StringComparison.OrdinalIgnoreCase) || File.Exists(report) || Directory.Exists(report)))
                throw new ArgumentException("Report must be a new file, different from inputs and output.");
            if (output != null && !Directory.Exists(Path.GetDirectoryName(output)))
                throw new DirectoryNotFoundException("Output directory does not exist.");
            if (report != null && !Directory.Exists(Path.GetDirectoryName(report)))
                throw new DirectoryNotFoundException("Report directory does not exist.");
            foreach (var path in paths) inputs[path] = Hash(path);
            // Only enable report writing after all path checks and fingerprints succeed.
            reportPath = report;
        }

        internal static string ResolveProject(string path)
        {
            path = Path.GetFullPath(path);
            if (!Directory.Exists(path)) return path;
            var files = Directory.GetFiles(path).Where(p => string.Equals(Path.GetExtension(p), ".xml", StringComparison.OrdinalIgnoreCase)).ToList();
            if (files.Count != 1) throw new ArgumentException("Project folder must contain exactly one XML file; supply the intended XML path.");
            return files[0];
        }

        internal static string Hash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        internal void AssertInputsUnchanged()
        {
            if (inputs.Any(p => Hash(p.Key) != p.Value))
                throw new IOException("An input changed during processing; no output should be used without review.");
        }

        internal void Saved(string output)
        {
            Result["written"] = true;
            Result["outputSha256"] = Hash(output);
            Result["stage"] = "verified";
        }

        internal string Complete(JavaScriptSerializer json)
        {
            try
            {
                AssertInputsUnchanged();
                Result["inputVerification"] = new { unchanged = inputs.Count > 0 ? (bool?)true : null, sha256 = inputs };
            }
            catch (Exception ex)
            {
                Result["success"] = false;
                Result["verificationError"] = ex.Message;
                Result["inputVerification"] = new { unchanged = false, sha256 = inputs };
            }
            if (reportPath != null)
            {
                try
                {
                    using (var stream = new FileStream(reportPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                        writer.WriteLine(json.Serialize(Result));
                }
                catch (Exception ex)
                {
                    Result["success"] = false;
                    Result["reportError"] = ex.Message;
                }
            }
            return json.Serialize(Result);
        }
    }
}
