using System.Collections.Generic;
using UnityEngine;

public enum PauseReason { PauseMenu, Dialogue, GameOver }

/// <summary>The only code allowed to write Time.timeScale. Time runs only when no reason holds it.</summary>
public static class GamePause
{
    private static readonly HashSet<PauseReason> holds = new HashSet<PauseReason>();

    public static bool IsPaused => holds.Count > 0;
    public static bool Has(PauseReason r) => holds.Contains(r);

    public static void Hold(PauseReason r)    { if (holds.Add(r))    Apply(); }
    public static void Release(PauseReason r) { if (holds.Remove(r)) Apply(); }
    public static void ReleaseAll()           { holds.Clear(); Apply(); }

    private static void Apply() => Time.timeScale = holds.Count > 0 ? 0f : 1f;

    // Static state survives if Domain Reload is disabled in the editor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnStart() => holds.Clear();
}