using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;
using System.Data;

namespace GamePlugin
{


    public class PlayerManager : Plugin
    {
        Database DB;
        bool gatherSpotsSpawned;
        bool gameStarted;
        List<int> takenColors = new List<int>(); // This exists to make it easier to filter out taken colors

        const float MAP_WIDTH = 20;

        // Multithreading set to false
        public override bool ThreadSafe => false;

        // Version number
        public override Version Version => new Version(1, 3, 1);

        public PlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
            DB = new Database();

            // Deleting old data in case it hasn't already been deleted
            string gatherSpotWipeQuery = "DELETE FROM GatherSpots";
            SQLiteCommand gatherSpotWipeCommand = new SQLiteCommand(gatherSpotWipeQuery, DB.myConnection);
            int gatherSpotWipeResult = gatherSpotWipeCommand.ExecuteNonQuery();
            string playersWipeQuery = "DELETE FROM Players";
            SQLiteCommand playersWipeCommand = new SQLiteCommand(playersWipeQuery, DB.myConnection);
            int playersWipeResult = playersWipeCommand.ExecuteNonQuery();
            string inventoryWipeQuery = "DELETE FROM InventorySlots";
            SQLiteCommand inventoryWipeCommand = new SQLiteCommand(inventoryWipeQuery, DB.myConnection);
            int inventoryWipeResult = inventoryWipeCommand.ExecuteNonQuery();

            gatherSpotsSpawned = false;
            gameStarted = false;
        }

        // What happens when a new client connects
        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            if (ClientManager.Count < 4)
            {
                Console.WriteLine("New playerCount: " + ClientManager.Count);
                SpawnGatherSpots(e.Client); // <---------- RELOCATE THIS TO WHEREVER THE GAME ACTUALLY BEGINS

                ushort newPlayerID = Convert.ToUInt16(ClientManager.Count);

                Random r = new Random();

                float newPlayerX = (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2;
                float newPlayerY = (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2;

                // Write new player's info to DB
                string newPlayerQuery = "INSERT INTO Players (ClientID, PlayerID, Health, PosX, PosY, MousePosX, MousePosY) values (@ClientID, @PlayerID, @Health, @PosX, @PosY, 0, 0)";
                SQLiteCommand newPlayerCommand = new SQLiteCommand(newPlayerQuery, DB.myConnection);
                newPlayerCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                newPlayerCommand.Parameters.AddWithValue("@PlayerID", newPlayerID);
                newPlayerCommand.Parameters.AddWithValue("@Health", 100);
                newPlayerCommand.Parameters.AddWithValue("@PosX", newPlayerX);
                newPlayerCommand.Parameters.AddWithValue("@PosY", newPlayerY);
                int newPlayerResult = newPlayerCommand.ExecuteNonQuery();

                int newColorNo = r.Next(1, 10);
                bool colorTaken = true;

                while(colorTaken)
                {
                    if(takenColors.Contains(newColorNo))
                    {
                        ++newColorNo;

                        if(newColorNo > 9)
                        {
                            newColorNo = 1;
                        }
                    }
                    
                    else
                    {
                        takenColors.Add(newColorNo);
                        colorTaken = false;
                    }
                }

                // Mark color's owner in the DB
                string markColorQuery = "UPDATE Colors SET ClientID = @ClientID WHERE ColorID = @MyColor";
                SQLiteCommand markColorCommand = new SQLiteCommand(markColorQuery, DB.myConnection);
                markColorCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                markColorCommand.Parameters.AddWithValue("@MyColor", newColorNo);
                int markColorResult = markColorCommand.ExecuteNonQuery();


                // Announce new player and their relevant stats to other clients (id, position and color)
                // Don't do this if there is only one client connected - for obvious reasons
                if (ClientManager.Count > 1)
                {
                    using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
                    {
                        newPlayerWriter.Write(e.Client.ID);
                        newPlayerWriter.Write(newPlayerID);

                        // Get the values of the color and send them
                        string colorValuesQuery = "SELECT R, G, B FROM Colors WHERE ClientID = (@ClientID)";
                        SQLiteCommand colorValuesCommand = new SQLiteCommand(colorValuesQuery, DB.myConnection);
                        colorValuesCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                        SQLiteDataReader colorValuesResult = colorValuesCommand.ExecuteReader();

                        if (colorValuesResult.HasRows)
                        {
                            while (colorValuesResult.Read())
                            {
                                byte R;
                                byte G;
                                byte B;

                                Byte.TryParse(colorValuesResult["R"].ToString(), out R);
                                Byte.TryParse(colorValuesResult["G"].ToString(), out G);
                                Byte.TryParse(colorValuesResult["B"].ToString(), out B);

                                newPlayerWriter.Write(R);
                                newPlayerWriter.Write(G);
                                newPlayerWriter.Write(B);
                            }
                        }

                        //// Use this for stuff with Lobby
                        //using (Message newPlayerMessage = Message.Create(Tags.PlayerJoinedTag, newPlayerWriter))
                        //{
                        //    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        //        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                        //}

                        // Lobbyless alternative
                        using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                        {
                            foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                                client.SendMessage(newPlayerMessage, SendMode.Reliable);
                        }
                    }
                }

                // Send the new player their own info (id, position, color) and the info about other players
                using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
                {
                    // Get the lobby info of all players
                    string existingPlayersQuery = "SELECT Players.ClientID as Client, PlayerID, R, G, B FROM Players INNER JOIN Colors ON Players.ClientID = Colors.ClientID";
                    SQLiteCommand existingPlayersCommand = new SQLiteCommand(existingPlayersQuery, DB.myConnection);
                    SQLiteDataReader existingPlayersResult = existingPlayersCommand.ExecuteReader();

                    if (existingPlayersResult.HasRows)
                    {
                        while (existingPlayersResult.Read())
                        {
                            ushort clientID;
                            ushort playerID;
                            byte R;
                            byte G;
                            byte B;

                            UInt16.TryParse(existingPlayersResult["Client"].ToString(), out clientID);
                            UInt16.TryParse(existingPlayersResult["PlayerID"].ToString(), out playerID);
                            Byte.TryParse(existingPlayersResult["R"].ToString(), out R);
                            Byte.TryParse(existingPlayersResult["G"].ToString(), out G);
                            Byte.TryParse(existingPlayersResult["B"].ToString(), out B);

                            playerWriter.Write(clientID);
                            playerWriter.Write(playerID);
                            playerWriter.Write(R);
                            playerWriter.Write(G);
                            playerWriter.Write(B);
                        }
                    }

                    //// Use this for stuff with Lobby
                    //using (Message playerMessage = Message.Create(Tags.PlayerJoinedTag, playerWriter))
                    //    e.Client.SendMessage(playerMessage, SendMode.Reliable);

                    // Lobbyless alternative
                    using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                        e.Client.SendMessage(playerMessage, SendMode.Reliable);
                }

                // If a message is received from a connected client, send it to CheckMessageTag to be handled
                e.Client.MessageReceived += CheckMessageTag;
            }

            else
            {
                // ADD SOME MORE ELEGANT WAY TO HANDLE A CLIENT WHEN THERE ARE ALREADY 4 PLAYERS ON THE SERVER
                e.Client.Disconnect();
            }
        }

        // Redirects received messages to the right method
        void CheckMessageTag(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                // Player movement
                if (message.Tag == Tags.MovePlayerTag)
                {
                    MovementMessageReceived(sender, e);
                }

                // Picking up items
                else if (message.Tag == Tags.GatherItemTag)
                {
                    GatherMessageReceived(sender, e);
                }
            }
        }

        // Reacting to player moving
        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                // Read player's inputs (movement, mouse position)
                using (DarkRiftReader reader = message.GetReader())
                {
                    float newX = reader.ReadSingle();
                    float newY = reader.ReadSingle();
                    float newMX = reader.ReadSingle();
                    float newMY = reader.ReadSingle();

                    // Get PlayerID
                    string playerIDQuery = "SELECT PlayerID FROM Players WHERE ClientID = (@ClientID)";
                    SQLiteCommand playerIDCommand = new SQLiteCommand(playerIDQuery, DB.myConnection);
                    playerIDCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                    SQLiteDataReader playerIDResult = playerIDCommand.ExecuteReader();

                    ushort playerID = 0;

                    if (playerIDResult.HasRows)
                    {
                        while (playerIDResult.Read())
                        {
                            UInt16.TryParse(playerIDResult["PlayerID"].ToString(), out playerID);
                        }
                    }

                    // Send player's inputs to other clients
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(playerID);
                        writer.Write(newX);
                        writer.Write(newY);
                        writer.Write(newMX);
                        writer.Write(newMY);
                        message.Serialize(writer);
                    }

                    foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                        c.SendMessage(message, SendMode.Reliable);

                    string newPlayerQuery = "UPDATE Players SET PosX = (@PosX), PosY = (@PosY), MousePosX = (@MX), MousePosY = (@MY) WHERE ClientID = (@ClientID)";
                    SQLiteCommand newPlayerCommand = new SQLiteCommand(newPlayerQuery, DB.myConnection);
                    newPlayerCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                    newPlayerCommand.Parameters.AddWithValue("@PosX", newX);
                    newPlayerCommand.Parameters.AddWithValue("@PosY", newY);
                    newPlayerCommand.Parameters.AddWithValue("@MX", newMX);
                    newPlayerCommand.Parameters.AddWithValue("@MY", newMY);
                    int newPlayerResult = newPlayerCommand.ExecuteNonQuery();
                }
            }
        }

        // Reacting to player picking up an item
        void GatherMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                // Read spot ID and check if that spot is still available (no one has gathered it yet)
                using (DarkRiftReader reader = message.GetReader())
                {
                    int spotID = reader.ReadUInt16();
                    float spotIDf = spotID;

                    string availabilityQuery = "SELECT IsAvailable FROM GatherSpots WHERE ID = (@SpotID)";
                    SQLiteCommand availabilityCommand = new SQLiteCommand(availabilityQuery, DB.myConnection);
                    availabilityCommand.Parameters.AddWithValue("@SpotID", spotID);
                    SQLiteDataReader isAvailableResult = availabilityCommand.ExecuteReader();
                    int isAvailable = 0;

                    if (isAvailableResult.HasRows)
                    {
                        while (isAvailableResult.Read())
                        {
                            Int32.TryParse(isAvailableResult["IsAvailable"].ToString(), out isAvailable);
                        }
                    }

                    else
                    {
                        Console.WriteLine("Error, could not find gather spot of that ID (IsAvailable search)");
                    }


                    // If gather attempt was successful
                    if (isAvailable == 1)
                    {
                        Console.WriteLine("Attempt to gather " + spotID + " approved");
                        string disableQuery = "UPDATE GatherSpots SET isAvailable = 0 WHERE ID = (@SpotID)";
                        SQLiteCommand disableCommand = new SQLiteCommand(disableQuery, DB.myConnection);
                        disableCommand.Parameters.AddWithValue("@SpotID", spotID);
                        int disableResult = disableCommand.ExecuteNonQuery();

                        // Show everyone which gather spot was taken
                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, SendMode.Reliable);

                        // Check gather spot's itemID
                        string itemIDQuery = "SELECT ItemID FROM GatherSpots WHERE ID = (@SpotID)";
                        SQLiteCommand itemIDCommand = new SQLiteCommand(itemIDQuery, DB.myConnection);
                        itemIDCommand.Parameters.AddWithValue("@SpotID", spotID);
                        SQLiteDataReader itemIDResult = itemIDCommand.ExecuteReader();

                        int itemID = 0;

                        Console.WriteLine("itemIDResult.HasRows: " + itemIDResult.HasRows);

                        if (itemIDResult.HasRows)
                        {
                            while (itemIDResult.Read())
                            {
                                Console.WriteLine("ItemID found");
                                Int32.TryParse(itemIDResult["ItemID"].ToString(), out itemID);
                            }

                        }

                        else
                        {
                            Console.WriteLine("Error, could not find gather spot of that ID (ItemID search)");
                        }

                        // Update player's client with their new inventory
                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write((ushort)itemID);

                            using (Message updatemessage = Message.Create(Tags.InventoryUpdateTag, writer))
                            {
                                e.Client.SendMessage(updatemessage, SendMode.Reliable);
                            }
                        }

                        // Check if player already has a stack of this item in an inventory slot
                        string stackQuery = "SELECT Amount FROM InventorySlots WHERE ItemID = (@ItemID) AND ClientID = (@ClientID)";
                        SQLiteCommand stackCommand = new SQLiteCommand(stackQuery, DB.myConnection);
                        stackCommand.Parameters.AddWithValue("@ItemID", itemID);
                        stackCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                        SQLiteDataReader stackResult = stackCommand.ExecuteReader();

                        bool stackResultHasRows = false;
                        int currentAmount = 0;

                        Console.WriteLine("stackResult.HasRows: " + stackResult.HasRows);

                        // Player has an existing stack of the item so just add one to the current amount
                        if (stackResult.HasRows)
                        {
                            stackResultHasRows = true;

                            while (stackResult.Read())
                            {
                                Int32.TryParse(stackResult["Amount"].ToString(), out currentAmount);
                                ++currentAmount;
                            }

                        }

                        if (stackResultHasRows)
                        {
                            Console.WriteLine("Player has an existing stack");
                            string newAmountQuery = "UPDATE InventorySlots SET Amount = @NewAmount WHERE ItemID = (@ItemID) AND ClientID = (@ClientID)";
                            SQLiteCommand newAmountCommand = new SQLiteCommand(newAmountQuery, DB.myConnection);
                            newAmountCommand.Parameters.AddWithValue("@ItemID", itemID);
                            newAmountCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                            newAmountCommand.Parameters.AddWithValue("@NewAmount", currentAmount);
                            int newAmountResult = newAmountCommand.ExecuteNonQuery();
                        }

                        // Player doesn't have an existing stack so create one
                        else
                        {
                            Console.WriteLine("Player does not have an existing stack");
                            string createStackQuery = "INSERT INTO InventorySlots (ClientID, ItemID, Amount) values ((@ClientID), (@ItemID), 1)";
                            SQLiteCommand createStackCommand = new SQLiteCommand(createStackQuery, DB.myConnection);
                            createStackCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                            createStackCommand.Parameters.AddWithValue("@ItemID", itemID);
                            int createStackResult = createStackCommand.ExecuteNonQuery();
                        }
                        
                    }

                    // If gather attempt failed
                    else
                    {
                        Console.WriteLine("Gather attempt failed");
                    }
                    Console.WriteLine("Inventory operations finished");
                }
            }
        }

        // Writing gather spots received from host to DB - Taru Konttinen 18.4.2018
        void SpawnGatherSpots(IClient client)
        {
            // If gather spots haven't been spawned/map hasn't been generated, generate it now and send it to all players
            if (!gatherSpotsSpawned)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    Console.WriteLine("Spawning gather spots");
                    gatherSpotsSpawned = true;
                    
                    short x = -18;
                    short y = -18;

                    int trees = 32;
                    int herbs = 32;

                    ushort itemID = 0;
                    bool empty = false;

                    Random r = new Random();

                    // I know I want the distance between gather spots to be 4, so I've made the 40x40 into a 10x10 grid
                    // This for fills the grid from bottom right to upper left, filling rows (x) and moving up a column (y) every time the x of 18 has been reached
                    // The data of the gather spots is written to the DB in the progress and sent to the client. Spots that end up being empty aren't reported.
                    for (int i = 0; i < 100; i++)
                    {
                        int randomItem = r.Next(3);
                        Console.WriteLine("RandomItem: " + randomItem);

                        if (trees == 0 & herbs == 0 || randomItem == 2 & 100 - i > herbs + trees)
                        {
                            empty = true;
                        }

                        else if (trees == 0 || randomItem == 0 & herbs > 0)
                        {
                            Console.WriteLine("Herbs is " + herbs);
                            itemID = 0;
                            herbs = herbs - 1;
                        }

                        else if (herbs == 0 || randomItem == 1 & trees > 0)
                        {
                            Console.WriteLine("Trees is " + trees);
                            itemID = 1;
                            trees = trees - 1;
                        }

                        else
                        {
                            if (trees > herbs)
                            {
                                Console.WriteLine("In else");
                                Console.WriteLine("Herbs is " + herbs);
                                itemID = 0;
                                herbs = herbs - 1;
                            }

                            else if (herbs > trees)
                            {
                                Console.WriteLine("In else");
                                Console.WriteLine("Trees is " + trees);
                                itemID = 1;
                                trees = trees - 1;
                            }
                        }

                        if (!empty)
                        {
                            //Writing IDs to DB
                            string writequery = "INSERT INTO GatherSpots VALUES (@ID, @IsAvailable, @ItemID, @PosX, @PosY)";
                            SQLiteCommand writeCommand = new SQLiteCommand(writequery, DB.myConnection);
                            writeCommand.Parameters.AddWithValue("@ID", i);
                            writeCommand.Parameters.AddWithValue("@IsAvailable", 1);
                            writeCommand.Parameters.AddWithValue("@ItemID", itemID);
                            writeCommand.Parameters.AddWithValue("@PosX", x);
                            writeCommand.Parameters.AddWithValue("@PosY", y);
                            int writeresult = writeCommand.ExecuteNonQuery();

                            // Writing pos and other info to message

                            writer.Write((ushort)i);
                            writer.Write(itemID);
                            writer.Write(x);
                            writer.Write(y);
                        }


                        if (x == 18)
                        {
                            x = -18;
                            y += 4;
                        }

                        else
                        {
                            x += 4;
                        }

                        empty = false;
                    }

                    using (Message message = Message.Create(Tags.GatherSpotsTag, writer))
                    {
                        foreach (IClient c in ClientManager.GetAllClients())
                            c.SendMessage(message, SendMode.Reliable);
                    }
                }

                Console.WriteLine("Finished writing gather spots to DB.");
            }


            // In case someone joins late, don't generate the map again, just tell them where things are. (isAvailable not included because people aren't supposed to join after the beginning)
            else
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {

                    string query = "SELECT ID, IsAvailable, ItemID, PosX, PosY FROM GatherSpots";
                    SQLiteCommand myCommand = new SQLiteCommand(query, DB.myConnection);
                    SQLiteDataReader result = myCommand.ExecuteReader();

                    if (result.HasRows)
                    {
                        while (result.Read())
                        {
                            ushort ID;
                            UInt16.TryParse(result["ID"].ToString(), out ID);
                            ushort ItemID;
                            UInt16.TryParse(result["ItemID"].ToString(), out ItemID);
                            short PosX;
                            Int16.TryParse(result["PosX"].ToString(), out PosX);
                            short PosY;
                            Int16.TryParse(result["PosY"].ToString(), out PosY);

                            Console.WriteLine(result["ID"].ToString() + " " + result["ItemID"].ToString() + " " + result["PosX"].ToString() + " " + result["PosY"].ToString());

                            writer.Write(ID);
                            writer.Write(ItemID);
                            writer.Write(PosX);
                            writer.Write(PosY);
                        }

                        using (Message message = Message.Create(Tags.GatherSpotsTag, writer))
                        {
                            client.SendMessage(message, SendMode.Reliable);
                        }
                    }

                }
            }
        }


        // Send a despawn message to remaining clients after removing a disconnected player
        void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Inform other players of the disconnect if there are other players
            if (ClientManager.Count > 1)
            {
                // Look for player with the disconnected client's ID and get their playerID
                string playerIDQuery = "SELECT PlayerID FROM Players WHERE ClientID = (@ClientID)";
                SQLiteCommand playerIDCommand = new SQLiteCommand(playerIDQuery, DB.myConnection);
                playerIDCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
                SQLiteDataReader playerIDResult = playerIDCommand.ExecuteReader();

                ushort playerID = 0;

                if (playerIDResult.HasRows)
                {
                    while (playerIDResult.Read())
                    {
                        UInt16.TryParse(playerIDResult["PlayerID"].ToString(), out playerID);
                    }
                }

                if (gameStarted)
                {
                    // If game is ongoing, send the message with this tag
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(playerID);

                        using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                        {
                            foreach (IClient client in ClientManager.GetAllClients())
                                client.SendMessage(message, SendMode.Reliable);
                        }
                    }
                }

                else
                {
                    // If we're still in lobby, send the message with another tag
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(playerID);

                        using (Message message = Message.Create(Tags.DisconnectLobbyPlayerTag, writer))
                        {
                            foreach (IClient client in ClientManager.GetAllClients())
                                client.SendMessage(message, SendMode.Reliable);
                        }
                    }
                }
            }

            // Look for player with the disconnected client's ID and erase them from their DB
            string deletePlayerQuery = "DELETE FROM Players WHERE ClientID = @ClientID";
            SQLiteCommand deletePlayerCommand = new SQLiteCommand(deletePlayerQuery, DB.myConnection);
            deletePlayerCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
            int deletePlayerResult = deletePlayerCommand.ExecuteNonQuery();

            // Get the ID of the color that is about to be disconnected and remove it from takenColors
            string colorIDQuery = "SELECT PlayerID FROM Players WHERE ClientID = (@ClientID)";
            SQLiteCommand colorIDCommand = new SQLiteCommand(colorIDQuery, DB.myConnection);
            colorIDCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
            SQLiteDataReader colorIDResult = colorIDCommand.ExecuteReader();

            ushort colorID = 0;

            if (colorIDResult.HasRows)
            {
                while (colorIDResult.Read())
                {
                    UInt16.TryParse(colorIDResult["PlayerID"].ToString(), out colorID);
                }
            }

            takenColors.Remove(colorID);

            // Release the color of the disconnected player
            string freeColorQuery = "UPDATE Colors SET ClientID = null WHERE ClientID = @ClientID";
            SQLiteCommand freeColorCommand = new SQLiteCommand(freeColorQuery, DB.myConnection);
            freeColorCommand.Parameters.AddWithValue("@ClientID", e.Client.ID);
            int freeColorWipeResult = freeColorCommand.ExecuteNonQuery();
        }
    }
}
