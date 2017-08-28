using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace WindowsFormApp
{
    public partial class NettyServer : Form
    {
        public NettyServer()
        {
            InitializeComponent();
        }

        private async void NettyServer_Load(object sender, EventArgs e)
        {
            var boot = new Bootstrap();
            boot.Group(null)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoKeepalive, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var line = channel.Pipeline;
                    line.AddLast();
                }));
            var cn = await boot.ConnectAsync();
            //
        }
    }
}
