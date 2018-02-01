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
    public partial class RxTest : Form
    {
        public RxTest()
        {
            InitializeComponent();


            var drag = from down in this.button1.MouseDownAsObservable()
                       from move in this.button1.MouseMoveAsObservable()
                       .StartWith(down).Skip(1).TakeUntil(this.button1.MouseUpAsObservable())
                       select new { down, move };

            drag.Subscribe(x =>
            {
                this.button1.Left += x.move.X;
                this.button1.Top += x.move.Y;
                Console.WriteLine($"{x.move.X},{x.move.Y}");
            });

            Observable.FromEventPattern(this.button1, "Click").Select(x => x.EventArgs)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(x => 
                {
                    MessageBox.Show("1");
                });
        }
    }

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
}
