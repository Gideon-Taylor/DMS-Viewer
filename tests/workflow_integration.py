"""Development tests for the native CLI workflow; end users need no Python."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

root = Path(__file__).resolve().parents[1]
cli = root / 'DMS.Cli/bin/Release/DMS.Cli.exe'
fixture = root / 'DMSLib/DMSLib.Tests/bin/Release/DMSLib.Tests.exe'
checks = 0

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def fingerprint(result, path):
    # .NET may expand Windows 8.3 paths (e.g. RUNNER~1) in its JSON keys.
    hashes = [value for key, value in result['inputVerification']['sha256'].items()
              if Path(key).samefile(path)]
    assert len(hashes) == 1
    return hashes[0]

with tempfile.TemporaryDirectory() as folder:
    work = Path(folder) / 'données-測試'
    work.mkdir()
    source = work / 'source.dat'
    subprocess.run([str(fixture), str(source)], capture_output=True, check=True)
    original = source.read_bytes()
    project = work / 'project.XML'
    project.write_text('''<?xml version="1.0"?>
<instance class="RDM"><rowset name="RecDefn"><row>
<szRecName>TEST_REC</szRecName><eRecType>0</eRecType><hRft><rowset name="RecField">
<row><atmFieldName>KEY_FIELD</atmFieldName><eFieldType>0</eFieldType><nLength>10</nLength><nDecimalPos>0</nDecimalPos><eFormat>0</eFormat><fUseEdit>0</fUseEdit><nDefGUICtl>99</nDefGUICtl></row>
<row><atmFieldName>START_DATE</atmFieldName><eFieldType>4</eFieldType><nLength>10</nLength><nDecimalPos>0</nDecimalPos><eFormat>0</eFormat><fUseEdit>0</fUseEdit><nDefGUICtl>99</nDefGUICtl></row>
</rowset></hRft></row></rowset></instance>
<instance class="FIELD"><rowset name="Field"><row><szFieldName>START_DATE</szFieldName><lVersion>7</lVersion></row></rowset></instance>
''', encoding='utf-8')
    xml_original = project.read_bytes()
    # Deliberately remove PATH, Python variables and all agent-specific configuration.
    # The native EXE must work with only normal Windows runtime settings.
    env = {k: v for k, v in os.environ.items() if k.upper() in
           {'SYSTEMROOT', 'WINDIR', 'TEMP', 'TMP', 'USERPROFILE', 'COMSPEC'}}

    def run(*args, ok=True, input_path=source, project_path=project, executable=cli):
        global checks
        proc = subprocess.run([str(executable), 'sync-schema', '--input', str(input_path),
                               '--project', str(project_path), *map(str, args)],
                              cwd=work, env=env, capture_output=True)
        result = json.loads(proc.stdout.decode('utf-8-sig'))
        assert result['success'] is ok, (result, proc.stderr)
        assert proc.returncode == (0 if ok else 1), proc
        assert source.read_bytes() == original and project.read_bytes() == xml_original
        checks += 1
        return result

    preview_path = work / 'preview.json'
    preview = run('--dry-run', '--report', preview_path, project_path=work)
    assert len(preview['changes']) == 1 and preview['changes'][0]['defaultValue'] is None
    assert preview['stage'] == 'preview' and not preview['written']
    assert fingerprint(preview, source) == sha(source)
    assert json.loads(preview_path.read_text(encoding='utf-8')) == preview

    output, report = work / 'corrected.dat', work / 'result.json'
    saved = run('--output', output, '--report', report)
    assert saved['stage'] == 'verified' and saved['written'] and saved['postcheck']['success']
    assert not saved['postcheck']['changes'] and saved['inputVerification']['unchanged']
    assert saved['outputSha256'] == sha(output)
    assert json.loads(report.read_text(encoding='utf-8')) == saved
    assert 'START_DATE:DATE(10)~~~' in output.read_text() and 'B(AA)' in output.read_text()

    # A complete input gets a report, but no duplicate DAT.
    noop = work / 'noop.dat'
    complete = run('--output', noop, '--report', work / 'complete.json', input_path=output)
    assert complete['stage'] == 'already-complete' and not complete['written'] and not noop.exists()

    for bad_output, bad_report in [(source, work / 'x.json'), (project, work / 'x.json'),
                                   (output, work / 'x.json'), (work / 'x.dat', source),
                                   (work / 'x.dat', project), (work / 'x.dat', report),
                                   (work / 'alias.dat', work / 'alias.dat'),
                                   (work / 'x.dat', work / 'missing' / 'result.json')]:
        run('--output', bad_output, '--report', bad_report, ok=False)
    assert not (work / 'x.dat').exists() and not (work / 'alias.dat').exists()
    extra_xml = work / 'other.XML'
    extra_xml.write_text('<instance/>')
    run('--dry-run', project_path=work, ok=False)
    extra_xml.unlink()

    # Failures after a valid setup still produce a structured JSON report.
    project.write_text(xml_original.decode().replace('class="FIELD"', 'class="IGNORED"'))
    xml_original = project.read_bytes()
    failed = run('--output', work / 'failed.dat', '--report', work / 'failed.json', ok=False)
    assert failed['unresolved'] and not failed['written'] and not (work / 'failed.dat').exists()
    assert json.loads((work / 'failed.json').read_text(encoding='utf-8')) == failed
    overrides = work / 'overrides.json'
    overrides.write_text(json.dumps({'fields': [dict(table='TEST_REC', name='START_DATE', version=7)]}))
    fixed = run('--overrides', overrides, '--output', work / 'override.dat')
    assert fixed['postcheck']['success'] and fingerprint(fixed, overrides) == sha(overrides)

    # Copy the built native runtime to a neutral folder and run it without Python/PATH.
    runtime = work / 'native-runtime'
    shutil.copytree(cli.parent, runtime)
    copied = run('--dry-run', executable=runtime / 'DMS.Cli.exe', input_path=output)
    assert copied['success'] and not copied['changes']

    # Explicit add-columns gets the same fingerprints, verified save, and JSON report.
    plan = work / 'columns.json'
    plan.write_text(json.dumps({'columns': [dict(table='TEST_REC', name='FLAG', after='KEY_FIELD',
                      type='CHAR', length=1, version=1, default='N')]}))
    proc = subprocess.run([str(cli), 'add-columns', '--input', str(source), '--changes', str(plan),
                           '--output', str(work / 'manual.dat'), '--report', str(work / 'manual.json')],
                          cwd=work, env=env, capture_output=True)
    manual = json.loads(proc.stdout.decode('utf-8-sig'))
    assert proc.returncode == 0 and manual['written'] and manual['stage'] == 'verified'
    assert fingerprint(manual, plan) == sha(plan)
    assert json.loads((work / 'manual.json').read_text(encoding='utf-8')) == manual
    checks += 1
    plan_before = plan.read_bytes()
    proc = subprocess.run([str(cli), 'add-columns', '--input', str(source), '--changes', str(plan),
                           '--output', str(work / 'refused.dat'), '--report', str(plan)],
                          cwd=work, env=env, capture_output=True)
    assert proc.returncode == 1 and not json.loads(proc.stdout.decode('utf-8-sig'))['success']
    assert not (work / 'refused.dat').exists() and plan.read_bytes() == plan_before
    checks += 1

print(f'PASS: {checks} native workflow checks (UTF-8 JSON, reports, fingerprints, nulls, idempotence, no overwrite, overrides, no Python/PATH)')
