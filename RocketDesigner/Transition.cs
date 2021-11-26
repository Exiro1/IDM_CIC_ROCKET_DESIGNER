using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Transition : RocketElement
	{
		public Transition(double len, double radiusUp,double radiusDown, double density, double thickness)
		{
			Len = len;
			this.radiusUp = radiusUp;
			this.radiusDown = radiusDown;
			this.density = density;
			this.thickness = thickness;
		}
		public double density;
		public double thickness;
		public double radiusUp;
		public double radiusDown;

		public override void WriteToXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("Transition");
			writer.WriteElementString("PartType", "Transition");
			writer.WriteElementString("Length", toInch(Len).ToString());
			writer.WriteElementString("Diameter", toInch(radiusDown * 2).ToString());
			writer.WriteElementString("Location", toInch(Loc).ToString());
			writer.WriteElementString("RearDiameter", toInch(radiusDown * 2).ToString());
			writer.WriteElementString("Color", "Black");
			writer.WriteEndElement();
		}

		public override void WriteToOpenRocket(XmlTextWriter writer)
		{

			writer.WriteStartElement("transition");
			writer.WriteElementString("name", "Transition");
			writer.WriteElementString("finish", "normal");

			writer.WriteStartElement("material");
			writer.WriteAttributeString("type", "bulk");
			writer.WriteAttributeString("density", density.ToString().Replace(",", "."));
			writer.WriteString("Body Material");
			writer.WriteEndElement();

			writer.WriteElementString("length", (Len).ToString().Replace(",", "."));
			writer.WriteElementString("thickness", (thickness).ToString().Replace(",", "."));
			writer.WriteElementString("shape", "conical");
			writer.WriteElementString("foreradius", "auto");
			writer.WriteElementString("aftradius", (radiusDown).ToString().Replace(",", "."));

			writer.WriteElementString("foreshoulderradius", "0.0");
			writer.WriteElementString("foreshoulderlength", "0.0");
			writer.WriteElementString("foreshoulderthickness", "0.0");
			writer.WriteElementString("foreshouldercapped", "false");

			writer.WriteElementString("aftshoulderradius", "0.0");
			writer.WriteElementString("aftshoulderlength", "0.0");
			writer.WriteElementString("aftshoulderthickness", "0.0");
			writer.WriteElementString("aftshouldercapped", "false");

			writer.WriteEndElement();
		}
	}
}
