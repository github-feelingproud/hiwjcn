using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormApp.AuthWebService;
using Lib.ioc;
using Lib.extension;
using Lib.data;
using Hiwjcn.Core.Domain.Auth;

namespace WindowsFormApp
{
    public partial class AuthServiceForm : Form
    {
        public AuthServiceForm()
        {
            InitializeComponent();
        }

        class AuthApiClient : AuthApiServiceClient, IDisposable
        {
            public void Dispose()
            {
                try
                {
                    this.Close();
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var scope = AppContext.Scope())
                {
                    var repo = scope.Resolve_<IRepository<AuthClient>>();
                    var client = (await repo.GetListAsync(null, 1)).FirstOrDefault();
                    if (client == null)
                    {
                        MessageBox.Show("client is null");
                        return;
                    }
                    using (var api = new AuthApiClient())
                    {
                        var code = await api.GetAuthCodeByPasswordAsync(client.UID, new List<string>().ToJson(), "", "");
                        if (!code.success)
                        {
                            MessageBox.Show(code.msg);
                            return;
                        }
                        var token = await api.GetAccessTokenAsync(client.UID, client.ClientSecretUID, code.data, string.Empty);
                        if (!token.success)
                        {
                            MessageBox.Show(token.msg);
                            return;
                        }
                        MessageBox.Show(token.data.Token);
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
    }
}
