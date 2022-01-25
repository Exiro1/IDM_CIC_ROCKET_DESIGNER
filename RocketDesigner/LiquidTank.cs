using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RocketDesigner
{
    internal class LiquidTank : RocketElement
    {

        public double cogX,cogY,cogZ;
        public double cogXmin, cogYmin, cogZmin;
        public double radius,density,height,thickness,liquidMass,maxLiquidMass;
        public int type;

        public LiquidTank(double radius, double heigth, double thickness, double density, double liquidMass,double maxLiquidMass, double posx, double posy, double posz,int type)
        {
            this.radius = radius;
            this.density = density;
            this.thickness = thickness;
            this.density = density;
            this.height = heigth;
            this.liquidMass = liquidMass;
            this.maxLiquidMass = maxLiquidMass;
            this.cogX = posx;
            this.cogY = posy;
            this.cogZ = posz;//posz+heigth*(1-(liquidMass/maxLiquidMass)/2);
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