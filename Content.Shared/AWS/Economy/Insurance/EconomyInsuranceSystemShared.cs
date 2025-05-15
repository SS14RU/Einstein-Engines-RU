using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Access.Components;
using Content.Shared.AWS.Economy.Bank;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.AWS.Economy.Insurance;

public abstract class EconomyInsuranceSystemShared : EntitySystem
{
    [PublicAPI]
    public bool TryGetInsuranceRecord(string charName, [NotNullWhen(true)] out EconomyInsuranceInfo? info)
    {
        info = null;

        if (!TryGetServer(out var server))
            return false;

        info = server.Value.Comp.InsuranceInfo.FirstOrDefault(x => x.InsurerName == charName);

        return info is not null;
    }
    [PublicAPI]
    public bool TryGetServer([NotNullWhen(true)] out Entity<EconomyInsuranceServerComponent>? server)
    {
        server = GetServer();

        return server?.Comp is not null;
    }

    private Entity<EconomyInsuranceServerComponent>? GetServer()
    {
        EntityManager.EntityQueryEnumerator<EconomyInsuranceServerComponent>().MoveNext(out var uid, out var comp);

        return (uid, comp)!;
    }
}
