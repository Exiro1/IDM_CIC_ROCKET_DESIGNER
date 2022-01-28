using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocketDesigner
{
    internal class Matlab
    {

		MLApp.MLApp matlab;

		bool available = false;
		bool installed = false;

		public Matlab()
        {
			using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (var key = hklm.OpenSubKey(@"Software\MathWorks"))
			{

				if (key == null)
				{
					// Doesn't exist...
					MessageBox.Show("Matlab n'est pas installé, certaines fonctionnalitées de Rocket Designer ne seront pas disponible");
				}
				else
				{
					installed = true;
					
				}
			}
		}

		public bool loadMatlab()
        {
			if (available)
			{
				try
				{
					int error = matlab.Visible;
				}
				catch (Exception ex)
				{
					available = false;
				}
			}

			if (available)
				return true;
			if(!isInstalled())
				return false;
			var activationContext = Type.GetTypeFromProgID("matlab.application.single");
			matlab = (MLApp.MLApp)Activator.CreateInstance(activationContext);
			if (matlab == null)
				return false;
			matlab.Visible = 0;
			matlab.Execute(@"cd " + Main.folderPath + "\\matlab");
			available = true;
			return true;
		}

		public bool isAvailable()
        {
			return available;
        }
		public bool isInstalled()
		{
			return installed;
		}

		public void displayGraphs(ParametersEnum.Parameters[] param, double[,] globalData)
        {
			if (!loadMatlab())
				return;

			object result = null;
			matlab.PutWorkspaceData("data", "base", globalData);
			matlab.Execute("figure");
			matlab.Execute("scatter3(data(:, 1), data(:, 2), data(:, 3)); title(\"Ca = fct(" + param[0].ToString() + "," + param[1].ToString() + ")\");xlabel(\"" + param[0].ToString() + "\"); ylabel(\"" + param[1].ToString() + "\"); zlabel(\"Ca\")");
			matlab.Execute("figure");
			matlab.Execute("scatter3(data(:, 1), data(:, 2), data(:, 4)); title(\"Cnalpha = fct(" + param[0].ToString() + "," + param[1].ToString() + ")\");xlabel(\"" + param[0].ToString() + "\"); ylabel(\"" + param[1].ToString() + "\"); zlabel(\"CNalpha\")");
			matlab.Execute("figure");
			matlab.Execute("scatter3(data(:, 1), data(:, 2), data(:, 5)); title(\"CP = fct(" + param[0].ToString() + "," + param[1].ToString() + ") (distance from nose)  \");xlabel(\"" + param[0].ToString() + "\"); ylabel(\"" + param[1].ToString() + "\"); zlabel(\"CP\")");
			matlab.Execute("figure");
			matlab.Execute("scatter3(data(:, 1), data(:, 2), data(:, 6)); title(\"Altitude max = fct(" + param[0].ToString() + "," + param[1].ToString() + ") (3DoF simu)  \");xlabel(\"" + param[0].ToString() + "\"); ylabel(\"" + param[1].ToString() + "\"); zlabel(\"meters\")");
			matlab.Execute("save('data')");
		}


		public MLApp.MLApp getMatlabInstance()
        {
			return matlab;
        }

		public void close()
        {
			if(available)
				matlab.Quit();
        }


    }
}
