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
		public Transition(double len, double radiusUp,double radiusDown)
		{
			Len = len;
			this.radiusUp = radiusUp;
			this.radiusDown = radiusDown;
		}

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
	}
}
