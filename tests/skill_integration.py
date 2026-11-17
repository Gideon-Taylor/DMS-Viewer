"""Exercise the source skill from a checkout and a copied, agent-neutral folder."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile

root = Path(__file__).resolve().parents[1]
skill = root / 'skills/dms-sync-schema'
helper = skill / 'scripts/sync_dat.py'
cli = root / 'DMS.Cli/bin/Release/DMS.Cli.exe'
fixture = root / 'DMSLib/DMSLib.Tests/bin/Release/DMSLib.Tests.exe'
checks = 0

with tempfile.TemporaryDirectory() as folder:
    work = Path(folder)
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
    env = os.environ.copy()
    env.pop('DMS_CLI', None)

    def run(script=helper, *args, success=True, environment=None):
        global checks
        proc = subprocess.run([sys.executable, str(script), '--input', str(source),
                               '--project', str(project), *map(str, args)],
                              cwd=work, env=environment or env, capture_output=True, text=True)
        result = json.loads(proc.stdout)
        assert result['success'] is success, (result, proc.stderr)
        assert proc.returncode == (0 if success else 1), proc
        assert source.read_bytes() == original and project.read_bytes() == xml_original
        checks += 1
        return result

    # Source checkout discovery must work from an unrelated current directory.
    preview = run()
    assert Path(preview['cli']) == cli and preview['preview']['changes'][0]['defaultValue'] is None
    output = work / 'corrected.dat'
    report = work / 'result.json'
    saved = run(helper, '--apply', '--output', output, '--report', report)
    assert saved['stage'] == 'verified' and saved['written'] and not saved['postcheck']['changes']
    assert saved['inputVerification']['unchanged']
    assert json.loads(report.read_text())['outputSha256'] == hashlib.sha256(output.read_bytes()).hexdigest()
    before = output.read_bytes()
    run(helper, '--apply', '--output', output, success=False)
    assert output.read_bytes() == before
    alias = work / 'alias.dat'
    run(helper, '--apply', '--output', alias, '--report', alias, success=False)
    assert not alias.exists()

    # A copy in any agent's directory has no assumed checkout or vendor paths.
    copied = work / 'agent-skills/dms-sync-schema'
    shutil.copytree(skill, copied)
    copied_helper = copied / 'scripts/sync_dat.py'
    missing = run(copied_helper, success=False)
    assert 'not found' in missing['error']
    explicit = run(copied_helper, '--cli', cli)
    assert Path(explicit['cli']) == cli
    configured = dict(env, DMS_CLI=str(cli))
    assert run(copied_helper, environment=configured)['preview']['changes']
    invalid_env = dict(env, DMS_CLI=str(work / 'missing.exe'))
    run(copied_helper, '--cli', cli, environment=invalid_env)
    run(copied_helper, '--cli', work / 'missing.exe', environment=configured, success=False)
    run(copied_helper, '--cli', cli.parent, success=False)
    standalone_output = work / 'copied-result.dat'
    standalone = run(copied_helper, '--cli', cli, '--apply', '--output', standalone_output)
    assert standalone['stage'] == 'verified' and standalone_output.read_bytes() == before

    # Optional binary packages remain usable without putting them in source control.
    runtime = copied / 'tools/dms-cli'
    shutil.copytree(cli.parent, runtime)
    files = {str(p.relative_to(runtime)).replace('\\', '/'): hashlib.sha256(p.read_bytes()).hexdigest()
             for p in runtime.rglob('*') if p.is_file()}
    (runtime / 'manifest.json').write_text(json.dumps({'viewer': {'commit': 'test-build'}, 'files': files}))
    assert run(copied_helper)['toolCommit'] == 'test-build'
    (runtime / 'DMS.Cli.exe').write_bytes(b'invalid')
    tampered = run(copied_helper, success=False)
    assert 'integrity check failed' in tampered['error']

print(f'PASS: {checks} source-skill checks (checkout discovery, copied skill, explicit CLI, environment precedence, verified output, no overwrite, optional package integrity)')
