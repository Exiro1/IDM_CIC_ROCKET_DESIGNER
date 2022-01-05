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
        public double min1, min2, max1, max2;

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
            nbr = (int)batchNumber_Numeric.Value;
            Close();
        }
    }
}
