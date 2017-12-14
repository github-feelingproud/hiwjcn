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
using Lib.helper;

namespace WindowsFormApp
{
    public partial class RegexTest : Form
    {
        public RegexTest()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(RegexHelper.IsMatch(textBox2.Text, textBox1.Text).ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetInnerExceptionAsJson());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var matchs = RegexHelper.FindMatchs(textBox2.Text, textBox1.Text);
                var json = matchs.Select(x => x.Value).ToJson();
                MessageBox.Show(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetInnerExceptionAsJson());
            }
        }
    }
}
