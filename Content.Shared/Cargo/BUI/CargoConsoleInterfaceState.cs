using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.BUI;

[NetSerializable, Serializable]
public sealed class CargoConsoleInterfaceState : BoundUserInterfaceState
{
    public string Name;
    public int Count;
    public int Capacity;
    public int Balance;
    public List<CargoOrderData> Orders;
    //SS14RU - start
    public string? CurrencyPrototype;
    //SS14RU - end

    //SS14RU - start
    //public CargoConsoleInterfaceState(string name, int count, int capacity, int balance, List<CargoOrderData> orders)
    public CargoConsoleInterfaceState(string name, int count, int capacity, int balance, List<CargoOrderData> orders, string? currencyPrototype = null)
    //SS14RU - end
    {
        Name = name;
        Count = count;
        Capacity = capacity;
        Balance = balance;
        Orders = orders;
        //SS14RU - start
        CurrencyPrototype = currencyPrototype;
        //SS14RU - end
    }
}
