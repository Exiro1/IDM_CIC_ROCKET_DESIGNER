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

		public void getResult(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int count, IdmCic.API.Model.Mainsystem.Element e, double mach, bool[] show, int distrib)
        {
			double[,] datas = generatePatch(start, param, limits, count, e, mach, show, distrib);
			matlab.displayGraphs(param, datas, show);
		}



		public double[,] generatePatch(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int count, IdmCic.API.Model.Mainsystem.Element e, double mach, bool[] dataToSave, int distrib)
		{

			progressBarCustom pbc = new progressBarCustom();
			pbc.Show();
			pbc.TopMost = true;
			double[,] globalData = new double[count, dataToSave.Count(c => true) + param.Length];
			int progress=0;
			for (int i = 0; i < count; i++)
			{
				double[] par = randomizeRocket(start, param, limits, distrib);
				progress++;
				pbc.SetProgressBarValue((int) ((100.0*progress)/(count*3.0)));
				double[] datas = getData(start, mach);
				progress++;
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
			}
			pbc.Close();
			return globalData;
		}

		

		//return best design after [epoch] number of generation -> return double[] {sweep,tipchord,thickness,chord,position,span};
		public double[] optimizeFin(Rocket start, IdmCic.API.Model.Mainsystem.Element e, double minMs,int generationPop, int keep,int epoch)
        {
			double[,] globaldata = new double[epoch*generationPop,8];
			int globalIndex = 0;
			ParametersEnum.Parameters[] param = new ParametersEnum.Parameters[] { ParametersEnum.Parameters.SWEEP, ParametersEnum.Parameters.TIPCHORD, ParametersEnum.Parameters.THICKNESS, ParametersEnum.Parameters.CHORD, ParametersEnum.Parameters.POSITION, ParametersEnum.Parameters.SPAN };
			double[,] limits = new double[param.Length, 2];
			double [] startFin = new double[] { getFin(start).sweepDist , getFin(start).TipChord , getFin(start).thickness , getFin(start).chord , getFin(start).Loc , getFin(start).span };
			bool[] datatosave = new bool[] { false, false, false, true, true, false, false };

			limits[0, 1] = startFin[0];
			limits[0, 0] = startFin[0] * 0.1;
			limits[1, 1] = startFin[1];
			limits[1, 0] = startFin[1] * 0.1;
			limits[2, 1] = startFin[2];
			limits[2, 0] = startFin[2] * 0.1;
			limits[3, 1] = startFin[3];
			limits[3, 0] = startFin[3] * 0.1;
			limits[4, 1] = startFin[4];
			limits[4, 0] = startFin[4] * 0.1;
			limits[5, 1] = startFin[5];
			limits[5, 0] = startFin[5] * 0.1;
			double[,] bests = new double[keep,10];
			double[,] g;
			for (int i = 0; i < epoch; i++)
			{
				g = generatePatch(start, param, limits, generationPop, e, 1, datatosave, 0);
				for(int k = 0; k < generationPop; k++)
                {
					globaldata[k+globalIndex,0] = g[k,0];
					globaldata[k + globalIndex, 1] = g[k, 1];
					globaldata[k + globalIndex, 2] = g[k, 2];
					globaldata[k + globalIndex, 3] = g[k, 3];
					globaldata[k + globalIndex, 4] = g[k, 4];
					globaldata[k + globalIndex, 5] = g[k, 5];
					globaldata[k + globalIndex, 6] = g[k, 6];
					globaldata[k + globalIndex, 7] = g[k, 7];
				}
				bests = eliminate(g, keep, minMs);
				if (bests[0, 6] == 0)
					return null;
				limits = getNewGeneration(bests, 0.1);
			}
			
			matlab.displayGraphs(param, globaldata, datatosave);

			return new double[] {bests[0,0], bests[0, 1] , bests[0, 2] , bests[0, 3] , bests[0, 4] , bests[0, 5] };
		}

		private double[,] getNewGeneration(double[,] bests, double deviation)
        {
			double[,] limits = new double[bests.GetLength(1)-2, 2];
			double[] startFin = new double[bests.GetLength(1) - 2];
			double d = 0;
			for (int i = 0; i < bests.GetLength(0); i++)
            {
				startFin[0] += bests[i, 0]*Math.Pow(2, bests.GetLength(0)-i);
				startFin[1] += bests[i, 1] * Math.Pow(2, bests.GetLength(0) - i);
				startFin[2] += bests[i, 2] * Math.Pow(2, bests.GetLength(0) - i);
				startFin[3] += bests[i, 3] * Math.Pow(2, bests.GetLength(0) - i);
				startFin[4] += bests[i, 4] * Math.Pow(2, bests.GetLength(0) - i);
				startFin[5] += bests[i, 5] * Math.Pow(2, bests.GetLength(0) - i);
				d += Math.Pow(2, bests.GetLength(0) - i);
			}
			startFin[0] /= d;
			startFin[1] /= d;
			startFin[2] /= d;
			startFin[3] /= d;
			startFin[4] /= d;
			startFin[5] /= d;
			limits[0, 1] = startFin[0];
			limits[0, 0] = startFin[0] * deviation;
			limits[1, 1] = startFin[1];
			limits[1, 0] = startFin[1] * deviation;
			limits[2, 1] = startFin[2];
			limits[2, 0] = startFin[2] * deviation;
			limits[3, 1] = startFin[3];
			limits[3, 0] = startFin[3] * deviation;
			limits[4, 1] = startFin[4];
			limits[4, 0] = startFin[4] * deviation;
			limits[5, 1] = startFin[5];
			limits[5, 0] = startFin[5] * deviation;
			return limits;
		}


		private double[,] eliminate(double[,] g, int keep, double minMs)
        {
			g = g.OrderBy(x => x[6]); //sort by altitude
			double[,] keepElem = new double[keep, 8];
			int added = 0;
			for (int i = 0; i < g.GetLength(0); i++)
			{
				if (g[i, 7] > minMs)
				{
					keepElem[added, 0] = g[i, 0];
					keepElem[added, 1] = g[i, 1];
					keepElem[added, 2] = g[i, 2];
					keepElem[added, 3] = g[i, 3];
					keepElem[added, 4] = g[i, 4];
					keepElem[added, 5] = g[i, 5];
					keepElem[added, 6] = g[i, 6];
					keepElem[added, 7] = g[i, 7];
					added++;
				}
				if (added >= keep)
					break;
			}
			if(added>0)
				ResizeArray<double>(ref keepElem, added, keepElem.GetLength(1));
			return keepElem;
		}

		void ResizeArray<T>(ref T[,] original, int newCoNum, int newRoNum)
		{
			var newArray = new T[newCoNum, newRoNum];
			int columnCount = original.GetLength(1);
			int columnCount2 = newRoNum;
			int columns = original.GetUpperBound(0);
			for (int co = 0; co <= columns; co++)
				Array.Copy(original, co * columnCount, newArray, co * columnCount2, columnCount);
			original = newArray;
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
		public Double[] randomizeRocket(Rocket start, ParametersEnum.Parameters[] param, double[,] limits, int distrib)
		{
			Fin fin0 = getFin(start);
			int i = 0;
			double[] pa = new double[param.Length];
			foreach(ParametersEnum.Parameters p in param)
			{
				pa[i] = changeParameterRan(fin0, p, limits[i, 0], limits[i, 1], distrib);
				i++;
			}
			return pa;
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