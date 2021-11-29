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

		public void startSimu(Element e, Rocket r)
		{
			IdmCic.API.Model.Physics.Point dry = e.GetCog(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.None);
			IdmCic.API.Model.Physics.Point wet = e.GetCog(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.IncludeTankContent);

			double dryMass = e.GetMass(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.None);
			double wetMass = e.GetMass(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.IncludeTankContent);
			Matrix Idry = e.GetInertiaMatrix(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.None);
			Matrix Iwet = e.GetInertiaMatrix(IdmCic.API.Utils.Calculation.MCI.ElementMassOptions.IncludeTankContent);

			double cogC = (wet.Z*1000 - dry.Z*1000) / (wetMass - dryMass);
			double cogO = dry.Z*1000 - dryMass * cogC ;

			double IC = (Iwet.Xx - Idry.Xx) / (wetMass - dryMass);
			double IO = Idry.Xx - dryMass * IC;

			object cp = getCP(600);
			object ca = getCA(600);
			object cn = getCN(600);

			double Fi = 50000;

			double sref = 2;

			double ergolMass = wetMass - dryMass;



			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test");
			MLApp.MLApp matlab = new MLApp.MLApp();
			matlab.Visible = 1;
			matlab.Execute(@"cd "+ fileName+"\\matlab");

			object result = null;
		
			System.Array input = new double[10];
			matlab.Feval("simu3ddl2", 4, out result, (double)IC, (double)IO, (double)wetMass, (double)Fi, ca, cp, cn, (double)sref, (double)r.getLen()*1000, (double)ergolMass,cogC,cogO);
			//matlab.Feval("simu3ddl2", 2, out result, 1,2);

			object[] res = result as object[];



		}
		

		static object getCP(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel._Worksheet sheet = (_Worksheet)app.ActiveSheet;
			var r = sheet.Range["M2"].Resize[size, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 1; i < size; i++)
			{
				d[i] = (double)array[i, 1];
			}
			return (object)d;
		}
		static object getCN(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel._Worksheet sheet = (_Worksheet)app.ActiveSheet;
			var r = sheet.Range["L2"].Resize[size, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 1; i < size; i++)
			{
				d[i] = (double)array[i, 1];
			}
			return (object)d;
		}
		static object getCA(int size)
		{
			Microsoft.Office.Interop.Excel.Application app = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
			Microsoft.Office.Interop.Excel._Worksheet sheet = (_Worksheet)app.ActiveSheet;
			var r = sheet.Range["G2"].Resize[600, 1];
			var array = (object[,]) r.Value;
			double[] d = new double[size];
			for (int i = 1; i < size; i++)
			{
				d[i] = (double)array[i, 1];
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
		public double toMeter(double inch)
		{
			return Math.Round(inch / 39.3701, 3);
		}

	}
}
