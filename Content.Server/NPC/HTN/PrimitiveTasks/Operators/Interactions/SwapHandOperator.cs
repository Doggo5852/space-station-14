using System.Threading;
using System.Threading.Tasks;
using Content.Server.Hands.Systems;
using Content.Shared.Hands.Components;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;


/// <summary>
/// Swaps to any free hand.
/// </summary>
/// 
/// This is also just a massive mess, i should probably make it cleaner
public sealed partial class SwapToFullHandOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        return (true, null);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
    var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

    if (!_entManager.TryGetComponent<HandsComponent>(owner, out var handsComp))
        return HTNOperatorStatus.Failed;

    var handsSystem = _entManager.System<HandsSystem>();

    const string rightHand = "body_part_slot_right_hand";

    
    handsSystem.SetActiveHand((owner, handsComp), rightHand);

    return HTNOperatorStatus.Finished;
    }
}
