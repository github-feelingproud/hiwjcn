using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib.extension;
using Lib.mq;
using RabbitMQ.Client.Events;
using Lib.helper;

namespace WindowsFormApp
{
    public partial class mq : Form
    {
        public mq()
        {
            InitializeComponent();
        }

        private ConnectionFactory _factory { get; set; }
        private Worker _worker { get; set; }

        private void mq_Load(object sender, EventArgs e)
        {
            try
            {
                this._factory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = true,
                    UseBackgroundThreadsForIO = true,
                    HostName = "mq.qipeilong.net",
                    UserName = "admin",
                    Password = "mypass",
                    VirtualHost = "/",
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(1)
                };

                this._worker = new Worker(this._factory, new SettingConfig()
                {
                    ExchangeName = "wj-test-exchange",
                    ExchangeType = ExchangeTypeEnum.direct,
                    QueueName = "wj-test-queue",
                    Ack = true,
                    ConsumerName = "worker",
                    RouteKey = "wj.#",
                    Args = new Dictionary<string, object>() { }
                });
            }
            catch (Exception err)
            {
                err.AddErrorLog();
                MessageBox.Show(err.Message);
                this.Close();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                this._worker?.Dispose();
            }
            catch (Exception err)
            {
                err.AddErrorLog();
            }
            base.OnClosing(e);
        }

    }

    public class Worker : RabbitMessageConsumerBase<string>
    {
        public Worker(ConnectionFactory factory, SettingConfig config) : base(factory, config)
        {
        }

        public override Task<bool?> OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
