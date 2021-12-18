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
using IdmCic.API.Model.IdmFiles;

namespace RocketDesigner
{


	public class Main : AbstractPlugin
	{

		static string folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\");



		private PluginAction calculateAero;
		private PluginAction openRas;
		private PluginAction createOpenRocket;
		private PluginAction create2D;
		private PluginAction create3D;
		private PluginAction testCheck;
		private PluginAction launchSimu;

		private PluginButton calculateAeroBtn;
		private PluginButton OpenRASBtn;
		private PluginButton createOpenRocketBtn;
		private PluginButton create2DBtn;
		private PluginButton create3DBtn;
		private PluginButton launchSimuBtn;
		private PluginCheckBox checkbox;

		private PluginObjectAction NoseConeAddAction;
		private PluginObjectAction FinsAddAction;
		private PluginObjectAction BodyAddAction;
		private PluginObjectAction TransiAddAction;
		private PluginObjectAction AssemblyAddAction;
		private PluginObjectAction EngineAddAction;
		private PluginObjectAction EngineFileAddAction;

		private PluginObjectButton NoseConeAddButton;
		private PluginObjectButton FinsAddButton;
		private PluginObjectButton BodyAddButton;
		private PluginObjectButton TransiAddButton;
		private PluginObjectButton AssemblyAddButton;
		private PluginObjectButton EngineAddButton;
		private PluginObjectButton EngineFileAddButton;

		// Expose the action tothe Hosting application
		public override System.Collections.Generic.IEnumerable<PluginObjectAction> GetObjectActions()
		{

			yield return NoseConeAddAction;
			yield return FinsAddAction;
			yield return BodyAddAction;
			yield return TransiAddAction;
			yield return AssemblyAddAction;
			yield return EngineAddAction;
			yield return EngineFileAddAction;
		}

		// Expose the control to the Hosting application

		public override System.Collections.Generic.IEnumerable<PluginObjectControl> GetObjectControls()
		{
			yield return NoseConeAddButton;
			yield return FinsAddButton;
			yield return BodyAddButton;
			yield return TransiAddButton;
			yield return AssemblyAddButton;
			yield return EngineAddButton;
			yield return EngineFileAddButton;
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
			get { return "0.3.0"; }
		}

		Excel.Workbook aero;

		Aerodynamics aerodynamics;
		SolidWorksUtil swUtil;
		public override void Initialise()
		{
			aerodynamics = new Aerodynamics();
			calculateAero = new PluginAction("calculateAero", calculateAeroImpl);
			openRas = new PluginAction("openRas", openRASImpl);
			createOpenRocket = new PluginAction("createOpenRocket", createOpenRocketImpl);
			create2D = new PluginAction("create2d", create2DImpl);
			create3D = new PluginAction("create3d", create3DImpl);
			launchSimu = new PluginAction("launchSimu", launchSimuImpl);
			testCheck = new PluginAction("test_action", SolidworksVisibleImpl);

			calculateAeroBtn = new PluginButton("calculateAero_button", "Calculate aero coef", calculateAero, "Rocket Designer", "Aerodynamics");
			OpenRASBtn = new PluginButton("openRas_button", "Open RASAero II", openRas, "Rocket Designer", "File Creator");
			createOpenRocketBtn = new PluginButton("createOpenRocket_button", "Create OpenRocket File", createOpenRocket, "Rocket Designer", "File Creator");
			create2DBtn = new PluginButton("create2d_button", "Create 2D Sketch", create2D, "Rocket Designer", "File Creator");
			create3DBtn = new PluginButton("create2d_button", "Create 3D Sketch", create3D, "Rocket Designer", "File Creator");
			checkbox = new PluginCheckBox("sw_visible", "SolidWorks Visible", testCheck, "Rocket Designer", "Other");
			launchSimuBtn = new PluginButton("launchSimu_button", "Launch Simulation", launchSimu, "Rocket Designer", "Aerodynamics");
			//IdmCic_tab
			calculateAeroBtn.IsVisibleAtStartUp = true;
			calculateAeroBtn.IsVisibleAfterLoadingMainSystem = true;
			calculateAeroBtn.LargeStyle = true;

			checkbox.IsVisibleAtStartUp = true;
			checkbox.IsVisibleAfterLoadingMainSystem = true;
			checkbox.Checked = false;

			OpenRASBtn.IsVisibleAtStartUp = true;
			OpenRASBtn.IsVisibleAfterLoadingMainSystem = true;
			OpenRASBtn.LargeStyle = true;

			createOpenRocketBtn.IsVisibleAtStartUp = true;
			createOpenRocketBtn.IsVisibleAfterLoadingMainSystem = true;
			createOpenRocketBtn.LargeStyle = true;

			create2DBtn.IsVisibleAtStartUp = true;
			create2DBtn.IsVisibleAfterLoadingMainSystem = true;
			create2DBtn.LargeStyle = true;

			create3DBtn.IsVisibleAtStartUp = true;
			create3DBtn.IsVisibleAfterLoadingMainSystem = true;
			create3DBtn.LargeStyle = true;


			launchSimuBtn.IsVisibleAtStartUp = true;
			launchSimuBtn.IsVisibleAfterLoadingMainSystem = true;
			launchSimuBtn.LargeStyle = true;


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
			EngineAddAction = new PluginObjectAction("engine_add_action", typeof(Equipment), EngineAddActionImpl);
			EngineFileAddAction = new PluginObjectAction("engine_file_add_action", typeof(Equipment), EngineFileAddActionImpl);

			NoseConeAddButton = new PluginObjectButton("nosecone_add_button", "Add Rocket Nose cone", NoseConeAddAction);
			FinsAddButton = new PluginObjectButton("fins_add_button", "Add Rocket Fins", FinsAddAction);
			BodyAddButton = new PluginObjectButton("body_add_button", "Add Rocket Body", BodyAddAction);
			TransiAddButton = new PluginObjectButton("transi_add_button", "Add Rocket Transition", TransiAddAction);
			AssemblyAddButton = new PluginObjectButton("assembly_add_button", "Set As Rocket Assembly", AssemblyAddAction);
			EngineAddButton = new PluginObjectButton("engine_add_button", "Add Rocket Engine", EngineAddAction);
			EngineFileAddButton = new PluginObjectButton("engine_file_add_button", "set Engine .eng File", EngineFileAddAction);


			swUtil = new SolidWorksUtil();


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
						//r.generateOpenRocketFile(folderPath + "test.ork");
						System.IO.File.Copy(filec, folderPath + "ras.CDX1", true);
						fileCreated = fileCreated + filec + ";";
					}
				}
			}
			return fileCreated;
		}

		public void launchSimuImpl(PluginActionArgs args)
		{
			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{
					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						calculAeroCoef(args, false);
						aerodynamics.startSimu(e,r);
					}
				}
			}
		}

			public void createOpenRocketImpl(PluginActionArgs args)
		{

			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{
					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						Datagen g = new Datagen();

						Datagen.Parameters[] param = { Datagen.Parameters.THICKNESS, Datagen.Parameters.SWEEP};
						double[,] lim = new double[2, 2];
						lim[0, 1] = 0.1;
						lim[0, 0] = 0.01;
						lim[1, 1] = 1;
						lim[1, 0] = 0.1;
						g.generatePatch(r, param, lim, 100);



						string path;
						SaveFileDialog saveFileDialog1 = new SaveFileDialog();
						saveFileDialog1.Filter = "ORK (*.ork)|*.ork";
						saveFileDialog1.RestoreDirectory = true;
						
						if (saveFileDialog1.ShowDialog() == DialogResult.OK)
						{
							r.generateOpenRocketFile(saveFileDialog1.FileName,e.Name);
						}
						else
						{

						}
					}
				}
			}
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

						swUtil.create2DSketch(r, dialogResult == DialogResult.Yes, front, end, h);
					}
				}
			}

		}

		public void create3DImpl(PluginActionArgs args)
		{
			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{

					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						r.generateXMLFile(e.Name.Replace(" ", ""));
						foreach (IdmCic.API.Model.Subsystems.Assembly rocket in s.Assemblies.ToList())
						{
							if (rocket.GetProperty("Rocket") != null)
							{

								IdmCic.API.Managers.MainSystemManager man = new IdmCic.API.Managers.MainSystemManager();

								man.ExportViewableObjectToSTEP(rocket.GetViewableObject(), folderPath + "rocket.stp", args.MainSystem.Configurations.First(), "MM");
								break;
							}
						}
						double h = 0;
						double front = 3;
						double end = 1;
						if (r.getRocketAssembly().GetProperty("frontFactor2D") != null)
							front = double.Parse((r.getRocketAssembly().GetProperty("frontFactor2D").Value.ToString()));
						if (r.getRocketAssembly().GetProperty("endFactor2D") != null)
							end = double.Parse((r.getRocketAssembly().GetProperty("endFactor2D").Value.ToString()));
						if (r.getRocketAssembly().GetProperty("Height2D") != null)
							h = double.Parse((r.getRocketAssembly().GetProperty("Height2D").Value.ToString()));
						swUtil.stepToBody("rocket.stp");
						swUtil.sketch3D(r.getFinCount() == 0 ? 4 : r.getFinCount(), h, r.getLen() * (1 + end), r.getLen() * front);

					}
				}
			}

		}

		private void SolidworksVisibleImpl(PluginActionArgs args)
		{
			swUtil.switchVisible();
		}

		public void calculAeroCoef(PluginActionArgs args, bool box)
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

			if(box)
				MessageBox.Show("Aerodynamic coefficients have been calculated correctly");

			var fileName = folderPath + "ras2.txt";
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

		private void calculateAeroImpl(PluginActionArgs args)
		{
			calculAeroCoef(args, true);
		}





		// Expose the action to the Hosting application
		public override System.Collections.Generic.IEnumerable<PluginAction> GetActions()
		{

			yield return calculateAero;
			yield return testCheck;
			yield return openRas;
			yield return create2D;
			yield return create3D;
			yield return createOpenRocket;
			yield return launchSimu;
		}
		// Expose the control to the Hosting application
		public override System.Collections.Generic.IEnumerable<PluginControl> GetControls()
		{
			yield return calculateAeroBtn;
			yield return checkbox;
			yield return OpenRASBtn;
			yield return create2DBtn;
			yield return create3DBtn;
			yield return createOpenRocketBtn;
			yield return launchSimuBtn;
		}

		public override void ApplicationQuit()
		{

			//aero.Close(false);

			swUtil.close();



			//MessageBox.Show("excel closed");
		}

		private void EngineFileAddActionImpl(PluginObjectActionArgs args)
		{
			if (((Equipment)args.IdmObject).GetProperty("RocketEngine") != null)
			{
				OpenFileDialog openFileDialog1 = new OpenFileDialog
				{
					Title = "Select Engine (.eng) File",

					CheckFileExists = true,
					CheckPathExists = true,

					DefaultExt = "txt.eng",
					Filter = "eng files (*.eng)|*.eng",
					FilterIndex = 2,
					RestoreDirectory = true,
					ReadOnlyChecked = true,
					ShowReadOnly = true
				};

				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					if (((Equipment)args.IdmObject).GetDocument("1") != null)
					{
						((DocumentFile)((Equipment)args.IdmObject).GetDocument("1")).ChangeFileBy(openFileDialog1.FileName);
					}
					else
					{
						DocumentFile file = ((Equipment)args.IdmObject).AddDocumentFromFile(openFileDialog1.FileName);
					}
				}
				
			}
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
			cog.Position.SetPropertyFormula("X", "[" + ass.GetFullPropertyName("GetCog(ITkCt)_x") + "]");
			cog.Position.SetPropertyFormula("Y", "[" + ass.GetFullPropertyName("GetCog(ITkCt)_y") + "]");
			cog.Position.SetPropertyFormula("Z", "[" + ass.GetFullPropertyName("GetCog(ITkCt)_z") + "]");

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

		private void EngineAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketEngine", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketEngine";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propTopRadius = ((Equipment)args.IdmObject).AddProperty("engCr", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTopRadius.Name = "Chamber Radius";
			propTopRadius.Value = 0.290;


			IdmCic.API.Model.IdmProperties.Property propBotRadius = ((Equipment)args.IdmObject).AddProperty("engNr", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propBotRadius.Name = "Nozzle Radius";
			propBotRadius.Value = 0.15;

			IdmCic.API.Model.IdmProperties.Property propThroat = ((Equipment)args.IdmObject).AddProperty("engNt", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propThroat.Name = "Nozzle Throat";
			propThroat.Value = 0.075;

			IdmCic.API.Model.IdmProperties.Property propHeight = ((Equipment)args.IdmObject).AddProperty("engHn", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propHeight.Name = "Height Nozzle";
			propHeight.Value = 0.2;

			IdmCic.API.Model.IdmProperties.Property propHeight2 = ((Equipment)args.IdmObject).AddProperty("engHc", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propHeight2.Name = "Height Chamber";
			propHeight2.Value = 0.3;


			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("engTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 0.01;

			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("engDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCone);
			shape.Name = "RocketChamber";

			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketEngine");

			HollowCone o1 = (HollowCone)(shape.ShapeDefinition);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propTopRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D2", "mm_m(m_mm([" + propThroat.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propHeight2.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");

			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCone);
			shape.Name = "RocketNozzle";
			HollowCone o2 = (HollowCone)(shape.ShapeDefinition);
			o2.SetPropertyFormula("D1", "mm_m(m_mm([" + propThroat.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D2", "mm_m(m_mm([" + propBotRadius.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D3", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			shape.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propHeight2.FullId.ToLower() + "_value]))");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.FilledCylinder);
			shape.Name = "TopChamber";
			FilledCylinder o3 = (FilledCylinder)(shape.ShapeDefinition);
			o3.SetPropertyFormula("D1", "mm_m(m_mm([" + propTopRadius.FullId.ToLower() + "_value]))");
			o3.SetPropertyFormula("D3", "mm_m(5)");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "bot_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value] + [" + propHeight2.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm(0))");

		}

		private void TransitionAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
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

			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("transiDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;

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

			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "bot_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm(0))");

		}
		private void BodyAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
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

			IdmCic.API.Model.IdmProperties.Property propEng = ((Equipment)args.IdmObject).AddProperty("bodyEngPos", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propEng.Name = "Engine Pos";
			propEng.Value = 0.00;

			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("bodyDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCylinder);
			shape.Name = "BodyShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketBody");

			HollowCylinder o1 = (HollowCylinder)(shape.ShapeDefinition);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");

			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;
			
			//4 fins
			CoordinateSystemDefinition sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_1";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value])-10)");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "180");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_2";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value])+10)");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "0");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_3";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm([" + propRadius.FullId.ToLower() + "_value])-10)");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_90_4";
			sys.Position.SetPropertyFormula("Y", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value])+10)");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "90");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			//3 fins
			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_1";
			sys.Position.SetPropertyFormula("X", "mm_m( (m_mm([" + propRadius.FullId.ToLower() + "_value])-10)*0.5)");
			sys.Position.SetPropertyFormula("Y", "mm_m( (m_mm(-[" + propRadius.FullId.ToLower() + "_value])+10)*COS(30*PI()/180))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "120");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_2";
			sys.Position.SetPropertyFormula("X", "mm_m( (m_mm([" + propRadius.FullId.ToLower() + "_value])-10)*0.5)");
			sys.Position.SetPropertyFormula("Y", "mm_m( (m_mm([" + propRadius.FullId.ToLower() + "_value])-10)*COS(30*PI()/180))");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "-120");
			sys.Position.SetPropertyFormula("Rotation3", "90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "fin_attach_120_3";
			sys.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propRadius.FullId.ToLower() + "_value])+10)");
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propLoc.FullId.ToLower() + "_value]))");
			sys.Position.SetPropertyFormula("Rotation1", "90");
			sys.Position.SetPropertyFormula("Rotation2", "0");
			sys.Position.SetPropertyFormula("Rotation3", "90");


			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "base_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "top_attach";
			//sys.Position.SetPropertyFormula("Rotation2", "-90");

			sys = ((Equipment)args.IdmObject).AddCoordinateSystem();
			sys.Name = "engine_attach";
			sys.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value] - [" + propEng.FullId.ToLower() + "_value]))");


		}
		private void NoseConeAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
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

			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("noseconeDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;


			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketNoseCone");

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Step);
			shape.Name = "NoseConeShape";


			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;


			var fileName = folderPath + "nosecone.STEP";
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
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
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
			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("finDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;
			int enfoncement = 10;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Topology);
			shape.Name = "RocketFinShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketFin");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			BooleanOperation bopp1 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedQuadrangle);
			bopp1.OperationType = OperationType.Union;




			ExtrudedQuadrangle o1 = (ExtrudedQuadrangle)(bopp1.Object3d);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propLen.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D2", "mm_m( (m_mm( [" + propH.FullId.ToLower() + "_value])+" + enfoncement + " )/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180))");
			o1.SetPropertyFormula("D3", "mm_m( (m_mm( [" + propH.FullId.ToLower() + "_value])+" + enfoncement + " )/sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("angle1", "[" + propAng.FullId.ToLower() + "_value]");
			o1.SetPropertyFormula("angle2", "[" + propAng2.FullId.ToLower() + "_value]");


			bopp1.Position.SetPropertyFormula("z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))/2");


			BooleanOperation bopp2 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.ExtrudedTriangle);
			bopp2.OperationType = OperationType.Union;
			bopp2.Object3d.SetPropertyFormula("D1", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			bopp2.Object3d.SetPropertyFormula("D2", "mm_m(sqrt(pow(m_mm([" + propH2.FullId.ToLower() + "_value]), 2)+pow(m_mm([" + propTh.FullId.ToLower() + "_value])/2, 2) ))");
			bopp2.Object3d.SetPropertyFormula("D3", "mm_m((m_mm([" + propH.FullId.ToLower() + "_value])+" + enfoncement + ")/sin(" + "[" + propAng.FullId.ToLower() + "_value]*3.14159265/180)+500)");



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
			bopp6.Object3d.SetPropertyFormula("D3", "mm_m( (m_mm([" + propH.FullId.ToLower() + "_value])+" + enfoncement + ") /sin(" + "[" + propAng2.FullId.ToLower() + "_value]*3.14159265/180) +500)");



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

			bopp4.Position.SetPropertyFormula("Y", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value])+" + enfoncement + ")");
			bopp4.Position.SetPropertyFormula("Z", "mm_m(m_mm(-[" + propTh.FullId.ToLower() + "_value]))");
			bopp4.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propH2.FullId.ToLower() + "_value]*3))");

			shape.Position.SetPropertyFormula("X", "mm_m(m_mm(-[" + propLen.FullId.ToLower() + "_value]-[" + propH3.FullId.ToLower() + "_value]))");



		}


		public void updateNoseCone(Equipment noseCone, ModelEventInfo info)
		{



			IdmCic.API.Model.IdmProperties.Property propRadius = noseCone.GetProperty("noseconeR");
			IdmCic.API.Model.IdmProperties.Property propHeight = noseCone.GetProperty("noseconeH");
			IdmCic.API.Model.IdmProperties.Property propTh = noseCone.GetProperty("noseconeTh");

			if (propRadius is null || propHeight is null || noseCone.Shapes.First() is null)
				return;

			if (swUtil.isAvailable())
			{
				swUtil.updateSWNoseFile((double)propRadius.Value * 1000, (double)propHeight.Value * 1000, (double)propTh.Value * 1000);
				var fileName = folderPath + "nosecone2.STEP";
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

	}
}