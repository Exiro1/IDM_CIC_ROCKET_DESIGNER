using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Nosecone : RocketElement
	{

		public enum NoseConeShape { Tangent }

		public Nosecone(NoseConeShape shape, double len, double radius, double bluntRadius)
		{
			this.shape = shape;
			Len = len;
			this.radius = radius;
			this.bluntRadius = bluntRadius;
			Loc = 0;
		}

		public NoseConeShape shape;
		public double radius;
		public double bluntRadius;

		public override void WriteToXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("NoseCone");
			writer.WriteElementString("PartType", "NoseCone");
			writer.WriteElementString("Length", toInch(Len).ToString());
			writer.WriteElementString("Diameter", toInch(radius * 2).ToString());
			writer.WriteElementString("Shape", "Tangent Ogive");
			writer.WriteElementString("BluntRadius", toInch(bluntRadius).ToString());
			writer.WriteElementString("Location", toInch(Loc).ToString());
			writer.WriteElementString("Color", "Black");
			writer.WriteEndElement();
		}
	}
}
