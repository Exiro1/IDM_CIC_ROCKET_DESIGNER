using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Nozzle : RocketElement
	{
		public Nozzle(string engFile, double radius, double rMix, double ISP)
		{
			this.As = radius*radius*Math.PI;
			path = engFile;
			this.rMix = rMix;
			this.ISP = ISP;
		}
		string path;
		public double As;
		string motorName,manufacturer;
		double diameter, len, propW, totW;
		public double rMix;
		public double ISP;
		double[] delays;
		public double[,] values;

		public double getAs()
		{
			return As;
		}

		public void unpack()
		{
			string[] lines = File.ReadAllLines(path);
			bool firstLine = true;
			int k = -1;
			foreach(string l in lines)
			{
				k++;
				if (l.StartsWith(";"))
					continue;
				if (firstLine)
				{
					string[] param = l.Split(' ');
					motorName = param[0];
					diameter = safeParse(param[1]);
					len = safeParse(param[2]);
					delays = new double[param[3].Split('-').Length];
					int i = 0;
					foreach (string del in param[3].Split('-'))
					{
						delays[i] = safeParse(del);
						i++;
					}
					propW = safeParse(param[4]);
					totW = safeParse(param[5]);
					manufacturer = param[6];
					firstLine = false;
					values = new double[lines.Length - k-1, 2];
					k = -1;
				}
				else
				{
					values[k, 0] = safeParse(l.Split(' ')[0]);
					values[k, 1] = safeParse(l.Split(' ')[1]);
				}
			}

		}

		public double safeParse(string d)
		{
			double res;
			if(!Double.TryParse(d, out res))
			{
				if (!Double.TryParse(d.Replace('.',','), out res))
				{
					return 0;
				}
			}
			return res;
		}

		public override void WriteToOpenRocket(XmlTextWriter writter)
		{
			return;
		}

		public override void WriteToXML(XmlTextWriter writter)
		{
			return;
		}


	}
}
