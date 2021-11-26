using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Body : RocketElement
	{
		public Body(double len, double finl, double radius, double density, double thickness)
		{
			this.finLoc = finl;
			Len = len;
			this.radius = radius;
			this.density = density;
			this.thickness = thickness;
		}
		public double density;
		public double finLoc;
		public double radius;
		public double thickness;

		public override void WriteToXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("BodyTube");
			writer.WriteElementString("PartType", "BodyTube");
			writer.WriteElementString("Length", toInch(Len).ToString());
			writer.WriteElementString("Diameter", toInch(radius * 2).ToString());
			writer.WriteElementString("LaunchLugDiameter", "0");
			writer.WriteElementString("LaunchLugLength", "0");
			writer.WriteElementString("RailGuideDiameter", "0");
			writer.WriteElementString("RailGuideHeight", "0");
			writer.WriteElementString("LaunchShoeArea", "0");
			writer.WriteElementString("Location", toInch(Loc).ToString());
			writer.WriteElementString("Color", "Black");
			writer.WriteElementString("BoattailLength", "0");
			writer.WriteElementString("BoattailRearDiameter", "0");
			writer.WriteElementString("BoattailOffset", "0");
			writer.WriteElementString("Overhang", "0");
			if (SideAttach.Count > 0)
			{
				((Fin)SideAttach.First()).WriteToXML(writer, SideAttach.Count, finLoc);
			}
			writer.WriteEndElement();
		}

		public override void WriteToOpenRocket(XmlTextWriter writer)
		{

			writer.WriteStartElement("bodytube");
			writer.WriteElementString("name", "Body");
			writer.WriteElementString("finish", "normal");

			writer.WriteStartElement("material");
			writer.WriteAttributeString("type", "bulk");
			writer.WriteAttributeString("density", density.ToString().Replace(",", "."));
			writer.WriteString("Body Material");
			writer.WriteEndElement();

			writer.WriteElementString("length", (Len).ToString().Replace(",", "."));
			writer.WriteElementString("thickness", (thickness).ToString().Replace(",", "."));
			writer.WriteElementString("radius", "auto");
			writer.WriteStartElement("motormount");
			writer.WriteElementString("ignitionevent", "automatic");
			writer.WriteElementString("ignitiondelay", "0.0");
			writer.WriteElementString("overhang", "-0.0");
			writer.WriteEndElement();
			
			if (SideAttach.Count > 0)
			{
				writer.WriteStartElement("subcomponents");
				((Fin)SideAttach.First()).WriteToOpenRocket(writer, SideAttach.Count, finLoc);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
	}
}
