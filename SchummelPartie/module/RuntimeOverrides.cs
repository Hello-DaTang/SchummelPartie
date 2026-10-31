using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using Object = UnityEngine.Object;

namespace SchummelPartie.module;

// Keep the first value for each object/member, even when a cheat writes every frame.
public sealed class RuntimeOverrides
{
    private readonly Dictionary<Object, Dictionary<string, Action>> _restore = new();

    public void Remember(Object target, string member, Action restore)
    {
        if (target == null) return;
        if (!_restore.TryGetValue(target, out var members))
            _restore[target] = members = new Dictionary<string, Action>();
        if (!members.ContainsKey(member)) members.Add(member, restore);
    }

    public void Set<T>(Object target, string member, Func<T> read, Action<T> write, T value)
    {
        if (target == null) return;
        if (!_restore.TryGetValue(target, out var members) || !members.ContainsKey(member))
        {
            var original = read();
            Remember(target, member, () => write(original));
        }
        write(value);
    }

    public void SetField<T>(Object target, FieldInfo field, T value)
    {
        if (target == null || field == null) return;
        Set(target, field.Name, () => (T)field.GetValue(target), original => field.SetValue(target, original), value);
    }

    public void Restore(Object target, string member)
    {
        // Destroyed Unity objects still need to be removed from the dictionary.
        if (ReferenceEquals(target, null) || !_restore.TryGetValue(target, out var members) ||
            !members.TryGetValue(member, out var restore)) return;
        members.Remove(member);
        if (members.Count == 0) _restore.Remove(target);
        TryRestore(target, restore);
    }

    public void RestoreAll()
    {
        var pending = new List<KeyValuePair<Object, Dictionary<string, Action>>>(_restore);
        _restore.Clear();
        foreach (var entry in pending)
            foreach (var restore in entry.Value.Values)
                TryRestore(entry.Key, restore);
    }

    private static void TryRestore(Object target, Action restore)
    {
        if (target == null) return;
        try
        {
            restore();
        }
        catch (Exception exception)
        {
            MelonLogger.Warning("[Pause] Could not restore a runtime value: " + exception.Message);
        }
    }
}
