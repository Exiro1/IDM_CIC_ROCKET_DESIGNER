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


		public string generateXMLFile(string name)
		{
			string filename = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "idmcic_data\\plugins\\test\\"+name+".CDX1");
			XmlTextWriter xmlTextWriter2 = new XmlTextWriter(filename, null);


			xmlTextWriter2.Formatting = Formatting.Indented;

			xmlTextWriter2.WriteStartDocument();

			xmlTextWriter2.WriteStartElement("RASAeroDocument");
			xmlTextWriter2.WriteElementString("FileVersion", "2");
			xmlTextWriter2.WriteStartElement("RocketDesign");


			RocketElement e = nose;
			e.WriteToXML(xmlTextWriter2);
			while(e.Bot != null)
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



	}

}
