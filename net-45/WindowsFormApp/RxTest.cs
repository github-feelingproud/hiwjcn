using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reactive;
using System.Reactive.Linq;

namespace WindowsFormApp
{
    public static class FormExtensions
    {
        public static IObservable<MouseEventArgs> MouseMoveAsObservable(this Control form)
        {
            return Observable.FromEventPattern<MouseEventArgs>(form, "MouseMove").Select(e => e.EventArgs);
        }

        public static IObservable<MouseEventArgs> MouseDownAsObservable(this Control form)
        {
            return Observable.FromEventPattern<MouseEventArgs>(form, "MouseDown").Select(e => e.EventArgs);
        }

        public static IObservable<MouseEventArgs> MouseUpAsObservable(this Control form)
        {
            return Observable.FromEventPattern<MouseEventArgs>(form, "MouseUp").Select(e => e.EventArgs);
        }
    }
    public partial class RxTest : Form
    {
        public RxTest()
        {
            InitializeComponent();

            this.button1.MouseDownAsObservable();
            this.button1.MouseUpAsObservable();
            this.button1.MouseMoveAsObservable();

            var drag = from down in button1.MouseDownAsObservable()
                       from move in button1.MouseMoveAsObservable()
                       .StartWith(down).TakeUntil(button1.MouseUpAsObservable())
                       select new { down, move };

            drag.Subscribe(x =>
            {
                this.button1.Left += x.move.X;
                this.button1.Top += x.move.Y;
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
