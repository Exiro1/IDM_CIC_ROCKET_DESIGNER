﻿using IdmCic.API.Model.Mainsystem;
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

		private Nosecone nose;
		public Dictionary<string, RocketElement> elems;
		Assembly assembly;

		public Rocket()
		{
			elems = new Dictionary<string, RocketElement>();
		}

		public void addElement(RocketElement element)
		{
			elems[element.Name] = element;
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

		public double toInch(double m)
		{
			return Math.Round(m * 39.3701, 3);
		}
		public Assembly getRocketAssembly()
		{
			return assembly;
		}


		public string generateXMLFile(string name)
		{
			string filename = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\" + name + ".CDX1");
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


			foreach (IdmCic.API.Model.Subsystems.Assembly e in ss.Assemblies.ToList())
			{
				if (e.GetProperty("Rocket") != null)
				{
					rocket = e;
				}
			}
			if (rocket is null)
				return null;

			Rocket r = new Rocket();
			r.assembly = rocket;
			foreach (EquipmentInstance ei in rocket.EquipmentInstances.ToList())
			{
				Equipment e = ei.Equipment;
				if (e.GetProperty("RocketBody") != null)
				{
					Body b = new Body((double)e.GetProperty("bodyH").Value, (double)e.GetProperty("bodyFinPos").Value, (double)e.GetProperty("bodyR").Value);
					b.Name = e.Name;
					r.addElement(b);
				}
				else if (e.GetProperty("RocketFin") != null)
				{
					double sweep = ((double)e.GetProperty("finh").Value) / Math.Tan((double)e.GetProperty("finA1").Value * Math.PI / 180);

					double invsweep = ((double)e.GetProperty("finh").Value) / Math.Tan((180 - (double)e.GetProperty("finA2").Value) * Math.PI / 180);

					Fin fi = new Fin((double)e.GetProperty("finl").Value, (double)e.GetProperty("finh").Value, sweep, (double)e.GetProperty("finl").Value - sweep - invsweep, (double)e.GetProperty("finth").Value, 0, 1, 1, "Hexagonal", 0);
					fi.Name = e.Name;
					r.addElement(fi);
				}
				else if (e.GetProperty("RocketNoseCone") != null)
				{
					Nosecone n = new Nosecone(Nosecone.NoseConeShape.Tangent, (double)e.GetProperty("noseconeH").Value, (double)e.GetProperty("noseconeR").Value, 0);
					n.Name = e.Name;
					r.addElement(n);
					r.setNose(n);
				}
				else if (e.GetProperty("RocketTransition") != null)
				{
					Transition tr = new Transition((double)e.GetProperty("transiH").Value, (double)e.GetProperty("transiTopR").Value, (double)e.GetProperty("transiBotR").Value);
					tr.Name = e.Name;
					r.addElement(tr);
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
			return r;
		}
	}
}
