using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarkRift.Server;
using DarkRift;

public class PlayerManager : Plugin
{

    public override bool ThreadSafe => false;
    public override Version Version => new Version(1, 0, 0);

    class Player
    {
        private float v;

        public ushort ID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public Player(ushort ID, float x, float y)
        {
            this.ID = ID;
            this.X = x;
            this.Y = y;
        }

        public Player(ushort ID, float x, float y, float v) : this(ID, x, y)
        {
            this.v = v;
        }
    }

    void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += MovementMessageReceived;
        const float MAP_WIDTH = 20;
        Random r = new Random();
        Player newPlayer = new Player(
        
        e.Client.ID,
            (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            (float)r.NextDouble() * MAP_WIDTH - MAP_WIDTH / 2,
            1f
        );
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
        {
            newPlayerWriter.Write(newPlayer.ID);
            newPlayerWriter.Write(newPlayer.X);
            newPlayerWriter.Write(newPlayer.Y);

            using (Message newPlayerMessage = Tags.SpawnPlayerTag(0, newPlayerWriter))
            {
                foreach (IClient client in ClientManager.GetAllClients().Where(x => x != e.Client))
                    client.SendMessage(newPlayerMessage, SendMode.Reliable);
            }
        }
        
    }

    public PlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
    {
        ClientManager.ClientConnected += ClientConnected;
        Dictionary<IClient, Player> players = new Dictionary<IClient, Player>();
    }

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

                    Player player = players[e.Client];

                    player.X = newX;
                    player.Y = newY;

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        writer.Write(player.ID);
                        writer.Write(player.X);
                        writer.Write(player.Y);
                        message.Serialize(writer);
                    }

                    foreach (IClient c in ClientManager.GetAllClients().Where(x => x != e.Client))
                        c.SendMessage(message, e.SendMode);
                }
            }
        }
    }
}
