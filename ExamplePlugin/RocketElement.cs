using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
	public abstract class RocketElement
	{

		string name;
		RocketElement top;
		RocketElement bot;
		List<RocketElement> sideAttach = new List<RocketElement>();
		private double loc;

		private double len;

		public RocketElement Top { get => top; set => top = value; }
		public RocketElement Bot { get => bot; set => bot = value; }
		public string Name { get => name; set => name = value; }
		public List<RocketElement> SideAttach { get => sideAttach; }
		public double Loc { get => loc; set => loc = value; }
		public double Len { get => len; set => len = value; }

		public RocketElement()
		{
		}

		public void addSideAttach(RocketElement element)
		{
			SideAttach.Add(element);
		}
		public double toInch(double m)
		{
			return Math.Round(m * 39.3701, 3);
		}
		public abstract void WriteToXML(XmlTextWriter writter);

	}
}
