using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DMSLib;

namespace DMS_Cli
{
    internal static class Program
    {
        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };

        private static int Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            var stdout = Console.Out;
            if (args.Length == 0 || args.Contains("--help"))
            {
                stdout.WriteLine("DMS.Cli add-columns --input source.dat --changes columns.json [--output modified.dat] [--dry-run] [--report result.json]\n" +
                    "DMS.Cli sync-schema --input source.dat --project project.XML [--overrides defaults.json] [--output modified.dat] [--dry-run] [--report result.json]\n" +
                    "Project may also be a folder containing exactly one XML file.\n" +
                    "Use --dry-run to preview. Saves are verified; a schema sync with no additions creates no duplicate DAT.\n" +
                    "Defaults must be explicit: JSON null = DAT NULL, \" \" = a literal space.\n" +
                    "Output and report must be new files. No input is modified. Python is not required.");
                return 0;
            }
            var workflow = new Workflow();
            try
            {
                // Library diagnostics go to stderr; stdout is one UTF-8 JSON result.
                Console.SetOut(Console.Error);
                Execute(args, workflow);
            }
            catch (Exception ex)
            {
                workflow.Result["success"] = false;
                workflow.Result["error"] = ex.Message;
            }
            finally { Console.SetOut(stdout); }
            stdout.WriteLine(workflow.Complete(Json));
            return (bool)workflow.Result["success"] ? 0 : 1;
        }

        private static void Execute(string[] args, Workflow workflow)
        {
            var sync = args[0] == "sync-schema";
            if (args[0] != "add-columns" && !sync) throw new ArgumentException("Unknown command. Use --help.");
            var options = Options(args.Skip(1).ToArray(), sync);
            var input = Path.GetFullPath(Required(options, "--input"));
            var dryRun = options.ContainsKey("--dry-run");
            var output = options.ContainsKey("--output") ? Path.GetFullPath(Required(options, "--output")) : null;
            var reportPath = options.ContainsKey("--report") ? Path.GetFullPath(Required(options, "--report")) : null;
            if (!dryRun && output == null) throw new ArgumentException("--output is required unless --dry-run is specified.");
            var project = sync ? Workflow.ResolveProject(Required(options, "--project")) : null;
            var overrides = sync && options.ContainsKey("--overrides") ? Path.GetFullPath(Required(options, "--overrides")) : null;
            var changesPath = sync ? null : Path.GetFullPath(Required(options, "--changes"));
            workflow.Result["dryRun"] = dryRun;
            workflow.Result["input"] = input;
            workflow.Result["output"] = output;
            if (sync) workflow.Result["project"] = project;
            workflow.Initialize(new[] { input, project, overrides, changesPath }, output, reportPath);
            var file = DMSReader.Read(input);
            if (file.Endian != "LE") throw new NotSupportedException("Editing big-endian DAT files is not supported by the writer.");
            Validate(file);
            workflow.Result["stage"] = "preview";
            if (sync)
            {
                var result = SchemaSync.Prepare(file, project, overrides);
                workflow.Result["changes"] = result.Changes;
                workflow.Result["differences"] = result.Differences;
                workflow.Result["skipped"] = result.Skipped;
                workflow.Result["unresolved"] = result.Unresolved;
                if (result.Unresolved.Count > 0)
                    throw new ArgumentException("Schema sync needs explicit overrides; no output was written.");
                result.Apply(); // Fully validate the planned additions in memory before saving.
                Validate(file);
                if (!dryRun && result.Changes.Count > 0)
                {
                    workflow.Result["stage"] = "save";
                    SaveVerified(file, output, reread =>
                    {
                        // Overrides apply only to missing fields, so omit them for this check.
                        var check = SchemaSync.Prepare(reread, project, null);
                        var clean = check.Changes.Count == 0 && check.Unresolved.Count == 0;
                        workflow.Result["postcheck"] = new { success = clean, changes = check.Changes,
                            differences = check.Differences, skipped = check.Skipped, unresolved = check.Unresolved };
                        if (!clean) throw new InvalidDataException("Saved DAT still needs additions; no output was published.");
                        workflow.AssertInputsUnchanged();
                    });
                    workflow.Saved(output);
                }
                else if (!dryRun) workflow.Result["stage"] = "already-complete";
                workflow.Result["success"] = true;
                return;
            }
            var additions = new List<object>();
            workflow.Result["changes"] = additions;
            foreach (var change in ReadPlan(changesPath))
            {
                var tableName = String(change, "table");
                var name = String(change, "name");
                var tables = file.Tables.Where(t => t.Name == tableName).ToList();
                if (tables.Count == 0) throw new ArgumentException("Table not found: " + tableName);
                var type = EnumValue<FieldTypes>(String(change, "type"));
                var column = new DMSNewColumn(name, Integer(change, "version", null), Integer(change, "length", null),
                    Integer(change, "decimalPositions", 0), (UseEditFlags)Unsigned(change, "useEdit", 0), type,
                    EnumValue<FieldFormats>(OptionalString(change, "format", "UPPER_OR_DEFAULT")),
                    EnumValue<GUIControls>(OptionalString(change, "gui", "DEFAULT")));
                if (!change.ContainsKey("after") || !change.ContainsKey("default"))
                    throw new ArgumentException("Each column requires explicit after and default properties (null is allowed).");
                var after = NullableString(change, "after");
                var value = NullableString(change, "default");
                foreach (var table in tables)
                {
                    var preceding = after == null ? null : table.GetColumnByName(after);
                    if (after != null && preceding == null) throw new ArgumentException("Preceding field not found: " + tableName + "." + after);
                    table.AddColumn(column, preceding, value);
                }
                additions.Add(new { table = tableName, field = name, after, type = type.ToString(),
                    exports = tables.Count, rows = tables.Sum(t => t.Rows.Count), defaultValue = value });
            }
            Validate(file);
            if (!dryRun)
            {
                workflow.Result["stage"] = "save";
                SaveVerified(file, output, reread => workflow.AssertInputsUnchanged());
                workflow.Saved(output);
            }
            workflow.Result["success"] = true;
        }

        private static Dictionary<string, string> Options(string[] args, bool sync)
        {
            var result = new Dictionary<string, string>();
            for (var i = 0; i < args.Length; i++)
            {
                var name = args[i];
                if (result.ContainsKey(name)) throw new ArgumentException("Duplicate option: " + name);
                if (name == "--dry-run") { result.Add(name, "true"); continue; }
                if (name != "--input" && name != "--output" && name != "--report" &&
                    !(sync ? name == "--project" || name == "--overrides" : name == "--changes"))
                    throw new ArgumentException("Unknown option: " + name);
                if (++i == args.Length || args[i].StartsWith("--")) throw new ArgumentException("Missing value for " + name);
                result.Add(name, args[i]);
            }
            return result;
        }

        private static string Required(Dictionary<string, string> options, string key)
        {
            string value;
            if (!options.TryGetValue(key, out value) || string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Missing " + key);
            return value;
        }

        private static List<Dictionary<string, object>> ReadPlan(string path)
        {
            var root = Json.DeserializeObject(File.ReadAllText(path)) as Dictionary<string, object>;
            if (root == null || root.Count != 1 || !root.ContainsKey("columns")) throw new ArgumentException("Plan must contain only a columns array.");
            var columns = root["columns"] as object[];
            if (columns == null || columns.Length == 0) throw new ArgumentException("columns must be a nonempty array.");
            var allowed = new[] { "table", "name", "after", "type", "length", "version", "decimalPositions", "useEdit", "format", "gui", "default" };
            return columns.Select(item =>
            {
                var column = item as Dictionary<string, object>;
                if (column == null || column.Keys.Except(allowed).Any()) throw new ArgumentException("Invalid column object or unknown property.");
                return column;
            }).ToList();
        }

        private static string String(Dictionary<string, object> obj, string key)
        {
            var value = NullableString(obj, key);
            if (string.IsNullOrEmpty(value)) throw new ArgumentException(key + " must be a nonempty string.");
            return value;
        }

        private static string NullableString(Dictionary<string, object> obj, string key)
        {
            object value;
            if (!obj.TryGetValue(key, out value) || (value != null && !(value is string))) throw new ArgumentException(key + " must be a string or null.");
            return (string)value;
        }

        private static string OptionalString(Dictionary<string, object> obj, string key, string fallback)
        {
            return obj.ContainsKey(key) ? String(obj, key) : fallback;
        }

        private static int Integer(Dictionary<string, object> obj, string key, int? fallback)
        {
            object value;
            if (!obj.TryGetValue(key, out value))
            {
                if (fallback.HasValue) return fallback.Value;
                throw new ArgumentException("Missing " + key);
            }
            if (!(value is int)) throw new ArgumentException(key + " must be a 32-bit integer.");
            return (int)value;
        }

        private static uint Unsigned(Dictionary<string, object> obj, string key, uint fallback)
        {
            object value;
            if (!obj.TryGetValue(key, out value)) return fallback;
            if (!(value is int) && !(value is long)) throw new ArgumentException(key + " must be an unsigned 32-bit integer.");
            return checked((uint)Convert.ToInt64(value));
        }

        private static T EnumValue<T>(string value) where T : struct
        {
            // Require enum names, not undefined integer strings.
            if (!Enum.GetNames(typeof(T)).Contains(value)) throw new ArgumentException("Unknown " + typeof(T).Name + ": " + value);
            return (T)Enum.Parse(typeof(T), value);
        }

        private static void Validate(DMSFile file)
        {
            foreach (var table in file.Tables)
            {
                var names = table.Columns.Select(c => c.Name).ToList();
                if (names.Count != table.Metadata.FieldCount || names.Count != table.Metadata.FieldMetadata.Count ||
                    names.Distinct().Count() != names.Count || !names.SequenceEqual(table.Metadata.FieldMetadata.Select(f => f.FieldName)) ||
                    table.Rows.Any(r => r.ColumnCount != names.Count))
                    throw new InvalidDataException("Inconsistent column metadata or rows in " + table.Name);
            }
        }

        private static byte[] Serialize(DMSFile file)
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 1024, true)) file.WriteToStream(sw);
                return ms.ToArray();
            }
        }

        private static void SaveVerified(DMSFile file, string output, Action<DMSFile> beforePublish)
        {
            // Keep the temporary file beside the destination so the final move stays on one volume.
            var temp = Path.Combine(Path.GetDirectoryName(output), ".dms-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                DMSWriter.Write(temp, file);
                var reread = DMSReader.Read(temp);
                Validate(reread);
                if (!Serialize(file).SequenceEqual(Serialize(reread)))
                    throw new InvalidDataException("Saved DAT failed read-back verification; no output was published.");
                beforePublish(reread);
                File.Move(temp, output); // Fails rather than replacing an existing destination.
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }
}
