//SS14RU - start
using System.Collections.Generic;
//SS14RU - end
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.BUI;

[NetSerializable, Serializable]
//SS14RU - start
public class CargoConsoleInterfaceState : BoundUserInterfaceState
//SS14RU - end
{
    public string Name;
    public int Count;
    public int Capacity;
    public int Balance;
    public List<CargoOrderData> Orders;

    public CargoConsoleInterfaceState(string name, int count, int capacity, int balance, List<CargoOrderData> orders)
    {
        Name = name;
        Count = count;
        Capacity = capacity;
        Balance = balance;
        Orders = orders;
    }
}
