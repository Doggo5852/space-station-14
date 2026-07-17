using Content.Shared.Damage.Components;
using Content.Shared.Damage;

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class HurtPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;


    [DataField("minDamage")]
    public float MinDamage = 80f;

    [DataField("maxDamage")]
    public float MaxDamage = 20f;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (_entManager.TryGetComponent<DamageableComponent>(owner, out var damageableComp))
        {
            if (damageableComp.TotalDamage.Float() > MinDamage)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}

public sealed partial class NotHurtPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;


    [DataField("maxDamage")]
    public float MaxDamage = 20f;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (_entManager.TryGetComponent<DamageableComponent>(owner, out var damageableComp))
        {
            if (damageableComp.TotalDamage.Float() < MaxDamage)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}