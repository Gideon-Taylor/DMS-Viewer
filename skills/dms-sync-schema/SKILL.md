---
name: dms-sync-schema
description: Compare and correct PeopleSoft Data Mover DAT exports against App Designer project XML. Use when given a DAT and project XML to identify or add missing columns, expand subrecords, and save a verified corrected copy with DMS CLI.
---

# DMS Schema Sync

Use `DMS.Cli.exe` directly. It handles preview validation, input fingerprints,
verified saves, post-save schema comparison and JSON reports in C#. End users need
Windows and .NET Framework 4.8, with no Python, agent SDK or vendor-specific plugin.
See the [repository build instructions](../../README.md#building-and-testing) for
building from source. Python is used only by development tests.

Locate the executable supplied by the user, a built checkout at
`DMS.Cli/bin/Release/DMS.Cli.exe`, or a packaged runtime alongside this skill at
`tools/dms-cli/DMS.Cli.exe`. Keep it with its built DLLs; do not copy just the EXE.
When this skill is copied into an agent's skill folder, point the agent at the
actual executable. No launcher, `--cli` option, or agent-specific configuration is
required. The repository skill contains instructions only, not compiled binaries.

## Inputs and scope

Accept the DAT path and App Designer project XML path. A project folder containing
exactly one XML is also accepted. Resolve ambiguous filenames before editing.

A request to compare only produces a preview. A request to correct/sync the DAT
includes authorization to create a separate corrected copy: preview, inspect the
result, then apply ordinary resolved additions without asking for approval again.
Use the user's destination/name when specified; otherwise save a clearly named
corrected copy in an agreed working/output folder. When dating a name, use the user's
current date and timezone. Never overwrite or modify the original DAT or XML.
Do not upload inputs or reports to GitHub or commit customer files to the tool repo.

The comparison uses actual XML record definitions, not saved comparison flags:

- Include SQL Table records (`eRecType=0`), excluding names starting `AUDIT_`.
- Treat DAT records absent from the project as unchanged for this workflow.
- Expand nested subrecords into physical fields. A subrecord name is not a DAT column.
- Add only missing fields, with metadata and predecessor positions from XML.
- Preserve existing fields, row values, and existing column order. Report DAT-only
  fields and existing metadata/order differences without changing them. A successful
  additions-only sync does not mean the entire schema is identical.
- Field versions come from standalone FIELD definitions, never the parent record version.

## Run

Invoke the executable with argument-safe shell quoting. For example, in PowerShell:

```powershell
$cli = '<absolute-path>\DMS.Cli.exe'
& $cli sync-schema --input '<source.dat>' --project '<project.XML-or-folder>' --dry-run --report '<new-preview.json>'
```

Read the JSON `changes`, `differences`, `skipped`, and `unresolved` from stdout or
the optional report. Diagnostics go to stderr; stdout is one UTF-8 JSON object.
Exit code 0 means success, 1 means a failed or unresolved operation. A preview
validates the additions in memory; it never creates a DAT.

For a correction request with resolved changes:

```powershell
& $cli sync-schema --input '<source.dat>' --project '<project.XML-or-folder>' --output '<new-corrected.dat>' --report '<new-result.json>'
```

The CLI validates the plan before writing, checks input SHA-256 hashes, writes a
temporary DAT, reopens and verifies it, and reruns schema comparison to confirm
zero remaining additions before publishing. It never overwrites input, output or
report files. If no additions are needed, it reports `stage: "already-complete"`,
`written: false` and creates no duplicate DAT. `--output` is still required for a
non-preview run; report paths must be new too.

Successful saves report `stage: "verified"`, `written: true`, `outputSha256`, and a
successful `postcheck` with no changes. Check `inputVerification.unchanged` as well.
Failures after valid path setup can still produce a diagnostic JSON report; a
setup-path error does not create a report. If reporting fails after a DAT was
saved, the result is unsuccessful and includes `reportError` and `written: true`;
review the result rather than assuming that no output exists.

Use locations the user has authorized and follow the executing environment's
filesystem permissions. This skill does not expand that authorization.

## Defaults and unresolved choices

The CLI uses XML constant defaults where provided. Otherwise it infers one literal
space for character fields and proper DAT NULLs for DATE/TIME/DATETIME. A date NULL
is a zero byte, encoded `B(AA)` on disk; it is not a space or the text `NULL`.
Numeric/binary fields without defaults and defaults referencing another record need
explicit choices. Missing standalone field versions also need explicit information.

When `unresolved` is nonempty, report the affected fields and request only the
missing values or versions. Do not guess numeric defaults or substitute a record
version. Use supplied authoritative field definitions/versions when available.
Add an override file containing only the missing-field exceptions needed:

```json
{"fields":[{"table":"EXAMPLE_REC","name":"COUNT_FIELD","default":"0","version":1}]}
```

Each entry has `table`, `name`, and `default` and/or `version`. Default is a string
or JSON null; version is a nonnegative integer. An explicitly specified default
wins over XML/inference. Pass `--overrides '<overrides.json>'` to `sync-schema` for
preview and saving. Unknown/duplicate/unused overrides are errors. The CLI omits
overrides during the post-save check because they apply only to missing fields.

Do not silently fall back to hand-editing DAT bytes if the CLI rejects XML,
subrecords, defaults or metadata. Diagnose the reported issue; preserve inputs.
Only little-endian editing is supported. Read-back verification is not proof of
PeopleSoft import acceptance.

## Report completion

State what was added, affected tables/rows, inferred defaults (especially NULLs),
where the corrected copy and report were saved, and whether inputs remained
unchanged and the output required zero further additions. Surface remaining
metadata/order/DAT-only differences as unapplied. If no additions were needed,
say so without claiming those reported differences were fixed.
