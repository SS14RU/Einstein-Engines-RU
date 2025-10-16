using System.Collections.Generic;
using Content.Shared.Cargo;
using Content.Shared.Cargo.BUI;
using Robust.Shared.Serialization;

namespace Content.Shared.AWS.Economy.Cargo;

[NetSerializable, Serializable]
public sealed class CargoConsoleAwsInterfaceState : CargoConsoleInterfaceState
{
    public string? CurrencyPrototype;

    public CargoConsoleAwsInterfaceState(string name, int count, int capacity, int balance, List<CargoOrderData> orders, string? currencyPrototype)
        : base(name, count, capacity, balance, orders)
    {
        CurrencyPrototype = currencyPrototype;
    }
}

