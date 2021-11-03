using System;
using System.Windows.Forms;
using IdmCic.API.Utils.Plugins;
using Excel = Microsoft.Office.Interop.Excel;
using SolidWorks.Interop.sldworks;
using System.Reflection;
using IdmCic.API.Model.Subsystems;
using IdmCic.API.Model.Mainsystem;
using IdmCic.API.Utils.Events;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Linq;
using Microsoft.Vbe.Interop;
using System.Diagnostics;
using System.IO;
using IdmCic.API.Model.Physics.TopologicalOperations;
using IdmCic.API.Model.Physics.Objects3D.Solids;
using System.Xml;
using Microsoft.Win32;
using IdmCic.API.Model.Physics;
using Microsoft.Office.Interop.Excel;
using Assembly = IdmCic.API.Model.Subsystems.Assembly;

namespace RocketDesigner
{


	public class Main : AbstractPlugin
	{

		private bool swinstalled = false;

		private PluginAction calculateAero;
		private PluginAction openRas;
		private PluginAction testCheck;

		private PluginButton calculateAeroBtn;
		private PluginButton OpenRASBtn;
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
			get { return "Rocket Helper"; }
		}
		public override string Version
		{
			get { return "0.1.0"; }
		}

		Excel.Workbook aero;
		SolidWorks.Interop.sldworks.SldWorks swApp;
		Aerodynamics aerodynamics;

		public override void Initialise()
		{
			aerodynamics = new Aerodynamics();
			calculateAero = new PluginAction("calculateAero", calculateAeroImpl);
			openRas = new PluginAction("openRas", openRASImpl);
			testCheck = new PluginAction("test_action", calculateAeroImpl);

			calculateAeroBtn = new PluginButton("calculateAero_button", "Calculate aero coef", calculateAero, "Rocket Designer", "Utils");
			OpenRASBtn = new PluginButton("openRas_button", "Open RASAero II", openRas, "Rocket Designer", "Utils");

			checkbox = new PluginCheckBox("test", "test", testCheck, "Rocket Designer", "test");
			//IdmCic_tab
			calculateAeroBtn.IsVisibleAtStartUp = true;
			calculateAeroBtn.IsVisibleAfterLoadingMainSystem = true;
			calculateAeroBtn.LargeStyle = true;

			OpenRASBtn.IsVisibleAtStartUp = true;
			OpenRASBtn.IsVisibleAfterLoadingMainSystem = true;
			OpenRASBtn.LargeStyle = true;

			using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (var key = hklm.OpenSubKey(@"Software\SolidWorks"))
			{
				if (key == null || true)
				{
					// Doesn't exist...
					MessageBox.Show("Solidwoks n'est pas installé, certaines fonctionnalitées de " + Name + " ne seront pas disponible");
				}
				else
				{
					swinstalled = true;
					swApp = new SldWorks();
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
			if (info.Dispatcher is IdmCic.API.Model.IdmProperties.Property && info.PropertyName == "Value" && (info.Dispatcher.Id == "noseconeH" || info.Dispatcher.Id == "noseconeR" || info.Dispatcher.Id == "noseconeTh"))
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

				Excel._Worksheet sheet = app.ActiveSheet;


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
		}
		// Expose the control to the Hosting application
		public override System.Collections.Generic.IEnumerable<PluginControl> GetControls()
		{
			yield return calculateAeroBtn;
			yield return checkbox;
			yield return OpenRASBtn;
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

			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("Rocket", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
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
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "180");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_2";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "0");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_3";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_4";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			//3 fins
			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_1";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]*0.5))");
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value]*COS(30*PI()/180)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "120");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_2";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]*0.5))");
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]*COS(30*PI()/180)))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-120");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_3";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value]))");
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
			o1.SetPropertyFormula("D2", "mm_m(m_mm( ([" + propH.FullId.ToLower() + "_value])/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180)))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm( ([" + propH.FullId.ToLower() + "_value])/sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180)))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("angle1", "[" + propAng.FullId.ToLower() + "_value]");
			o1.SetPropertyFormula("angle2", "[" + propAng2.FullId.ToLower() + "_value]");


			bopp1.Position.SetPropertyFormula("z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))/2");


			BooleanOperation bopp2 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedTriangle);
			bopp2.OperationType = OperationType.Union;
			bopp2.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			bopp2.Object3d.SetPropertyFormula("D2", "mm_m(sqrt(pow(m_mm([" + propH2.FullId.ToLower() + "_value]), 2)+pow(m_mm([" + propTh.FullId.ToLower() + "_value])/2, 2) ))");
			bopp2.Object3d.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value])/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180) + 500)");



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
			bopp6.Object3d.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value])/sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180) + 500)");



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

			bopp4.Position.SetPropertyFormula("Y", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
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


			//h = 20;
			int err = 0;
			int warn = 0;
			var fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\nosecone.SLDPRT");
			ModelDoc2 doc = swApp.OpenDoc6(fileName, 1, 0, "", ref err, ref warn);
			swApp.ActivateDoc2("nosecone.SLDPRT", false, err);
			SolidWorks.Interop.sldworks.ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

			var myModelView = Part.ActiveView;


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Spline3", "SKETCHSEGMENT", 8.99265592656241E-03, 3.52640490662842E-03, 0, false, 0, null, 0);

			Part.Extension.SelectByID2("Spline3", "SKETCHSEGMENT", 8.99265592656241E-03, 3.52640490662842E-03, 0, false, 0, null, 0);

			ISelectionMgr mng = ((ISelectionMgr)Part.ISelectionManager);

			SketchSpline equationCurve = (SketchSpline)mng.GetSelectedObject6(1, -1);
			String x, y, z;
			double s, e, aa, bb, cc;
			bool a, b, c;
			//equationCurve.GetEquationParameters2(out x, out y, out z, out s, out e, out a, out aa, out bb, out cc, out b, out c);

			equationCurve.SetEquationParameters2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ")", "", 0.0, h, false, 0, 0, 0, false, false);



			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);

			Part.Extension.SelectByID2("Spline8", "SKETCHSEGMENT", 8.99265592656241E-03, 3.52640490662842E-03, 0, false, 0, null, 0);

			Part.Extension.SelectByID2("Spline8", "SKETCHSEGMENT", 8.99265592656241E-03, 3.52640490662842E-03, 0, false, 0, null, 0);

			mng = ((ISelectionMgr)Part.ISelectionManager);

			equationCurve = (SketchSpline)mng.GetSelectedObject6(1, -1);

			//equationCurve.GetEquationParameters2(out x, out y, out z, out s, out e, out a, out aa, out bb, out cc, out b, out c);

			equationCurve.SetEquationParameters2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ")", "", 0.0, h - th, false, 0, 0, 0, false, false);



			



			

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D2@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.25182822338028E-02, 4.27555022788384E-03, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D2@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.20759839343665E-02, 4.06914435481359E-03, 0, false, 0, null, 0);
			Dimension myDimension6 = (Dimension)Part.Parameter("D2@Esquisse1");
			myDimension6.SystemValue = h * 0.001;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D3@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.25182822338028E-02, 4.27555022788384E-03, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D3@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.20759839343665E-02, 4.06914435481359E-03, 0, false, 0, null, 0);
			Dimension myDimension3 = (Dimension)Part.Parameter("D3@Esquisse1");
			myDimension3.SystemValue = (r-th) * 0.001;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			/*
			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D6@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.25182822338028E-02, 4.27555022788384E-03, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D6@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.20759839343665E-02, 4.06914435481359E-03, 0, false, 0, null, 0);
			Dimension myDimension4 = (Dimension)Part.Parameter("D6@Esquisse1");
			double rho = (r * r + h * h) / (2 * r);
			myDimension4.SystemValue = Math.Atan2(h,Math.Sqrt(rho*rho-h*h)) - 0.087;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);
			*/

			


			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D1@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.25182822338028E-02, 4.27555022788384E-03, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D1@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.20759839343665E-02, 4.06914435481359E-03, 0, false, 0, null, 0);
			Dimension myDimension = (Dimension)Part.Parameter("D1@Esquisse1");
			myDimension.SystemValue = r * 0.001;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true);

			Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
			Part.EditSketch();
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D4@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.25182822338028E-02, 4.27555022788384E-03, 0, false, 0, null, 0);
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("D4@Esquisse1@Pièce54.SLDPRT", "DIMENSION", 2.20759839343665E-02, 4.06914435481359E-03, 0, false, 0, null, 0);
			Dimension myDimension2 = (Dimension)Part.Parameter("D4@Esquisse1");
			myDimension2.SystemValue = (h - th) * 0.001;

			Part.ClearSelection2(true);
			Part.SketchManager.InsertSketch(true); 


			fileName = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\nosecone2.STEP");
			Part.SaveAs3(fileName, 0, 2);
			swApp.CloseDoc("nosecone.SLDPRT");

		}



		/*
		public void test(MainSystem syst)
		{
			Point p = this.IdmStudy.MainSystem.GetCog(IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementActivation | IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementMargin, IdmStudy.MainSystem.GetConfiguration(""));
		}
		*/

		public double toInch(double m)
		{
			return Math.Round(m * 39.3701, 3);
		}

		public double toMeter(double inch)
		{
			return Math.Round(inch / 39.3701, 3);
		}

	}

}