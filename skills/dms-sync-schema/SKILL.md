---
name: dms-sync-schema
description: Compare and correct PeopleSoft Data Mover DAT exports against App Designer project XML. Use when given a DAT and project XML to identify or add missing columns, expand subrecords, and save a verified corrected copy with DMS CLI.
---

# DMS Schema Sync

Use DMS CLI through `scripts/sync_dat.py`. These instructions and the Python helper
are agent independent; no agent SDK, account, plugin or vendor-specific tool is
required. The current CLI requires Windows and .NET Framework 4.8; the helper
requires Python 3. See the [repository build instructions](../../README.md#building-and-testing)
when using a source checkout.

The helper finds `DMS.Cli/bin/Release/DMS.Cli.exe` in an ancestor checkout. If this
skill was copied into an agent's skill directory, pass `--cli '<DMS.Cli.exe>'` or
set the `DMS_CLI` environment variable to the executable's absolute path. Precedence
is `--cli`, then `DMS_CLI`, then the checkout's Release build. Explicit paths never
silently fall back. Keep the executable with its built DLLs; do not copy just the
EXE. An optional packaged runtime is also supported if present, with manifest
hash verification. The repository skill contains source only, not compiled binaries.

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

Resolve this skill's directory and invoke the helper with argument-safe shell quoting:

```powershell
python '<skill-dir>\scripts\sync_dat.py' --input '<source.dat>' --project '<project.XML>' --report '<new-preview.json>'
```

The helper resolves the CLI and runs `sync-schema --dry-run`.
Read the JSON `preview.changes`, `preview.differences`, `preview.skipped`, and
`preview.unresolved`. Diagnostics go to stderr; stdout contains one JSON object.
Exit code 0 means success, 1 means a failed or unresolved operation.

For a correction request with resolved changes:

```powershell
python '<skill-dir>\scripts\sync_dat.py' --input '<source.dat>' --project '<project.XML>' --apply --output '<new-corrected.dat>' --report '<new-result.json>'
```

The helper previews again before saving. The CLI saves to a temporary file,
reopens and verifies it, then publishes the new DAT without overwriting an existing
file. The helper checks input hashes and reruns comparison on the output to confirm
zero remaining additions. If no additions are needed, `--apply` reports
`already-complete` and creates no duplicate DAT. Report paths must also be new.

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
wins over XML/inference. Pass `--overrides '<overrides.json>'` to the helper for
preview and apply. Unknown/duplicate/unused overrides are errors. The helper omits
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
