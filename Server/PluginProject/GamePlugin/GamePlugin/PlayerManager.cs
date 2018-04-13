using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Server;

namespace AgarPlugin
{
    public class PlayerManager : Plugin
    {
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();

        const float MAP_WIDTH = 10;

        // Multithreading set to false
        public override bool ThreadSafe => false;

        // Version number
        public override Version Version => new Version(1, 3, 1);

        public PlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
                ClientManager.ClientConnected += ClientConnected;
                ClientManager.ClientDisconnected += ClientDisconnected;
        }

        // What happens when a new client connects
        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
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

            e.Client.MessageReceived += MovementMessageReceived;
        }

        // Reacting to player moving
        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
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
