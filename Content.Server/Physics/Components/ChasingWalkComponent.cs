
using Content.Server.Administration.Systems;
using Content.Server.Physics.Controllers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Physics.Components;

/// <summary>
/// A component which makes its entity chasing entity with selected component.
/// </summary>
[RegisterComponent, Access(typeof(ChasingWalkSystem), typeof(AdminVerbSystem)), AutoGenerateComponentPause]
public sealed partial class ChasingWalkComponent : Component
{
    /// <summary>
    /// The next moment in time when the entity is pushed toward its goal
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan NextImpulseTime;

    /// <summary>
    /// Push-to-target frequency.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ImpulseInterval = 2f;

    /// <summary>
    /// The minimum speed at which this entity will move.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MinSpeed = 1.5f;

    /// <summary>
    /// The maximum speed at which this entity will move.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxSpeed = 3f;

    /// <summary>
    /// The current speed.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Speed;

    /// <summary>
    /// The minimum time interval in which an object can change its motion target.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ChangeVectorMinInterval = 5f;

    /// <summary>
    /// The maximum time interval in which an object can change its motion target.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ChangeVectorMaxInterval = 25f;

    /// <summary>
    /// The next change of direction time.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan NextChangeVectorTime;

    /// <summary>
    /// The component that the entity is chasing
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry ChasingComponent = [];

    /// <summary>
    /// The maximum radius in which the entity chooses the target component to follow
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxChaseRadius = 25;

    /// <summary>
    /// The entity uid, chasing by the component owner
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? ChasingEntity;

    /// <summary>
    /// whether the entity should point in the direction its moving
    /// </summary>
    [DataField]
    public bool RotateWithImpulse;

    /// <summary>
    /// Should the object give a pop up
    /// </summary>
    
    [DataField]
    public bool SmokeTrail;

    /// <summary>
    /// Can the missile gain altitude in order to not collide with obstacles
    /// </summary>
    
    [DataField]
    public bool CanLoft;

    /// <summary>
    /// Delay after which the missile goes up. Changes InAir to true
    /// </summary>

    [DataField]
    public float RiseTime = 3f;

    /// <summary>
    /// When is the missile going up
    /// </summary>

    [DataField]
    public TimeSpan LoftTime;
    
    /// <summary>
    /// When should the missile dive down for the target. Changes InAir to false
    /// </summary>

    [DataField]
    public float DiveDistance = 20f;

    /// <summary>
    /// Is the missile currently in air
    /// </summary>

    [DataField]
    public bool InAir;
    /// <summary>
    /// Sprite rotation offset.
    /// </summary>
    [DataField]
    public Angle RotationAngleOffset = Angle.Zero;
}
