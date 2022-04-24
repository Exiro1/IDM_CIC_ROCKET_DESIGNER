using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocketDesigner
{
    public partial class Optimize : Form
    {
        public Optimize()
        {
            InitializeComponent();
        }


        public bool cancel = true;
        public int pop, keep, epoch;
        public double[] deviation;

        private void Optimize_Load(object sender, EventArgs e)
        {

        }

        public double ms;


        private void buttonOptimize_Click(object sender, EventArgs e)
        {
            pop = (int)numericUpDownPop.Value;
            keep = (int)numericUpDownKeep.Value;
            epoch = (int)numericUpDownEpoch.Value;
            ms = (double)numericUpDownMs.Value;

            deviation = new double[] { (double)numericSweep.Value*0.01, (double)numericTip.Value * 0.01, (double)numericTh.Value * 0.01, (double)numericChord.Value * 0.01, (double)numericPos.Value * 0.01, (double)numericSpan.Value * 0.01 };

            cancel = false;
            Close();
        }


    }
}
