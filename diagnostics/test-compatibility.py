import pathlib
import runpy

ROOT = pathlib.Path(__file__).resolve().parent
blocks = runpy.run_path(str(ROOT / 'compare-save.py'))['blocks']
original = blocks((ROOT / 'game.decoded.sii').read_text(encoding='utf-8-sig'))
unchanged = blocks((ROOT / 'game.decoded.sii.roundtrip.sii').read_text(encoding='utf-8-sig'))
assert original == unchanged, 'A no-op save changed data'

edited = blocks((ROOT / 'game.decoded.sii.edits.sii').read_text(encoding='utf-8-sig'))
expected = {name: (kind, dict(fields)) for name, (kind, fields) in original.items()}
for line in (ROOT / 'game.decoded.sii.expected.tsv').read_text(encoding='utf-8-sig').splitlines():
    name, field, value = line.split('\t', 2)
    expected[name][1][field] = value
player_id = next(name for name, (kind, _) in original.items() if kind == 'player')
player = original[player_id][1]
assigned = player['assigned_vehicles']
owned = player['my_vehicles[0]']
expected[assigned][1]['trailer'] = player['trailers[0]']
expected[assigned][1]['stored_trailer_placements'] = '1'
expected[assigned][1]['stored_trailer_placements[0]'] = '(4, 5, 6) (1; 0, 0, 0)'
expected[owned][1]['stored_vehicle_placement'] = '(1, 2, 3) (1; 0, 0, 0)'
assert edited.keys() == expected.keys(), 'An edit added or lost a save block'
for name, data in expected.items():
    if edited[name] != data:
        before, after = data[1], edited[name][1]
        changes = {key: (before.get(key), after.get(key)) for key in before.keys() | after.keys()
                   if before.get(key) != after.get(key)}
        raise AssertionError(f'Unexpected changes in {name}: {changes}')

print(f'PASS: all {len(original)} blocks and all field values survive an unchanged save.')
print('PASS: feature edits reach the expected fields, with zero unrelated changes.')
print('PASS: modern trailer assignments and truck/trailer positions reach player_vehicles.')

convoy = blocks((ROOT / 'game.decoded.sii.convoy.sii').read_text(encoding='utf-8-sig'))
expected_convoy = {name: (kind, dict(fields)) for name, (kind, fields) in original.items()}
for name in (assigned, owned):
    expected_convoy[name][1]['stored_vehicle_placement'] = '(7, 8, 9) (1; 0, 0, 0)'
assert convoy == expected_convoy, 'Convoy import changed unrelated data'
print('PASS: convoy import/export uses modern positions and preserves everything else.')

economy_id = next(name for name, (kind, _) in original.items() if kind == 'economy')
before_economy = original[economy_id][1]
screens = [before_economy[f'screen_access_list[{i}]'] for i in range(int(before_economy['screen_access_list']))]
screens = [screen for screen in screens if screen != '"screen.skills"']
expected_unlock = {name: (kind, dict(fields)) for name, (kind, fields) in original.items()}
for key in list(expected_unlock[economy_id][1]):
    if key == 'screen_access_list' or key.startswith('screen_access_list['):
        del expected_unlock[economy_id][1][key]
expected_unlock[economy_id][1]['screen_access_list'] = str(len(screens))
expected_unlock[economy_id][1].update({f'screen_access_list[{i}]': value for i, value in enumerate(screens)})
unlocked = blocks((ROOT / 'game.decoded.sii.unlock.sii').read_text(encoding='utf-8-sig'))
assert unlocked == expected_unlock, 'Unlocking Skills changed unrelated save data'
expected_unlock[economy_id][1]['experience_points'] = '1942499'
level300 = blocks((ROOT / 'game.decoded.sii.level300.sii').read_text(encoding='utf-8-sig'))
assert level300 == expected_unlock, 'Setting level 300 changed unrelated save data'
print('PASS: Skills unlock updates only its restriction/count/indices; level 300 adds only the XP change.')
