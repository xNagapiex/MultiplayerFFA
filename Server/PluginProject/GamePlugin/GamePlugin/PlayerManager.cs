using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;

namespace GamePlugin
{


    public class PlayerManager : Plugin
    {
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();
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
            SpawnGatherSpots(e.Client); // <---------- RELOCATE THIS TO WHEREVER THE GAME ACTUALLY BEGINS

            Random r = new Random();
            Player newPlayer = new Player(
                e.Client.ID,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
                (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,

                (float)0,
                (float)0,

                (byte)r.Next(55, 255),
                (byte)r.Next(55, 255),
                (byte)r.Next(55, 255)
            );

            // Announce new player and their relevant stats to other clients (id, position and color)
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {

                newPlayerWriter.Write(newPlayer.ID);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Y);

                newPlayerWriter.Write(newPlayer.ColorR);
                newPlayerWriter.Write(newPlayer.ColorG);
                newPlayerWriter.Write(newPlayer.ColorB);

                using (Message newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }

            // Add new player to the dictionary so that we can keep track of them
            players.Add(e.Client, newPlayer);

            // Send the new player their own info (id, position, color)
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in players.Values)
                {
                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);

                    playerWriter.Write(player.ColorR);
                    playerWriter.Write(player.ColorG);
                    playerWriter.Write(player.ColorB);
                }

                using (Message playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
            }

            // If a message is received from a connected client, send it to CheckMessageTag to be handled
            e.Client.MessageReceived += CheckMessageTag;
        }

        void CheckMessageTag(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    MovementMessageReceived(sender, e);
                }

                // Work in Progress, has to do with item gathering
                //else if (message.Tag == Tags.GatherItemTag)
                //{
                //    GatherMessageReceived(sender, e);
                //}
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

        // Writing gather spots received from host to DB
        void SpawnGatherSpots(IClient client)
        {
            if (!gatherSpotsSpawned)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    DB.OpenConnection();
                    string query = "DELETE FROM GatherSpots";
                    SQLiteCommand wipeCommand = new SQLiteCommand(query, DB.myConnection);
                    int result = wipeCommand.ExecuteNonQuery();
                    DB.CloseConnection();
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

                    for (int i = 0; i < 100; i++)
                    {
                        int randomItem = r.Next(3);
                        Console.WriteLine("RandomItem: " + randomItem);

                        if(trees == 0 & herbs == 0 || randomItem == 2 & 100 - i > herbs + trees)
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

                            else if(herbs > trees)
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
                            DB.OpenConnection();
                            string writequery = "INSERT INTO GatherSpots VALUES (@ID, @IsAvailable, @ItemID, @PosX, @PosY)";
                            SQLiteCommand writeCommand = new SQLiteCommand(writequery, DB.myConnection);
                            writeCommand.Parameters.AddWithValue("@ID", i);
                            writeCommand.Parameters.AddWithValue("@IsAvailable", 1);
                            writeCommand.Parameters.AddWithValue("@ItemID", itemID);
                            writeCommand.Parameters.AddWithValue("@PosX", x);
                            writeCommand.Parameters.AddWithValue("@PosY", y);
                            int writeresult = writeCommand.ExecuteNonQuery();
                            DB.CloseConnection();

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

            else
            {
                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {

                            string query = "SELECT ID, IsAvailable, ItemID, PosX, PosY FROM GatherSpots";
                            SQLiteCommand myCommand = new SQLiteCommand(query, DB.myConnection);
                            DB.OpenConnection();
                            SQLiteDataReader result = myCommand.ExecuteReader();

                            if (result.HasRows)
                            {
                                while (result.Read())
                                {
                                    ushort ID = (ushort)result["ID"];
                                    ushort ItemID = (ushort)result["ItemID"];
                                    ushort PosX = (ushort)result["PosX"];
                                    ushort PosY = (ushort)result["PosY"];

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

                            DB.CloseConnection();
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
                gatherSpotsSpawned = false;
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
