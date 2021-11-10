using System;
using System.Windows.Forms;
using IdmCic.API.Utils.Plugins;
using Excel = Microsoft.Office.Interop.Excel;
using SolidWorks.Interop.sldworks;

using IdmCic.API.Model.Subsystems;
using IdmCic.API.Model.Mainsystem;
using IdmCic.API.Utils.Events;
using System.Linq;
using System.Diagnostics;
using System.IO;
using IdmCic.API.Model.Physics.TopologicalOperations;
using IdmCic.API.Model.Physics.Objects3D.Solids;
using Microsoft.Win32;
using IdmCic.API.Model.Physics;
using Microsoft.Office.Interop.Excel;
using Assembly = IdmCic.API.Model.Subsystems.Assembly;
using SwConst;

namespace RocketDesigner
{


	public class Main : AbstractPlugin
	{

		private bool swinstalled = false;

		private PluginAction calculateAero;
		private PluginAction openRas;
		private PluginAction create2D;
		private PluginAction testCheck;

		private PluginButton calculateAeroBtn;
		private PluginButton OpenRASBtn;
		private PluginButton create2DBtn;
		private PluginCheckBox checkbox;

		private PluginObjectAction NoseConeAddAction;
		private PluginObjectAction FinsAddAction;
		private PluginObjectAction BodyAddAction;
		private PluginObjectAction TransiAddAction;
		private PluginObjectAction AssemblyAddAction;

		private PluginObjectButton NoseConeAddButton;
		private PluginObjectButton FinsAddButton;
		private PluginObjectButton BodyAddButton;
		private PluginObjectButton TransiAddButton;
		private PluginObjectButton AssemblyAddButton;

		// Expose the action tothe Hosting application
		public override System.Collections.Generic.IEnumerable<PluginObjectAction> GetObjectActions()
		{
			
			yield return NoseConeAddAction;
			yield return FinsAddAction;
			yield return BodyAddAction;
			yield return TransiAddAction;
			yield return AssemblyAddAction;
		}

		// Expose the control to the Hosting application

		public override System.Collections.Generic.IEnumerable<PluginObjectControl> GetObjectControls()
		{
			yield return NoseConeAddButton;
			yield return FinsAddButton;
			yield return BodyAddButton;
			yield return TransiAddButton;
			yield return AssemblyAddButton;
		}


		public override string Description
		{
			get { return "This is a plugin for designing rockets"; }
		}
		public override string Name
		{
			get { return "Rocket Desinger"; }
		}
		public override string Version
		{
			get { return "0.2.0"; }
		}

		Excel.Workbook aero;
		SldWorks swApp;
		Aerodynamics aerodynamics;

		public override void Initialise()
		{
			aerodynamics = new Aerodynamics();
			calculateAero = new PluginAction("calculateAero", calculateAeroImpl);
			openRas = new PluginAction("openRas", openRASImpl);
			create2D = new PluginAction("create2d", create2DImpl);
			testCheck = new PluginAction("test_action", calculateAeroImpl);

			calculateAeroBtn = new PluginButton("calculateAero_button", "Calculate aero coef", calculateAero, "Rocket Designer", "Aerodynamics");
			OpenRASBtn = new PluginButton("openRas_button", "Open RASAero II", openRas, "Rocket Designer", "Aerodynamics");
			create2DBtn = new PluginButton("create2d_button", "Create 2D Sketch", create2D, "Rocket Designer", "Aerodynamics");
			checkbox = new PluginCheckBox("test", "test", testCheck, "Rocket Designer", "test");
			//IdmCic_tab
			calculateAeroBtn.IsVisibleAtStartUp = true;
			calculateAeroBtn.IsVisibleAfterLoadingMainSystem = true;
			calculateAeroBtn.LargeStyle = true;

			OpenRASBtn.IsVisibleAtStartUp = true;
			OpenRASBtn.IsVisibleAfterLoadingMainSystem = true;
			OpenRASBtn.LargeStyle = true;

			create2DBtn.IsVisibleAtStartUp = true;
			create2DBtn.IsVisibleAfterLoadingMainSystem = true;
			create2DBtn.LargeStyle = true;

			using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (var key = hklm.OpenSubKey(@"Software\SolidWorks"))
			{
				if (key == null)
				{
					// Doesn't exist...
					MessageBox.Show("Solidwoks n'est pas installé, certaines fonctionnalitées de " + Name + " ne seront pas disponible");
				}
				else
				{
					swinstalled = true;
					swApp = (SldWorks)Activator.CreateInstance(System.Type.GetTypeFromProgID("SldWorks.Application"));
					swApp.Visible = false;
				}
			}



			/*
			Excel.Application oXL;
			oXL = new Excel.Application();

			aero = oXL.Workbooks.Open("C:\\Users\\cgene\\AppData\\Roaming\\idmcic_data\\plugins\\test\\BaseAérodynamique_Finale.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing);
			oXL.Visible = false;
			*/

			NoseConeAddAction = new PluginObjectAction("nosecone_add_action", typeof(Equipment), NoseConeAddActionImpl);
			FinsAddAction = new PluginObjectAction("fins_add_action", typeof(Equipment), FinsAddActionImpl);
			BodyAddAction = new PluginObjectAction("body_add_action", typeof(Equipment), BodyAddActionImpl);
			TransiAddAction = new PluginObjectAction("transi_add_action", typeof(Equipment), TransitionAddActionImpl);
			AssemblyAddAction = new PluginObjectAction("assembly_add_action", typeof(Assembly), AssemblyAddActionImpl);

			NoseConeAddButton = new PluginObjectButton("nosecone_add_button", "Add Rocket Nose cone", NoseConeAddAction);
			FinsAddButton = new PluginObjectButton("fins_add_button", "Add Rocket Fins", FinsAddAction);
			BodyAddButton = new PluginObjectButton("body_add_button", "Add Rocket Body", BodyAddAction);
			TransiAddButton = new PluginObjectButton("transi_add_button", "Add Rocket Transition", TransiAddAction);
			AssemblyAddButton = new PluginObjectButton("assembly_add_button", "Set As Rocket Assembly", AssemblyAddAction);
		}

		public override object MainSystemLoaded(MainSystem system)
		{

			system.OnPropertyChanged += System_OnPropertyChanged;


			return null;
		}

		private void System_OnPropertyChanged(ModelEventInfo info)
		{
			if (info.Dispatcher is IdmCic.API.Model.IdmProperties.Property && info.PropertyName == "Value" && (info.Dispatcher.Id == "noseconeH" || info.Dispatcher.Id == "noseconeR" || info.Dispatcher.Id == "noseconeTh") && info.IsRootEvent)
			{
				updateNoseCone((Equipment)info.Dispatcher.Parent, info);
			}
			if (info.Dispatcher is IdmCic.API.Model.IdmProperties.Property && info.PropertyName == "Value" && (info.Dispatcher.Id == "mach"))
			{
				aerodynamics.updateCP((Assembly)info.Dispatcher.Parent);
			}
		}

		public string updateRocketXML(MainSystem mainSystem)
		{
			string fileCreated = "";
			foreach (Element e in mainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{

					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						string filec = r.generateXMLFile(e.Name.Replace(" ", ""));
						System.IO.File.Copy(filec, Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\ras.CDX1"), true);
						fileCreated = fileCreated + filec + ";";
					}
				}
			}
			return fileCreated;
		}

		public void openRASImpl(PluginActionArgs args)
		{

			string files = updateRocketXML(args.MainSystem);

			var filename = files.Split(';')[0];


			string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string exefile = Path.Combine(assemblyFolder, "RAS2.exe");
			Process p = new Process();
			p.StartInfo = new ProcessStartInfo(exefile);
			p.StartInfo.WorkingDirectory = assemblyFolder;

			p.StartInfo.Arguments = filename;
			p.Start();

		}

		public void create2DImpl(PluginActionArgs args)
		{
				foreach (Element e in args.MainSystem.Elements)
				{
					foreach (RelatedSubsystem s in e.RelatedSubsystems)
					{
					    
						Rocket r = Rocket.getRocketFromElement(s);
						if (r != null)
						{
							r.generateXMLFile(e.Name.Replace(" ", ""));
							DialogResult dialogResult = MessageBox.Show("Include Fins?", "2D Sketch", MessageBoxButtons.YesNo);
							double h = 0;
							double front = 3;
							double end = 1;
							if (r.getRocketAssembly().GetProperty("frontFactor2D") != null)
								front = double.Parse((r.getRocketAssembly().GetProperty("frontFactor2D").Value.ToString()));
							if (r.getRocketAssembly().GetProperty("endFactor2D") != null)
								end = double.Parse((r.getRocketAssembly().GetProperty("endFactor2D").Value.ToString()));
							if (r.getRocketAssembly().GetProperty("Height2D") != null)
								h = double.Parse((r.getRocketAssembly().GetProperty("Height2D").Value.ToString()));
							create2DSketch(r, dialogResult == DialogResult.Yes,front,end,h);
						}
					}
				}

		}


		private void calculateAeroImpl(PluginActionArgs args)
		{
			string files = updateRocketXML(args.MainSystem);

			//var filename = files.Split(';')[0];

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
			MessageBox.Show("Aerodynamic coefficients have been calculated correctly");

			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\ras2.txt");
			if (System.IO.File.Exists(fileName))
				System.IO.File.Delete(fileName);

			System.IO.File.Move(fileName.Replace("txt", "CSV"), fileName);
			try
			{
				Microsoft.Office.Interop.Excel.Application app = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

				foreach (Worksheet ws in app.ActiveWorkbook.Worksheets)
				{
					if (ws.Name == "ras2")
					{
						app.DisplayAlerts = false;
						ws.Delete();
						app.DisplayAlerts = true;
					}
				}

				Excel._Worksheet sheet = (_Worksheet)app.ActiveSheet;


				Excel.Workbooks wbs = app.Workbooks;

				wbs.OpenText(fileName, DataType: Excel.XlTextParsingType.xlDelimited, Semicolon: true, Comma: false);

				Worksheet worksheet = (Worksheet)wbs[wbs.Count].Worksheets["ras2"];

				worksheet.Copy(After: sheet);

				wbs[wbs.Count].Close();

				foreach (Element e in args.MainSystem.Elements)
				{
					foreach (RelatedSubsystem s in e.RelatedSubsystems)
					{

						foreach (IdmCic.API.Model.Subsystems.Assembly assemb in s.Assemblies.ToList())
						{
							if (assemb.Name == "Rocket")
							{
								bool ex = false;

								foreach (CoordinateSystemDefinition coord in assemb.CoordinateSystems)
								{
									if (coord.Name == "CP")
									{
										aerodynamics.updateCP(assemb);
										ex = true;
									}
								}
								if (!ex)
								{
									CoordinateSystemDefinition cp = assemb.AddCoordinateSystem();
									cp.Name = "CP";
									aerodynamics.updateCP(assemb);
								}


							}
						}
					}
				}

			}
			catch
			{

			}


		}

		



		// Expose the action to the Hosting application
		public override System.Collections.Generic.IEnumerable<PluginAction> GetActions()
		{

			yield return calculateAero;
			yield return testCheck;
			yield return openRas;
			yield return create2D;
		}
		// Expose the control to the Hosting application
		public override System.Collections.Generic.IEnumerable<PluginControl> GetControls()
		{
			yield return calculateAeroBtn;
			yield return checkbox;
			yield return OpenRASBtn;
			yield return create2DBtn;
		}

		public override void ApplicationQuit()
		{

			//aero.Close(false);

			if (swinstalled)
				swApp.ExitApp();

			//MessageBox.Show("excel closed");
		}



		private void AssemblyAddActionImpl(PluginObjectActionArgs args)
		{
			if (!typeof(Assembly).IsInstanceOfType(args.IdmObject))
					return;

			Assembly ass = ((Assembly)args.IdmObject);

			ass.Name = "Rocket";

			foreach (CoordinateSystemDefinition coord in ass.CoordinateSystems)
			{
				if (coord.Name == "COG")
				{
					return;
				}
			}
			CoordinateSystemDefinition cog = ass.AddCoordinateSystem();
			cog.Name = "COG";
			cog.Position.SetPropertyFormula("X", "[" + ass.GetFullPropertyName("GetCog()_x") + "]");
			cog.Position.SetPropertyFormula("Y", "[" + ass.GetFullPropertyName("GetCog()_y") + "]");
			cog.Position.SetPropertyFormula("Z", "[" + ass.GetFullPropertyName("GetCog()_z") + "]");

			IdmCic.API.Model.IdmProperties.Property propMach = ass.AddProperty("mach", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propMach.Name = "Mach";
			propMach.Value = 0.0;

			IdmCic.API.Model.IdmProperties.Property propFront = ass.AddProperty("frontFactor2D", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propFront.Name = "Front factor 2D";
			propFront.Value = 3;

			IdmCic.API.Model.IdmProperties.Property propEnd = ass.AddProperty("endFactor2D", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propEnd.Name = "Rear Factor 2D";
			propEnd.Value = 1;

			IdmCic.API.Model.IdmProperties.Property propHeight = ass.AddProperty("Height2D", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propHeight.Name = "Height 2D";
			propHeight.Value = 2000;

			IdmCic.API.Model.IdmProperties.Property propType = ass.AddProperty("Rocket", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "Rocket";
			propType.Value = true;
			propType.Hidden = true;


		}
		private string getNewID(RelatedSubsystem ss, string part)
		{
			int id = 0;
			foreach (Equipment ekip in ss.Equipments)
			{
				if (ekip.Name.Contains(part))
				{
					int lid = -1;
					int.TryParse(ekip.Name.Replace(part, ""), out lid);
					if (id == lid)
						id++;
				}
			}
			return part + id;
		}

		private void TransitionAddActionImpl(PluginObjectActionArgs args)
		{
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketTransition", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketTransition";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propTopRadius = ((Equipment)args.IdmObject).AddProperty("transiTopR", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTopRadius.Name = "Top Radius";
			propTopRadius.Value = 0.3;

			

			IdmCic.API.Model.IdmProperties.Property propBotRadius = ((Equipment)args.IdmObject).AddProperty("transiBotR", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propBotRadius.Name = "Bot Radius";
			propBotRadius.Value = 0.3;

			IdmCic.API.Model.IdmProperties.Property propHeight = ((Equipment)args.IdmObject).AddProperty("transiH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propHeight.Name = "Height";
			propHeight.Value = 1;

			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("transiTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 0.02;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCone);
			shape.Name = "TransiShape";

			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketTransition");

			HollowCone o1 = (HollowCone)(shape.ShapeDefinition);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propTopRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D2", "mm_m(m_mm([" + propBotRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");

			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "bot_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm(0))");

		}
		private void BodyAddActionImpl(PluginObjectActionArgs args)
		{

			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketBody", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketBody";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propRadius = ((Equipment)args.IdmObject).AddProperty("bodyR", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propRadius.Name = "Radius";
			propRadius.Value = 0.3;

			IdmCic.API.Model.IdmProperties.Property propH = ((Equipment)args.IdmObject).AddProperty("bodyH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH.Name = "Height";
			propH.Value = 5;

			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("bodyTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 0.02;

			IdmCic.API.Model.IdmProperties.Property propLoc = ((Equipment)args.IdmObject).AddProperty("bodyFinPos", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propLoc.Name = "Fin Pos";
			propLoc.Value = 0.00;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCylinder);
			shape.Name = "BodyShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketBody");

			HollowCylinder o1 = (HollowCylinder)(shape.ShapeDefinition);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");


			//4 fins
			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_1";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "180");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_2";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "0");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_3";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_4";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(-([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			//3 fins
			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_1";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)*0.5))");
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(-([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)*COS(30*PI()/180)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "120");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_2";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)*0.5))");
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)*COS(30*PI()/180)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-120");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_3";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-([" + propRadius.FullId.ToLower() + "_value]-10* 0.001)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "0");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "base_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			sys.Position.SetPropertyFormula("Rotation2", "-90");



		}
		private void NoseConeAddActionImpl(PluginObjectActionArgs args)
		{

			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketNoseCone", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketNoseCone";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propRadius = ((Equipment)args.IdmObject).AddProperty("noseconeR", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propRadius.Name = "Radius";
			propRadius.Value = 0.3;
			IdmCic.API.Model.IdmProperties.Property propH = ((Equipment)args.IdmObject).AddProperty("noseconeH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH.Name = "Height";
			propH.Value = 1;
			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("noseconeTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 0.02;

			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketNoseCone");

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Step);
			shape.Name = "NoseConeShape";


			



			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\nosecone.STEP");
			((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)shape.ShapeDefinition).SetStepObjectFromFile(fileName);
			shape.Position.SetPropertyFormula("Rotation2", "-90");
			//shape.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");


			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "bot_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm(0))");

			//updateNoseCone(((Equipment)args.IdmObject), null);

		}
		private void FinsAddActionImpl(PluginObjectActionArgs args)
		{
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketFin", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketFin";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propH = ((Equipment)args.IdmObject).AddProperty("finH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH.Name = "Height";
			propH.Value = 250 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propH2 = ((Equipment)args.IdmObject).AddProperty("finH2", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH2.Name = "Height (LE)";
			propH2.Value = 40 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propH3 = ((Equipment)args.IdmObject).AddProperty("finH3", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH3.Name = "Height (TE)";
			propH3.Value = 40 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("finTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 50 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propAng = ((Equipment)args.IdmObject).AddProperty("finA1", IdmCic.API.Model.IdmProperties.IdmPropertyType.Angle);
			propAng.Name = "Angle LE";
			propAng.Value = 25;
			IdmCic.API.Model.IdmProperties.Property propAng2 = ((Equipment)args.IdmObject).AddProperty("finA2", IdmCic.API.Model.IdmProperties.IdmPropertyType.Angle);
			propAng2.Name = "Angle TL";
			propAng2.Value = 90;
			IdmCic.API.Model.IdmProperties.Property propLen = ((Equipment)args.IdmObject).AddProperty("finL", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propLen.Name = "Length";
			propLen.Value = 1200 * 0.001;


			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Topology);
			shape.Name = "RocketFinShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketFin");


			BooleanOperation bopp1 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedQuadrangle);
			bopp1.OperationType = OperationType.Union;




			ExtrudedQuadrangle o1 = (ExtrudedQuadrangle)(bopp1.Object3d);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D2", "mm_m(m_mm( (([" + propH.FullId.ToLower() + "_value]+10* 0.001))/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180)))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm( (([" + propH.FullId.ToLower() + "_value]+10* 0.001))/sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180)))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("angle1", "[" + propAng.FullId.ToLower() + "_value]");
			o1.SetPropertyFormula("angle2", "[" + propAng2.FullId.ToLower() + "_value]");


			bopp1.Position.SetPropertyFormula("z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))/2");


			BooleanOperation bopp2 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedTriangle);
			bopp2.OperationType = OperationType.Union;
			bopp2.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			bopp2.Object3d.SetPropertyFormula("D2", "mm_m(sqrt(pow(m_mm([" + propH2.FullId.ToLower() + "_value]), 2)+pow(m_mm([" + propTh.FullId.ToLower() + "_value])/2, 2) ))");
			bopp2.Object3d.SetPropertyFormula("D3", "mm_m(m_mm(([" + propH.FullId.ToLower() + "_value]+10* 0.001))/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180) + 500)");



			bopp2.Object3d.SetPropertyFormula("angle1", "ATAN([" + propH2.FullId.ToLower() + "_value]/([" + propTh.FullId.ToLower() + "_value]/2))*180/3.14159265");
			bopp2.Position.SetPropertyFormula("X", "mm_m(-300*COS([" + propAng.FullId.ToLower() + "_value]*3.14159265/180))");
			bopp2.Position.SetPropertyFormula("Y", "mm_m(-300*SIN([" + propAng.FullId.ToLower() + "_value]*3.14159265/180))");
			bopp2.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))/2");

			bopp2.Position.SetPropertyFormula("Rotation1", "90");
			bopp2.Position.SetPropertyFormula("Rotation2", "90 + [" + propAng.FullId.ToLower() + "_value]");
			bopp2.Position.SetPropertyFormula("Rotation3", "-90");


			BooleanOperation bopp5 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.Box);
			bopp5.OperationType = OperationType.Difference;
			bopp5.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))*2");
			bopp5.Object3d.SetPropertyFormula("D2", "mm_m(1000)");
			bopp5.Object3d.SetPropertyFormula("D3", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))*2");

			bopp5.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))");
			bopp5.Position.SetPropertyFormula("Z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))");
			bopp5.Position.SetPropertyFormula("Rotation3", "[" + propAng2.FullId.ToLower() + "_value] - 90");


			BooleanOperation bopp6 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedTriangle);
			bopp6.OperationType = OperationType.Union;
			bopp6.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			bopp6.Object3d.SetPropertyFormula("D2", "mm_m(sqrt(pow(m_mm([" + propH3.FullId.ToLower() + "_value]), 2)+pow(m_mm([" + propTh.FullId.ToLower() + "_value])/2, 2) ))");
			bopp6.Object3d.SetPropertyFormula("D3", "mm_m(m_mm(([" + propH.FullId.ToLower() + "_value]+10* 0.001))/sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180) + 500)");



			bopp6.Object3d.SetPropertyFormula("angle1", "ATAN([" + propH3.FullId.ToLower() + "_value]/([" + propTh.FullId.ToLower() + "_value]/2))*180/3.14159265");
			bopp6.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value])) + mm_m(-300*COS([" + propAng2.FullId.ToLower() + "_value]*3.14159265/180))");
			bopp6.Position.SetPropertyFormula("Y", "mm_m(-300*SIN([" + propAng2.FullId.ToLower() + "_value]*3.14159265/180))");
			bopp6.Position.SetPropertyFormula("Z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))/2");

			bopp6.Position.SetPropertyFormula("Rotation1", "-90");
			bopp6.Position.SetPropertyFormula("Rotation2", "90 - [" + propAng2.FullId.ToLower() + "_value]");
			bopp6.Position.SetPropertyFormula("Rotation3", "-90");

			BooleanOperation bopp3 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.Box);
			bopp3.OperationType = OperationType.Difference;
			bopp3.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))*2");
			bopp3.Object3d.SetPropertyFormula("D2", "mm_m(1000)");
			bopp3.Object3d.SetPropertyFormula("D3", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))*2");
			bopp3.Position.SetPropertyFormula("X", "mm_m(m_mm((3/2)*[" + propLen.FullId.ToLower() + "_value]))");
			bopp3.Position.SetPropertyFormula("Z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))");
			bopp3.Position.SetPropertyFormula("Rotation3", "180");

			BooleanOperation bopp4 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.Box);
			bopp4.OperationType = OperationType.Difference;
			bopp4.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))*2");
			bopp4.Object3d.SetPropertyFormula("D2", "mm_m(1000)");
			bopp4.Object3d.SetPropertyFormula("D3", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))*2");

			bopp4.Position.SetPropertyFormula("Y", "mm_m(m_mm(([" + propH.FullId.ToLower() + "_value]+10* 0.001)))");
			bopp4.Position.SetPropertyFormula("Z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))");
			bopp4.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propH2.FullId.ToLower() + "_value]*3))");

			shape.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propLen.FullId.ToLower() + "_value]))");



		}


		public void updateNoseCone(Equipment noseCone, ModelEventInfo info)
		{



			IdmCic.API.Model.IdmProperties.Property propRadius = noseCone.GetProperty("noseconeR");
			IdmCic.API.Model.IdmProperties.Property propHeight = noseCone.GetProperty("noseconeH");
			IdmCic.API.Model.IdmProperties.Property propTh = noseCone.GetProperty("noseconeTh");

			if (propRadius is null || propHeight is null || noseCone.Shapes.First() is null)
				return;
			
			if (swinstalled)
			{
				updateSWNoseFile((double)propRadius.Value * 1000, (double)propHeight.Value * 1000, (double)propTh.Value * 1000);
				var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\nosecone2.STEP");
				((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).SetStepObjectFromFile(fileName);
			}
			else
			{
				if (info.FullPropertyPath.Contains("noseconeR"))
				{
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D2 *= ((double)info.NewValue / (double)0.3);
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D3 *= ((double)info.NewValue / (double)0.3);
				}
				else if (info.FullPropertyPath.Contains("noseconeH"))
				{
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D1 *= ((double)info.NewValue / (double)1);
				}
			}
		}
		public void updateSWNoseFile(double r, double h, double th)
		{
			int err = 0;
			int warn = 0;

			double x = 0;
			double y = 0;
			
			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\Pièce.SLDPRT");
			
			//((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)
			swApp.NewDocument(swApp.GetUserPreferenceStringValue(8), 0, 0, 0);
			
			swApp.ActivateDoc2("Pièce.SLDPRT", false, err);
			
			ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;
			
			var myModelView = Part.ActiveView;
			swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

			Part.InsertSketch();
			Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			
			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			//Part.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, "Esquisse");
			Part.EditSketch();
			Part.ClearSelection2(true);
			

			Part.SketchManager.CreateEquationSpline2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ")", "", "0", "" + h, false, 0, 0, 0, true, true);
			
			Part.ClearSelection2(true);

			Part.SketchManager.InsertSketch(true);
			
			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SetPickMode();
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.SketchAddConstraints("sgCOINCIDENT");
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			
			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(0, 0, 0, 10, 0, 0);
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.SketchManager.CreateEquationSpline2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ") - " + th, "", "0", "" + h, false, 0, 0, 0, true, true);
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			
			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(h * 0.001, r * 0.001, 0, h * 0.001, (r - th) * 0.001, 0);
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SetPickMode();
			Part.Extension.SelectByID2("Spline4", "SKETCHSEGMENT", 0, 1, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Line1", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.SketchManager.SketchTrim(1, 0, 0, 0);
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.SketchManager.CreateCenterLine(0, 0, 0, 1, 0, 0);
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.Extension.SelectByID2("Line3@Esquisse1", "SKETCHSEGMENT", 0, 0, 0, true, 16, null, 0);
			Part.FeatureManager.FeatureRevolve2(true, true, false, false, false, false, 0, 0, 6.2831853071796, 0, false, false, 0.01, 0.01, 0, 0, 0, true, true, true);
			
			((SelectionMgr)Part.SelectionManager).EnableContourSelection = false;
			Part.ClearSelection2(true);


			fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\nosecone2.STEP");
			Part.SaveAs3(fileName, 0, 2);
			swApp.CloseDoc(Part.GetTitle());

		}



		/*
		public void test(MainSystem syst)
		{
			Point p = this.IdmStudy.MainSystem.GetCog(IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementActivation | IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementMargin, IdmStudy.MainSystem.GetConfiguration(""));
		}
		*/

		public void create2DSketch(Rocket rocket, bool withFins, double front, double end, double h2d)
		{
			int err = 0;
			int warn = 0;

			double x = 0;
			double y = 0;
			int pnbr = 5;
			int dimnbr = 2;

			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\Pièce.SLDPRT");

			swApp.NewDocument(swApp.GetUserPreferenceStringValue(((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)), 0, 0, 0);

			swApp.ActivateDoc2("Pièce.SLDPRT", false, err);
			ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

			var myModelView = Part.ActiveView;
			swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

			Part.InsertSketch();
			Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, "Esquisse");
			Part.EditSketch();
			Part.ClearSelection2(true);



			double r = rocket.getNosecone().radius*1000;
			double h = rocket.getNosecone().Len*1000;

			Part.SketchManager.CreateEquationSpline2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ")", "", "0", "" + h, false, 0, 0, 0, true, true);
			Part.ClearSelection2(true);

			Part.SketchManager.InsertSketch(true);


			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SetPickMode();
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.SketchAddConstraints("sgCOINCIDENT");
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);


			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SetPickMode();
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point4", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.AddVerticalDimension2(r * 0.001, 0, 0);
			Dimension myDimension = (Dimension)Part.Parameter("D1@Esquisse");
			myDimension.SystemValue = r * 0.001;


			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			x = h * 0.001;
			y = r * 0.001;


			RocketElement e = rocket.getNosecone();
			
			while (e.Bot != null)
			{
				e = e.Bot;
				if (typeof(Body).IsInstanceOfType(e))
				{

					if (withFins && e.SideAttach.Count>0)
					{
						int subbody = (int)((((Body)e).Len - ((Fin)e.SideAttach.First()).chord - ((Body)e).finLoc) * 1000);

						int finTE = (int)((((Fin)e.SideAttach.First()).chord - ((Fin)e.SideAttach.First()).sweepDist - ((Fin)e.SideAttach.First()).TipChord) * 1000);
						//int subbody = (int)((((Body)e).Len - ((Fin)e.SideAttach.First()).chord - ((Fin)e.SideAttach.First()).Loc) * 1000);


						createBody(ref x, ref y, subbody, (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
						createBody(ref x, ref y, (int)(((Fin)e.SideAttach.First()).sweepDist * 1000), (int) ( (((Fin)e.SideAttach.First()).span + ((Body)e).radius) * 1000), Part, ref pnbr, ref dimnbr);
						createBody(ref x, ref y, (int)(((Fin)e.SideAttach.First()).TipChord * 1000), (int)((((Fin)e.SideAttach.First()).span + ((Body)e).radius) * 1000), Part, ref pnbr, ref dimnbr);
						createBody(ref x, ref y, finTE, (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
						if (((Body)e).finLoc != 0){
							createBody(ref x, ref y, (int)(((Body)e).finLoc * 1000), (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
						}
					}
					else
					{
						createBody(ref x, ref y, (int)(((Body)e).Len * 1000), (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
					}
						

				}
				if (typeof(Transition).IsInstanceOfType(e))
				{
					createBody(ref x, ref y, (int)(((Transition)e).Len*1000), (int)(((Transition)e).radiusDown*1000), Part, ref pnbr, ref dimnbr);
				}
			}
			endRocket(ref x, ref y,front, end, h2d, Part, ref pnbr, ref dimnbr);

			string path;
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "IGES (*.igs)|*.igs";
			saveFileDialog1.RestoreDirectory = true;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				path = saveFileDialog1.FileName;
				Part.SaveAs3(path, 0, 2);
				Part.SaveAs3(path.Replace(".igs",".SLDPRT"), 0, 2);
				swApp.CloseDoc(Part.GetTitle());
			}
			else
			{

			}
		}


		public void createBody(ref double x, ref double y, int h, int r, ModelDoc2 Part, ref int pointNbr, ref int dimNbr)
		{
			Dimension myDimension;
			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SketchManager.CreateLine(x, y, 0, x + h * 0.001, r * 0.001, 0);

			Part.ClearSelection2(true);

			if (pointNbr == 5)
				pointNbr++;
			Part.SetPickMode();
			Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			if (pointNbr == 6)
			{
				Part.Extension.SelectByID2("Point4", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			}
			else
			{
				Part.Extension.SelectByID2("Point" + (pointNbr - 1), "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			}
			if (h != 0)
			{
				Part.AddHorizontalDimension2(Math.Abs(h) * 0.001, 0, 0);
				myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
				myDimension.SystemValue = Math.Abs(h) * 0.001;
				dimNbr++;
			}
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.AddVerticalDimension2(r * 0.001, 0, 0);
			myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
			myDimension.SystemValue = r * 0.001;
			dimNbr++;
			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			pointNbr++;

			x = x + h * 0.001;
			y = r * 0.001;

		}


		public void endRocket(ref double x, ref double y, double front, double end, double h, ModelDoc2 Part, ref int pointNbr, ref int dimNbr)
		{
			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.SketchManager.CreateLine(x, y, 0, x, 0, 0);

			Part.ClearSelection2(true);

			pointNbr++;

			double len = x;

			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(x, 0, 0, x + end * len, 0, 0);
			Part.ClearSelection2(true);
			x = x + end * len;
			y = 0;
			pointNbr++;

			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(x, y, 0, x, y + h * 0.001, 0);
			Part.ClearSelection2(true);
			x = x;
			y = y + h * 0.001;
			Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.AddVerticalDimension2(h * 0.001, 0, 0);
			Dimension myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
			myDimension.SystemValue = h * 0.001;
			dimNbr++;
			pointNbr++;
			Part.ClearSelection2(true);


			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(x, y, 0, x - (end + 1 + front) * len, y, 0);
			Part.ClearSelection2(true);
			x = x - (end + 1 + front) * len;
			y = y;
			Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.AddHorizontalDimension2((front) * len, 0, 0);
			myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
			myDimension.SystemValue = (front) * len;
			dimNbr++;
			pointNbr++;
			Part.ClearSelection2(true);


			Part.ClearSelection2(true);
			Part.SketchManager.CreateLine(x, y, 0, x, 0, 0);
			Part.ClearSelection2(true);
			x = x;
			y = 0;
			pointNbr++;

			Part.SketchManager.CreateLine(x, y, 0, x + front * len, 0, 0);

			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
			Part.SketchAddConstraints("sgCOINCIDENT");
			Part.ClearSelection2(true);


			x = 0;
			y = 0;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.InsertPlanarRefSurface();

			Part.ClearSelection2(true);
		}


		public double toInch(double m)
		{
			return Math.Round(m * 39.3701, 3);
		}

		public double toMeter(double inch)
		{
			return Math.Round(inch / 39.3701, 3);
		}

		/*
		 * - step -> sw part -> combine
		 * - creer une piece quart / tier de cylindre
		 * - ajouter la piece cylindre au sw part de la fusée
		 * - positioner les 2 corps
		 * - combine
		 * - export igs
		*/

		public void ddSketch()
		{
			//
			int err = 0;
			int warn = 0;

			double x = 0;
			double y = 0;
			int pnbr = 5;
			int dimnbr = 2;

			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\Pièce.SLDPRT");

			swApp.NewDocument(swApp.GetUserPreferenceStringValue(((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)), 0, 0, 0);

			swApp.ActivateDoc2("Pièce.SLDPRT", false, err);
			SolidWorks.Interop.sldworks.ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

			var myModelView = Part.ActiveView;
			swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

			Part.InsertSketch();
			Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
			Part.ClearSelection2(true);

			Part.SketchManager.CreateCenterLine(0, 0, 0, 3, 0, 0);
			Part.ClearSelection2(true);

			Part.SketchManager.CreateLine(0, 0, 0, 2, 1.5, 0);
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Line1", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.AddRadialDimension2(0, 0, 0);
			Dimension myDimension = (Dimension)Part.Parameter("D1@Esquisse1");
			myDimension.SystemValue = 120 * Math.PI / 180;
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.AddDimension2(0, 0, 0);
			myDimension = (Dimension)Part.Parameter("D2@Esquisse1");
			myDimension.SystemValue = 3;
			Part.ClearSelection2(true);

			Part.SketchManager.CreateArc(0, 0, 0, -1.5, 2.598076, 0, -2.974223, 0.392423, 0, 1);
			Part.ClearSelection2(true);

			Part.SketchManager.CreateLine(-2.974223, 0.392423, 0, 0, 0, 0);
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.Extension.SelectByID2("Line3", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
			Part.AddRadialDimension2(0, 0, 0);
			myDimension = (Dimension)Part.Parameter("D3@Esquisse1");
			myDimension.SystemValue = 60 * Math.PI / 180;
			Part.ClearSelection2(true);



			Part.InsertSketch();

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);

			Part.FeatureManager.FeatureExtrusion2(true, false, false, 0, 0, 10, 0.01, false, false, false, false, 1.74532925199433E-02, 1.74532925199433E-02, false, false, false, false, true, true, true, 0, 0, false);
			Part.SelectionManager.EnableContourSelection = false;


			Part.Extension.SelectByID2("Pièce.SLDPRT", "COMPONENT", 0, 0, 0, false, 0, null, 0);
			Part.SaveAs3(@"C:\Users\cgene\OneDrive\Documents\idm_files\cover.SLDPRT", 0, 0);
			Part.ClearSelection2(true);
			Part.EditRebuild3();
			Part.Save3(1, ref err, ref warn);

			((PartDoc)Part).InsertPart3(@"C:\Users\cgene\OneDrive\Documents\idm_files\test.SLDPRT", 21, "Défaut");

			Part.Extension.SelectByID2("Boss.-Extru.1", "SOLIDBODY", 0, 0, 0, false, 1, null, 0);
			Part.Extension.SelectByID2("<test>-<Combiner1>", "SOLIDBODY", 0, 0, 0, true, 2, null, 0);
			Part.FeatureManager.InsertCombineFeature(15902, null, null);

		}

	}

}