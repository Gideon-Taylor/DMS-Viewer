"""Run DMS CLI with preview, input fingerprints and post-save checks (Python 3)."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys


def digest(path):
    sha = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            sha.update(block)
    return sha.hexdigest()


def resolve_cli(explicit=None):
    """Prefer an explicit path, then DMS_CLI, then a build in an ancestor checkout."""
    requested = explicit or os.environ.get('DMS_CLI')
    if requested:
        cli = Path(requested).expanduser().resolve(strict=True)
        if not cli.is_file():
            raise ValueError('--cli / DMS_CLI must name the DMS.Cli.exe file.')
        return cli, None
    script = Path(__file__).resolve()
    for ancestor in script.parents:
        cli = ancestor / 'DMS.Cli/bin/Release/DMS.Cli.exe'
        if cli.is_file():
            return cli, None
    # Optional packaged distribution. No binaries are required in the source skill.
    directory = script.parents[1] / 'tools/dms-cli'
    cli = directory / 'DMS.Cli.exe'
    if cli.is_file():
        manifest = json.loads((directory / 'manifest.json').read_text(encoding='utf-8'))
        for relative, expected in manifest['files'].items():
            if digest(directory / relative) != expected:
                raise RuntimeError('Packaged CLI integrity check failed: ' + relative)
        return cli, manifest
    raise FileNotFoundError('DMS.Cli.exe not found. Build the checkout in Release mode, '
                            'pass --cli PATH, or set DMS_CLI. See the repository README.')


def cli_call(cli, source, project, *args):
    proc = subprocess.run([str(cli), 'sync-schema', '--input', str(source),
                           '--project', str(project), *map(str, args)],
                          capture_output=True, timeout=180)
    if proc.stderr:
        sys.stderr.write(proc.stderr.decode('utf-8', errors='replace'))
    try:
        result = json.loads(proc.stdout.decode('utf-8-sig'))
    except (ValueError, UnicodeError) as ex:
        raise RuntimeError('DMS CLI did not return valid UTF-8 JSON.') from ex
    if bool(result.get('success')) != (proc.returncode == 0):
        raise RuntimeError('DMS CLI success flag and exit code disagree.')
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--input', required=True, type=Path)
    parser.add_argument('--project', required=True, type=Path,
                        help='App Designer XML, or a folder containing exactly one XML file')
    parser.add_argument('--output', type=Path, help='New corrected DAT path; required with --apply')
    parser.add_argument('--overrides', type=Path)
    parser.add_argument('--apply', action='store_true', help='Save after a successful preview')
    parser.add_argument('--report', type=Path, help='Optional new JSON report file (never overwritten)')
    parser.add_argument('--cli', type=Path, help='DMS.Cli.exe path; otherwise DMS_CLI or an ancestor Release build')
    args = parser.parse_args()
    result = {'success': False, 'stage': 'setup'}
    watched = {}
    report_ready = False
    try:
        source = args.input.resolve(strict=True)
        project = args.project.resolve(strict=True)
        if project.is_dir():
            files = [p for p in project.iterdir() if p.is_file() and p.suffix.lower() == '.xml']
            if len(files) != 1:
                raise ValueError('Project folder must contain exactly one XML file; provide the intended XML path.')
            project = files[0]
        cli, manifest = resolve_cli(args.cli)
        watched = {source: digest(source), project: digest(project)}
        extra = []
        if args.overrides:
            overrides = args.overrides.resolve(strict=True)
            watched[overrides] = digest(overrides)
            extra = ['--overrides', overrides]
        if args.apply and args.output is None:
            raise ValueError('--output is required with --apply.')
        if args.output is not None and (args.output.resolve() in watched or args.output.exists()):
            raise ValueError('Output must be a new file distinct from the inputs.')
        if args.report is not None and (args.report.resolve() in watched or args.report.exists() or
                                       (args.output and args.report.resolve() == args.output.resolve())):
            raise ValueError('Report must be a new file distinct from inputs and output.')
        report_ready = True
        preview = cli_call(cli, source, project, *extra, '--dry-run')
        result = {'success': preview['success'], 'stage': 'preview', 'preview': preview,
                  'written': False, 'cli': str(cli), 'cliSha256': digest(cli),
                  'toolCommit': manifest['viewer']['commit'] if manifest else None}
        if preview['success'] and args.apply and preview['changes']:
            saved = cli_call(cli, source, project, *extra, '--output', args.output.resolve())
            result.update(success=saved['success'], stage='save', saved=saved, written=saved['success'])
            if saved['success']:
                # Overrides refer only to missing fields, so omit them for the idempotence check.
                check = cli_call(cli, args.output.resolve(), project, '--dry-run')
                clean = check['success'] and not check['changes']
                result.update(success=clean, stage='verified' if clean else 'postcheck-failed',
                              postcheck=check, outputSha256=digest(args.output.resolve()))
        elif preview['success'] and args.apply:
            result['stage'] = 'already-complete'
    except Exception as ex:
        result.update(success=False, error=str(ex))
    try:
        unchanged = all(digest(path) == expected for path, expected in watched.items())
        result['inputVerification'] = {'unchanged': unchanged,
                                       'sha256': {str(p): sha for p, sha in watched.items()}}
        if not unchanged:
            result.update(success=False, error='An input changed during processing; review the result before use.')
    except OSError as ex:
        result.update(success=False, verificationError=str(ex))
    if args.report is not None and report_ready:
        try:
            with args.report.open('x', encoding='utf-8') as stream:
                json.dump(result, stream, indent=2, ensure_ascii=True)
                stream.write('\n')
        except Exception as ex:
            result.update(success=False, reportError=str(ex))
    print(json.dumps(result, ensure_ascii=True))
    return 0 if result['success'] else 1


if __name__ == '__main__':
    raise SystemExit(main())
