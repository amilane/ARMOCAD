# -*- coding: utf-8 -*-
import clr

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Mechanical import Duct

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *

ids = uidoc.Selection.GetElementIds()
idd = [str(i) for i in ids]
if idd == []:
	MessageBox.Show("Nothing selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information)
else:
	if isinstance(idd, list) == True:
		elems = [doc.GetElement(ElementId(int(i))) for i in idd]
	else:
		elems = doc.GetElement(ElementId(int(idd)))

ducts = filter(lambda e: isinstance(e, Duct) == True, elems)
if ducts == []:
    MessageBox.Show("Duct not selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information)

t = Transaction(doc, "Swap Duct")
t.Start()
for i in ducts:
    try:
        h = i.Height
        w = i.Width
        i.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(h)
        i.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(w)
    except:
        pass
t.Commit()

