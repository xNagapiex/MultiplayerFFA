using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Tags
{
    public static readonly ushort SpawnPlayerTag = 0;
    public static readonly ushort MovePlayerTag = 1;
    public static readonly ushort DespawnPlayerTag = 2;
    public static readonly ushort GatherItemTag = 3;
    public static readonly ushort GatherSpotsTag = 4;
    public static readonly ushort PlayerJoinedTag = 5;
    public static readonly ushort InventoryUpdateTag = 6;
    public static readonly ushort DisconnectLobbyPlayerTag = 7;
    public static readonly ushort StartGameTag = 8;
}