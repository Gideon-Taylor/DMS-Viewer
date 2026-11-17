"""Synthetic App Designer XML + DAT process-level tests; no customer files."""
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
cli = root / 'DMS.Cli/bin/Release/DMS.Cli.exe'
fixture = root / 'DMSLib/DMSLib.Tests/bin/Release/DMSLib.Tests.exe'
checks = 0

def field(name, kind=0, length=1, default='', ref='', use_edit=0):
    return dict(atmFieldName=name, eFieldType=kind, nLength=length, nDecimalPos=0,
                eFormat=0, fUseEdit=use_edit, nDefGUICtl=99,
                atmDefFieldName=default, atmDefRecName=ref)

def record(name, fields, kind=0):
    inst = ET.Element('instance', {'class': 'RDM'})
    row = ET.SubElement(ET.SubElement(inst, 'rowset', name='RecDefn'), 'row')
    ET.SubElement(row, 'szRecName').text = name
    ET.SubElement(row, 'eRecType').text = str(kind)
    rows = ET.SubElement(ET.SubElement(row, 'hRft'), 'rowset', name='RecField')
    for values in fields:
        f = ET.SubElement(rows, 'row')
        for k, v in values.items():
            ET.SubElement(f, k).text = str(v)
    return ET.tostring(inst, encoding='unicode')

def version(name, number=17):
    inst = ET.Element('instance', {'class': 'FIELD'})
    row = ET.SubElement(ET.SubElement(inst, 'rowset', name='Field'), 'row')
    ET.SubElement(row, 'szFieldName').text = name
    ET.SubElement(row, 'lVersion').text = str(number)
    return ET.tostring(inst, encoding='unicode')

with tempfile.TemporaryDirectory() as folder:
    work = Path(folder)
    source = work / 'input.dat'
    subprocess.run([str(fixture), str(source)], check=True, capture_output=True)
    initial = hashlib.sha256(source.read_bytes()).hexdigest()
    project = work / 'project.XML'
    output = work / 'output.dat'
    overrides = work / 'overrides.json'
    key = field('KEY_FIELD', length=10)

    def xml(*instances):
        # Deliberately no wrapper; also exercise an XML declaration and adjacent instances.
        project.write_text("<?xml version='1.0'?>" + ''.join(instances), encoding='utf-8')

    def run(*args, ok=True, input_path=None):
        global checks
        process = subprocess.run([str(cli), 'sync-schema', '--input', str(input_path or source),
                                  '--project', str(project), *map(str, args)], capture_output=True, text=True)
        result = json.loads(process.stdout)
        assert process.returncode == (0 if ok else 1), (result, process.stderr)
        assert result['success'] is ok, result
        assert hashlib.sha256(source.read_bytes()).hexdigest() == initial, 'Input modified'
        checks += 1
        return result

    xml(record('TEST_REC', [key, field('ACTIVE', default='Y'), field('START_DATE', kind=4, length=10), field('TITLE', length=30)]),
        version('ACTIVE'), version('START_DATE'), version('TITLE'))
    result = run('--dry-run')
    assert [c['defaultValue'] for c in result['changes']] == ['Y', None, ' ']
    assert [c['after'] for c in result['changes']] == ['KEY_FIELD', 'ACTIVE', 'START_DATE']
    assert all(c['version'] == 17 for c in result['changes']) and not output.exists()
    run('--output', output)
    text = output.read_text()
    assert 'ACTIVE:CHAR(1)~~~START_DATE:DATE(10)~~~TITLE:CHAR(30)~~~' in ''.join(text.splitlines())
    saved = output.read_bytes()
    run('--output', output, ok=False)
    assert output.read_bytes() == saved
    assert run('--dry-run', input_path=output)['changes'] == [], 'Not idempotent'
    assert any(d['kind'] == 'dat-only-field' for d in result['differences']), 'Extras were not reported'

    # A non-SQL record or a record not in the project is left unchanged.
    xml(record('TEST_REC', [key, field('IGNORED')], kind=1))
    result = run('--dry-run')
    assert not result['changes'] and result['skipped'][0]['reason'] == 'not-sql-table'
    xml(record('OTHER', [key]))
    result = run('--dry-run')
    assert not result['changes'] and result['skipped'][0]['reason'] == 'not-in-project'
    audit = work / 'audit.dat'
    audit.write_text(source.read_text().replace('TEST_REC', 'AUDIT_TST'))
    xml(record('AUDIT_TST', [key, field('IGNORED')]))
    result = run('--dry-run', input_path=audit)
    assert not result['changes'] and result['skipped'][0]['reason'] == 'audit-record'

    # Recursively expand two subrecords. Definitions can follow their references.
    sub = field('SUB_ONE', kind=7, length=0)
    nested = field('SUB_TWO', kind=7, length=0)
    xml(record('TEST_REC', [key, sub]), record('SUB_ONE', [nested], kind=3),
        record('SUB_TWO', [field('AUDIT_USER', length=30), field('AUDIT_DATE', kind=6, length=26)], kind=3),
        version('AUDIT_USER'), version('AUDIT_DATE'))
    result = run('--dry-run')
    assert [c['field'] for c in result['changes']] == ['AUDIT_USER', 'AUDIT_DATE']
    assert result['changes'][1]['defaultValue'] is None
    xml(record('TEST_REC', [key, sub]))
    run('--dry-run', ok=False)
    xml(record('TEST_REC', [key, sub]), record('SUB_ONE', [sub], kind=3))
    run('--dry-run', ok=False)
    xml(record('TEST_REC', [key, sub]), record('SUB_ONE', [key], kind=3))
    run('--dry-run', ok=False)  # expanded duplicate name

    # Numeric defaults need a choice. A missing field version also needs a choice.
    xml(record('TEST_REC', [key, field('COUNT', kind=2, length=5)]))
    result = run('--dry-run', ok=False)
    assert len(result['unresolved']) == 2
    unresolved_output = work / 'unresolved.dat'
    run('--output', unresolved_output, ok=False)
    assert not unresolved_output.exists()
    overrides.write_text(json.dumps({'fields': [dict(table='TEST_REC', name='COUNT', default='0', version=3)]}))
    result = run('--dry-run', '--overrides', overrides)
    assert result['changes'][0]['version'] == 3 and result['changes'][0]['defaultSource'] == 'override'

    # An XML default referencing another record is not a constant.
    xml(record('TEST_REC', [key, field('NEW_FLAG', default='OTHER_FIELD', ref='OTHER_REC')]), version('NEW_FLAG'))
    result = run('--dry-run', ok=False)
    assert 'references a record field' in result['unresolved'][0]
    overrides.write_text(json.dumps({'fields': [dict(table='TEST_REC', name='NEW_FLAG', default='N')]}))
    run('--dry-run', '--overrides', overrides)
    overrides.write_text(json.dumps({'fields': [dict(table='TEST_REC', name='TYPO', default='N')]}))
    run('--dry-run', '--overrides', overrides, ok=False)
    overrides.write_text(json.dumps({'fields': [dict(table='TEST_REC', name='NEW_FLAG', default='N', typo=1)]}))
    run('--dry-run', '--overrides', overrides, ok=False)

    # Prepend first; preserve type/default details including unsigned UseEdit flags.
    xml(record('TEST_REC', [field('FIRST', default='N', use_edit=2147483648), key]), version('FIRST'))
    result = run('--dry-run')
    assert result['changes'][0]['after'] is None and result['changes'][0]['useEdit'] == 2147483648
    prepend = work / 'prepend.dat'
    run('--output', prepend)
    assert 'FIRST:CHAR(1)~~~KEY_FIELD:CHAR(10)~~~' in ''.join(prepend.read_text().splitlines())

    # Multiple exports with different layouts: only the still-missing export is edited.
    def section(text):
        return text[text.index('EXPORT  TEST_REC.'):text.index('REM Ended:')]
    multi = work / 'multi.dat'
    raw = source.read_text()
    end = raw.index('REM Ended:')
    multi.write_text(raw[:end] + section(prepend.read_text()) + raw[end:])
    result = run('--dry-run', input_path=multi)
    assert len(result['changes']) == 1 and result['changes'][0]['exportIndex'] == 0
    multi_out = work / 'multi-out.dat'
    run('--output', multi_out, input_path=multi)
    assert run('--dry-run', input_path=multi_out)['changes'] == []

    # No deletion, reordering or existing metadata change is applied.
    xml(record('TEST_REC', [field('PRICE', kind=3, length=8), field('KEY_FIELD', length=9)]))
    result = run('--dry-run')
    assert not result['changes']
    assert any(d['kind'] == 'existing-column-order' for d in result['differences'])
    assert any(d.get('property') == 'length' for d in result['differences'])
    assert any(d['kind'] == 'dat-only-field' for d in result['differences'])
    preserved = work / 'preserved.dat'
    run('--output', preserved)
    assert preserved.read_bytes() == source.read_bytes(), 'Existing fields were changed'

    # Literal XML defaults retain whitespace; numeric XML constants need no override.
    xml(record('TEST_REC', [key, field('COUNT', kind=2, length=5, default='0')]), version('COUNT'))
    result = run('--dry-run')
    assert result['changes'][0]['defaultValue'] == '0' and result['changes'][0]['defaultSource'] == 'xml'
    xml(record('TEST_REC', [key, field('SPACED', length=3, default=' X ')]), version('SPACED'))
    assert run('--dry-run')['changes'][0]['defaultValue'] == ' X '
    xml(record('TEST_REC', [key, field('INVALID', kind=65536)]), version('INVALID'))
    run('--dry-run', ok=False)

    # DTDs/entities must not be resolved, and duplicate definitions must fail.
    project.write_text('<!DOCTYPE root [<!ENTITY x SYSTEM "file:///missing">]><root>&x;</root>')
    run('--dry-run', ok=False)
    xml(record('TEST_REC', [key]), record('TEST_REC', [key]))
    run('--dry-run', ok=False)

print(f'PASS: {checks} schema-sync process checks (XML defaults, nulls, subrecords, overrides, skips, idempotence, split exports, XML safety)')
