using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdmCic.API;

namespace RocketDesigner
{
    partial class Datagen
	{
		
		Random ran;
		Matlab matlab;
		Aerodynamics aero;
		public Datagen(Matlab matlab, Aerodynamics aero)
		{
			ran = new Random();
			this.aero = aero;
			this.matlab = matlab;
		}

		public void generatePatch(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int count, IdmCic.API.Model.Mainsystem.Element e, double mach, bool[] show)
		{


			double[,] globalData = new double[count, show.Count(c => true) + param.Length];
			for(int i = 0; i < count; i++)
			{
				double[] par = randomizeRocket(start, param, limits);
				double[] datas = getData(start, mach);
				double[] sim = getSimData(start, e);

				xlApptemp.Workbooks[xlApptemp.Workbooks.Count].Close(false);
				int j = 0;
				int k = 0;
				foreach (double d in par)
				{
					globalData[i, j] = d;
					j++;
				}
				foreach (double d in datas)
				{
					if (show[k])
					{
						globalData[i, j] = d;
						j++;
					}
					k++;
				}
				foreach (double d in sim)
				{
					if (k < show.Length && show[k]) {
						globalData[i, j] = d;
						j++;
					}
					k++;
				}
			}
			matlab.displayGraphs(param, globalData, show);
		}

        private double[] getSimData(Rocket start, IdmCic.API.Model.Mainsystem.Element e)
        {
			return aero.startSimu3DDL(e, start, matlab, false, false);
		}


		Microsoft.Office.Interop.Excel.Application xlApptemp;


		public double[] getData(Rocket r, double mach)
		{
			string filec = r.generateXMLFile("datagen");
			//r.generateOpenRocketFile(folderPath + "test.ork");
			System.IO.File.Copy(filec, Main.folderPath + "ras.CDX1", true);
			

			string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string exefile = Path.Combine(assemblyFolder, "RAS.exe");
			Process p = new Process();
			p.StartInfo = new ProcessStartInfo(exefile);
			p.StartInfo.WorkingDirectory = assemblyFolder;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			p.Start();
			p.WaitForExit();

			var fileName = Main.folderPath + "ras2.txt";
			if (System.IO.File.Exists(fileName))
				System.IO.File.Delete(fileName);

			System.IO.File.Move(fileName.Replace("txt", "CSV"), fileName);
			xlApptemp = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
			Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
			Microsoft.Office.Interop.Excel.Range range;

			xlApptemp.Workbooks.OpenText(fileName, DataType: Microsoft.Office.Interop.Excel.XlTextParsingType.xlDelimited, Semicolon: true, Comma: false);
			Worksheet worksheet = (Worksheet)xlApptemp.Workbooks[xlApptemp.Workbooks.Count].Worksheets["ras2"];

			Microsoft.Office.Interop.Excel.Range data = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[2+ ((int)(1+mach*100)), 7];
			double CaPon = Double.Parse((string)data.FormulaR1C1Local);
			data = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[2 + ((int)(1 + mach * 100)), 12];
			double CnAlpha = Double.Parse((string)data.FormulaR1C1Local);
			data = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[2 + ((int)(1 + mach * 100)), 13];
			double CP = Double.Parse((string)data.FormulaR1C1Local);


			return new double[] { CaPon, CnAlpha, toMeter(CP) };
		}

		public double toMeter(double inch)
		{
			return Math.Round(inch / 39.3701, 3);
		}


		/**
		 * param = liste de parametre
		 * limits : tableau double des minimum et maximum pour chaque parametre
		 *			min0 max0
		 *			min1 max1
		 *			...
		 */
		public Double[] randomizeRocket(Rocket start, ParametersEnum.Parameters[] param, double[,] limits)
		{
			Fin fin0 = getFin(start);
			int i = 0;
			double[] pa = new double[param.Length];
			foreach(ParametersEnum.Parameters p in param)
			{
				pa[i] = changeParameterRan(fin0, p, limits[i, 0], limits[i, 1]);
				i++;
			}
			return pa;
		}


		public double changeParameterRan(Fin f, ParametersEnum.Parameters pm, double min, double max)
		{
			double ran = randomizer(min, max);
			switch (pm)
			{
				case ParametersEnum.Parameters.TIPCHORD:
					f.TipChord = ran;
					break;
				case ParametersEnum.Parameters.SWEEP:
					f.sweepDist = ran;
					break;
				case ParametersEnum.Parameters.THICKNESS:
					f.thickness = ran;
					break;
				case ParametersEnum.Parameters.CHORD:
					f.chord = ran;
					break;
				case ParametersEnum.Parameters.POSITION:
					break;
				case ParametersEnum.Parameters.SPAN:
					f.span = ran;
					break;
				case ParametersEnum.Parameters.LEANGLE:
					f.sweepDist = f.span/Math.Tan(ran);
					break;
				case ParametersEnum.Parameters.TEANGLE:
					f.TipChord = f.chord-f.sweepDist-f.span/Math.Tan(ran);
					break;
			}
			return ran;
		}
		
		public double randomizer(double min, double max)
		{
			return ran.NextDouble() * (max-min) + min;
		}

		private Fin getFin(Rocket r)
		{
			RocketElement el = r.getNosecone();
			while (el.Bot != null)
			{
				if(el.SideAttach.Count > 0)
				{
					return (Fin) el.SideAttach.First();
				}
				el = el.Bot;
			}
			return null;
		}

	}
}