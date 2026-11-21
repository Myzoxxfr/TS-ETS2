from pathlib import Path
import struct
import xml.etree.ElementTree as ET
import zipfile

ROOT = Path(__file__).resolve().parent.parent
PROJECT = ROOT / 'TS SE Tool'
source_zip = Path(__file__).resolve().parent / 'upstream-v0.3.11.0.zip'
with zipfile.ZipFile(source_zip) as source:
    archive_root = source.namelist()[0]
    # These are the newer controls and handlers that prompted the port.
    for relative in ('TS SE Tool/Forms/MainTabs/FormMethodsCompanyTab.cs',
                     'TS SE Tool/Forms/MainTabs/FormMethodsTruckTab.cs',
                     'TS SE Tool/Forms/MainTabs/FormMethodsTrailerTab.cs.cs',
                     ):
        assert (ROOT / relative).read_bytes() == source.read(archive_root + relative), relative

designer = (PROJECT / 'FormMain.Designer.cs').read_text(encoding='utf-8-sig')
assert 'buttonUserCompanyGaragesSelectAll.Click +=' in designer
assert 'buttonUserCompanyCitiesSelectAll.Click +=' in designer
assert 'buttonUserCompanyDriversSelectAll.Click +=' in designer
assert 'AssemblyVersion("0.3.12.0")' in (PROJECT / 'Properties/AssemblyInfo.cs').read_text(encoding='utf-8-sig')
ns = {'m': 'http://schemas.microsoft.com/developer/msbuild/2003'}
tree = ET.parse(PROJECT / 'TS SE Tool.csproj')
assert all(item.text == 'x86' for item in tree.findall('.//m:PlatformTarget', ns))
for item in tree.findall('.//m:Compile', ns):
    assert (PROJECT / item.attrib['Include']).is_file(), item.attrib['Include']
event = tree.find('.//m:PostBuildEvent', ns).text
assert '/MOVE' not in event and '/MOV' in event
contents = tree.findall('.//m:Content', ns)
assert any('img\\**\\*' in item.attrib.get('Include', '') for item in contents)
for relative in ('img/UI/globe.png', 'lang/Default/lngfile.txt',
                 'lang/CityToCountry.csv', 'lang/CountryProperties.csv', 'gameref/cache/ETS2/def.sdf'):
    assert (PROJECT / relative).is_file(), relative
dll = (PROJECT / 'SII_Decrypt.dll').read_bytes()
pe = struct.unpack_from('<I', dll, 0x3c)[0]
assert dll[pe:pe + 4] == b'PE\0\0'
assert struct.unpack_from('<H', dll, pe + 4)[0] == 0x14c, 'The decoder must match the x86 app target'
print('PASS: version 0.3.12.0; Select All handlers and truck/trailer controls preserved.')
print('PASS: source includes, runtime support files, decoder architecture and post-build setup verified.')
