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

        const float MAP_WIDTH = 20;

        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public PlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            if (ClientManager.Count < 2)
            {
                ClientManager.ClientConnected += ClientConnected;
            }
        }

        void ClientConnected(object sender, ClientConnectedEventArgs e)
        {

            Random r = new Random();
            Player newPlayer = new Player(
                e.Client.ID,
                (float)0,
                (float)0,

                (float)0,
                (float)0,
                (float)0,
                (float)0,

                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200),
                (byte)r.Next(0, 200)
            );

            // Announce new player to others
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

                // Deal with new player themselves
                players.Add(e.Client, newPlayer);
                

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
        }

        // Reacting to player moving
        void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == Tags.MovePlayerTag)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();
                        float newRX = reader.ReadSingle();
                        float newRY = reader.ReadSingle();
                        float newRZ = reader.ReadSingle();
                        float newRW = reader.ReadSingle();

                        Player player = players[e.Client];

                        player.X = newX;
                        player.Y = newY;
                        player.RX = newRX;
                        player.RY = newRY;
                        player.RZ = newRZ;
                        player.RW = newRW;

                        using (DarkRiftWriter writer = DarkRiftWriter.Create())
                        {
                            writer.Write(player.ID);
                            writer.Write(player.X);
                            writer.Write(player.Y);
                            writer.Write(player.RX);
                            writer.Write(player.RY);
                            writer.Write(player.RZ);
                            writer.Write(player.RW);
                            message.Serialize(writer);
                        }

                        foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                            c.SendMessage(message, e.SendMode);
                    }
                }
            }
        }
    }


    class Player
    {
        public ushort ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        // Details of the current rotation Quaternion
        public float RX { get; set; }
        public float RY { get; set; }
        public float RZ { get; set; }
        public float RW { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }

        public Player(ushort ID, float x, float y, float rx, float ry, float rz, float rw, byte colorR, byte colorG, byte colorB)
        {
            this.ID = ID;
            this.X = x;
            this.Y = y;

            this.RX = rx;
            this.RY = ry;
            this.RZ = rz;
            this.RW = rw;

            this.ColorR = colorR;
            this.ColorG = colorG;
            this.ColorB = colorB;
        }
    }
}
