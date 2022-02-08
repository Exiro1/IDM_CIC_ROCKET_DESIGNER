using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RocketDesigner.Datagen;

namespace RocketDesigner
{
    public partial class BatchGenerator : Form
    {
        public BatchGenerator()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        public ParametersEnum.Parameters p1;
        public ParametersEnum.Parameters p2;
        public double min1, min2, max1, max2, machnbr;
        public bool cancel = true;
        public bool ca, cp, cna, alt, mach, ms, qinf;
        public int distrib = 1; //0 = normal; 1 = uniform;

        private void comboBoxDistrib_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((comboBoxDistrib.SelectedIndex == 0))
            {
                min1_label.Text = "deviation";
                min2_label.Text = "deviation";
                max1_label.Text = "mean";
                max2_label.Text = "mean";
            }
            else if ((comboBoxDistrib.SelectedIndex == 1))
            {
                min1_label.Text = "minimum";
                min2_label.Text = "minimum";
                max1_label.Text = "maximum";
                max2_label.Text = "maximum";
            }
        }

        private void BatchGenerator_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void BatchGenerator_Load(object sender, EventArgs e)
        {
            param1_comboBox.Items.Add(ParametersEnum.Parameters.CHORD);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.TIPCHORD);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.LEANGLE);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.TEANGLE);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.POSITION);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.SPAN);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.THICKNESS);
            param1_comboBox.Items.Add(ParametersEnum.Parameters.SWEEP);

            param2_comboBox.Items.Add(ParametersEnum.Parameters.CHORD);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.TIPCHORD);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.LEANGLE);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.TEANGLE);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.POSITION);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.SPAN);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.THICKNESS);
            param2_comboBox.Items.Add(ParametersEnum.Parameters.SWEEP);

            comboBoxDistrib.SelectedIndex = 1;

        }

        public int nbr;

       
        private void generate_button_Click(object sender, EventArgs e)
        {
            p1 = (ParametersEnum.Parameters)param1_comboBox.SelectedItem;
            p2 = (ParametersEnum.Parameters)param2_comboBox.SelectedItem;
            
            min1 = (double)min1_Numeric.Value;
            min2 = (double)min2_Numeric.Value;
            max1 = (double)max1_Numeric.Value;
            max2 = (double)max2_Numeric.Value;

            if (p1 == ParametersEnum.Parameters.LEANGLE || p1 == ParametersEnum.Parameters.TEANGLE)
            {
                min1 *= Math.PI / 180;
                max1 *= Math.PI / 180;
            }

            if (p2 == ParametersEnum.Parameters.LEANGLE || p2 == ParametersEnum.Parameters.TEANGLE)
            {
                min2 *= Math.PI / 180;
                max2 *= Math.PI / 180;
            }

            ca = checkBoxCa.Checked;
            cp = checkBoxCP.Checked;
            cna = checkBoxCNa.Checked;
            alt = checkBoxAlt.Checked;
            qinf = checkBoxQinf.Checked;
            mach = checkBoxMach.Checked;
            ms = checkBoxMs.Checked;
            machnbr = (double)numericMach.Value;
            distrib = comboBoxDistrib.SelectedIndex;
            nbr = (int)batchNumber_Numeric.Value;
            cancel = false;
            Close();
        }

        internal bool[] getShow()
        {
            return new bool[] {ca,cna,cp,alt,ms,qinf,mach};
        }
    }
}
