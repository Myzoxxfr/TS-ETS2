import collections
import json
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parent
ITEMS = ROOT.parent / 'TS SE Tool' / 'CustomClasses' / 'Save' / 'Items'


def blocks(text):
    result = {}
    current = None
    for line in text.splitlines():
        match = re.fullmatch(r'(\w+)\s*:\s*(\S+)\s*\{', line.strip())
        if match:
            assert current is None
            kind, name = match.groups()
            assert name not in result, name
            current = (kind, name, {})
        elif line.strip() == '}' and current:
            kind, name, fields = current
            result[name] = (kind, fields)
            current = None
        elif current and ':' in line:
            key, value = line.strip().split(':', 1)
            current[2][key.strip()] = value.strip()
    assert current is None
    return result


if __name__ == '__main__':
    original = blocks((ROOT / 'game.decoded.sii').read_text(encoding='utf-8-sig'))
    assert len(original) > 1000
    dispatcher = (ITEMS / 'SiiNunit.cs').read_text(encoding='utf-8-sig')
    mapping = dict(re.findall(r'case "(\w+)":\s*\{\s*(?:EconomyNameless = nameless;\s*)?return new (\w+)\(_input\)', dispatcher))
    # The empty and closing-brace cases precede economy in this dispatcher.
    mapping['economy'] = 'Economy'
    known = {}
    for kind, cls in mapping.items():
        source = (ITEMS / (cls + '.cs')).read_text(encoding='utf-8-sig')
        known[kind] = set(re.findall(r'case "([\w]+)":', source))
    kinds = collections.Counter(kind for kind, _ in original.values())
    unknown_fields = collections.defaultdict(set)
    for kind, fields in original.values():
        if kind in known:
            unknown_fields[kind].update(re.sub(r'\[\d+\]$', '', key) for key in fields
                                        if re.sub(r'\[\d+\]$', '', key) not in known[kind])
    report = {
        'block_count': len(original),
        'types': dict(kinds),
        'unmodeled_block_types': {k: n for k, n in kinds.items() if k not in known},
        'unmodeled_fields': {k: sorted(v) for k, v in unknown_fields.items() if v},
    }
    output = ROOT / 'game.decoded.sii.roundtrip.sii'
    if output.exists():
        after = blocks(output.read_text(encoding='utf-8-sig'))
        report['missing_blocks'] = sorted(original.keys() - after.keys())
        report['changed_fields'] = {
            name: {key: [value, after[name][1].get(key)] for key, value in fields.items()
                   if value != after[name][1].get(key)}
            for name, (_, fields) in original.items() if name in after
        }
        report['changed_fields'] = {k: v for k, v in report['changed_fields'].items() if v}
        report['added_fields'] = {
            name: sorted(fields.keys() - original[name][1].keys())
            for name, (_, fields) in after.items() if name in original
            and fields.keys() - original[name][1].keys()
        }
    (ROOT / 'compatibility.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k not in ('changed_fields', 'added_fields')}, indent=2))
    if 'changed_fields' in report:
        print('Blocks with changed/lost fields:', len(report['changed_fields']))
        for name, fields in list(report['changed_fields'].items())[:12]:
            print(name, fields)
