using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
    internal class SolidTank : RocketElement
    {

        public double cogX,cogY,cogZ;
        public double radius,density, height, thickness, solidMass, maxSolidMass;
        public int type;

        public SolidTank(double radius, double height, double thickness, double density, double solidMass,double maxSolidMass, double cogx, double cogy, double cogz, int type)
        {
            this.radius = radius;
            this.density = density;
            this.thickness = thickness;
            this.density = density;
            this.height = height;
            this.solidMass = solidMass;
            this.maxSolidMass = maxSolidMass;
            this.cogX = cogx;
            this.cogY = cogy;
            this.cogZ = cogz;//cogz+ height / 2;
            this.type = type;
        }


        public override void WriteToOpenRocket(XmlTextWriter writter)
        {
        }

        public override void WriteToXML(XmlTextWriter writter)
        {
        }
    }
}
