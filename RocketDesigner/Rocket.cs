using IdmCic.API.Model.Mainsystem;
using IdmCic.API.Model.Subsystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Rocket
	{
		private Engine engine;
		private Nosecone nose;
		public Dictionary<string, RocketElement> elems;
		public List<RocketElement> tanks;

		Assembly assembly;
		double totalLen;
		int finCount;
		double radius;
		public string version;
		public Rocket()
		{
			elems = new Dictionary<string, RocketElement>();
			tanks = new List<RocketElement>();
		}

		public void addElement(RocketElement element)
		{
			elems[element.Name] = element;
		}

		public Engine getEngine()
		{
			return engine;
		}

		public RocketElement getElement(string name)
		{
			return elems[name];
		}

		internal void setNose(Nosecone n)
		{
			this.nose = n;
		}

		public Nosecone getNosecone()
		{
			return nose;
		}
		public double getRadius()
		{
			return radius;
		}
		public double toInch(double m)
		{
			return Math.Round(m * 39.3701, 3);
		}
		public Assembly getRocketAssembly()
		{
			return assembly;
		}
		public double getLen()
		{
			return totalLen;
		}

		public int getFinCount()
		{
			return finCount;
		}


		public string generateOpenRocketFile(string path,string name)
		{
			string filename = path;
			XmlTextWriter xmlTextWriter2 = new XmlTextWriter(filename, null);
			

			xmlTextWriter2.Formatting = Formatting.Indented;

			xmlTextWriter2.WriteStartDocument();

			xmlTextWriter2.WriteStartElement("openrocket");
			xmlTextWriter2.WriteAttributeString("version","1.4");
			xmlTextWriter2.WriteAttributeString("creator", "OpenRocket 15.03dev");
			xmlTextWriter2.WriteStartElement("rocket");

			xmlTextWriter2.WriteElementString("name", name);
			//xmlTextWriter2.WriteElementString("motorconfiguration", "Sheet Metal");
			xmlTextWriter2.WriteElementString("referencetype", "maximum");


			xmlTextWriter2.WriteStartElement("subcomponents");
			xmlTextWriter2.WriteStartElement("stage");

			xmlTextWriter2.WriteElementString("name", "stage 1");

			xmlTextWriter2.WriteStartElement("subcomponents");

			RocketElement e = nose;
			e.WriteToOpenRocket(xmlTextWriter2);
			while (e.Bot != null)
			{
				e = e.Bot;
				e.WriteToOpenRocket(xmlTextWriter2);
				if (e.SideAttach.Count > 0)
				{
					finCount = e.SideAttach.Count;
				}
			}
			xmlTextWriter2.WriteEndElement();//sub


			xmlTextWriter2.WriteEndElement();//stage

			xmlTextWriter2.WriteEndElement();//sub

			xmlTextWriter2.WriteEndElement();//rocket

			xmlTextWriter2.WriteStartElement("simulations");
			xmlTextWriter2.WriteEndElement();

			xmlTextWriter2.WriteEndElement();//openrocket

			xmlTextWriter2.WriteEndDocument();
			xmlTextWriter2.Close();

			return filename;
		}


		public string generateXMLFile(string name)
		{
			string filename = Path.Combine(Main.folderPath + name + ".CDX1");
			XmlTextWriter xmlTextWriter2 = new XmlTextWriter(filename, null);


			xmlTextWriter2.Formatting = Formatting.Indented;

			xmlTextWriter2.WriteStartDocument();

			xmlTextWriter2.WriteStartElement("RASAeroDocument");
			xmlTextWriter2.WriteElementString("FileVersion", "2");
			xmlTextWriter2.WriteStartElement("RocketDesign");


			RocketElement e = nose;
			e.WriteToXML(xmlTextWriter2);
			while (e.Bot != null)
			{
				e = e.Bot;
				e.WriteToXML(xmlTextWriter2);
				if (e.SideAttach.Count > 0)
				{
					finCount = e.SideAttach.Count;
				}
			}

			//OTHER
			xmlTextWriter2.WriteElementString("Surface", "Sheet Metal");
			xmlTextWriter2.WriteElementString("CP", "0");
			xmlTextWriter2.WriteElementString("ModifiedBarrowman", "True");
			xmlTextWriter2.WriteElementString("Turbulence", "False");
			xmlTextWriter2.WriteElementString("SustainerNozzle", "0");
			xmlTextWriter2.WriteElementString("Booster1Nozzle", "0");
			xmlTextWriter2.WriteElementString("Booster2Nozzle", "0");
			xmlTextWriter2.WriteElementString("UseBooster1", "False");
			xmlTextWriter2.WriteElementString("UseBooster2", "False");
			xmlTextWriter2.WriteElementString("Comments", "");
			xmlTextWriter2.WriteEndElement();
			//LAUNCH CONF
			xmlTextWriter2.WriteStartElement("LaunchSite");
			xmlTextWriter2.WriteElementString("Altitude", "0");
			xmlTextWriter2.WriteElementString("Pressure", "0");
			xmlTextWriter2.WriteElementString("RodAngle", "0");
			xmlTextWriter2.WriteElementString("RodLength", "4");
			xmlTextWriter2.WriteElementString("Temperature", "74"); //Farh
			xmlTextWriter2.WriteElementString("WindSpeed", "0");
			xmlTextWriter2.WriteEndElement();
			//RECOVERY
			xmlTextWriter2.WriteStartElement("Recovery");
			xmlTextWriter2.WriteElementString("Altitude1", "1000");
			xmlTextWriter2.WriteElementString("Altitude2", "1000");
			xmlTextWriter2.WriteElementString("DeviceType1", "None");
			xmlTextWriter2.WriteElementString("DeviceType2", "None");
			xmlTextWriter2.WriteElementString("Event1", "False");
			xmlTextWriter2.WriteElementString("Event2", "False");
			xmlTextWriter2.WriteElementString("Size1", "1");
			xmlTextWriter2.WriteElementString("Size2", "1");
			xmlTextWriter2.WriteElementString("EventType1", "None");
			xmlTextWriter2.WriteElementString("EventType2", "None");
			xmlTextWriter2.WriteElementString("CD1", "1,33");
			xmlTextWriter2.WriteElementString("CD2", "1,33");
			xmlTextWriter2.WriteEndElement();

			xmlTextWriter2.WriteEndElement();

			xmlTextWriter2.WriteEndDocument();
			xmlTextWriter2.Close();

			return filename;

		}

		internal static Rocket getRocketFromElement(RelatedSubsystem ss)
		{

			IdmCic.API.Model.Subsystems.Assembly rocket = null;

			string vers = "";
			foreach (IdmCic.API.Model.Subsystems.Assembly e in ss.Assemblies.ToList())
			{
				if (e.GetProperty("Rocket") != null)
				{
					rocket = e;
					if(e.GetProperty("Version") != null)
						vers = (string)e.GetProperty("Version").Value;
				}
			}
			if (rocket is null)
				return null;

			Rocket r = new Rocket();
			r.version = vers;
			r.radius = 0;
			r.assembly = rocket;
			foreach (EquipmentInstance ei in rocket.EquipmentInstances.ToList())
			{
				Equipment e = ei.Equipment;
				if (e.GetProperty("RocketBody") != null)
				{
					Body b = new Body((double)e.GetProperty("bodyH").Value, (double)e.GetProperty("bodyFinPos").Value, (double)e.GetProperty("bodyR").Value, (double)e.GetProperty("bodyDe").Value, (double)e.GetProperty("bodyTh").Value);
					b.Name = e.Name;
					if(b.radius > r.radius)
						r.radius = b.radius;
					r.addElement(b);
				}
				else if (e.GetProperty("RocketFin") != null)
				{
					double sweep = ((double)e.GetProperty("finh").Value) / Math.Tan((double)e.GetProperty("finA1").Value * Math.PI / 180);

					double invsweep = ((double)e.GetProperty("finh").Value) / Math.Tan((180 - (double)e.GetProperty("finA2").Value) * Math.PI / 180);

					Fin fi = new Fin((double)e.GetProperty("finl").Value, (double)e.GetProperty("finh").Value, sweep, (double)e.GetProperty("finl").Value - sweep - invsweep, (double)e.GetProperty("finth").Value, 0, 1, 1, "Hexagonal", 0, (double)e.GetProperty("finDe").Value);
					fi.Name = e.Name;
					r.addElement(fi);
				}
				else if (e.GetProperty("RocketNoseCone") != null)
				{
					Nosecone n = new Nosecone(Nosecone.NoseConeShape.Tangent, (double)e.GetProperty("noseconeH").Value, (double)e.GetProperty("noseconeR").Value, 0, (double)e.GetProperty("noseconeDe").Value, (double)e.GetProperty("noseconeTh").Value);
					n.Name = e.Name;
					if (n.radius > r.radius)
						r.radius = n.radius;
					r.addElement(n);
					r.setNose(n);
				}
				else if (e.GetProperty("RocketTransition") != null)
				{
					Transition tr = new Transition((double)e.GetProperty("transiH").Value, (double)e.GetProperty("transiTopR").Value, (double)e.GetProperty("transiBotR").Value, (double)e.GetProperty("transiDe").Value, (double)e.GetProperty("transiTh").Value);
					tr.Name = e.Name;
					r.addElement(tr);
				}
				else if (e.GetProperty("RocketEngine") != null)
				{
					if (e.GetDocument("1") != null)
					{
						Engine eng = new Engine(e.GetDocument("1").GetFullFilePath(), (double)e.GetProperty("engNr").Value, (double)e.GetProperty("engRMix").Value, (double)e.GetProperty("engISP").Value);
						eng.Name = e.Name;
						r.addElement(eng);
						r.engine = eng;
					}
				}
				else if (e.GetProperty("RocketSTank") != null)
				{
					double x=0, y=0, z=0;
					foreach(Shape s in e.Shapes)
                    {
						if (s.Name == "fuel")
                        {
							x = s.Position.X;
							y = s.Position.Y;
							z = s.Position.Z;
						}
                    }
					SolidTank b = new SolidTank((double)e.GetProperty("StankRad").Value, (double)e.GetProperty("StankH").Value, (double)e.GetProperty("StankTh").Value, (double)e.GetProperty("StankDe").Value, (double)e.GetProperty("StankMass").Value, (double)e.GetProperty("StankMaxMass").Value, ei.GetAbsolutePosition().X+x, ei.GetAbsolutePosition().Y+y, ei.GetAbsolutePosition().Z+z, (int)e.GetProperty("StankType").Value);
					b.Name = e.Name;
					r.addElement(b);
					r.tanks.Add(b);
				}
				else if (e.GetProperty("RocketLTank") != null)
				{
					double x = 0, y = 0, z = 0;
					foreach (Shape s in e.Shapes)
					{
						if (s.Name == "fuel")
						{
							x = s.Position.X;
							y = s.Position.Y;
							z = s.Position.Z;
						}
					}
					LiquidTank b = new LiquidTank((double)e.GetProperty("LtankRad").Value, (double)e.GetProperty("LtankH").Value, (double)e.GetProperty("LtankTh").Value, (double)e.GetProperty("LtankDe").Value, (double)e.GetProperty("LtankMass").Value, (double)e.GetProperty("LtankMaxMass").Value, ei.GetAbsolutePosition().X+x, ei.GetAbsolutePosition().Y+y, ei.GetAbsolutePosition().Z+z, (int)e.GetProperty("LtankType").Value);
					b.Name = e.Name;
					r.addElement(b);
					r.tanks.Add(b);
				}
			}
			foreach (EquipmentInstance ei in rocket.EquipmentInstances.ToList())
			{
				try
				{
					RocketElement el1 = r.getElement(ei.Equipment.Name);
					if (typeof(Nosecone).IsInstanceOfType(el1))
						continue;
					if (typeof(Fin).IsInstanceOfType(el1))
					{
						string attachedTo = ((EquipmentInstance)ei.ParentCoordinateSystem.Parent).Equipment.Name;
						RocketElement el2 = r.getElement(attachedTo);
						el2.addSideAttach(el1);
					}
					else if (typeof(LiquidTank).IsInstanceOfType(el1) || typeof(SolidTank).IsInstanceOfType(el1))
					{
						string attachedTo = ((EquipmentInstance)ei.ParentCoordinateSystem.Parent).Equipment.Name;
						RocketElement el2 = r.getElement(attachedTo);
						el1.Top = el2;
						//el2.Bot = el1;  (faut trouver une solution à ce probleme)
					}
					else
					{
						string attachedTo = ((EquipmentInstance)ei.ParentCoordinateSystem.Parent).Equipment.Name;
						RocketElement el2 = r.getElement(attachedTo);
						el1.Top = el2;
						el2.Bot = el1;
					}
				}
				catch (Exception e)
				{

				}
			}
			RocketElement el = r.getNosecone();
			while (el.Bot != null)
			{
				el.Bot.Loc = el.Loc + el.Len;
				el = el.Bot;
			}
			r.totalLen = el.Loc + el.Len;
			return r;
		}
	}
}
