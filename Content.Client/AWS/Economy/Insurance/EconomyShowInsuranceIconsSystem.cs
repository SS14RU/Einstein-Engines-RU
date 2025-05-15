using Content.Shared.Overlays;
using Content.Shared.Security.Components;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Content.Client.Overlays;
using Content.Shared.AWS.Economy.Insurance;
using Robust.Shared.Timing;
using Robust.Client.Timing;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.IdentityManagement;

namespace Content.Client.AWS.Economy.Insurance;

public sealed class EconomyShowInsuranceIconsSystem : EquipmentHudSystem<EconomyShowInsuranceIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IClientGameTiming _timing = default!;
    [Dependency] private readonly EconomyInsuranceSystem _insurance = default!;

    private readonly TimeSpan _checkTimeRelay = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EconomyInsuranceComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, EconomyInsuranceComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        if (!TryGetInsuranceIcon(uid, component, out var icon))
            return;

        ev.StatusIcons.Add(icon);
    }

    private bool TryGetInsuranceIcon(EntityUid uid, EconomyInsuranceComponent component, [NotNullWhen(true)] out EconomyInsuranceIconPrototype? icon)
    {
        icon = null;

        var curTime = _timing.CurTime;
        if (component.NextIconCheck >= curTime)
        {
            icon = component.Icon;
            return true;
        }

        //Prevent to dissable memory leak
        component.Icon = null!;
        var charName = Identity.Name(uid, EntityManager);

        if (_insurance.TryGetInsuranceRecord(charName, out var info))
            if (_prototype.TryIndex(info.InsuranceProto, out var prototype))
                if (_prototype.TryIndex(prototype.Icon, out var indexedIcon))
                {
                    component.Icon = indexedIcon;
                    component.NextIconCheck = curTime + _checkTimeRelay;

                    icon = indexedIcon;

                    return true;
                }

        return false;
    }
}
