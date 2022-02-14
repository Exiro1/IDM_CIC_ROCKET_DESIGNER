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
    public partial class progressBarCustom : Form
    {
        public progressBarCustom()
        {
            InitializeComponent();
        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }

        public void SetProgressBarValue(int pbValue)
        {
            progressBar.Value = Math.Min(100,pbValue);
        }

    }
}
