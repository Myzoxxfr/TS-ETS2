from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent


def replace(relative, old, new):
    path = ROOT / relative
    text = path.read_text(encoding='utf-8-sig')
    assert old in text, (relative, old)
    path.write_text(text.replace(old, new), encoding='utf-8')


replace('TS SE Tool/CustomClasses/Save/Items/SaveCompatibility.cs', 'Vector_3f_4f', 'SCS_Placement')
replace('diagnostics/build-check.ps1', '..\\TS-SE-Tool-master\\TS SE Tool', '..\\TS SE Tool')
replace('diagnostics/build-check.ps1', '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll /reference:System.Drawing.dll')
replace('diagnostics/compare-save.py', "ROOT.parent / 'TS-SE-Tool-master' / 'TS SE Tool'", "ROOT.parent / 'TS SE Tool'")
replace('diagnostics/InspectSave.cs', 'Vector_3f_4f', 'SCS_Placement')
replace('diagnostics/InspectSave.cs', 'new Vector_3f(', 'new SCS_Float_3(')
replace('diagnostics/InspectSave.cs', 'save.InitializeCompatibility(File.ReadAllLines(args[0]));', 'save.InitializeCompatibility(File.ReadAllLines(args[0]), 105);')
replace('diagnostics/InspectSave.cs', 'reloaded.InitializeCompatibility(changed.Split(new[] { "\\r\\n" }, StringSplitOptions.None));', 'reloaded.InitializeCompatibility(changed.Split(new[] { "\\r\\n" }, StringSplitOptions.None), 105);')
replace('diagnostics/InspectSave.cs', 'save.PrintOut(0)', 'save.PrintOut(105)')
replace('diagnostics/InspectSave.cs', 'static class Globals {', 'static class TSSET_Help { internal static void fmRemoveWritenBlock(string id) { FormMain.SiiNunitData.NamelessControlList.Remove(id); } }\n    static class Globals {')
print('Adapted the compatibility helper and save-data tests to the 0.3.11.0 data types.')
