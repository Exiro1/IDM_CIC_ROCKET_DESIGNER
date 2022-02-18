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

		public enum NoseConeShape { Tangent, VonKarman }
		
		public Nosecone(NoseConeShape shape, double len, double radius, double bluntRadius, double density,double thickness)
		{
			this.shape = shape;
			Len = len;
			this.radius = radius;
			this.bluntRadius = bluntRadius;
			this.density = density;
			this.thickness = thickness;
			Loc = 0;
		}

		public string ShapeToString(NoseConeShape shape, bool rasaero)
        {
			if (rasaero)
			{
				switch (shape)
				{
					case NoseConeShape.Tangent:
						return "Tangent Ogive";
					case NoseConeShape.VonKarman:
						return "Von Karman Ogive";

					default: return "Tangent Ogive";
				}
            }
            else
            {
				switch (shape)
				{
					case NoseConeShape.Tangent:
						return "ogive";
					case NoseConeShape.VonKarman:
						return "Von Karman Ogive";

					default: return "ogive";
				}
			}
        }


		public NoseConeShape shape;
		public double radius;
		public double bluntRadius;
		public double density;
		public double thickness;

		public override void WriteToXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("NoseCone");
			writer.WriteElementString("PartType", "NoseCone");
			writer.WriteElementString("Length", toInch(Len).ToString());
			writer.WriteElementString("Diameter", toInch(radius * 2).ToString());
			writer.WriteElementString("Shape", ShapeToString(shape,true));
			writer.WriteElementString("BluntRadius", toInch(bluntRadius).ToString());
			writer.WriteElementString("Location", toInch(Loc).ToString());
			writer.WriteElementString("Color", "Black");
			writer.WriteEndElement();
		}

		public override void WriteToOpenRocket(XmlTextWriter writer)
		{

			writer.WriteStartElement("nosecone");
			writer.WriteElementString("name", "Nosecone");
			writer.WriteElementString("finish", "normal");

			writer.WriteStartElement("material");
			writer.WriteAttributeString("type", "bulk");
			writer.WriteAttributeString("density", density.ToString().Replace(",", "."));
			writer.WriteString("Body Material");
			writer.WriteEndElement();

			writer.WriteElementString("length", (Len).ToString().Replace(",", "."));
			writer.WriteElementString("thickness", (thickness).ToString().Replace(",", "."));
			writer.WriteElementString("shape", ShapeToString(shape, false));
			writer.WriteElementString("shapeparameter", "1.0");
			writer.WriteElementString("aftradius", (radius).ToString().Replace(",","."));
			writer.WriteElementString("aftshoulderradius", "0.0");
			writer.WriteElementString("aftshoulderlength", "0.0");
			writer.WriteElementString("aftshoulderthickness", "0.0");
			writer.WriteElementString("aftshouldercapped", "false");

			writer.WriteEndElement();
		}
	}
}
