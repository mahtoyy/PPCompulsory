using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompulsoryPPGui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        SemaphoreSlim s = new SemaphoreSlim(1, 1);
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ulong min = ulong.Parse(textBox1.Text.Trim());
            ulong max = ulong.Parse(textBox2.Text.Trim());

            List<ulong> list = new List<ulong>();

            await Task.Run(() => GenerateParallelSemaphore(list, min, max));

            //Task.WaitAll(task);
            //list.Sort();

            listBox1.DataSource = list;

            //foreach (ulong value in list) { listBox1.Items.Add(value); };


        }


        public bool IsPrime(ulong number)
        {
            if (number == 1) return false;
            if (number % 2 == 0 && number >2) return false;

            var boundary = (ulong)Math.Floor(Math.Sqrt(number));

            for (ulong i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        private void GenerateParallelSemaphore(List<ulong> list, ulong min, ulong max)
        {
            Parallel.ForEach(Partitioner.Create((int)min, (int)max), (range, loopState) =>
            {
                //Random rnd = new Random(Interlocked.Increment(ref seed));
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (IsPrime((ulong)i))
                    {
                        s.Wait();
                        list.Add((ulong)i);
                        s.Release();
                    }
                }
            });
            list.Sort();
        }
    }
}
