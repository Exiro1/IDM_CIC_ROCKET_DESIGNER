using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SwConst;


namespace RocketDesigner
{
    class SolidWorksUtil
    {

        SldWorks swApp;
        private bool swInstalled = false;
        private bool swAvailable = false;

        public SolidWorksUtil()
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = hklm.OpenSubKey(@"Software\SolidWorks"))
            {

                if (key == null)
                {
                    MessageBox.Show("Solidwoks n'est pas installé, certaines fonctionnalitées de Rocket Designer ne seront pas disponible");
                }
                else
                {
                    swInstalled = true;
                }
            }
        }

        public bool loadSW()
        {
            if (swAvailable)
            {
                try
                {
                    bool error = swApp.Visible;
                }
                catch (Exception ex)
                {
                    swAvailable = false;
                }
            }

            if (swAvailable) return true;
            if (!swInstalled) return false;

            swApp = (SldWorks)Activator.CreateInstance(System.Type.GetTypeFromProgID("SldWorks.Application"));
            swAvailable = swApp != null;

            if (!swAvailable) return false;
            swApp.Visible = false;
            return true;

        }


        SolidWorksException solidWorksNotStartedException()
        {
            throw new SolidWorksException("Solidworks hasn't started");
        }

        public void restart()
        {
            swApp = null;
            swAvailable = false;
        }


        public void switchVisible()
        {
            if (!loadSW())
                return;
            if (swApp == null)
                solidWorksNotStartedException();

            bool visible = !swApp.Visible;

            swApp.Visible = visible;
        }

        public void updateSWNoseFile(double r, double h, double th, int type)
        {
            if (!loadSW())
                return;

            if (swApp == null)
                solidWorksNotStartedException();

            int err = 0;
            int warn = 0;

            double x = 0;
            double y = 0;

            var fileName = Main.folderPath + "Pièce.SLDPRT";

            //((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)
            swApp.NewDocument(swApp.GetUserPreferenceStringValue(8), 0, 0, 0);

            swApp.ActivateDoc2("Pièce.SLDPRT", false, err);

            ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

            var myModelView = Part.ActiveView;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

            Part.InsertSketch();
            Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            //Part.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, "Esquisse");
            Part.EditSketch();
            Part.ClearSelection2(true);

            if (type == 0)
                Part.SketchManager.CreateEquationSpline2("", "(( (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") )^2-(" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "-x)^2)^(1/2) + " + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " - (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);
            if (type == 1)
                Part.SketchManager.CreateEquationSpline2("", "(" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "/pi^0.5)*(arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")-sin(2*arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "))/2)^0.5", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);

            Part.ClearSelection2(true);

            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.SketchAddConstraints("sgCOINCIDENT");
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(0, 0, 0, 10, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            //"("+r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+"/pi^0.5)*(arccos(1-2*x/"+h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+")-sin(2*arccos(1-2*x/"+h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+"))/2)^0.5"
            if (type == 0)
                Part.SketchManager.CreateEquationSpline2("", "(( (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") )^2-(" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "-x)^2)^(1/2) + " + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " - (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") - " + th.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);
            if (type == 1)
                Part.SketchManager.CreateEquationSpline2("", "(" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "/pi^0.5)*(arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")-sin(2*arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "))/2)^0.5 - " + th.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);

            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(h * 0.001, r * 0.001, 0, h * 0.001, (r - th) * 0.001, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);



            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Spline4", "SKETCHSEGMENT", 0, 1, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Line1", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.SketchManager.SketchTrim(1, 0, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateCenterLine(0, 0, 0, 1, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.Extension.SelectByID2("Line3@Esquisse1", "SKETCHSEGMENT", 0, 0, 0, true, 16, null, 0);
            Part.FeatureManager.FeatureRevolve2(true, true, false, false, false, false, 0, 0, 6.2831853071796, 0, false, false, 0.01, 0.01, 0, 0, 0, true, true, true);

            ((SelectionMgr)Part.SelectionManager).EnableContourSelection = false;
            Part.ClearSelection2(true);


            fileName = Main.folderPath + "nosecone2.STEP";
            Part.SaveAs3(fileName, 0, 2);
            Part.SaveAs3(fileName.Replace("STEP", "SLDPRT"), 0, 2);
            swApp.CloseDoc(Part.GetTitle());
        }


        public void generateNoseConeMold(double r, double h, double th, int type)
        {
            if (!loadSW())
                return;

            if (swApp == null)
                solidWorksNotStartedException();

            int err = 0;
            int warn = 0;

            double x = 0;
            double y = 0;

            var fileName = Main.folderPath + "Pièce.SLDPRT";

            //((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)
            swApp.NewDocument(swApp.GetUserPreferenceStringValue(8), 0, 0, 0);

            swApp.ActivateDoc2("Pièce.SLDPRT", false, err);

            ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

            var myModelView = Part.ActiveView;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);


            Part.InsertSketch();
            Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);



            Part.InsertSketch();
            Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            //Part.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, "Esquisse");
            Part.EditSketch();
            Part.ClearSelection2(true);

            if (type == 0)
                Part.SketchManager.CreateEquationSpline2("", "(( (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") )^2-(" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "-x)^2)^(1/2) + " + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " - (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);
            if (type == 1)
                Part.SketchManager.CreateEquationSpline2("", "(" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "/pi^0.5)*(arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")-sin(2*arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "))/2)^0.5", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);

            Part.ClearSelection2(true);

            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.SketchAddConstraints("sgCOINCIDENT");
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(0, 0, 0, 10, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            //"("+r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+"/pi^0.5)*(arccos(1-2*x/"+h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+")-sin(2*arccos(1-2*x/"+h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)+"))/2)^0.5"
            if (type == 0)
                Part.SketchManager.CreateEquationSpline2("", "(( (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") )^2-(" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "-x)^2)^(1/2) + " + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " - (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") - " + th.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);
            if (type == 1)
                Part.SketchManager.CreateEquationSpline2("", "(" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "/pi^0.5)*(arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")-sin(2*arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "))/2)^0.5 - " + th.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);

            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(h * 0.001, r * 0.001, 0, h * 0.001, (r - th) * 0.001, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);



            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Spline4", "SKETCHSEGMENT", 0, 1, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Line1", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.SketchManager.SketchTrim(1, 0, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);
            Part.SketchManager.CreateCenterLine(0, 0, 0, 1, 0, 0);
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.Extension.SelectByID2("Line3@Esquisse1", "SKETCHSEGMENT", 0, 0, 0, true, 16, null, 0);
            Part.FeatureManager.FeatureRevolve2(true, true, false, false, false, false, 0, 0, 6.2831853071796, 0, false, false, 0.01, 0.01, 0, 0, 0, true, true, true);

            ((SelectionMgr)Part.SelectionManager).EnableContourSelection = false;
            Part.ClearSelection2(true);


            fileName = Main.folderPath + "nosecone2.STEP";
            Part.SaveAs3(fileName, 0, 2);
            Part.SaveAs3(fileName.Replace("STEP", "SLDPRT"), 0, 2);
            swApp.CloseDoc(Part.GetTitle());
        }







        /*
		public void test(MainSystem syst)
		{
			Point p = this.IdmStudy.MainSystem.GetCog(IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementActivation | IdmCic.API.Utils.Calculation.MCI.MainSystemMassOptions.IncludeElementMargin, IdmStudy.MainSystem.GetConfiguration(""));
		}
		*/

        public void create2DSketch(Rocket rocket, bool withFins, double front, double end, double h2d)
        {

            if (!loadSW())
                return;

            if (swApp == null)
                solidWorksNotStartedException();

            int err = 0;
            int warn = 0;

            double x = 0;
            double y = 0;
            int pnbr = 5;
            int dimnbr = 2;

            var fileName = Main.folderPath + "Pièce.SLDPRT";

            swApp.NewDocument(swApp.GetUserPreferenceStringValue(((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)), 0, 0, 0);

            swApp.ActivateDoc2("Pièce.SLDPRT", false, err);
            ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;

            var myModelView = Part.ActiveView;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

            Part.InsertSketch();
            Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.SelectedFeatureProperties(0, 0, 0, 0, 0, 0, 0, true, false, "Esquisse");
            Part.EditSketch();
            Part.ClearSelection2(true);



            double r = rocket.getNosecone().radius * 1000;
            double h = rocket.getNosecone().Len * 1000;
            
            //Part.SketchManager.CreateEquationSpline2("", "(( (" + r * r + "+" + h * h + ")/(2*" + r + ") )^2-(" + h + "-x)^2)^(1/2) + " + r + " - (" + r * r + "+" + h * h + ")/(2*" + r + ")", "", "0", "" + h, false, 0, 0, 0, true, true);
            if (rocket.getNosecone().shape == Nosecone.NoseConeShape.Tangent)
                Part.SketchManager.CreateEquationSpline2("", "(( (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ") )^2-(" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "-x)^2)^(1/2) + " + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " - (" + (r * r).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "+" + (h * h).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")/(2*" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);
            if (rocket.getNosecone().shape == Nosecone.NoseConeShape.VonKarman)
                Part.SketchManager.CreateEquationSpline2("", "(" + r.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "/pi^0.5)*(arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + ")-sin(2*arccos(1-2*x/" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "))/2)^0.5", "", "0", "" + h.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture), false, 0, 0, 0, true, true);

            Part.ClearSelection2(true);

            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.SketchAddConstraints("sgCOINCIDENT");
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);


            Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point4", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.AddVerticalDimension2(r * 0.001, 0, 0);
            Dimension myDimension = (Dimension)Part.Parameter("D1@Esquisse");
            myDimension.SystemValue = r * 0.001;


            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            x = h * 0.001;
            y = r * 0.001;


            RocketElement e = rocket.getNosecone();

            while (e.Bot != null)
            {
                e = e.Bot;
                if (typeof(Body).IsInstanceOfType(e))
                {

                    if (withFins && e.SideAttach.Count > 0)
                    {
                        int subbody = (int)((((Body)e).Len - ((Fin)e.SideAttach.First()).chord - ((Body)e).finLoc) * 1000);

                        int finTE = (int)((((Fin)e.SideAttach.First()).chord - ((Fin)e.SideAttach.First()).sweepDist - ((Fin)e.SideAttach.First()).TipChord) * 1000);
                        //int subbody = (int)((((Body)e).Len - ((Fin)e.SideAttach.First()).chord - ((Fin)e.SideAttach.First()).Loc) * 1000);


                        createBody(ref x, ref y, subbody, (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
                        createBody(ref x, ref y, (int)(((Fin)e.SideAttach.First()).sweepDist * 1000), (int)((((Fin)e.SideAttach.First()).span + ((Body)e).radius) * 1000), Part, ref pnbr, ref dimnbr);
                        createBody(ref x, ref y, (int)(((Fin)e.SideAttach.First()).TipChord * 1000), (int)((((Fin)e.SideAttach.First()).span + ((Body)e).radius) * 1000), Part, ref pnbr, ref dimnbr);
                        createBody(ref x, ref y, finTE, (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
                        if (((Body)e).finLoc != 0)
                        {
                            createBody(ref x, ref y, (int)(((Body)e).finLoc * 1000), (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
                        }
                    }
                    else
                    {
                        createBody(ref x, ref y, (int)(((Body)e).Len * 1000), (int)(((Body)e).radius * 1000), Part, ref pnbr, ref dimnbr);
                    }


                }
                if (typeof(Transition).IsInstanceOfType(e))
                {
                    createBody(ref x, ref y, (int)(((Transition)e).Len * 1000), (int)(((Transition)e).radiusDown * 1000), Part, ref pnbr, ref dimnbr);
                }
            }
            double len = x;
            double rectArea = x * 1000 * (front + end + 1) * h2d;
            endRocket(ref x, ref y, front, end, h2d, Part, ref pnbr, ref dimnbr);


            Part.Extension.SelectByID2("", "FACE", 0, 0, 0, false, 0, null, 0);
            /*
			Part.ClearSelection2(true);
			Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
			*/
            ModelDoc2 swModel = default(ModelDoc2);
            ModelDocExtension swModelExt = default(ModelDocExtension);
            SelectionMgr swSelMgr = default(SelectionMgr);
            Component2 swComp = default(Component2);

            swModel = (ModelDoc2)swApp.ActiveDoc;
            swModelExt = swModel.Extension;
            swSelMgr = (SelectionMgr)swModel.SelectionManager;
            double[] v = swModelExt.GetSectionProperties2(swSelMgr.GetSelectedObject5(1));
            double area = v[1] * 1000000;
            MessageBox.Show("Surface Area : " + (rectArea - area) + " mm²");

            string path;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "IGES (*.igs)|*.igs";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;
                Part.SaveAs3(path, 0, 2);
                Part.SaveAs3(path.Replace(".igs", ".SLDPRT"), 0, 2);
                swApp.CloseDoc(Part.GetTitle());
            }
            else
            {

            }
        }


        public void createBody(ref double x, ref double y, int h, int r, ModelDoc2 Part, ref int pointNbr, ref int dimNbr)
        {
            if (!loadSW())
                return;

            if (swApp == null)
                solidWorksNotStartedException();

            Dimension myDimension;
            Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SketchManager.CreateLine(x, y, 0, x + h * 0.001, r * 0.001, 0);

            Part.ClearSelection2(true);

            if (pointNbr == 5)
                pointNbr++;
            Part.SetPickMode();
            Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            if (pointNbr == 6)
            {
                Part.Extension.SelectByID2("Point4", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            }
            else
            {
                Part.Extension.SelectByID2("Point" + (pointNbr - 1), "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            }
            if (h != 0)
            {
                Part.AddHorizontalDimension2(Math.Abs(h) * 0.001, 0, 0);
                myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
                myDimension.SystemValue = Math.Abs(h) * 0.001;
                dimNbr++;
            }
            Part.ClearSelection2(true);
            Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.AddVerticalDimension2(r * 0.001, 0, 0);
            myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
            myDimension.SystemValue = r * 0.001;
            dimNbr++;
            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            pointNbr++;

            x = x + h * 0.001;
            y = r * 0.001;

        }


        public void endRocket(ref double x, ref double y, double front, double end, double h, ModelDoc2 Part, ref int pointNbr, ref int dimNbr)
        {

            if (!loadSW())
                return;

            if (swApp == null)
                solidWorksNotStartedException();

            Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.EditSketch();
            Part.ClearSelection2(true);

            Part.SketchManager.CreateLine(x, y, 0, x, 0, 0);

            Part.ClearSelection2(true);

            pointNbr++;

            double len = x;

            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(x, 0, 0, x + end * len, 0, 0);
            Part.ClearSelection2(true);
            x = x + end * len;
            y = 0;
            pointNbr++;

            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(x, y, 0, x, y + h * 0.001, 0);
            Part.ClearSelection2(true);
            x = x;
            y = y + h * 0.001;
            Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.AddVerticalDimension2(h * 0.001, 0, 0);
            Dimension myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
            myDimension.SystemValue = h * 0.001;
            dimNbr++;
            pointNbr++;
            Part.ClearSelection2(true);


            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(x, y, 0, x - (end + 1 + front) * len, y, 0);
            Part.ClearSelection2(true);
            x = x - (end + 1 + front) * len;
            y = y;
            Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point3", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.AddHorizontalDimension2((front) * len, 0, 0);
            myDimension = (Dimension)Part.Parameter("D" + dimNbr + "@Esquisse");
            myDimension.SystemValue = (front) * len;
            dimNbr++;
            pointNbr++;
            Part.ClearSelection2(true);


            Part.ClearSelection2(true);
            Part.SketchManager.CreateLine(x, y, 0, x, 0, 0);
            Part.ClearSelection2(true);
            x = x;
            y = 0;
            pointNbr++;

            Part.SketchManager.CreateLine(x, y, 0, x + front * len, 0, 0);

            Part.ClearSelection2(true);
            Part.Extension.SelectByID2("Point" + pointNbr, "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point1@Origine", "EXTSKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.SketchAddConstraints("sgCOINCIDENT");
            Part.ClearSelection2(true);


            x = 0;
            y = 0;

            Part.ClearSelection2(true);
            Part.SketchManager.InsertSketch(true);

            Part.Extension.SelectByID2("Esquisse", "SKETCH", 0, 0, 0, false, 0, null, 0);
            Part.InsertPlanarRefSurface();

            Part.ClearSelection2(true);
        }

        internal void close()
        {
            if (swAvailable)
                swApp.ExitApp();
        }

        public double toInch(double m)
        {
            return Math.Round(m * 39.3701, 3);
        }

        public double toMeter(double inch)
        {
            return Math.Round(inch / 39.3701, 3);
        }

        /*
		 * - step -> sw part -> combine
		 * - creer une piece quart / tier de cylindre
		 * - ajouter la piece cylindre au sw part de la fusée
		 * - positioner les 2 corps
		 * - combine
		 * - export igs
		*/




        public void stepToBody(string stp)
        {
            if (!loadSW())
                return;
            if (swApp == null)
                solidWorksNotStartedException();
            int err = 0;
            int warn = 0;

            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swImportNeutral_SolidandSurface, true);
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swImportNeutralReferencePlane, true);
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swImportNeutral_AttributesAndProperties, true);
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swImportNeutralRunDiagnostics, true);
            swApp.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swImportNeutralAssemblyStructureMapping, (int)swImportNeutralAssemblyStructureMapping_e.swImportNeutralAssemblyStructureMapping_MultibodyPart);
            swApp.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swImportNeutralAssemblyStructureMapping, 2);

            var fileName = Main.folderPath + stp;
            ImportStepData importStepData = swApp.GetImportFileData(fileName);

            ModelDoc2 doc = swApp.LoadFile4(fileName, "C", importStepData, ref err);
            swApp.ActivateDoc2(stp, false, err);
            SolidWorks.Interop.sldworks.ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;
            var myModelView = Part.ActiveView;

            object[] arrBody = (object[])((PartDoc)Part).GetBodies2((int)swBodyType_e.swSolidBody, true);

            bool f = false;
            /*foreach (object body in arrBody)
			{
				Part.Extension.SelectByID2(((Body2)body).Name, "SOLIDBODY", 0, 0, 0, true, 0, null, 0);
			}*/
            Part.ClearSelection2(true);
            foreach (object body in arrBody)
            {
                Part.Extension.SelectByID2(((Body2)body).Name, "SOLIDBODY", 0, 0, 0, true, 2, null, 0);
                f = true;
            }
            Part.FeatureManager.InsertCombineFeature(15903, null, null);


            Part.SaveAs3(Main.folderPath + "rocket.SLDPRT", 0, 2);
            swApp.CloseDoc(Part.GetTitle());
        }


        public void sketch3D(int angle, double h, double l1, double l2)
        {
            if (!loadSW())
                return;
            if (swApp == null)
                solidWorksNotStartedException();
            //
            int err = 0;
            int warn = 0;

            double x = 0;
            double y = 0;
            int pnbr = 5;
            int dimnbr = 2;

            var fileName = Main.folderPath + "Pièce.SLDPRT";

            swApp.NewDocument(swApp.GetUserPreferenceStringValue(((int)swUserPreferenceStringValue_e.swDefaultTemplatePart)), 0, 0, 0);

            swApp.ActivateDoc2("Pièce.SLDPRT", false, err);
            SolidWorks.Interop.sldworks.ModelDoc2 Part = (ModelDoc2)swApp.ActiveDoc;


            var myModelView = Part.ActiveView;
            swApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false);

            Part.InsertSketch();
            Part.Extension.SelectByID2("Plan de face", "PLANE", 0, 0, 0, false, 0, null, 0);
            Part.ClearSelection2(true);

            Part.SketchManager.CreateCenterLine(0, 0, 0, 3, 0, 0);
            Part.ClearSelection2(true);

            Part.SketchManager.CreateLine(0, 0, 0, 2, 1.5, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Line1", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.AddRadialDimension2(0, 0, 0);
            Dimension myDimension = (Dimension)Part.Parameter("D1@Esquisse1");
            if (angle == 3)
                myDimension.SystemValue = 120 * Math.PI / 180;
            else
                myDimension.SystemValue = 45 * Math.PI / 180;
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.AddDimension2(0, 0, 0);
            myDimension = (Dimension)Part.Parameter("D2@Esquisse1");
            myDimension.SystemValue = h;
            Part.ClearSelection2(true);

            Part.SketchManager.CreateArc(0, 0, 0, -1.5, 2.598076, 0, -2.974223, 0.392423, 0, 0);
            Part.ClearSelection2(true);

            Part.SketchManager.CreateLine(-2.974223, 0.392423, 0, 0, 0, 0);
            Part.ClearSelection2(true);

            Part.Extension.SelectByID2("Line2", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Line3", "SKETCHSEGMENT", 0, 0, 0, true, 0, null, 0);
            Part.AddRadialDimension2(0, 0, 0);
            myDimension = (Dimension)Part.Parameter("D3@Esquisse1");
            if (angle == 3)
                myDimension.SystemValue = 60 * Math.PI / 180;
            else
                myDimension.SystemValue = 250 * Math.PI / 180;

            if (angle == 3)
                myDimension.SystemValue = 60 * Math.PI / 180;
            else
                myDimension.SystemValue = 270 * Math.PI / 180;
            Part.ClearSelection2(true);

            Part.SetPickMode();
            Part.Extension.SelectByID2("Point4", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.Extension.SelectByID2("Point5", "SKETCHPOINT", 0, 0, 0, true, 0, null, 0);
            Part.SketchAddConstraints("sgCOINCIDENT");
            Part.ClearSelection2(true);

            Part.InsertSketch();

            Part.Extension.SelectByID2("Esquisse1", "SKETCH", 0, 0, 0, false, 0, null, 0);

            Part.FeatureManager.FeatureExtrusion2(false, false, false, 0, 0, l1, l2, false, false, false, false, 1.74532925199433E-02, 1.74532925199433E-02, false, false, false, false, true, true, true, 0, 0, false);
            Part.SelectionManager.EnableContourSelection = false;


            Part.Extension.SelectByID2("Pièce.SLDPRT", "COMPONENT", 0, 0, 0, false, 0, null, 0);
            Part.SaveAs3(Main.folderPath + "cover.SLDPRT", 0, 0);
            Part.ClearSelection2(true);
            Part.EditRebuild3();
            Part.Save3(1, ref err, ref warn);

            ((PartDoc)Part).InsertPart3(Main.folderPath + "rocket.SLDPRT", 21, "Défaut");

            Part.Extension.SelectByID2("<rocket>-<Combiner1>", "SOLIDBODY", 0, 0, 0, false, 1, null, 0);
            Part.Extension.SelectByID2("Boss.-Extru.1", "SOLIDBODY", 0, 0, 0, true, 2, null, 0);
            
            Feature f = Part.FeatureManager.InsertCombineFeature(15902, null, null);
            Part.ClearSelection2(true);
            Part.Extension.SelectByID2("Combiner1[1]", "SOLIDBODY", 0, 0, 0, true, 2, null, 0);
            Part.FeatureManager.InsertDeleteBody2(false);
            Part.ClearSelection2(true);
            Part.Extension.SelectByID2("<rocket>-<Combiner1>", "BODYFEATURE", 0, 0, 0, false, 0, null, 0);
            string path;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "IGES (*.igs)|*.igs";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = saveFileDialog1.FileName;
                Part.SaveAs3(path, 0, 2);
                Part.SaveAs3(path.Replace(".igs", ".SLDPRT"), 0, 2);
                swApp.CloseDoc(Part.GetTitle());
            }
            else
            {

            }
        }

        internal bool isAvailable()
        {
            return swAvailable;
        }

        public bool isInstalled()
        {
            return swInstalled;
        }
    }

}

