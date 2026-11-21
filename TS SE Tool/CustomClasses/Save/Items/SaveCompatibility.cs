using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TS_SE_Tool.Save.DataFormat;

namespace TS_SE_Tool.Save.Items
{
    // Compare against the old writer's initial output so untouched data stays exactly as loaded.
    internal class SaveCompatibility
    {
        private sealed class Block
        {
            internal string Header;
            internal Dictionary<string, List<string>> Fields = new Dictionary<string, List<string>>();
        }

        private readonly Dictionary<string, Block> original;
        private Dictionary<string, Block> baseline;
        private readonly Dictionary<string, string[]> vehicleFields = new Dictionary<string, string[]>();
        private string playerName;

        internal SaveCompatibility(string[] lines)
        {
            original = ReadBlocks(string.Join(Environment.NewLine, lines));
        }

        internal void BindVehicles(Player player)
        {
            playerName = original.Single(x => x.Value.Header.Split(':')[0].Trim() == "player").Key;
            var fields = original[playerName].Fields;
            if (!fields.ContainsKey("assigned_vehicles"))
                return;

            var assigned = Value(fields["assigned_vehicles"][0]);
            // The existing truck controls operate on the truck slot; other vehicle modes stay untouched.
            if (Value(fields["assigned_vehicles_mode"][0]) == "truck" && original.ContainsKey(assigned))
            {
                Alias("assigned_truck", assigned, "vehicle", v => player.assigned_truck = v);
                Alias("assigned_trailer", assigned, "trailer", v => player.assigned_trailer = v);
                Alias("truck_placement", assigned, "stored_vehicle_placement", v => player.truck_placement = new SCS_Placement(v));
                Alias("trailer_placement", assigned, "stored_trailer_placements[0]", v => player.trailer_placement = new SCS_Placement(v));
                Alias("assigned_trailer_connected", assigned, "stored_trailer_attached", v => player.assigned_trailer_connected = bool.Parse(v));
            }
            var ownedLine = fields["my_vehicles"].FirstOrDefault(x => x.TrimStart().StartsWith("my_vehicles[0]:"));
            if (ownedLine != null && original.ContainsKey(Value(ownedLine)))
            {
                var owned = Value(ownedLine);
                Alias("my_truck", owned, "vehicle", v => player.my_truck = v);
                Alias("my_trailer", owned, "trailer", v => player.my_trailer = v);
                Alias("my_truck_placement", owned, "stored_vehicle_placement", v => player.my_truck_placement = new SCS_Placement(v));
                Alias("my_trailer_placement", owned, "stored_trailer_placements[0]", v => player.my_trailer_placement = new SCS_Placement(v));
                Alias("my_trailer_attached", owned, "stored_trailer_attached", v => player.my_trailer_attached = bool.Parse(v));
            }
        }

        private void Alias(string oldField, string block, string newField, Action<string> load)
        {
            vehicleFields.Add(oldField, new[] { block, newField });
            var key = Family(newField);
            if (original[block].Fields.TryGetValue(key, out var lines))
            {
                var line = lines.FirstOrDefault(x => x.TrimStart().StartsWith(newField + ":"));
                if (line != null) load(Value(line));
            }
        }

        internal void CaptureBaseline(string text)
        {
            baseline = ReadBlocks(text);
        }

        internal static string ReadTruckPosition(string[] lines)
        {
            var blocks = ReadBlocks(string.Join(Environment.NewLine, lines));
            var player = blocks.Values.Single(x => x.Header.Split(':')[0].Trim() == "player");
            return Value(TruckBlock(blocks, player).Fields[player.Fields.ContainsKey("assigned_vehicles")
                ? "stored_vehicle_placement" : "truck_placement"][0]);
        }

        internal static string MoveTruck(string[] lines, string position)
        {
            var blocks = ReadBlocks(string.Join(Environment.NewLine, lines));
            var player = blocks.Values.Single(x => x.Header.Split(':')[0].Trim() == "player");
            var truck = TruckBlock(blocks, player);
            bool modern = player.Fields.ContainsKey("assigned_vehicles");
            SetField(truck, modern ? "stored_vehicle_placement" : "truck_placement", new SCS_Placement(position).ToString());
            if (modern)
            {
                var ownedLine = player.Fields["my_vehicles"].FirstOrDefault(x => x.TrimStart().StartsWith("my_vehicles[0]:"));
                if (ownedLine != null && blocks.TryGetValue(Value(ownedLine), out var owned)
                    && Value(owned.Fields["vehicle"][0]) == Value(truck.Fields["vehicle"][0]))
                    SetField(owned, "stored_vehicle_placement", new SCS_Placement(position).ToString());
            }
            else SetField(player, "trailer_placement", "(0, 0, 0) (1; 0, 0, 0)");
            var placements = modern ? "stored_trailer_placements" : "slave_trailer_placements";
            if (truck.Fields.TryGetValue(placements, out var array))
                for (int i = 1; i < array.Count; i++)
                    array[i] = array[i].Split(':')[0] + ": (0, 0, 0) (1; 0, 0, 0)";
            return Render(blocks);
        }

        private static Block TruckBlock(Dictionary<string, Block> blocks, Block player)
        {
            if (!player.Fields.ContainsKey("assigned_vehicles")) return player;
            if (Value(player.Fields["assigned_vehicles_mode"][0]) != "truck")
                throw new InvalidDataException("Select truck mode in the game before editing truck positions.");
            return blocks[Value(player.Fields["assigned_vehicles"][0])];
        }

        internal string Merge(string text)
        {
            if (baseline == null) throw new InvalidOperationException("Save baseline has not been captured.");
            var edited = ReadBlocks(text);
            var result = ReadBlocks(Render(original));
            foreach (var item in edited)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                    continue;
                }
                if (!baseline.TryGetValue(item.Key, out var before))
                    throw new InvalidDataException("Missing baseline block: " + item.Key);
                foreach (var field in item.Value.Fields)
                {
                    if (before.Fields.TryGetValue(field.Key, out var previous) && previous.SequenceEqual(field.Value))
                        continue;
                    if (item.Key == playerName && vehicleFields.TryGetValue(field.Key, out var target))
                        SetField(result[target[0]], target[1], Value(field.Value[0]));
                    else
                        result[item.Key].Fields[field.Key] = field.Value;
                }
                foreach (var removed in before.Fields.Keys.Except(item.Value.Fields.Keys))
                    result[item.Key].Fields.Remove(removed);
            }
            foreach (var removed in baseline.Keys.Except(edited.Keys))
                result.Remove(removed);
            return Render(result);
        }

        private static void SetField(Block block, string field, string value)
        {
            var family = Family(field);
            if (field == family)
            {
                block.Fields[field] = new List<string> { " " + field + ": " + value };
                return;
            }
            if (!block.Fields.TryGetValue(family, out var lines))
                throw new InvalidDataException("Missing vehicle placement array: " + family);
            int index = lines.FindIndex(x => x.TrimStart().StartsWith(field + ":"));
            if (index >= 0) lines[index] = " " + field + ": " + value;
            else
            {
                // A newly positioned trailer needs an array entry as well as its count.
                if (!field.EndsWith("[0]") || Value(lines[0]) != "0")
                    throw new InvalidDataException("Missing vehicle placement: " + field);
                lines[0] = " " + family + ": 1";
                lines.Add(" " + field + ": " + value);
            }
        }

        private static string Value(string line) => line.Substring(line.IndexOf(':') + 1).Trim();
        private static string Family(string field) => field.Split('[')[0];

        private static Dictionary<string, Block> ReadBlocks(string text)
        {
            var blocks = new Dictionary<string, Block>();
            Block current = null;
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    int colon = trimmed.IndexOf(':');
                    if (current == null && colon >= 0 && trimmed.EndsWith("{"))
                    {
                        var name = trimmed.Substring(colon + 1).TrimEnd('{').Trim();
                        current = new Block { Header = trimmed };
                        blocks.Add(name, current);
                    }
                    else if (trimmed == "}") current = null;
                    else if (current != null && colon >= 0)
                    {
                        var key = Family(trimmed.Substring(0, colon).Trim());
                        if (!current.Fields.TryGetValue(key, out var fields))
                            current.Fields.Add(key, fields = new List<string>());
                        fields.Add(trimmed);
                    }
                    else if (current != null && trimmed.Length != 0)
                        throw new InvalidDataException("Unsupported save line: " + line);
                }
            }
            if (current != null) throw new InvalidDataException("Unclosed save block.");
            return blocks;
        }

        private static string Render(Dictionary<string, Block> blocks)
        {
            var output = new StringBuilder("SiiNunit\r\n{\r\n");
            foreach (var block in blocks.Values)
            {
                output.AppendLine(block.Header);
                foreach (var field in block.Fields.Values)
                    foreach (var line in field) output.AppendLine(" " + line.TrimStart());
                output.AppendLine("}");
            }
            return output.AppendLine("}").ToString();
        }
    }
}
