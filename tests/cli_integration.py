"""Process-level CLI checks, using only synthetic test data. Requires Python 3."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile

root = Path(__file__).resolve().parents[1]
cli = root / 'DMS.Cli/bin/Release/DMS.Cli.exe'
fixture = root / 'DMSLib/DMSLib.Tests/bin/Release/DMSLib.Tests.exe'

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

with tempfile.TemporaryDirectory() as directory:
    work = Path(directory)
    source = work / 'source.dat'
    subprocess.run([str(fixture), str(source)], check=True, capture_output=True)
    original = digest(source)
    plan = work / 'changes.json'
    output = work / 'result.dat'
    change = dict(table='TEST_REC', name='NEW_DATE', after='KEY_FIELD', type='DATE', length=10, version=1, default=None)

    def run(columns, *extra, ok=True):
        plan.write_text(json.dumps({'columns': columns}), encoding='utf-8')
        process = subprocess.run([str(cli), 'add-columns', '--input', str(source), '--changes', str(plan), *map(str, extra)], capture_output=True, text=True)
        result = json.loads(process.stdout)
        assert result['success'] is ok, (result, process.stderr)
        assert process.returncode == (0 if ok else 1), process
        assert digest(source) == original, 'Input changed'
        return result

    result = run([change], '--dry-run')
    assert result['dryRun'] and result['changes'][0]['rows'] == 1
    assert not output.exists()
    run([dict(change, name='BAD', type='CHAR', length=1, default=' N')], '--output', output, ok=False)
    assert not output.exists()
    run([change, dict(change, name='OTHER', after='MISSING')], '--output', output, ok=False)
    assert not output.exists(), 'Partial output was published'
    run([dict(change, typo=1)], '--output', output, ok=False)
    run([dict(change, name='KEY_FIELD')], '--output', output, ok=False)
    run([dict(change, type='NO_SUCH_TYPE')], '--output', output, ok=False)
    run([dict(change, default=' ')], '--output', output, ok=False)
    run([change], '--output', source, ok=False)
    run([change], '--output', output)
    text = output.read_text()
    assert 'NEW_DATE:DATE(10)~~~' in text and 'B(AA)' in text
    saved = digest(output)
    run([change], '--output', output, ok=False)
    assert digest(output) == saved, 'Existing output overwritten'

    # An inserted numeric column must retain both its type and scale on disk.
    numbers = work / 'numbers.dat'
    run([dict(change, name='AMOUNT', type='NUMBER', length=7, decimalPositions=2, default='12.34')], '--output', numbers)
    assert 'AMOUNT:NUMBER(7,2)~~~' in numbers.read_text()

    # All split exports of the named record must be changed, not just the first.
    text = source.read_text()
    start = text.index('EXPORT  TEST_REC.')
    end = text.index('REM Ended:')
    split = work / 'split.dat'
    split.write_text(text[:end] + text[start:end] + text[end:])
    source = split
    original = digest(source)
    result = run([change], '--dry-run')
    assert result['changes'][0]['exports'] == 2 and result['changes'][0]['rows'] == 2

print('PASS: CLI integration checks (dry run, verified save, invalid plans, no clobber, decimals, split exports)')
