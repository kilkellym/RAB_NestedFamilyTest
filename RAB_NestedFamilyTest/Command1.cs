#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Create = Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

namespace RAB_NestedFamilyTest
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public List<string> ReturnList;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            ReturnList = new List<string>();
            
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            collector.OfClass(typeof(FamilyInstance));

            foreach (FamilyInstance curFI in collector)
            {
                GetNestedFamilies(doc, curFI, 0);
            }

            // output results to OUTPUT window
            foreach(string result in ReturnList)
                Debug.Print(result);

            TaskDialog.Show("Complete", "Export nested family names complete. Check the output window in Visual Studio.");

            return Result.Succeeded;
        }

        private void GetNestedFamilies(Document doc, FamilyInstance curFI, int count)
        {
            ReturnList.Add(count.ToString() + " - " + curFI.Name);
            count++;

            // to prevent stack overflow error
            if (count > 100)
                return;

            Family curFamily = curFI.Symbol.Family;

            // Get Family document for family
            Document familyDoc = doc.EditFamily(curFamily);

            if (null != familyDoc && familyDoc.IsFamilyDocument == true)
            {
                FilteredElementCollector collector1 = new FilteredElementCollector(familyDoc);
                collector1.OfClass(typeof(FamilyInstance)).ToElements();

                if(collector1.Count() > 0)
                {
                    foreach (FamilyInstance curFI2 in collector1)
                    {
                        GetNestedFamilies(familyDoc, curFI2, count);
                    }
                }
            }
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
