import re
from pathlib import Path
root = Path(__file__).resolve().parent.parent
dist = Path('C:/KSPDev/GameData/WildBlueIndustries/Snacks')

def catalog(language):
    entries = {}
    for line in (dist / 'Localization' / (language+'.cfg')).read_text(encoding='utf-8').splitlines():
        match = re.match(r'\s*(#LOC_Snacks_\w+)\s*=\s?(.*)', line)
        if match:
            assert match[1] not in entries, 'Duplicate key '+match[1]
            entries[match[1]] = match[2]
    return entries

english, chinese = catalog('en-us'), catalog('zh-cn')
assert english.keys() == chinese.keys(), 'Catalog key parity'
placeholder = re.compile(r'\{\d+(?::[^}]+)?\}')
for key in english:
    assert sorted(placeholder.findall(english[key])) == sorted(placeholder.findall(chinese[key])), 'Placeholder mismatch '+key
    assert '<<' not in english[key]+chinese[key], 'Unadapted placeholder '+key

references = set()
for path in list((root / 'Snacks').rglob('*.cs')) + list(dist.rglob('*.cfg')) + [dist / 'LifeSupportResources/Stress.txt']:
    if any(part in ('obj','bin','Localization') for part in path.parts) or not path.exists(): continue
    references.update(re.findall(r'#LOC_Snacks_\w+', path.read_bytes().decode('utf-8', errors='replace')))
assert references <= english.keys(), 'Missing keys '+str(references-english.keys())

# Config braces occur on their own lines; formatting placeholders are value text.
for path in [dist / 'LifeSupportResources/Air.txt', dist / 'ModuleManagerPatches/MM_Stock.cfg'] + list((dist / 'Parts/Payload').glob('*.cfg')):
    depth = 0
    for number, line in enumerate(path.read_bytes().decode('utf-8', errors='replace').splitlines(), 1):
        line = line.split('//')[0].strip()
        if line == '{': depth += 1
        elif line == '}': depth -= 1
        assert depth >= 0, f'Unexpected closing brace: {path}:{number}'
    assert depth == 0, 'Unclosed node '+str(path)

stock = (dist / 'ModuleManagerPatches/MM_Stock.cfg').read_text(encoding='utf-8')
assert '@PART[crewCabin,mk3Cockpit_Shuttle,mk2CrewCabin]' in stock
assert 'RecyclerCapacity = 4' in stock and 'maxAmount = 200' in stock
assert '@maxAmount *= 200' not in stock
for name, volume in [('radialSnackTin',50),('snackTin500',920)]:
    data = (dist / 'Parts/Payload' / (name+'.cfg')).read_text(encoding='utf-8')
    assert data.count('name = ModuleCargoPart') == 1
    assert f'packedVolume = {volume}' in data
for name, model in [('snackTin500','FL-T100_A'),('snackTin1500','FL-T200_A'),('snackTin4500','FL-T400_A')]:
    data = (dist / 'Parts/Payload' / (name+'.cfg')).read_text(encoding='utf-8')
    assert 'WildBlueIndustries/Snacks/Parts/Payload/'+model in data
    assert (dist / 'Parts/Payload' / (model+'.mu')).exists()
for filename in ('Stress.txt','Stress.cfg'):
    path = dist / 'LifeSupportResources' / filename
    if path.exists():
        data = path.read_bytes().decode('utf-8', errors='replace')
        assert not re.search(r'(?:conditionSummary|resourceName)\s*=\s*#LOC', data)
assert not (dist / 'Extras/RenameSoil.cfg').exists(), 'Optional RenameSoil should not be enabled'
print(f'PASS: {len(english)} catalog keys, {len(references)} referenced keys, matching placeholders, config structure, unchanged recycler balance/Stress IDs, cargo modules, and existing tank model paths.')
