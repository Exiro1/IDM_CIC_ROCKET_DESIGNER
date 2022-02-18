using IdmCic.API.Model.IdmFiles;
using IdmCic.API.Model.Mainsystem;
using IdmCic.API.Model.Physics;
using IdmCic.API.Model.Physics.Objects3D.Solids;
using IdmCic.API.Model.Physics.TopologicalOperations;
using IdmCic.API.Model.Subsystems;
using IdmCic.API.Utils.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocketDesigner
{
    internal class ComponentCreator
    {



		public void EngineFileAddActionImpl(PluginObjectActionArgs args)
		{
			if (((Equipment)args.IdmObject).GetProperty("RocketNozzle") != null)
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

		public void AssemblyAddActionImpl(PluginObjectActionArgs args)
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

			IdmCic.API.Model.IdmProperties.Property propVers = ass.AddProperty("Version", IdmCic.API.Model.IdmProperties.IdmPropertyType.Text);
			propVers.Name = "Version";
			propVers.Value = Main.version;
			propVers.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propType = ass.AddProperty("Rocket", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "Rocket";
			propType.Value = true;
			propType.Hidden = true;


		}
		public string getNewID(RelatedSubsystem ss, string part)
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

		public void NozzleAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketNozzle", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketNozzle";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propTopRadius = ((Equipment)args.IdmObject).AddProperty("nozCr", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTopRadius.Name = "Chamber Radius";
			propTopRadius.Value = 0.290;


			IdmCic.API.Model.IdmProperties.Property propBotRadius = ((Equipment)args.IdmObject).AddProperty("nozNr", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propBotRadius.Name = "Nozzle Radius";
			propBotRadius.Value = 0.15;

			IdmCic.API.Model.IdmProperties.Property propThroat = ((Equipment)args.IdmObject).AddProperty("nozNt", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propThroat.Name = "Nozzle Throat";
			propThroat.Value = 0.075;

			IdmCic.API.Model.IdmProperties.Property propHeight = ((Equipment)args.IdmObject).AddProperty("nozHn", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propHeight.Name = "Height exit side";
			propHeight.Value = 0.2;

			IdmCic.API.Model.IdmProperties.Property propHeight2 = ((Equipment)args.IdmObject).AddProperty("nozHc", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propHeight2.Name = "Height Chamber side";
			propHeight2.Value = 0.3;


			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("nozTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 0.01;

			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("nozDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;

			IdmCic.API.Model.IdmProperties.Property propRmix = ((Equipment)args.IdmObject).AddProperty("nozRMix", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propRmix.Name = "Mixing Ratio";
			propRmix.Value = 6;

			IdmCic.API.Model.IdmProperties.Property propISP = ((Equipment)args.IdmObject).AddProperty("nozISP", IdmCic.API.Model.IdmProperties.IdmPropertyType.DecimalWithoutUnit);
			propISP.Name = "ISP";
			propISP.Value = 260;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCone);
			shape.Name = "RocketNozzleUp";

			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketNozzle");

			HollowCone o1 = (HollowCone)(shape.ShapeDefinition);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propTopRadius.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D2", "mm_m(m_mm([" + propThroat.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propHeight2.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");

			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCone);
			shape.Name = "RocketNozzleDown";
			HollowCone o2 = (HollowCone)(shape.ShapeDefinition);
			o2.SetPropertyFormula("D1", "mm_m(m_mm([" + propThroat.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D2", "mm_m(m_mm([" + propBotRadius.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D3", "mm_m(m_mm([" + propHeight.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			shape.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propHeight2.FullId.ToLower() + "_value]))");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.FilledCylinder);
			shape.Name = "TopNozzle";
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

		public void TransitionAddActionImpl(PluginObjectActionArgs args)
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
		public void BodyAddActionImpl(PluginObjectActionArgs args)
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
		public void NoseConeAddActionImpl(PluginObjectActionArgs args)
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

			IdmCic.API.Model.IdmProperties.Property proType = ((Equipment)args.IdmObject).AddProperty("noseconeTy", IdmCic.API.Model.IdmProperties.IdmPropertyType.IntegerWithoutUnit);
			propDensity.Name = "Type";
			propDensity.Value = 0;


			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketNoseCone");

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Step);
			shape.Name = "NoseConeShape";


			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			var fileName = Main.folderPath + "nosecone.STEP";
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
		public void FinsAddActionImpl(PluginObjectActionArgs args)
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

		//ss.Assemblies.ToList()[0].EquipmentInstances.ToList()[1].GetAbsolutePosition()
		public void SolidTankAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketSTank", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketSolidTank";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propH = ((Equipment)args.IdmObject).AddProperty("StankH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH.Name = "Height";
			propH.Value = 1500 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("StankTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 30 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propTh2 = ((Equipment)args.IdmObject).AddProperty("StankTh2", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh2.Name = "TopBotThickness";
			propTh2.Value = 40 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propRad = ((Equipment)args.IdmObject).AddProperty("StankRad", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propRad.Name = "Radius";
			propRad.Value = 200 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propMass = ((Equipment)args.IdmObject).AddProperty("StankMass", IdmCic.API.Model.IdmProperties.IdmPropertyType.Mass);
			propMass.Name = "Loaded Mass";
			propMass.Value = 0;
			IdmCic.API.Model.IdmProperties.Property propMassMax = ((Equipment)args.IdmObject).AddProperty("StankMaxMass", IdmCic.API.Model.IdmProperties.IdmPropertyType.Mass);
			propMassMax.Name = "Mass capacity";
			propMassMax.Value = 0;
			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("StankDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;
			IdmCic.API.Model.IdmProperties.Property propFtype = ((Equipment)args.IdmObject).AddProperty("StankType", IdmCic.API.Model.IdmProperties.IdmPropertyType.IntegerWithoutUnit);
			propFtype.Name = "Propellant type";
			propFtype.Value = 0;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Topology);
			shape.Name = "RocketSTankShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketSTank");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			BooleanOperation bopp1 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.FilledCylinder);
			FilledCylinder o1 = (FilledCylinder)(bopp1.Object3d);
			bopp1.OperationType = OperationType.Union;
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D3", "mm_m(m_mm([" + propTh2.FullId.ToLower() + "_value]))");

			BooleanOperation bopp2 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.FilledCylinder);
			FilledCylinder o2 = (FilledCylinder)(bopp2.Object3d);
			bopp2.OperationType = OperationType.Union;
			o2.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D3", "mm_m(m_mm([" + propTh2.FullId.ToLower() + "_value]))");
			bopp2.Position.SetPropertyFormula("z", "mm_m(m_mm([" + propTh2.FullId.ToLower() + "_value]+[" + propH.FullId.ToLower() + "_value]))");

			BooleanOperation bopp3 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCylinder);
			HollowCylinder o3 = (HollowCylinder)(bopp3.Object3d);
			bopp3.OperationType = OperationType.Union;
			o3.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o3.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
			o3.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			bopp3.Position.SetPropertyFormula("z", "mm_m(m_mm([" + propTh2.FullId.ToLower() + "_value]))");

			IdmCic.API.Model.Subsystems.Shape fuel = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCylinder);

			HollowCylinder o4 = (HollowCylinder)(fuel.ShapeDefinition);
			o4.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]-[" + propTh.FullId.ToLower() + "_value]))");
			o4.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
			o4.SetPropertyFormula("D4", "mm_m(m_mm(([" + propRad.FullId.ToLower() + "_value]-[" + propTh.FullId.ToLower() + "_value])*(1-sqrt(1-[" + propMass.FullId.ToLower() + "_value]/[" + propMassMax.FullId.ToLower() + "_value]))))");
			fuel.Position.SetPropertyFormula("z", "mm_m(m_mm([" + propTh2.FullId.ToLower() + "_value]))");
		}

		public void LiquidTankAddActionImpl(PluginObjectActionArgs args)
		{
			((Equipment)args.IdmObject).MciDataOrigin = MciDataOrigin.FromGeometry;
			IdmCic.API.Model.IdmProperties.Property propType = ((Equipment)args.IdmObject).AddProperty("RocketLTank", IdmCic.API.Model.IdmProperties.IdmPropertyType.Bool);
			propType.Name = "RocketLiquidTank";
			propType.Value = true;
			propType.Hidden = true;

			IdmCic.API.Model.IdmProperties.Property propH = ((Equipment)args.IdmObject).AddProperty("LtankH", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propH.Name = "Height";
			propH.Value = 1500 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propTh = ((Equipment)args.IdmObject).AddProperty("LtankTh", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propTh.Name = "Thickness";
			propTh.Value = 30 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propRad = ((Equipment)args.IdmObject).AddProperty("LtankRad", IdmCic.API.Model.IdmProperties.IdmPropertyType.Distance);
			propRad.Name = "Radius";
			propRad.Value = 200 * 0.001;
			IdmCic.API.Model.IdmProperties.Property propMass = ((Equipment)args.IdmObject).AddProperty("LtankMass", IdmCic.API.Model.IdmProperties.IdmPropertyType.Mass);
			propMass.Name = "Loaded Mass";
			propMass.Value = 0;
			IdmCic.API.Model.IdmProperties.Property propMassMax = ((Equipment)args.IdmObject).AddProperty("LtankMaxMass", IdmCic.API.Model.IdmProperties.IdmPropertyType.Mass);
			propMassMax.Name = "Mass capacity";
			propMassMax.Value = 0;
			IdmCic.API.Model.IdmProperties.Property propDensity = ((Equipment)args.IdmObject).AddProperty("LtankDe", IdmCic.API.Model.IdmProperties.IdmPropertyType.Density);
			propDensity.Name = "Density";
			propDensity.Value = 1780;
			IdmCic.API.Model.IdmProperties.Property propFtype = ((Equipment)args.IdmObject).AddProperty("LtankType", IdmCic.API.Model.IdmProperties.IdmPropertyType.IntegerWithoutUnit);
			propFtype.Name = "Propellant type";
			propFtype.Value = 0;

			if (((Equipment)args.IdmObject).Shapes.Count > 0)
				return;

			IdmCic.API.Model.Subsystems.Shape shape = ((Equipment)args.IdmObject).AddShape(IdmCic.API.Model.Physics.Objects3D.Object3dType.Topology);
			shape.Name = "RocketLTankShape";
			((Equipment)args.IdmObject).Name = getNewID((RelatedSubsystem)((Equipment)args.IdmObject).Parent, "RocketLTank");
			shape.MassDefinition.SetPropertyFormula("masspercubemeter", "[" + propDensity.FullId.ToLower() + "_value]");
			shape.MassDefinition.MassType = MassType.Volume;

			BooleanOperation bopp1 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowSphere);
			bopp1.OperationType = OperationType.Union;
			HollowSphere o1 = (HollowSphere)(bopp1.Object3d);
			o1.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			o1.SetPropertyFormula("angle1", "180");
			o1.SetPropertyFormula("angle2", "360");
			bopp1.Position.SetPropertyFormula("Rotation1", "90");

			BooleanOperation bopp2 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowSphere);
			HollowSphere o2 = (HollowSphere)(bopp2.Object3d);
			bopp2.OperationType = OperationType.Union;
			o2.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");
			o2.SetPropertyFormula("angle1", "180");
			o2.SetPropertyFormula("angle2", "360");
			bopp2.Position.SetPropertyFormula("z", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]-2*[" + propRad.FullId.ToLower() + "_value]))");
			bopp2.Position.SetPropertyFormula("Rotation1", "-90");

			BooleanOperation bopp3 = ((IdmCic.API.Model.Physics.Objects3D.Miscs.Topology)shape.ShapeDefinition).AddBooleanOperation(IdmCic.API.Model.Physics.Objects3D.Object3dType.HollowCylinder);
			HollowCylinder o3 = (HollowCylinder)(bopp3.Object3d);
			bopp3.OperationType = OperationType.Union;
			o3.SetPropertyFormula("D1", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");
			o3.SetPropertyFormula("D3", "mm_m(m_mm([" + propH.FullId.ToLower() + "_value]))");
			o3.SetPropertyFormula("D4", "mm_m(m_mm([" + propTh.FullId.ToLower() + "_value]))");

			shape.Position.SetPropertyFormula("Z", "mm_m(m_mm([" + propRad.FullId.ToLower() + "_value]))");


		}
	}
}
