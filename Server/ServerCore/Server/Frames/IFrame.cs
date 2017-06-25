using System.Net.Sockets;

namespace Server.Frames
{
    public interface IFrame
    {
        Frames FrameType { get; set; }
        TcpClient Connection { get; set; }
        string Login { get; set; }
    }
}
