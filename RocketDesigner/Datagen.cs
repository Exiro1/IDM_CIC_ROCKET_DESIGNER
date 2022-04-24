using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdmCic.API;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RocketDesigner
{
	

	partial class Datagen
	{
		private class User32
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct Rect
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}

			[DllImport("user32.dll")]
			public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
		}

		Random ran;
		Matlab matlab;
		Aerodynamics aero;
		public Datagen(Matlab matlab, Aerodynamics aero)
		{
			ran = new Random();
			this.aero = aero;
			this.matlab = matlab;
		}

		public void getResult(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int count, IdmCic.API.Model.Mainsystem.Element e, double mach, bool[] show, int distrib)
        {
			double[,] datas = generatePatch(start, param, limits, count, e, mach, show, distrib);
			matlab.displayGraphs(param, datas, show);
		}

		public Process showRocket(Rocket r, double alt)
        {
			string filec = r.generateXMLFile("gen_test");
			System.IO.File.Copy(filec, Main.folderPath + "alt_"+((int)alt)+"_m.CDX1", true);
			string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string exefile = Path.Combine(assemblyFolder, "RAS2.exe");
			Process p = new Process();
			p.StartInfo = new ProcessStartInfo(exefile);
			p.StartInfo.WorkingDirectory = assemblyFolder;
			p.StartInfo.Arguments = Main.folderPath + "alt_" + ((int)alt) + "_m.CDX1";
			p.Start();
			return p;
		}

		public double[,] generatePatch(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int count, IdmCic.API.Model.Mainsystem.Element e, double mach, bool[] dataToSave, int distrib, double area = 0)
		{
			progressBarCustom pbc = new progressBarCustom();
			pbc.Show();
			pbc.TopMost = true;
			double[,] globalData = new double[count, dataToSave.Count(c => true) + param.Length];
			int progress=0;
			for (int i = 0; i < count; i++)
			{
				
				double[] par = randomizeRocket(start, param, limits, distrib, area);
				
				progress++;
				pbc.SetProgressBarValue((int) ((100.0*progress)/(count*3.0)));
				double[] datas = getData(start, mach);
				progress++;
				//Process p = showRocket(start);
				pbc.SetProgressBarValue((int)((100.0 * progress) / (count * 3.0)));
				double[] sim = getSimData(start, e);
				progress++;
				pbc.SetProgressBarValue((int)((100.0 * progress) / (count * 3.0)));

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
					if (dataToSave[k])
					{
						globalData[i, j] = d;
						j++;
					}
					k++;
				}
				foreach (double d in sim)
				{
					if (k < dataToSave.Length && dataToSave[k])
					{
						globalData[i, j] = d;
						j++;
					}
					k++;
				}
				globalData[i, j] = sim[4];
				//p.Kill();
			}
			
			pbc.Close();
			return globalData;
		}

		

		//return best design after [epoch] number of generation -> return double[] {sweep,tipchord,thickness,chord,position,span};
		public double[] optimizeFin(Rocket start, IdmCic.API.Model.Mainsystem.Element e, double minMs,int generationPop, int keep,int epoch, double[] devi)
        {
			double[,] globaldata = new double[epoch*generationPop,9];
			int globalIndex = 0;
			double area;
			ParametersEnum.Parameters[] param = new ParametersEnum.Parameters[] { ParametersEnum.Parameters.SWEEP, ParametersEnum.Parameters.TIPCHORD, ParametersEnum.Parameters.THICKNESS, ParametersEnum.Parameters.CHORD, ParametersEnum.Parameters.POSITION, ParametersEnum.Parameters.SPAN };
			double[,] limits = new double[param.Length, 2];
			Fin f = getFin(start);
			double [] startFin = new double[] { f.sweepDist , f.TipChord , f.thickness , f.chord , f.Loc , f.span };
			bool[] datatosave = new bool[] { false, false, false, true, true, false, false };

			limits[0, 1] = startFin[0];
			limits[0, 0] = startFin[0] * devi[0];
			limits[1, 1] = startFin[1];
			limits[1, 0] = startFin[1] * devi[1];
			limits[2, 1] = startFin[2];
			limits[2, 0] = startFin[2] * devi[2];
			limits[3, 1] = startFin[3];
			limits[3, 0] = startFin[3] * devi[3];
			limits[4, 1] = startFin[4];
			limits[4, 0] = startFin[4] * devi[4];
			limits[5, 1] = startFin[5];
			limits[5, 0] = startFin[5] * devi[5];
			area = (startFin[5]/2)*(startFin[3]+startFin[1]); // (span/2)*(chord+tipchord)

			double[] datas0 = getData(start, 1.01);
			double[] sim0 = getSimData(start, e);
			xlApptemp.Workbooks[xlApptemp.Workbooks.Count].Close(false);

			double altinitsim = sim0[0];
			double minmssim = sim0[1];

			screenShot(start, limits, -1, altinitsim, minmssim);

			double[,] bests = new double[keep,10];
			double[,] g;
			for (int i = 0; i < epoch; i++)
			{
				g = generatePatch(start, param, limits, generationPop, e, 1, datatosave, 0);
				for(int k = 0; k < generationPop; k++)
                {
					globaldata[globalIndex,0] = g[k,0];
					globaldata[globalIndex, 1] = g[k, 1];
					globaldata[globalIndex, 2] = g[k, 2];
					globaldata[globalIndex, 3] = g[k, 3];
					globaldata[globalIndex, 4] = g[k, 4];
					globaldata[globalIndex, 5] = g[k, 5];
					globaldata[globalIndex, 6] = g[k, 6]; //alt
					globaldata[globalIndex, 7] = g[k, 7]; //ms
					globaldata[globalIndex, 8] = g[k, 8]; //err
					globalIndex++;
				}
				bests = eliminate(g, keep, minMs);
				if (bests[0, 6] == 0)
					return null;

				limits = getNewGeneration(bests, area, devi);
				//show result of this gen
				screenShot(start, limits, i, bests[0,6], bests[0,7]);
				
			}
			matlab.displayOpti(param, globaldata, datatosave);
			globaldata = globaldata.OrderByDescending(x => x[6]);
			for (int i = 0; i < globaldata.GetLength(0); i++)
			{
				if (globaldata[i, 7] > minMs && globaldata[i, 8] == 0) //no error + Ms > minMs 
				{
					return new double[] { globaldata[i, 0], globaldata[i, 1], globaldata[i, 2], globaldata[i, 3], globaldata[i, 4], globaldata[i, 5], globaldata[i, 6], globaldata[i, 7], altinitsim, minmssim };
				}
			}
			return new double[] { globaldata[0, 0], globaldata[0, 1], globaldata[0, 2], globaldata[0, 3], globaldata[0, 4], globaldata[0, 5], globaldata[0, 6], globaldata[0, 7], altinitsim ,minmssim};
		}

		public void screenShot(Rocket start, double[,] limits, int index, double alt, double ms)
        {
			start.setFin(changeParam(getFin(start), new double[] { limits[0, 1], limits[1, 1], limits[2, 1], limits[3, 1], limits[4, 1], limits[5, 1] }), 0);
			Process p = showRocket(start, alt);

			System.Threading.Thread.Sleep(4000);

			var rect = new User32.Rect();
			User32.GetWindowRect(p.MainWindowHandle, ref rect);

			int width = rect.right - rect.left;
			int height = rect.bottom - rect.top;

			var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			using (Graphics graphics = Graphics.FromImage(bmp))
			{
				graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
			}
			bmp.Save(Main.folderPath + @"\img\gen_"+(index+1)+"_alt_"+((int) alt) +"_ms_"+ms.ToString("F", System.Globalization.CultureInfo.InvariantCulture) + ".png", ImageFormat.Png);

			p.Kill();
			p.WaitForExit();
			File.Delete(Main.folderPath + "alt_" + ((int)alt) + "_m.CDX1");

		}


		private double[,] getNewGeneration(double[,] bests, double area, double[] devi)
        {
			double[,] limits = new double[bests.GetLength(1)-2, 2];
			double[] startFin = new double[bests.GetLength(1) - 2];
			double d = 0;
			for (int i = 0; i < bests.GetLength(0); i++)
			{
				if (bests[i, 0] != 0 || bests[i, 1] != 0)
				{
					startFin[0] += bests[i, 0] * bests[i, 6];
					startFin[1] += bests[i, 1] * bests[i, 6];
					startFin[2] += bests[i, 2] * bests[i, 6];
					startFin[3] += bests[i, 3] * bests[i, 6];
					startFin[4] += bests[i, 4] * bests[i, 6];
					startFin[5] += bests[i, 5] * bests[i, 6];
					d += bests[i, 6];
				}
			}
			startFin[0] /= d;
			startFin[1] /= d;
			startFin[2] /= d;
			startFin[3] /= d;
			startFin[4] /= d;
			startFin[5] /= d;
			limits[0, 1] = startFin[0];
			limits[0, 0] = startFin[0] * devi[0];
			limits[1, 1] = startFin[1];
			limits[1, 0] = startFin[1] * devi[1];
			limits[2, 1] = startFin[2];
			limits[2, 0] = startFin[2] * devi[2];
			limits[3, 1] = startFin[3];
			limits[3, 0] = startFin[3] * devi[3];
			limits[4, 1] = startFin[4];
			limits[4, 0] = startFin[4] * devi[4];
			limits[5, 1] = startFin[5];
			limits[5, 0] = startFin[5] * devi[5];
			return limits;
		}


		private double[,] eliminate(double[,] g, int keep, double minMs)
        {
			g = g.OrderByDescending(x => x[6]); //sort by altitude
			double[,] keepElem = new double[keep, 9];
			int added = 0;
			for (int i = 0; i < g.GetLength(0); i++)
			{
				if (g[i, 7] > minMs && g[i,8] == 0) //no error + Ms > minMs 
				{
					keepElem[added, 0] = g[i, 0];
					keepElem[added, 1] = g[i, 1];
					keepElem[added, 2] = g[i, 2];
					keepElem[added, 3] = g[i, 3];
					keepElem[added, 4] = g[i, 4];
					keepElem[added, 5] = g[i, 5];
					keepElem[added, 6] = g[i, 6];
					keepElem[added, 7] = g[i, 7];
					keepElem[added, 8] = g[i, 8];
					added++;
				}
				if (added >= keep)
					break;
			}
			/*
			if(added>0)
				ResizeArray<double>(ref keepElem, added, keepElem.GetLength(1));
			*/
			return keepElem;
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
			//xlApptemp.Visible = false;
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
		public Double[] randomizeRocket(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int distrib, double area)
		{
			Fin fin0 = getFin(start);
			int i = 0;
			double[] pa = new double[param.Length];
			foreach(ParametersEnum.Parameters p in param)
			{
				pa[i] = changeParameterRan(fin0, p, limits[i, 0], limits[i, 1], distrib);
				i++;
			}

			//constraint
			fin0.TipChord = Math.Min(fin0.chord, fin0.TipChord);
			if (area > 0)
			{
				fin0.TipChord = Math.Max(0, (2 * area / fin0.span) - fin0.chord);
			}
			fin0.sweepDist = Math.Min(fin0.sweepDist, Math.Max(0,fin0.chord-fin0.TipChord/4));
			
			i = 0;
			foreach (ParametersEnum.Parameters p in param)
			{
				if (p == ParametersEnum.Parameters.TIPCHORD)
					pa[i] = fin0.TipChord;
				if (p == ParametersEnum.Parameters.SWEEP)
					pa[i] = fin0.TipChord;
				i++;
			}
			
			return pa;
		}


		public Fin  changeParam(Fin fin, double[] values)
        {
			fin.span = values[5];
			fin.sweepDist = values[0];
			fin.TipChord = values[1];
			fin.thickness = values[2];
			fin.chord = values[3];
			//fin.Loc = result[4];
			return fin;
		}

		public double changeParameterRan(Fin f, ParametersEnum.Parameters pm, double min, double max,int distrib)
		{
			double ran = randomizer(min, max, distrib);
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
		
		public double randomizer(double min, double max, int distrib)
		{
			if(distrib == 1)
				return ran.NextDouble() * (max-min) + min;
			if (distrib == 0)
            {
				double u1 = 1.0 - ran.NextDouble(); //uniform(0,1] random doubles
				double u2 = 1.0 - ran.NextDouble();
				double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
							 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
				return max + min * randStdNormal; //random normal (mean,deviation²)
			}
			return ran.NextDouble() * (max - min) + min;
		}

		private Fin getFin(Rocket r)
		{
			RocketElement el = r.getNosecone();
			while (el.Bot != null)
			{
				el = el.Bot;
				if (el.SideAttach.Count > 0)
				{
					return (Fin) el.SideAttach.First();
				}
				
			}
			return null;
		}

	}
}