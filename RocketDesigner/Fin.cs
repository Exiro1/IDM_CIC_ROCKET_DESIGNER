using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public class Fin : RocketElement
	{
		

		public Fin(double chord, double span, double sweepDist, double tipChord, double thickness, double lERad, double fX1, double fX3, String section,int count)
		{
			this.chord = chord;
			this.span = span;
			this.sweepDist = sweepDist;
			TipChord = tipChord;
			this.thickness = thickness;
			LERad = lERad;
			FX1 = fX1;
			FX3 = fX3;
			this.count = count;
		}

		public double chord;
		public double span;
		public double sweepDist;
		public double TipChord;
		public double thickness;
		public double LERad;
		public double FX1;
		public double FX3;
		public int count;

		public void WriteToXML(XmlTextWriter writer, int fcount, double finloc)
		{
			writer.WriteStartElement("Fin");
			writer.WriteElementString("Count", fcount.ToString());
			writer.WriteElementString("Chord", toInch(chord).ToString());
			writer.WriteElementString("Span", toInch(span).ToString());
			writer.WriteElementString("SweepDistance", toInch(sweepDist).ToString());
			writer.WriteElementString("TipChord", toInch(TipChord).ToString());
			writer.WriteElementString("Thickness", toInch(thickness).ToString());
			writer.WriteElementString("LERadius", toInch(LERad).ToString());
			writer.WriteElementString("Location", toInch(finloc + chord).ToString());
			writer.WriteElementString("AirfoilSection", "Hexagonal");
			writer.WriteElementString("FX1", toInch(FX1).ToString());
			writer.WriteElementString("FX3", toInch(FX3).ToString());
			writer.WriteEndElement();
		}
		public override void WriteToXML(XmlTextWriter writer)
		{
			writer.WriteStartElement("Fin");
			writer.WriteElementString("Count", count.ToString());
			writer.WriteElementString("Chord", toInch(chord).ToString());
			writer.WriteElementString("Span", toInch(span).ToString());
			writer.WriteElementString("SweepDistance", toInch(sweepDist).ToString());
			writer.WriteElementString("TipChord", toInch(TipChord).ToString());
			writer.WriteElementString("Thickness", toInch(thickness).ToString());
			writer.WriteElementString("LERadius", toInch(LERad).ToString());
			writer.WriteElementString("Location", toInch(Loc + chord).ToString());
			writer.WriteElementString("AirfoilSection", "Hexagonal");
			writer.WriteElementString("FX1", toInch(FX1).ToString());
			writer.WriteElementString("FX3", toInch(FX3).ToString());
			writer.WriteEndElement();
		}
	}
}
