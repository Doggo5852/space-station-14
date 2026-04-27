using System.Linq;
using System.Numerics;
using Content.Server.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Content.Server.Popups;
using Robust.Server.Player;
using Content.Shared.Popups;
using Robust.Shared.Physics.Events;


namespace Content.Server.Physics.Controllers;

/// <summary>
/// A system which makes its entity chasing another entity with selected component.
/// </summary>
public sealed class ChasingWalkSystem : VirtualController
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    private readonly HashSet<Entity<IComponent>> _potentialChaseTargets = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChasingWalkComponent, MapInitEvent>(OnChasingMapInit);
        SubscribeLocalEvent<ChasingWalkComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnChasingMapInit(EntityUid uid, ChasingWalkComponent component, MapInitEvent args)
    {
        component.NextImpulseTime = _gameTiming.CurTime;
        component.NextChangeVectorTime = _gameTiming.CurTime;
        component.LoftTime = _gameTiming.CurTime;

        if (component.SmokeTrail)
        {
            foreach (var session in _player.Sessions)
            {
                if (session.AttachedEntity is not { } playerEnt)
                    continue;

                var coords = Transform(playerEnt).Coordinates;

                _popup.PopupCoordinates("You see a smoke trail in the air.", coords, session, PopupType.LargeCaution);
            }
        }
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var query = EntityQueryEnumerator<ChasingWalkComponent>();
        while (query.MoveNext(out var uid, out var chasing))
        {
            //Set Velocity to Target
            if (chasing.NextImpulseTime <= _gameTiming.CurTime)
            {
                ForceImpulse(uid, chasing);
                chasing.NextImpulseTime += TimeSpan.FromSeconds(chasing.ImpulseInterval);
            }
            //Change Target
            if (chasing.NextChangeVectorTime <= _gameTiming.CurTime)
            {
                ChangeTarget(uid, chasing);

                var delay = TimeSpan.FromSeconds(_random.NextFloat(chasing.ChangeVectorMinInterval, chasing.ChangeVectorMaxInterval));
                chasing.NextChangeVectorTime += delay;
            }
            // Lofting 
            if (chasing.LoftTime + TimeSpan.FromSeconds(chasing.RiseTime) <= _gameTiming.CurTime && !chasing.InAir && chasing.CanLoft)
            {
                var coords = Transform(uid).Coordinates;
                _popup.PopupCoordinates("The missile pitches up", coords, PopupType.MediumCaution);
                chasing.InAir = true;
            }
        }
    }

    private void OnPreventCollide(EntityUid uid, ChasingWalkComponent component, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;
        if (component.InAir)
        {
            args.Cancelled = true;
        }

    }


    private void ChangeTarget(EntityUid uid, ChasingWalkComponent component)
    {
        if (component.ChasingComponent.Count <= 0)
            return;

        //We find our coordinates and calculate the radius of the target search.
        var xform = Transform(uid);
        var range = component.MaxChaseRadius;
        var compType = _random.Pick(component.ChasingComponent.Values).Component.GetType();
        _potentialChaseTargets.Clear();
        _lookup.GetEntitiesInRange(compType, _transform.GetMapCoordinates(xform), range, _potentialChaseTargets, LookupFlags.Uncontained);

        //If there are no required components in the radius, don't moving.
        if (_potentialChaseTargets.Count <= 0)
            return;

        //In the case of finding required components, we choose a random one of them and remember its uid.
        component.ChasingEntity = _random.Pick(_potentialChaseTargets).Owner;
        component.Speed = _random.NextFloat(component.MinSpeed, component.MaxSpeed);
    }

    //pushing the entity toward its target
    private void ForceImpulse(EntityUid uid, ChasingWalkComponent component)
    {
        if (Deleted(component.ChasingEntity) || component.ChasingEntity == null)
        {
            ChangeTarget(uid, component);
            return;
        }

        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        //Calculating direction to the target.
        var pos1 = _transform.GetWorldPosition(uid);
        var pos2 = _transform.GetWorldPosition(component.ChasingEntity.Value);

        var delta = pos2 - pos1;
        var speed = delta.Length() > 0 ? delta.Normalized() * component.Speed : Vector2.Zero;

        //_physics.SetLinearVelocity(uid, speed); 
        _physics.SetBodyStatus(uid, physics, BodyStatus.InAir); //If this is not done, from the explosion up close, the tesla will "Fall" to the ground, and almost stop moving.

        //Lead pursuit guidance
        //TODO: should be as a different component or atleast an option
        
        Vector2 targetVelocity = Vector2.Zero;

        if (TryComp<PhysicsComponent>(component.ChasingEntity.Value, out var targetPhysics))
        {
            targetVelocity = targetPhysics.LinearVelocity;
        }

        var distance = delta.Length();

        if (distance <= 0.001f)
            return;

        if (distance <= component.DiveDistance && component.InAir)
        {
            component.InAir = false;
            component.CanLoft = false;
            var coords = Transform(uid).Coordinates;
            _popup.PopupCoordinates("The missile dives rapidly", coords, PopupType.MediumCaution);
        }

        var timeToIntercept = distance / component.Speed;
        var interceptPoint = pos2 + targetVelocity * timeToIntercept;
        var interceptDir = interceptPoint - _transform.GetWorldPosition(uid);
        var currentAngle = _transform.GetWorldRotation(uid);
        

        // First we find out which way do we turn (left is -, right is +)

        var toTarget = interceptDir.Normalized();
        var forward = (currentAngle + Angle.FromDegrees(90)).ToVec();
        var cross = forward.X * toTarget.Y - forward.Y * toTarget.X;

        if (cross < 0)
        {
            // turn left
            currentAngle += component.TurnRate * component.ImpulseInterval;
        }
        else if (cross > 0)
        {
            // turn right
            currentAngle -= component.TurnRate * component.ImpulseInterval;
        }

        _transform.SetWorldRotation(uid, currentAngle);
        currentAngle = _transform.GetWorldRotation(uid) - Angle.FromDegrees(90);
        var velocityToIntercept = currentAngle.ToVec() * component.Speed; // just breaks
        _physics.SetLinearVelocity(uid, velocityToIntercept);
        if (component.RotateWithImpulse)
        {
            var ang = velocityToIntercept.ToAngle() + Angle.FromDegrees(90); // we want "Up" to be forward, bullet convention.
            //_transform.SetWorldRotation(uid, currentAngle + Angle.FromDegrees(90));
        }
    }
}
