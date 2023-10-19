#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAB_NestedFamilyTest
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public int counter;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            collector.OfClass(typeof(FamilyInstance));

            List<Element> listFamilyInstances = new FilteredElementCollector(doc, doc.ActiveView.Id)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(a => a.SuperComponent == null)
            .SelectMany(a => a.GetSubComponentIds())
            .Select(a => doc.GetElement(a))
            .ToList();

            counter = 0;

            foreach (FamilyInstance curFI in collector)
            {
                Family curFam = curFI.Symbol.Family;
                GetNestedFamilies(doc, curFam, 0);
            }
            
            // output results to OUTPUT window
            TaskDialog.Show("Complete", $"Set {counter} families to shared.");

            return Result.Succeeded;
        }
        private void GetNestedFamilies(Document doc, Family curFamily, int count)
        {
            counter++;

            // to prevent stack overflow error
            if (count > 100)
                return;

            using(Transaction t = new Transaction(doc))
            {
                t.Start("Set shared param");
                // set as shared
                curFamily.get_Parameter(BuiltInParameter.FAMILY_SHARED).Set(1);
                t.Commit();
            }
            

            // Get Family document for family
            Document familyDoc = doc.EditFamily(curFamily);
            

            familyDoc.OwnerFamily.get_Parameter(BuiltInParameter.FAMILY_SHARED).Set(1);

            if (null != familyDoc && familyDoc.IsFamilyDocument == true)
            {
                try
                {
                    FilteredElementCollector collector1 = new FilteredElementCollector(familyDoc);
                    collector1.OfClass(typeof(Family)).ToElements();

                    if (collector1.Count() > 0)
                    {
                        foreach (Family curFI2 in collector1)
                        {
                            GetNestedFamilies(familyDoc, curFI2, count);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Print(e.Message);
                }
            }
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
    }
}
