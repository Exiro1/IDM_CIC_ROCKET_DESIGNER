using IdmCic;
using IdmCic.API.Model.Mainsystem;
using IdmCic.API.Model.Physics;
using IdmCic.API.Model.Subsystems;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocketDesigner
{
	class Aerodynamics
	{
		public void updateCP(Assembly assemb)
		{
			double dist = 0;
			if (assemb.GetProperty("mach") != null)
				dist = getCP(double.Parse(assemb.GetProperty("mach").Value.ToString()));
			foreach (CoordinateSystemDefinition coord in assemb.CoordinateSystems)
			{
				if (coord.Name == "CP")
				{
					coord.Position.SetPropertyFormula("Z", "mm_m(" + (dist * 1000).ToString().Replace(",", ".") + ")");
				}
			}
			CD(assemb);
			staticMargin(assemb);
			CNa(assemb);
		}

		private Fin getFin(Rocket r)
		{
			RocketElement el = r.getNosecone();
			while (el.Bot != null)
			{
				if (el.SideAttach.Count > 0)
				{
					return (Fin)el.SideAttach.First();
				}
				el = el.Bot;
			}
			return null;
		}

		public void writeExcel(Element e, Rocket r,string path, bool runsimu, bool showGraph)
        {
			Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
			Workbook wb = app.Workbooks.Open(Main.folderPath+ path);
			app.Visible = false;

			Worksheet sheet = (Worksheet)wb.Worksheets["Propulsion"];
			sheet.Activate();

			//PROPULSION
			double[,] thrust = (double[,])getThrust(r);
			for (int i = 0; i < thrust.GetLength(0); i++)
			{
				sheet.Range[("B" + (i+3))].FormulaR1C1Local = thrust[i, 0];
				sheet.Range[("C" + (i+3))].FormulaR1C1Local = thrust[i, 1];
			}
			sheet.Range["H3"].FormulaR1C1Local = r.getEngine().ISP;
			sheet.Range["J3"].FormulaR1C1Local = r.getEngine().rMix;
			sheet.Range["K3"].FormulaR1C1Local = r.getEngine().getAs();
			/////////////
		
			//INIT
			sheet = (Worksheet)wb.Worksheets["Conditions initiales"];
			sheet.Activate();
			sheet.Range["G3"].FormulaR1C1Local = runsimu ? 1 : 0;
			sheet.Range["G6"].FormulaR1C1Local = showGraph ? 1 : 0;
			/////////////


			//STRUCTURE
			Fin fin = getFin(r);
			sheet = (Worksheet)wb.Worksheets["Structure"];
			sheet.Activate();
			double dryMass = e.GetMass(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.None);
			IdmCic.API.Model.Physics.Point dry = e.GetCog(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.None);
			sheet.Range["C9"].FormulaR1C1Local = dryMass;
			sheet.Range["D9"].FormulaR1C1Local = dry.Z*1000; //in simu Z = X
			sheet.Range["E9"].FormulaR1C1Local = dry.Y*1000;
			sheet.Range["F9"].FormulaR1C1Local = dry.X*1000;
			sheet.Range["G9"].FormulaR1C1Local = r.getLen()*1000;
			sheet.Range["H9"].FormulaR1C1Local = r.getRadius() * 2*1000;

			sheet.Range["I9"].FormulaR1C1Local = 33000000000;// shear modulus
			sheet.Range["J9"].FormulaR1C1Local = (fin.thickness/ fin.chord);// thickness/chord
			double area = (fin.span / 2) * (fin.chord + fin.TipChord);
			sheet.Range["K9"].FormulaR1C1Local = ((fin.chord* fin.chord)/area);// chord²/area
			sheet.Range["L9"].FormulaR1C1Local = (fin.TipChord/fin.chord);// tipchord/chord

			sheet.Range["C13"].FormulaR1C1Local = 0;
			sheet.Range["D13"].FormulaR1C1Local = 0;
			sheet.Range["E13"].FormulaR1C1Local = 0;
			sheet.Range["F13"].FormulaR1C1Local = 0;
			sheet.Range["G13"].FormulaR1C1Local = 0;
			sheet.Range["C14"].FormulaR1C1Local = 0;
			sheet.Range["D14"].FormulaR1C1Local = 0;
			sheet.Range["E14"].FormulaR1C1Local = 0;
			sheet.Range["F14"].FormulaR1C1Local = 0;
			sheet.Range["G14"].FormulaR1C1Local = 0;

			//TANKS
			foreach (RocketElement t in r.tanks)
			{
				if (typeof(SolidTank).IsInstanceOfType(t))
				{
					SolidTank st = (SolidTank)t;
					if (((SolidTank)t).type == 0)//oxydant
					{
						sheet.Range["C13"].FormulaR1C1Local = 1;
						sheet.Range["D13"].FormulaR1C1Local = st.height*1000; //in simu Z = X
						sheet.Range["E13"].FormulaR1C1Local = st.maxSolidMass;
						sheet.Range["F13"].FormulaR1C1Local = st.solidMass;
						sheet.Range["G13"].FormulaR1C1Local = st.cogZ*1000;
					}
					else if(st.type == 1)//reducteur
					{
						sheet.Range["C14"].FormulaR1C1Local = 1;
						sheet.Range["D14"].FormulaR1C1Local = st.height*1000; //in simu Z = X
						sheet.Range["E14"].FormulaR1C1Local = st.maxSolidMass;
						sheet.Range["F14"].FormulaR1C1Local = st.solidMass;
						sheet.Range["G14"].FormulaR1C1Local = st.cogZ*1000;
					}
				}
				if (typeof(LiquidTank).IsInstanceOfType(t))
				{
					LiquidTank lt = (LiquidTank)t;
					if (((LiquidTank)t).type == 0)
					{
						sheet.Range["C13"].FormulaR1C1Local = 2;
						sheet.Range["D13"].FormulaR1C1Local = lt.height*1000; //in simu Z = X
						sheet.Range["E13"].FormulaR1C1Local = lt.maxLiquidMass;
						sheet.Range["F13"].FormulaR1C1Local = lt.liquidMass;
						sheet.Range["G13"].FormulaR1C1Local = lt.cogZ*1000;
					}
					else if(lt.type == 1)
					{
						sheet.Range["C14"].FormulaR1C1Local = 2;
						sheet.Range["D14"].FormulaR1C1Local = lt.height*1000; //in simu Z = X
						sheet.Range["E14"].FormulaR1C1Local = lt.maxLiquidMass;
						sheet.Range["F14"].FormulaR1C1Local = lt.liquidMass;
						sheet.Range["G14"].FormulaR1C1Local = lt.cogZ*1000;
					}
				}
			}
			//////
			//////////
			wb.Save();
			wb.Close();
			app.Quit();
		}

		public static bool checkVersion(string version, string requiredVersion)
        {
			if (int.Parse(version.Split('.')[0]) >= int.Parse(requiredVersion.Split('.')[0]))
            {
				if (int.Parse(version.Split('.')[1]) >= int.Parse(requiredVersion.Split('.')[1]))
				{
					if (int.Parse(version.Split('.')[2]) >= int.Parse(requiredVersion.Split('.')[2]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public double[] startSimu3DDL(Element e, Rocket r, Matlab matlab,bool runsimu, bool showGraph)
		{

			if (!checkVersion(r.version, "0.5.0"))
			{
				MessageBox.Show("Rocket was made with on a outdated version of RocketDesigner, you have to rebuild it");
				return null;
			}

			writeExcel(e, r, "\\matlab\\3ddl\\Données_Excel.xlsx",runsimu,showGraph);
			object cp = getCP(600);
			object ca = getCA(600);
			object cn = getCN(600);



			if (matlab.loadMatlab())
			{
				object result = null;
				System.Array input = new double[10];
				matlab.getMatlabInstance().Execute(@"cd " + Main.folderPath + "\\matlab\\3ddl");
				matlab.getMatlabInstance().Execute("clear all");
				matlab.getMatlabInstance().PutWorkspaceData("CP", "base", cp);
				matlab.getMatlabInstance().PutWorkspaceData("CA", "base", ca);
				matlab.getMatlabInstance().PutWorkspaceData("CNa", "base", cn);
				matlab.getMatlabInstance().Execute("save('aero')");
				matlab.getMatlabInstance().Execute("simu3ddl2");
				string status = matlab.getMatlabInstance().Execute("status");
				double err = status.Split(';')[0].Remove(0, 17).Contains("OK") ? 0 : 1;
				double alt = safeParse(status.Split(';')[1]);
				double ms = safeParse(status.Split(';')[2]);
				double qinf = safeParse(status.Split(';')[3]);
				double mach = safeParse(status.Split(';')[4].Replace("\"\n\n", ""));
				if (showGraph) { 
				MessageBox.Show("Status du vol : " + status.Split(';')[0].Remove(0, 17) + "\n" +
					"Altitude max : " + status.Split(';')[1] + " m\n" +
					"Marge statique min : " + status.Split(';')[2] + " calibre\n" +
					"Qinf max : " + status.Split(';')[3] + " Pa\n" +
					"Mach max : " + status.Split(';')[4].Replace("\"\n\n", "") + "\n"
				);
				}
				return new double[] {alt, ms, qinf, mach,err };
			}
			return null;
		}

		public void startSimu6DDL(Element e, Rocket r, Matlab matlab)
		{
			if (!checkVersion(r.version, "0.5.0"))
			{
				MessageBox.Show("Rocket was made with on a outdated version of RocketDesigner, you have to rebuild it");
				return;
			}

			writeExcel(e, r, "\\matlab\\6ddl\\Données_Excel.xlsx", true,true);
			object cp = getCP(600);
			object ca = getCA(600);
			object cn = getCN(600);



			if (matlab.loadMatlab())
			{
				object result = null;
				System.Array input = new double[10];
				matlab.getMatlabInstance().Execute(@"cd " + Main.folderPath + "\\matlab\\6ddl");
				matlab.getMatlabInstance().PutWorkspaceData("CP", "base", cp);
				matlab.getMatlabInstance().PutWorkspaceData("CA", "base", ca);
				matlab.getMatlabInstance().PutWorkspaceData("CNa", "base", cn);
				matlab.getMatlabInstance().Execute("save('aero')");
				matlab.getMatlabInstance().Execute("Matlab_6DDL");

			}
		}

		public double safeParse(string d)
		{
			double res;
			if (!Double.TryParse(d, out res))
			{
				if (!Double.TryParse(d.Replace('.', ','), out res))
				{
					return 0;
				}
			}
			return res;
		}


		static object getThrust(Rocket r) {
			Nozzle eng = (Nozzle)r.getEngine();
			eng.unpack();
			return (object)eng.values;
		}

		static object getCP(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			Worksheet sheet = (Worksheet)wb.Worksheets["ras2"];
			var r = sheet.Range["M2"].Resize[size, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 0; i < size; i++)
			{
				
				d[i] = toMeter((double)array[i+1, 1]);
			}
			return (object)d;
		}
		static object getCN(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			Worksheet sheet = (Worksheet)wb.Worksheets["ras2"];
			var r = sheet.Range["L2"].Resize[size, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 0; i < size; i++)
			{
				d[i] = (double)array[i+1, 1];
			}
			return (object)d;
		}
		static object getCA(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			Worksheet sheet = (Worksheet)wb.Worksheets["ras2"];
			var r = sheet.Range["G2"].Resize[600, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 0; i < size; i++)
			{
				d[i] = (double)array[i+1, 1];
			}
			return (object)d;
		}

		


		#region Excel Chart
		public void staticMargin(Assembly assemb)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			try
			{
				string col = "Q";

				Worksheet worksheet = (Worksheet)wb.Worksheets["ras2"];

				((Range)worksheet.Cells[1, 17]).FormulaR1C1 = "Marge Statique";
				((Range)worksheet.Cells[2, 17]).Select();
				app.ActiveCell.FormulaR1C1 = "=((RC[-4]/39)*1000) - @IdmGet(\"" + assemb.GetFullPropertyName("GetCog()_z") + "\")";
				app.ActiveCell.AutoFill(worksheet.Range[col + "2", col + "1001"], XlAutoFillType.xlFillDefault);

				Microsoft.Office.Interop.Excel.Shape sh = worksheet.Shapes.AddChart2(227, XlChartType.xlLine, 0, 0, 350, 200);
				sh.Name = "Marge Statique";
				foreach (Microsoft.Office.Interop.Excel.Series series in (Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection())
				{
					series.Delete();
				}
				((Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection()).NewSeries();
				((Series)sh.Chart.FullSeriesCollection(1)).Name = "='ras2'!$" + col + "$1";
				((Series)sh.Chart.FullSeriesCollection(1)).Values = "='ras2'!$" + col + "$2:$" + col + "$1001";
				((Series)sh.Chart.FullSeriesCollection(1)).XValues = "='ras2'!$A$2:$A$1001";
				((Range)worksheet.Cells[1, 1]).Select();

			}
			catch (Exception er)
			{

			}
		}

		public void CD(Assembly assemb)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			try
			{
				string col = "R";

				Worksheet worksheet = (Worksheet)wb.Worksheets["ras2"];

				((Range)worksheet.Cells[1, 18]).FormulaR1C1 = "CD";
				((Range)worksheet.Cells[2, 18]).Select();
				app.ActiveCell.FormulaR1C1 = "=RC[-15]";
				app.ActiveCell.AutoFill(worksheet.Range[col + "2", col + "1001"], XlAutoFillType.xlFillDefault);

				Microsoft.Office.Interop.Excel.Shape sh = worksheet.Shapes.AddChart2(227, XlChartType.xlLine, 350, 0, 350, 200);
				sh.Name = "CD";
				foreach (Microsoft.Office.Interop.Excel.Series series in (Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection())
				{
					series.Delete();
				}
				((Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection()).NewSeries();
				((Series)sh.Chart.FullSeriesCollection(1)).Name = "='ras2'!$" + col + "$1";
				((Series)sh.Chart.FullSeriesCollection(1)).Values = "='ras2'!$" + col + "$2:$" + col + "$1001";
				((Series)sh.Chart.FullSeriesCollection(1)).XValues = "='ras2'!$A$2:$A$1001";
				((Range)worksheet.Cells[1, 1]).Select();
			}
			catch (Exception er)
			{

			}
		}

		public void CNa(Assembly assemb)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			try
			{
				string col = "S";

				Worksheet worksheet = (Worksheet)wb.Worksheets["ras2"];

				((Range)worksheet.Cells[1, 19]).FormulaR1C1 = "CNalpha";
				((Range)worksheet.Cells[2, 19]).Select();
				app.ActiveCell.FormulaR1C1 = "=RC[-7]";
				app.ActiveCell.AutoFill(worksheet.Range[col + "2", col + "1001"], XlAutoFillType.xlFillDefault);

				Microsoft.Office.Interop.Excel.Shape sh = worksheet.Shapes.AddChart2(227, XlChartType.xlLine, 700, 0, 350, 200);
				sh.Name = "CNalpha";
				foreach (Microsoft.Office.Interop.Excel.Series series in (Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection())
				{
					series.Delete();
				}
				((Microsoft.Office.Interop.Excel.SeriesCollection)sh.Chart.SeriesCollection()).NewSeries();
				((Series)sh.Chart.FullSeriesCollection(1)).Name = "='ras2'!$" + col + "$1";
				((Series)sh.Chart.FullSeriesCollection(1)).Values = "='ras2'!$" + col + "$2:$" + col + "$1001";
				((Series)sh.Chart.FullSeriesCollection(1)).XValues = "='ras2'!$A$2:$A$1001";
				((Range)worksheet.Cells[1, 1]).Select();
			}
			catch (Exception er)
			{

			}
		}

		#endregion
		public double getCP(Double mach)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel.Workbook wb = app.ActiveWorkbook;
			try
			{
				Worksheet worksheet = (Worksheet)wb.Worksheets["ras2"];

				mach = Math.Round(mach * 100);
				int row = Math.Min(Math.Max(1, (int)mach), 2500);
				string cp = ((Range)worksheet.Cells[1 + row, 13]).FormulaLocal.ToString();
				return toMeter(Double.Parse(cp));
			}
			catch (Exception er)
			{

			}

			return 0;

		}
		static double toMeter(double inch)
		{
			return Math.Round(inch / 39.3701, 3);
		}

	}
}
