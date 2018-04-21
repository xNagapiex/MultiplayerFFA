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
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();
        List<SQLiteCommand> OpenCommands = new List<SQLiteCommand>();
        Database DB;
        bool gatherSpotsSpawned;

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

            string playersWipeQuery = "DELETE FROM Players";
            SQLiteCommand playersWipeCommand = new SQLiteCommand(playersWipeQuery, DB.myConnection);
            int playersWipeResult = playersWipeCommand.ExecuteNonQuery();
            string inventoryWipeQuery = "DELETE FROM InventorySlots";
            SQLiteCommand inventoryWipeCommand = new SQLiteCommand(inventoryWipeQuery, DB.myConnection);
            int inventoryWipeResult = inventoryWipeCommand.ExecuteNonQuery();

            gatherSpotsSpawned = false;
            ////INSERT INTO DATABASE EXAMPLE

            //string query = "INSERT INTO Recipe ('Name', 'IsPriority') VALUES (@Name, @IsPriority)";
            //SQLiteCommand myCommand = new SQLiteCommand(query, DB.myConnection);
            //DB.OpenConnection();
            //myCommand.Parameters.AddWithValue("@Name", "Tawny Antlers");
            //myCommand.Parameters.AddWithValue("@IsPriority", 0);
            //int result = myCommand.ExecuteNonQuery();
            //DB.CloseConnection();

            //Console.WriteLine("Rows Added: {0}", result);



            ////SELECT FROM DATABASE EXAMPLE

            //string query = "SELECT Recipe.Name as Recipe, Material.Name as Material, " +
            //    "Quantity, IsPriority FROM Recipe inner join Material on Recipe.Name = RecipeName order by Recipe.Name";
            //SQLiteCommand myCommand = new SQLiteCommand(query, databaseObject.myConnection);
            //databaseObject.OpenConnection();
            //SQLiteDataReader result = myCommand.ExecuteReader();

            //if (result.HasRows)
            //{
            //    while (result.Read())
            //    {
            //        Console.WriteLine("{0}\n{1}\n{2}\nIsPriority: {3}\n\n", result["Material"], result["Recipe"], result["Quantity"], result["IsPriority"]);
            //    }
            //}

            //databaseObject.CloseConnection();
        }

        // What happens when a new client connects
        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            //SpawnGatherSpots(e.Client); // <---------- RELOCATE THIS TO WHEREVER THE GAME ACTUALLY BEGINS

            Random r = new Random();
            Player newPlayer = new Player(
                e.Client.ID,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,

                0,
                0,

                (byte)r.Next(55, 255),
                (byte)r.Next(55, 255),
                (byte)r.Next(55, 255)
            );

            // Write new player's info to DB
            string newPlayerQuery = "INSERT INTO Players (ID, Health, PosX, PosY, MousePosX, MousePosY) values (@ID, @Health, @PosX, @PosY, @MousePosX, @MousePosY)";
            SQLiteCommand newPlayerCommand = new SQLiteCommand(newPlayerQuery, DB.myConnection);
            newPlayerCommand.Parameters.AddWithValue("@ID", e.Client.ID);
            newPlayerCommand.Parameters.AddWithValue("@Health", 100);
            newPlayerCommand.Parameters.AddWithValue("@PosX", newPlayer.X);
            newPlayerCommand.Parameters.AddWithValue("@PosY", newPlayer.Y);
            newPlayerCommand.Parameters.AddWithValue("@MousePosX", newPlayer.MX);
            newPlayerCommand.Parameters.AddWithValue("@MousePosY", newPlayer.MY);
            int newPlayerResult = newPlayerCommand.ExecuteNonQuery();

            // Announce new player and their relevant stats to other clients (id, position and color)
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.ID);

                newPlayerWriter.Write(newPlayer.ColorR);
                newPlayerWriter.Write(newPlayer.ColorG);
                newPlayerWriter.Write(newPlayer.ColorB);

                // Use this for stuff with Lobby
                using (Message newPlayerMessage = Message.Create(Tags.PlayerJoinedTag, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }

                //// Lobbyless alternative
                //using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                //{
                //    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                //        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                //}
            }

            // Add new player to the dictionary so that we can keep track of them
            players.Add(e.Client, newPlayer);

            Console.WriteLine("New playerCount: " + players.Count());

            // Send the new player their own info (id, position, color)
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);

                    playerWriter.Write(player.ColorR);
                    playerWriter.Write(player.ColorG);
                    playerWriter.Write(player.ColorB);
                }

                // Use this for stuff with Lobby
                using (Message playerMessage = Message.Create(Tags.PlayerJoinedTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);

                //// Lobbyless alternative
                //using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                //    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }

            // If a message is received from a connected client, send it to CheckMessageTag to be handled
            e.Client.MessageReceived += CheckMessageTag;
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

                        Player player = players[e.Client];

                        player.X = newX;
                        player.Y = newY;
                        player.MX = newMX;
                        player.MY = newMY;

                        // Send player's inputs to other clients
                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(player.ID);
                            writer.Write(player.X);
                            writer.Write(player.Y);
                            writer.Write(player.MX);
                            writer.Write(player.MY);
                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, SendMode.Reliable);
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

                        if (itemIDResult.HasRows)
                        {
                            while (isAvailableResult.Read())
                            {
                                Console.WriteLine("ItemID found");
                                Int32.TryParse(itemIDResult["ItemID"].ToString(), out itemID);
                            }

                        }

                        else
                        {
                            Console.WriteLine("Error, could not find gather spot of that ID (ItemID search)");
                        }

                        // Check if player already has a stack of this item in an inventory slot
                        string stackQuery = "SELECT Amount FROM InventorySlots WHERE ItemID = (@ItemID) AND PlayerID = (@PlayerID)";
                        SQLiteCommand stackCommand = new SQLiteCommand(stackQuery, DB.myConnection);
                        stackCommand.Parameters.AddWithValue("@ItemID", itemID);
                        stackCommand.Parameters.AddWithValue("@PlayerID", e.Client.ID);
                        SQLiteDataReader stackResult = stackCommand.ExecuteReader();

                        bool stackResultHasRows = false;
                        int currentAmount = 0;

                        // Player has an existing stack of the item so just add one to the current amount
                        if (stackResult.HasRows)
                        {
                            stackResultHasRows = true;

                            while (isAvailableResult.Read())
                            {
                                Int32.TryParse(stackResult["Amount"].ToString(), out currentAmount);
                                ++currentAmount;
                            }

                        }

                        if (stackResultHasRows)
                        {
                            Console.WriteLine("Player has an existing stack");
                            string newAmountQuery = "UPDATE InventorySlots SET Amount = @NewAmount WHERE ItemID = (@ItemID) AND PlayerID = (@PlayerID)";
                            SQLiteCommand newAmountCommand = new SQLiteCommand(newAmountQuery, DB.myConnection);
                            newAmountCommand.Parameters.AddWithValue("@ItemID", itemID);
                            newAmountCommand.Parameters.AddWithValue("@PlayerID", e.Client.ID);
                            newAmountCommand.Parameters.AddWithValue("@NewAmount", currentAmount);
                            int newAmountResult = newAmountCommand.ExecuteNonQuery();
                        }

                        // Player doesn't have an existing stack so create one
                        else
                        {
                            Console.WriteLine("Player does not have an existing stack");
                            string createStackQuery = "INSERT INTO InventorySlots (PlayerID, ItemID, Amount) values ((@PlayerID), (@ItemID), 1)";
                            SQLiteCommand createStackCommand = new SQLiteCommand(createStackQuery, DB.myConnection);
                            createStackCommand.Parameters.AddWithValue("@PlayerID", e.Client.ID);
                            createStackCommand.Parameters.AddWithValue("@ItemID", itemID);
                            int createStackResult = createStackCommand.ExecuteNonQuery();
                        }


                        // Update player's client with their new inventory


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
                    // Deleting old data in case it hasn't already been deleted
                    string query = "DELETE FROM GatherSpots";
                    SQLiteCommand wipeCommand = new SQLiteCommand(query, DB.myConnection);
                    int result = wipeCommand.ExecuteNonQuery();
                    Console.WriteLine("Old gather spots wiped from DB");

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
                                itemID = 0;
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
            players.Remove(e.Client);

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (Message message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                        client.SendMessage(message, SendMode.Reliable);
                }
            }

            if(players.Count == 0)
            {
                //// Deleting old data when all players have left
                //string query = "DELETE FROM GatherSpots";
                //SQLiteCommand wipeCommand = new SQLiteCommand(query, DB.myConnection);
                //int result = wipeCommand.ExecuteNonQuery();
                //string playersWipeQuery = "DELETE FROM Players";
                //SQLiteCommand playersWipeCommand = new SQLiteCommand(playersWipeQuery, DB.myConnection);
                //int playersWipeResult = playersWipeCommand.ExecuteNonQuery();
                //string inventoryWipeQuery = "DELETE FROM InventorySlots";
                //SQLiteCommand inventoryWipeCommand = new SQLiteCommand(inventoryWipeQuery, DB.myConnection);
                //int inventoryWipeResult = inventoryWipeCommand.ExecuteNonQuery();
                //gatherSpotsSpawned = false;
            }
        }
    }

    class Player
    {
        public ushort ID { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        // Mouse position
        public float MX { get; set; }
        public float MY { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }

        public Player(ushort ID, float x, float y, float mx, float my, byte colorR, byte colorG, byte colorB)
        {
            this.ID = ID;

            this.X = x;
            this.Y = y;

            this.MX = mx;
            this.MY = my;

            this.ColorR = colorR;
            this.ColorG = colorG;
            this.ColorB = colorB;
        }
    }

}
