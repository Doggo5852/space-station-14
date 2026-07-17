namespace Content.Server.NPC.Components;

/// <summary>
/// Marker for an NPC leader
/// </summary>
[RegisterComponent]
public sealed partial class NPCLeaderComponent : Component
{
}

[RegisterComponent]
public sealed partial class NPCLeaderBlueComponent : Component
{
}

[RegisterComponent]
public sealed partial class NPCLeaderRedComponent : Component
{
}


// waypoint style stuff
[RegisterComponent]
public sealed partial class NPCHealMarkerComponent : Component
{
}
[RegisterComponent]
public sealed partial class NPCAttackMarkerComponent : Component
{
}
[RegisterComponent]
public sealed partial class NPCDefendMarkerComponent : Component
{
}