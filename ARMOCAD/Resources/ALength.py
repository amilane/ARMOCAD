# -*- coding: utf-8 -*-
import clr
import System

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Mechanical import Duct

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import MessageBox, MessageBoxButtons, MessageBoxIcon

# элементы выбранные пользователем
ids = uidoc.Selection.GetElementIds()
idd = [str(i) for i in ids]
if idd == []:
    MessageBox.Show("Ничего не выбрано!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)
else:
    if isinstance(idd, list) == True:
        elems = [doc.GetElement(ElementId(int(i))) for i in idd]
    else:
        elems = doc.GetElement(ElementId(int(idd)))

# фильтрация по категориям
ducts = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_DuctCurves), elems)
flexDucts = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_FlexDuctCurves), elems)
pipes = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_PipeCurves), elems)
wires = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_Wire), elems)
cables = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_CableTray), elems)
conduits = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_Conduit), elems)
lines = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_Lines), elems)


def countLen(listElems):
    length = 0
    if listElems != []:
        for e in listElems:
            length += e.Location.Curve.Length
        message = listElems[0].Category.Name +": "+ str(round(UnitUtils.ConvertFromInternalUnits(length, DisplayUnitType.DUT_METERS), 2))+" м\n"
    else: message = ""
    return message

message = ""
message += countLen(ducts)
message += countLen(flexDucts)
message += countLen(pipes)
message += countLen(wires)
message += countLen(cables)
message += countLen(conduits)
message += countLen(lines)

MessageBox.Show(message, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information)
    