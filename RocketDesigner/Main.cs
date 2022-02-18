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

		/*
		 * path to change in RAS aero : L ; ar (2*) ;
		 */


		public static string folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\RocketDesigner\\");
		public static string version = "0.5.5";


		private PluginAction calculateAero;
		private PluginAction openRas;
		private PluginAction createOpenRocket;
		private PluginAction create2D;
		private PluginAction create3D;
		private PluginAction testCheck;
		private PluginAction launchSimu;
		private PluginAction launchSimu6ddl;
		private PluginAction generateBatch;
		private PluginAction optimizeFin;

		private PluginButton calculateAeroBtn;
		private PluginButton OpenRASBtn;
		private PluginButton createOpenRocketBtn;
		private PluginButton create2DBtn;
		private PluginButton create3DBtn;
		private PluginButton launchSimuBtn;
		private PluginButton launchSimu6ddlBtn;
		private PluginButton generateBatchBtn;
		private PluginButton optimizeFinBtn;
		private PluginCheckBox checkbox;

		private PluginObjectAction NoseConeAddAction;
		private PluginObjectAction FinsAddAction;
		private PluginObjectAction BodyAddAction;
		private PluginObjectAction TransiAddAction;
		private PluginObjectAction AssemblyAddAction;
		private PluginObjectAction EngineAddAction;
		private PluginObjectAction EngineFileAddAction;
		private PluginObjectAction LtankAddAction;
		private PluginObjectAction StankAddAction;

		private PluginObjectButton NoseConeAddButton;
		private PluginObjectButton FinsAddButton;
		private PluginObjectButton BodyAddButton;
		private PluginObjectButton TransiAddButton;
		private PluginObjectButton AssemblyAddButton;
		private PluginObjectButton EngineAddButton;
		private PluginObjectButton EngineFileAddButton;
		private PluginObjectButton LtankAddButton;
		private PluginObjectButton StankAddButton;

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
			yield return LtankAddAction;
			yield return StankAddAction;
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
			yield return LtankAddButton;
			yield return StankAddButton;
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
			yield return launchSimu6ddl;
			yield return generateBatch;
			yield return optimizeFin;
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
			yield return launchSimu6ddlBtn;
			yield return generateBatchBtn;
			yield return optimizeFinBtn;
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
			get { return version; }
		}

		Excel.Workbook aero;

		Aerodynamics aerodynamics;
		SolidWorksUtil swUtil;
		Matlab matlabUtil;
		ComponentCreator compCreator;
		public override void Initialise()
		{

			aerodynamics = new Aerodynamics();
			calculateAero = new PluginAction("calculateAero", calculateAeroImpl);
			openRas = new PluginAction("openRas", openRASImpl);
			createOpenRocket = new PluginAction("createOpenRocket", createOpenRocketImpl);
			create2D = new PluginAction("create2d", create2DImpl);
			create3D = new PluginAction("create3d", create3DImpl);
			launchSimu = new PluginAction("launchSimu", launchSimuImpl);
			launchSimu6ddl = new PluginAction("launchSimu6ddl", launchSimu6ddlImpl);
			testCheck = new PluginAction("test_action", SolidworksVisibleImpl);
			generateBatch = new PluginAction("generateBatch", generateBatchImpl);
			optimizeFin = new PluginAction("optimizeFin", optimizeImpl);

			calculateAeroBtn = new PluginButton("calculateAero_button", "Calculate aero coef", calculateAero, "Rocket Designer", "Aerodynamics");
			OpenRASBtn = new PluginButton("openRas_button", "Open RASAero II", openRas, "Rocket Designer", "File Creator");
			createOpenRocketBtn = new PluginButton("createOpenRocket_button", "Create OpenRocket File", createOpenRocket, "Rocket Designer", "File Creator");
			create2DBtn = new PluginButton("create2d_button", "Create 2D Sketch", create2D, "Rocket Designer", "File Creator");
			create3DBtn = new PluginButton("create2d_button", "Create 3D Sketch", create3D, "Rocket Designer", "File Creator");
			checkbox = new PluginCheckBox("sw_visible", "SolidWorks Visible", testCheck, "Rocket Designer", "Other");
			launchSimuBtn = new PluginButton("launchSimu_button", "Launch Simulation 3DDL", launchSimu, "Rocket Designer", "Aerodynamics");
			launchSimu6ddlBtn = new PluginButton("launchSimu_button", "Launch Simulation 6DDL", launchSimu6ddl, "Rocket Designer", "Aerodynamics");
			generateBatchBtn = new PluginButton("generateBatch_button", "Generate Data", generateBatch, "Rocket Designer", "Data Analysis");
			optimizeFinBtn = new PluginButton("optimizeFin_button", "Optimize Fin", optimizeFin, "Rocket Designer", "Data Analysis");
			//IdmCic_tab



			swUtil = new SolidWorksUtil();
			matlabUtil = new Matlab();
			compCreator = new ComponentCreator();

			bool swavailable = swUtil.isInstalled();
			bool matlabAvailable = matlabUtil.isInstalled();

			calculateAeroBtn.IsVisibleAtStartUp = false;
			calculateAeroBtn.IsVisibleAfterLoadingMainSystem = true;
			calculateAeroBtn.LargeStyle = true;

			checkbox.IsVisibleAtStartUp = false;
			checkbox.IsVisibleAfterLoadingMainSystem = swavailable;
			checkbox.Checked = false;

			OpenRASBtn.IsVisibleAtStartUp = false;
			OpenRASBtn.IsVisibleAfterLoadingMainSystem = true;
			OpenRASBtn.LargeStyle = true;

			createOpenRocketBtn.IsVisibleAtStartUp = false;
			createOpenRocketBtn.IsVisibleAfterLoadingMainSystem = true;
			createOpenRocketBtn.LargeStyle = true;

			create2DBtn.IsVisibleAtStartUp = false;
			create2DBtn.IsVisibleAfterLoadingMainSystem = swavailable;
			create2DBtn.LargeStyle = true;

			create3DBtn.IsVisibleAtStartUp = false;
			create3DBtn.IsVisibleAfterLoadingMainSystem = swavailable;
			create3DBtn.LargeStyle = true;


			launchSimuBtn.IsVisibleAtStartUp = false;
			launchSimuBtn.IsVisibleAfterLoadingMainSystem = matlabAvailable;
			launchSimuBtn.LargeStyle = true;

			launchSimu6ddlBtn.IsVisibleAtStartUp = false;
			launchSimu6ddlBtn.IsVisibleAfterLoadingMainSystem = matlabAvailable;
			launchSimu6ddlBtn.LargeStyle = true;

			generateBatchBtn.IsVisibleAtStartUp = false;
			generateBatchBtn.IsVisibleAfterLoadingMainSystem = matlabAvailable;
			generateBatchBtn.LargeStyle = true;

			optimizeFinBtn.IsVisibleAtStartUp = false;
			optimizeFinBtn.IsVisibleAfterLoadingMainSystem = true;
			optimizeFinBtn.LargeStyle = true;

			/*
			Excel.Application oXL;
			oXL = new Excel.Application();

			aero = oXL.Workbooks.Open("C:\\Users\\cgene\\AppData\\Roaming\\idmcic_data\\plugins\\test\\BaseAérodynamique_Finale.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing, Type.Missing, Type.Missing,
													Type.Missing, Type.Missing);
			oXL.Visible = false;
			*/

			NoseConeAddAction = new PluginObjectAction("nosecone_add_action", typeof(Equipment), compCreator.NoseConeAddActionImpl);
			FinsAddAction = new PluginObjectAction("fins_add_action", typeof(Equipment), compCreator.FinsAddActionImpl);
			BodyAddAction = new PluginObjectAction("body_add_action", typeof(Equipment), compCreator.BodyAddActionImpl);
			TransiAddAction = new PluginObjectAction("transi_add_action", typeof(Equipment), compCreator.TransitionAddActionImpl);
			AssemblyAddAction = new PluginObjectAction("assembly_add_action", typeof(Assembly), compCreator.AssemblyAddActionImpl);
			EngineAddAction = new PluginObjectAction("engine_add_action", typeof(Equipment), compCreator.NozzleAddActionImpl);
			EngineFileAddAction = new PluginObjectAction("engine_file_add_action", typeof(Equipment), compCreator.EngineFileAddActionImpl);
			LtankAddAction = new PluginObjectAction("liquid_tank_add_action", typeof(Equipment), compCreator.LiquidTankAddActionImpl);
			StankAddAction = new PluginObjectAction("solid_tank_add_action", typeof(Equipment), compCreator.SolidTankAddActionImpl);

			NoseConeAddButton = new PluginObjectButton("nosecone_add_button", "Add Rocket Nose cone", NoseConeAddAction);
			FinsAddButton = new PluginObjectButton("fins_add_button", "Add Rocket Fins", FinsAddAction);
			BodyAddButton = new PluginObjectButton("body_add_button", "Add Rocket Body", BodyAddAction);
			TransiAddButton = new PluginObjectButton("transi_add_button", "Add Rocket Transition", TransiAddAction);
			AssemblyAddButton = new PluginObjectButton("assembly_add_button", "Set As Rocket Assembly", AssemblyAddAction);
			EngineAddButton = new PluginObjectButton("engine_add_button", "Add Rocket Nozzle", EngineAddAction);
			EngineFileAddButton = new PluginObjectButton("engine_file_add_button", "set Engine .eng File", EngineFileAddAction);
			LtankAddButton = new PluginObjectButton("liquid_tank_add_button", "Add Liquid Tank", LtankAddAction);
			StankAddButton = new PluginObjectButton("solid_tank_add_button", "Add Solid Tank", StankAddAction);

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
						aerodynamics.startSimu3DDL(e,r,matlabUtil, true, true);
					}
				}
			}
		}
		public void launchSimu6ddlImpl(PluginActionArgs args)
		{
			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{
					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						calculAeroCoef(args, false);
						aerodynamics.startSimu6DDL(e, r, matlabUtil);
					}
				}
			}
		}

		public void optimizeImpl(PluginActionArgs args)
        {
			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{
					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						Optimize f = new Optimize();
						f.ShowDialog();
						if (f.cancel)
							return;
						
						Datagen g = new Datagen(matlabUtil, aerodynamics);

						double[] result = g.optimizeFin(r,e,f.ms,f.pop,f.keep,f.epoch);
						if (result == null)
						{
							MessageBox.Show("no fin profile satisfies the minimum static margin");
						}
						else
						{
							MessageBox.Show("sweep : " + result[0] + "\n" + "tipchord : " + result[1] + "\n" + "thickness : " + result[2] + "\n" + "chord : " + result[3] + "\n" + "position : " + result[4] + "\n" + "span : " + result[5]);
							MessageBox.Show("Optimized max alt : " + result[6] + " m / initial max alt : "+result[8]+" m\n" + "Optimized min Ms : " + result[7] +" cal / initial min Ms : "+result[9]+" cal\n");

							Fin fin = getFin(r);
						
							fin.span = result[5];
							fin.sweepDist = result[0];
							fin.TipChord = result[1];
							fin.thickness = result[2];
							fin.chord = result[3];
							//fin.Loc = result[4];

							string filec = r.generateXMLFile(e.Name.Replace(" ", ""));
							System.IO.File.Copy(filec, folderPath + "optimized.CDX1", true);
							string assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
							string exefile = Path.Combine(assemblyFolder, "RAS2.exe");
							Process p = new Process();
							p.StartInfo = new ProcessStartInfo(exefile);
							p.StartInfo.WorkingDirectory = assemblyFolder;
							p.StartInfo.Arguments = folderPath + "optimized.CDX1";
							p.Start();
							DialogResult dialogResult = MessageBox.Show("Apply Optimisation ?", "Optimisation", MessageBoxButtons.YesNo);
							if(dialogResult == DialogResult.Yes)
								applyFinParameter(fin.span, fin.sweepDist, fin.TipChord, fin.chord, fin.Loc, s);
						}
					}
				}
			}
		}

		public void applyFinParameter(double span, double sweep, double tipchord, double chord, double loc, RelatedSubsystem r)
		{
			
			Assembly rocket = null;
			foreach (IdmCic.API.Model.Subsystems.Assembly e in r.Assemblies.ToList())
			{
				if (e.GetProperty("Rocket") != null)
				{
					rocket = e;
				}
			}
			if (rocket != null)
			{
				foreach (EquipmentInstance ei in rocket.EquipmentInstances.ToList())
				{
					Equipment e = ei.Equipment;

					if (e.GetProperty("RocketFin") != null)
					{
						double alpha = Math.Atan(span / sweep) * 180 / Math.PI;
						double beta = Math.Atan2(span, -chord + sweep + tipchord) * 180 / Math.PI;
						e.GetProperty("finh").Value = span;
						e.GetProperty("fina1").Value = alpha;
						e.GetProperty("fina2").Value = beta;
						e.GetProperty("finl").Value = chord;
						return; //evite de faire tout les ailerons
					}
				}
			}
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

		public void generateBatchImpl(PluginActionArgs args)
		{
			foreach (Element e in args.MainSystem.Elements)
			{
				foreach (RelatedSubsystem s in e.RelatedSubsystems)
				{
					Rocket r = Rocket.getRocketFromElement(s);
					if (r != null)
					{
						BatchGenerator f = new BatchGenerator();
						f.ShowDialog();
						if (f.cancel)
							return;

						Datagen g = new Datagen(matlabUtil, aerodynamics);

						ParametersEnum.Parameters[] param = { (ParametersEnum.Parameters)f.p1, (ParametersEnum.Parameters)f.p2 };

						double[,] lim = new double[2, 2];
						lim[0, 1] = f.max1;
						lim[0, 0] = f.min1;
						lim[1, 1] = f.max2;
						lim[1, 0] = f.min2;
						g.getResult(r, param, lim, f.nbr, e,f.machnbr, f.getShow(),f.distrib);

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





		

		public override void ApplicationQuit()
		{

			//aero.Close(false);

			swUtil.close();
			matlabUtil.close();



			//MessageBox.Show("excel closed");
		}




		public void updateNoseCone(Equipment noseCone, ModelEventInfo info)
		{



			IdmCic.API.Model.IdmProperties.Property propRadius = noseCone.GetProperty("noseconeR");
			IdmCic.API.Model.IdmProperties.Property propHeight = noseCone.GetProperty("noseconeH");
			IdmCic.API.Model.IdmProperties.Property propTh = noseCone.GetProperty("noseconeTh");

			IdmCic.API.Model.IdmProperties.Property propType = noseCone.GetProperty("noseconeTy");
			int type = 0;
			if (propType != null)
            {
				type = (int)propType.Value;

			}



			if (propRadius is null || propHeight is null || noseCone.Shapes.First() is null)
				return;

			if (swUtil.loadSW())
			{
				swUtil.updateSWNoseFile((double)propRadius.Value * 1000, (double)propHeight.Value * 1000, (double)propTh.Value * 1000,type);
				var fileName = folderPath + "nosecone2.STEP";
				((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D2 = 1;
				((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D3 = 1;
				((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D1 = 1;
				((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).SetStepObjectFromFile(fileName);

				using (StreamWriter sw = File.CreateText(folderPath + "noseconeDim.txt"))
				{
					sw.WriteLine(((double)propRadius.Value * 1000) + ";" + ((double)propHeight.Value * 1000));
				}
				if (noseCone.GetDocument("1") != null)
				{
					noseCone.GetDocument("1").ChangeFileBy(folderPath + "noseconeDim.txt");
				}
				else
				{
					DocumentFile file = noseCone.AddDocumentFromFile(folderPath + "noseconeDim.txt");
				}
			}
			else
			{
				double r = 0.3;
				double h = 1;
				if (noseCone.GetDocument("1") != null) { 
					string dim = File.ReadAllText(noseCone.GetDocument("1").GetFullFilePath());
					r = safeParse(dim.Split(';')[0]) / 1000;
					h = safeParse(dim.Split(';')[1]) / 1000;
				}
				if (info.FullPropertyPath.Contains("noseconeR"))
				{
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D2 = ((double)info.NewValue / (double)r);
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D3 = ((double)info.NewValue / (double)r);
				}
				else if (info.FullPropertyPath.Contains("noseconeH"))
				{
					((IdmCic.API.Model.Physics.Objects3D.Miscs.Step)noseCone.Shapes.First().ShapeDefinition).D1 = ((double)info.NewValue / (double)h);
				}

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

	}
}