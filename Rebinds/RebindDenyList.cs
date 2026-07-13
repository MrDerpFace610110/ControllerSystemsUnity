using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to define which keys or buttons cannot be used when rebinded
/// </summary>
[CreateAssetMenu(menuName = "Controller System/Rebind Deny List")]
public class RebindDenyList : ScriptableObject
{
    [System.Serializable]
    public class GroupBlockList
    {
        public string groupName;
        public List<string> blockedControlPaths = new();
    }

    public List<string> globalBlockedControlPaths = new();
    public List<GroupBlockList> groupBlockedControlPaths = new();

    public IEnumerable<string> GetBlockedPaths(string groupName)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach(var p in globalBlockedControlPaths)
        {
            if(!string.IsNullOrWhiteSpace(p) && seen.Add(p))
                yield return p;
        }

        foreach(var g in groupBlockedControlPaths)
        {
            if(!string.Equals(g.groupName?.Trim(), groupName?.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            foreach(var p in g.blockedControlPaths)
            {
                if(!string.IsNullOrWhiteSpace(p) && seen.Add(p))
                    yield return p;
            }
        }
    }
}