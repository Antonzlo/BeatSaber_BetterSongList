using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// PoC sorter. BetterSongList ожидает у sorter members: isReady, Prepare(token), DoSort(ref IEnumerable<BeatmapLevel>, bool).
// Здесь сигнатуры реализованы через обычные имена, а вызов — через reflection от BetterSongList.
public class DifficultyExpandingSorter {
    readonly string[] desiredOrder;

    public bool isReady => true;

    public DifficultyExpandingSorter(params string[] order) {
        desiredOrder = order ?? new[] { "Easy", "Normal", "Hard", "Expert", "ExpertPlus" };
    }

    public Task Prepare(CancellationToken token) => Task.CompletedTask;

    // Обратите внимание: BetterSongList может вызывать DoSort с IEnumerable<BeatmapLevel>.
    // Здесь используем общую сигнатуру с IEnumerable<object>, чтобы reflection был более гибким.
    public void DoSort(ref IEnumerable<object> outV, bool asc) {
        var list = outV?.ToList() ?? new List<object>();
        var expanded = new List<object>();

        Type previewIface = null;
        var sample = list.FirstOrDefault();
        if (sample != null) {
            var t = sample.GetType();
            previewIface = Array.Find(t.GetInterfaces(), x => x.Name == "IPreviewBeatmapLevel" || x.Name.Contains("PreviewBeatmapLevel"));
        }

        foreach (var lvl in list) {
            var diffs = GetDifficultiesForLevel(lvl);
            if (diffs == null || diffs.Count == 0) {
                expanded.Add(lvl);
                continue;
            }

            foreach (var diff in diffs) {
                if (previewIface != null) {
                    var proxy = LevelDifficultyProxy.Create(previewIface, lvl, diff);
                    if (proxy != null) expanded.Add(proxy);
                    else expanded.Add(lvl);
                } else {
                    expanded.Add(lvl);
                }
            }
        }

        expanded = expanded
            .OrderBy(x => GetDifficultyIndexForItem(x))
            .ThenBy(x => GetLevelName(x), StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!asc) expanded.Reverse();
        outV = expanded;
    }

    int GetDifficultyIndexForItem(object item) {
        var name = LevelDifficultyProxy.TryGetDifficultyName(item);
        if (name == null) return desiredOrder.Length;
        for (int i = 0; i < desiredOrder.Length; ++i)
            if (string.Equals(desiredOrder[i], name, StringComparison.OrdinalIgnoreCase)) return i;
        return desiredOrder.Length;
    }

    string GetLevelName(object item) {
        if (item == null) return "";
        try {
            var t = item.GetType();
            var prop = t.GetProperty("songName") ?? t.GetProperty("levelName") ?? t.GetProperty("name");
            var v = prop?.GetValue(item);
            return v?.ToString() ?? "";
        } catch { return ""; }
    }

    List<string> GetDifficultiesForLevel(object lvl) {
        if (lvl == null) return null;
        try {
            var t = lvl.GetType();
            var candidates = new[] { "previewDifficultyBeatmapSets", "difficultyBeatmapSets", "previewDifficultyBeatmapSets", "difficultyBeatmaps" };
            foreach (var name in candidates) {
                var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p == null) continue;
                var setsObj = p.GetValue(lvl) as IEnumerable;
                if (setsObj == null) continue;
                var res = new List<string>();
                foreach (var set in setsObj) {
                    if (set == null) continue;
                    var inner = set.GetType().GetProperty("difficultyBeatmaps", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(set) as IEnumerable
                             ?? set.GetType().GetProperty("beatmaps", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(set) as IEnumerable;
                    if (inner == null) continue;
                    foreach (var db in inner) {
                        if (db == null) continue;
                        var diffProp = db.GetType().GetProperty("difficulty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                    ?? db.GetType().GetProperty("beatmapDifficulty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                    ?? db.GetType().GetProperty("difficultyName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var val = diffProp?.GetValue(db);
                        if (val != null) {
                            var s = val.ToString();
                            if (!string.IsNullOrEmpty(s) && !res.Contains(s, StringComparer.OrdinalIgnoreCase)) res.Add(s);
                        }
                    }
                }
                if (res.Count > 0) return res;
            }
        } catch { }
        return null;
    }
}
