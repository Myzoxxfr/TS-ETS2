using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TS_SE_Tool.Save.Items;
using TS_SE_Tool.Save.DataFormat;

namespace TS_SE_Tool
{
    class FormMain { internal static SiiNunit SiiNunitData; }
    static class TSSET_Help { internal static void fmRemoveWritenBlock(string id) { FormMain.SiiNunitData.NamelessControlList.Remove(id); } }
    static class Globals { internal static int[] PlayerLevelUps = { 200 }; }
}
namespace TS_SE_Tool.Utilities
{
    static class IO_Utilities
    {
        internal static List<string> Errors = new List<string>();
        public static void ErrorLogWriter(string message) { Errors.Add(message); }
    }
}
class InspectSave
{
    static int Main(string[] args)
    {
        try
        {
            var save = new SiiNunit(File.ReadAllLines(args[0]));
            TS_SE_Tool.FormMain.SiiNunitData = save;
            save.InitializeCompatibility(File.ReadAllLines(args[0]), 105);
            Console.WriteLine("Parsed blocks: " + save.SiiNitems.Count);
            File.WriteAllLines(args[0] + ".errors.txt", TS_SE_Tool.Utilities.IO_Utilities.Errors);
            Console.WriteLine("Reader log entries: " + TS_SE_Tool.Utilities.IO_Utilities.Errors.Count);
            File.WriteAllText(args[0] + ".roundtrip.sii", save.PrintOut(105));
            Console.WriteLine("Whole-save round trip succeeded.");
            if (TS_SE_Tool.Utilities.IO_Utilities.Errors.Any(message => message.Contains(Environment.NewLine)))
                throw new Exception("The reader reported a parse exception or an unknown block.");
            var expected = new List<string>();
            Action<string, string, object> expect = (block, key, value) => expected.Add(block + "\t" + key + "\t" + value);
            var bankId = save.Economy.bank;
            var economyId = save.EconomyNameless;
            save.Bank.money_account += 123456;
            expect(bankId, "money_account", save.Bank.money_account);
            save.Economy.experience_points += 1000;
            expect(economyId, "experience_points", save.Economy.experience_points);
            save.Economy.adr = (byte)(save.Economy.adr == 63 ? 0 : 63);
            save.Economy.setPlayerSkillsArray();
            expect(economyId, "adr", save.Economy.adr);
            save.Economy.visited_cities_count[0]++;
            expect(economyId, "visited_cities_count[0]", save.Economy.visited_cities_count[0]);
            var truckId = save.Player.assigned_truck;
            if (truckId == "null" || string.IsNullOrEmpty(truckId))
                throw new Exception("New vehicle assignment did not load.");
            Vehicle truck = save.SiiNitems[truckId];
            truck.engine_wear = 0;
            truck.transmission_wear = 0;
            truck.engine_wear_unfixable = 0;
            truck.transmission_wear_unfixable = 0;
            truck.fuel_relative = 1;
            expect(truckId, "engine_wear", 0);
            expect(truckId, "transmission_wear", 0);
            expect(truckId, "engine_wear_unfixable", 0);
            expect(truckId, "transmission_wear_unfixable", 0);
            expect(truckId, "fuel_relative", 1);
            var garageItem = save.SiiNitems.First(x => x.Value is Garage && ((Garage)x.Value).status == 0);
            ((Garage)garageItem.Value).status = 3;
            expect(garageItem.Key, "status", 3);
            var paintItem = save.SiiNitems.First(x => x.Value is Vehicle_Paint_job_Accessory);
            ((Vehicle_Paint_job_Accessory)paintItem.Value).base_color = new SCS_Float_3("(1, 0, 0)");
            expect(paintItem.Key, "base_color", "(1, 0, 0)");
            var jobItem = save.SiiNitems.First(x => x.Value is Job_offer_Data);
            ((Job_offer_Data)jobItem.Value).units_count++;
            expect(jobItem.Key, "units_count", ((Job_offer_Data)jobItem.Value).units_count);
            save.Player.assigned_trailer = save.Player.trailers[0];
            save.Player.my_truck_placement = new SCS_Placement("(1, 2, 3) (1; 0, 0, 0)");
            save.Player.trailer_placement = new SCS_Placement("(4, 5, 6) (1; 0, 0, 0)");
            var changed = save.PrintOut(105);
            if (changed != save.PrintOut(105)) throw new Exception("Repeated serialization changed the save.");
            File.WriteAllText(args[0] + ".edits.sii", changed);
            File.WriteAllLines(args[0] + ".expected.tsv", expected);
            var reloaded = new SiiNunit(changed.Split(new[] { "\r\n" }, StringSplitOptions.None));
            TS_SE_Tool.FormMain.SiiNunitData = reloaded;
            reloaded.InitializeCompatibility(changed.Split(new[] { "\r\n" }, StringSplitOptions.None), 105);
            if (reloaded.Bank.money_account != save.Bank.money_account || reloaded.Player.assigned_trailer != save.Player.assigned_trailer
                || reloaded.Player.my_truck_placement.ToString() != save.Player.my_truck_placement.ToString())
                throw new Exception("Edited values did not survive reloading.");
            File.WriteAllText(args[0] + ".convoy.sii", SaveCompatibility.MoveTruck(File.ReadAllLines(args[0]), "(7, 8, 9) (1; 0, 0, 0)"));
            var moved = File.ReadAllLines(args[0] + ".convoy.sii");
            if (SaveCompatibility.ReadTruckPosition(moved) != "(7, 8, 9) (1; 0, 0, 0)")
                throw new Exception("Convoy position export did not read the modern position.");
            TestFieldMerge();
            TestIgnoredBlock(File.ReadAllText(args[0]));
            TestPlayerProgress(args[0]);
            Console.WriteLine("Money, XP, skills, cities, repair, fuel, garage, paint, jobs, trailer assignment and positions exercised.");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }

    static void TestFieldMerge()
    {
        var raw = "SiiNunit\n{\ntest : unit {\n items: 2\n items[0]: a\n items[1]: b\n future: kept\n}\n}";
        var baseline = "SiiNunit\n{\ntest : unit {\n items: 2\n items[0]: a\n items[1]: b\n old_default: 0\n}\n}";
        var changed = "SiiNunit\n{\ntest : unit {\n items: 1\n items[0]: c\n old_default: 0\n}\ntest : newunit {\n value: 42\n}\n}";
        var merge = new SaveCompatibility(raw.Split('\n'));
        merge.CaptureBaseline(baseline);
        var result = merge.Merge(changed);
        if (result.Contains("items[1]") || result.Contains("old_default") || !result.Contains("future: kept") || !result.Contains("newunit"))
            throw new Exception("Array shrinking, field preservation or new-block serialization failed.");
        var oldSave = new[] { "SiiNunit", "{", "player : unit {", " truck_placement: (0, 0, 0) (1; 0, 0, 0)",
            " trailer_placement: (0, 0, 0) (1; 0, 0, 0)", " slave_trailer_placements: 0", "}", "}" };
        var oldMoved = SaveCompatibility.MoveTruck(oldSave, "(7, 8, 9) (1; 0, 0, 0)");
        if (SaveCompatibility.ReadTruckPosition(oldMoved.Split('\n')) != "(7, 8, 9) (1; 0, 0, 0)")
            throw new Exception("Legacy convoy positions failed.");
        bool rejected = false;
        try { new SaveCompatibility(new[] { "SiiNunit", "{", "test : unclosed {", " value: 1" }); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected) throw new Exception("Truncated saves must be rejected.");
        Console.WriteLine("Array shrinking, unknown fields, new blocks and legacy convoy positions passed.");
    }

    static void TestIgnoredBlock(string text)
    {
        var fixture = text.Insert(text.LastIndexOf('}'), "car_job_generator : compatibility.test_marker {\r\n future_value: kept\r\n}\r\n");
        var lines = fixture.Split(new[] { "\r\n" }, StringSplitOptions.None);
        var save = new SiiNunit(lines);
        TS_SE_Tool.FormMain.SiiNunitData = save;
        save.InitializeCompatibility(lines, 105);
        save.NamelessIgnoreList.Add("compatibility.test_marker");
        if (save.PrintOut(105).Contains("compatibility.test_marker"))
            throw new Exception("Ignored blocks were restored by the preservation layer.");
        Console.WriteLine("The newer writer's ignored-block removal still works.");
    }

    static void TestPlayerProgress(string path)
    {
        TS_SE_Tool.Globals.PlayerLevelUps = new int[] { 200, 500, 700, 900, 1000, 1100, 1300, 1600, 1700, 2100,
            2300, 2600, 2700, 2900, 3000, 3100, 3400, 3700, 4000, 4300, 4600, 4700, 4900, 5200, 5700, 5900,
            6000, 6200, 6600, 6800 };
        var economy = new Economy();
        foreach (var test in new[] { new uint[] { 0, 0, 200 }, new uint[] { 199, 0, 200 },
            new uint[] { 200, 1, 700 }, new uint[] { 700, 2, 1400 }, new uint[] { 99700, 30, 106500 },
            new uint[] { 922499, 150, 922500 }, new uint[] { 922500, 151, 929300 } })
        {
            economy.experience_points = test[0];
            var level = economy.getPlayerLvl();
            if (level[0] != test[1] || level[1] != test[2]) throw new Exception("Incorrect XP boundary: " + test[0]);
        }
        foreach (int level in new[] { 1, 150, 151, 300, 1000 })
        {
            economy.setPlayerExp(level);
            if (economy.getPlayerLvl()[0] != level) throw new Exception("Level setter failed at " + level);
        }
        economy.screen_access_list = new List<string> { "\"screen.skills\"", "\"screen.bus.skills\"", "\"screen.recruit\"" };
        economy.setPlayerExp(-10);
        if (economy.getPlayerLvl()[0] != 0 || !economy.screen_access_list.Contains("\"screen.skills\""))
            throw new Exception("Level zero should not unlock Skills.");
        economy.setPlayerExp(1);
        if (!economy.screen_access_list.SequenceEqual(new[] { "\"screen.bus.skills\"", "\"screen.recruit\"" })
            || economy.UnlockSkillsMenu()) throw new Exception("Unlock changed other restrictions or was not idempotent.");
        economy.setPlayerExp(int.MaxValue);
        var maximum = economy.getPlayerLvl();
        if (economy.experience_points != uint.MaxValue || maximum[0] != 631628 || maximum[1] <= uint.MaxValue)
            throw new Exception("Large XP must not overflow or loop forever.");
        var profile = new TS_SE_Tool.SaveFileProfileData { CachedExperiencePoints = uint.MaxValue };
        if (!profile.getPlayerLvl().SequenceEqual(maximum)) throw new Exception("Profile level calculation differs from the save.");

        var lines = File.ReadAllLines(path);
        var save = new SiiNunit(lines);
        TS_SE_Tool.FormMain.SiiNunitData = save;
        save.InitializeCompatibility(lines, 105);
        uint beforeXp = save.Economy.experience_points;
        var beforeSkills = save.Economy._playerSkills.ToArray();
        if (!save.Economy.UnlockSkillsMenu()) throw new Exception("The fixture must contain a Skills restriction.");
        if (save.Economy.experience_points != beforeXp || !save.Economy._playerSkills.SequenceEqual(beforeSkills))
            throw new Exception("Unlocking Skills changed XP or skill points.");
        File.WriteAllText(path + ".unlock.sii", save.PrintOut(105));
        save.Economy.setPlayerExp(300);
        if (save.Economy.experience_points != 1942499) throw new Exception("Level 300 XP is incorrect.");
        File.WriteAllText(path + ".level300.sii", save.PrintOut(105));
        Console.WriteLine("Skills-only unlock, automatic unlock, level 300 and uint XP boundaries passed.");
    }
}
