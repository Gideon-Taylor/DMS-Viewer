using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using DMSLib;

namespace DMS_Cli
{
    internal sealed class SchemaSync
    {
        internal readonly List<object> Changes = new List<object>();
        internal readonly List<object> Differences = new List<object>();
        internal readonly List<object> Skipped = new List<object>();
        internal readonly List<string> Unresolved = new List<string>();
        private readonly List<Action> additions = new List<Action>();

        internal void Apply()
        {
            foreach (var add in additions) add();
        }

        private sealed class Project
        {
            internal readonly Dictionary<string, XElement> Records = new Dictionary<string, XElement>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> Versions = new Dictionary<string, int>(StringComparer.Ordinal);

            internal Project(string path)
            {
                // App Designer exports multiple top-level instance elements, not one XML root.
                // Never resolve DTDs or external entities from an imported project.
                var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment,
                    DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 64 * 1024 * 1024 };
                using (var reader = XmlReader.Create(path, settings))
                {
                    while (!reader.EOF)
                    {
                        if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "instance") { reader.Read(); continue; }
                        var instance = (XElement)XNode.ReadFrom(reader);
                        var cls = (string)instance.Attribute("class");
                        if (cls == "RDM")
                        {
                            var row = Definition(instance, "RecDefn");
                            Records.Add(Required(row, "szRecName"), row);
                        }
                        else if (cls == "FIELD")
                        {
                            var row = Definition(instance, "Field");
                            Versions.Add(Required(row, "szFieldName"), Number(row, "lVersion"));
                        }
                    }
                }
                if (Records.Count == 0) throw new InvalidDataException("Project XML contains no record definitions.");
            }

            internal List<XElement> Expand(string name, HashSet<string> ancestors = null)
            {
                ancestors = ancestors ?? new HashSet<string>(StringComparer.Ordinal);
                if (ancestors.Count >= 64) throw new InvalidDataException("Subrecord nesting exceeds 64 levels.");
                if (!ancestors.Add(name)) throw new InvalidDataException("Recursive subrecord: " + name);
                XElement record;
                if (!Records.TryGetValue(name, out record)) throw new InvalidDataException("Subrecord definition missing from XML: " + name);
                var rowset = record.Element("hRft")?.Elements("rowset").SingleOrDefault(e => (string)e.Attribute("name") == "RecField");
                if (rowset == null) throw new InvalidDataException("Record has no RecField rowset: " + name);
                var result = new List<XElement>();
                foreach (var field in rowset.Elements("row"))
                {
                    if (Number(field, "eFieldType") == 7)
                    {
                        var sub = Required(field, "atmFieldName");
                        XElement subrecord;
                        if (!Records.TryGetValue(sub, out subrecord) || Number(subrecord, "eRecType") != 3)
                            throw new InvalidDataException("Missing or invalid subrecord definition: " + sub);
                        result.AddRange(Expand(sub, ancestors));
                    }
                    else result.Add(field);
                }
                ancestors.Remove(name);
                var names = result.Select(f => Required(f, "atmFieldName")).ToList();
                if (names.Distinct().Count() != names.Count) throw new InvalidDataException("Duplicate expanded field in " + name);
                return result;
            }

            private static XElement Definition(XElement instance, string name)
            {
                var row = instance.Elements("rowset").SingleOrDefault(e => (string)e.Attribute("name") == name)?.Elements("row").SingleOrDefault();
                if (row == null) throw new InvalidDataException("Missing " + name + " definition row.");
                return row;
            }
        }

        private sealed class Override
        {
            internal bool HasDefault;
            internal string Default;
            internal int? Version;
            internal bool Used;
        }

        internal static SchemaSync Prepare(DMSFile file, string path, string overridesPath)
        {
            var project = new Project(path);
            var overrides = ReadOverrides(overridesPath);
            var result = new SchemaSync();
            for (var export = 0; export < file.Tables.Count; export++)
            {
                var table = file.Tables[export];
                XElement record;
                string reason = null;
                if (table.Name.StartsWith("AUDIT_", StringComparison.Ordinal)) reason = "audit-record";
                else if (!project.Records.TryGetValue(table.Name, out record)) reason = "not-in-project";
                else if (Number(record, "eRecType") != 0) reason = "not-sql-table";
                if (reason != null) { result.Skipped.Add(new { table = table.Name, exportIndex = export, reason }); continue; }

                var fields = project.Expand(table.Name);
                var names = fields.Select(f => Required(f, "atmFieldName")).ToList();
                var old = table.Columns.Select(c => c.Name).ToList();
                var common = old.Where(names.Contains).ToList();
                var desiredCommon = names.Where(old.Contains).ToList();
                if (!common.SequenceEqual(desiredCommon))
                    result.Differences.Add(new { table = table.Name, exportIndex = export, kind = "existing-column-order", actual = common, expected = desiredCommon });
                foreach (var extra in old.Except(names))
                    result.Differences.Add(new { table = table.Name, exportIndex = export, field = extra, kind = "dat-only-field" });

                for (var i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];
                    var name = names[i];
                    var label = table.Name + "." + name;
                    var type = Defined<FieldTypes>(Number(field, "eFieldType"));
                    var length = Number(field, "nLength");
                    var scale = Number(field, "nDecimalPos");
                    var format = Defined<FieldFormats>(Number(field, "eFormat"));
                    var gui = Defined<GUIControls>(Number(field, "nDefGUICtl"));
                    var useEdit = uint.Parse(Required(field, "fUseEdit"), CultureInfo.InvariantCulture);
                    if (old.Contains(name))
                    {
                        var index = old.IndexOf(name);
                        var metadata = table.Metadata.FieldMetadata[index];
                        Difference(result, table.Name, export, name, "fieldType", metadata.FieldType, type);
                        Difference(result, table.Name, export, name, "length", metadata.FieldLength, length);
                        Difference(result, table.Name, export, name, "decimalPositions", metadata.DecimalPositions, scale);
                        Difference(result, table.Name, export, name, "format", metadata.FieldFormat, format);
                        Difference(result, table.Name, export, name, "gui", metadata.DefaultGUIControl, gui);
                        Difference(result, table.Name, export, name, "useEdit", (uint)metadata.UseEditMask, useEdit);
                        Difference(result, table.Name, export, name, "columnType", table.Columns[index].Type, ColumnType(type));
                        var expectedSize = length.ToString(CultureInfo.InvariantCulture) +
                            (scale > 0 ? "," + scale.ToString(CultureInfo.InvariantCulture) : "");
                        Difference(result, table.Name, export, name, "columnSize", table.Columns[index].Size, expectedSize);
                        continue;
                    }

                    Override replacement;
                    overrides.TryGetValue(label, out replacement);
                    if (replacement != null) replacement.Used = true;
                    int version;
                    var hasVersion = project.Versions.TryGetValue(name, out version);
                    if (replacement?.Version != null) { version = replacement.Version.Value; hasVersion = true; }
                    if (!hasVersion) result.Unresolved.Add(label + ": field version missing; include its FIELD definition or supply a version override.");
                    string value = null;
                    string defaultSource;
                    if (replacement != null && replacement.HasDefault) { value = replacement.Default; defaultSource = "override"; }
                    else if (!string.IsNullOrEmpty(Text(field, "atmDefRecName")))
                    {
                        defaultSource = "unresolved";
                        result.Unresolved.Add(label + ": XML default references a record field; supply a default override.");
                    }
                    else if (field.Element("atmDefFieldName") != null && field.Element("atmDefFieldName").Value.Length > 0)
                    { value = field.Element("atmDefFieldName").Value; defaultSource = "xml"; }
                    else if (type == FieldTypes.CHAR || type == FieldTypes.IMAGE_REF || type == FieldTypes.LONG_CHAR)
                    { value = " "; defaultSource = "character-blank"; }
                    else if (type == FieldTypes.DATE || type == FieldTypes.TIME || type == FieldTypes.DATETIME)
                    { defaultSource = "temporal-null"; }
                    else
                    {
                        defaultSource = "unresolved";
                        result.Unresolved.Add(label + ": no XML default; supply an explicit default override for " + type + ".");
                    }
                    var definition = new DMSNewColumn(name, version, length, scale, (UseEditFlags)useEdit, type, format, gui);
                    // Choose the XML predecessor even if existing columns have a different order.
                    // Earlier missing predecessors are added first; no existing field is moved.
                    var after = i == 0 ? null : names[i - 1];
                    result.Changes.Add(new { table = table.Name, exportIndex = export, field = name, after,
                        type = type.ToString(), length, decimalPositions = scale, version = hasVersion ? (int?)version : null,
                        useEdit, format = format.ToString(), gui = gui.ToString(), defaultValue = value, defaultSource, rows = table.Rows.Count });
                    result.additions.Add(() => table.AddColumn(definition, after == null ? null : table.GetColumnByName(after), value));
                }
            }
            foreach (var entry in overrides.Where(e => !e.Value.Used))
                result.Unresolved.Add(entry.Key + ": override does not match any missing field in an included SQL table.");
            return result;
        }

        private static void Difference<T>(SchemaSync result, string table, int export, string field, string property, T actual, T expected)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
                result.Differences.Add(new { table, exportIndex = export, field, kind = "existing-field-property", property,
                    actual = actual.ToString(), expected = expected.ToString() });
        }

        private static Dictionary<string, Override> ReadOverrides(string path)
        {
            var result = new Dictionary<string, Override>(StringComparer.Ordinal);
            if (path == null) return result;
            var json = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            var root = json.DeserializeObject(File.ReadAllText(path)) as Dictionary<string, object>;
            if (root == null || root.Count != 1 || !root.ContainsKey("fields") || !(root["fields"] is object[]))
                throw new ArgumentException("Overrides must contain only a fields array.");
            foreach (var item in (object[])root["fields"])
            {
                var obj = item as Dictionary<string, object>;
                var allowed = new[] { "table", "name", "default", "version" };
                if (obj == null || obj.Keys.Except(allowed).Any() || !obj.ContainsKey("table") || !obj.ContainsKey("name") ||
                    !(obj["table"] is string) || !(obj["name"] is string) || string.IsNullOrWhiteSpace((string)obj["table"]) ||
                    string.IsNullOrWhiteSpace((string)obj["name"]) || (!obj.ContainsKey("default") && !obj.ContainsKey("version")))
                    throw new ArgumentException("Each override requires table, name, and default and/or version.");
                var replacement = new Override { HasDefault = obj.ContainsKey("default") };
                if (replacement.HasDefault)
                {
                    if (obj["default"] != null && !(obj["default"] is string)) throw new ArgumentException("Override default must be a string or null.");
                    replacement.Default = (string)obj["default"];
                }
                if (obj.ContainsKey("version"))
                {
                    if (!(obj["version"] is int) || (int)obj["version"] < 0) throw new ArgumentException("Override version must be a nonnegative integer.");
                    replacement.Version = (int)obj["version"];
                }
                result.Add((string)obj["table"] + "." + (string)obj["name"], replacement);
            }
            return result;
        }

        private static string Text(XElement row, string name) { return ((string)row.Element(name) ?? "").Trim(); }
        private static string ColumnType(FieldTypes type)
        {
            switch (type)
            {
                case FieldTypes.IMAGE_REF: return "CHAR";
                case FieldTypes.LONG_CHAR: return "LONG";
                case FieldTypes.SIGNED_NUMBER: return "NUMBER";
                case FieldTypes.IMG_OR_ATTACH: return "IMAGE";
                default: return type.ToString();
            }
        }
        private static string Required(XElement row, string name)
        {
            var value = Text(row, name);
            if (value.Length == 0) throw new InvalidDataException("Missing XML property: " + name);
            return value;
        }
        private static int Number(XElement row, string name) { return int.Parse(Required(row, name), CultureInfo.InvariantCulture); }
        private static T Defined<T>(int value) where T : struct
        {
            var parsed = (T)Enum.ToObject(typeof(T), value);
            if (Convert.ToInt64(parsed, CultureInfo.InvariantCulture) != value || !Enum.IsDefined(typeof(T), parsed))
                throw new InvalidDataException("Unknown XML " + typeof(T).Name + ": " + value);
            return parsed;
        }
    }
}
