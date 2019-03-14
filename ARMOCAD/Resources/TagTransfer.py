# -*- coding: utf-8 -*-
import clr
import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *

ids = uidoc.Selection.GetElementIds()

idd = [str(i) for i in ids]
if len(idd) != 2:
  MessageBox.Show("Выберите 2 элемента для переноса параметра", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)
else:
  elems = [doc.GetElement(ElementId(int(i))) for i in idd]
  elFrom = filter(lambda e: e.Category.Id.IntegerValue != int(BuiltInCategory.OST_DetailComponents), elems)
  elTo = filter(lambda e: e.Category.Id.IntegerValue == int(BuiltInCategory.OST_DetailComponents), elems)
  if len(elFrom) == len(elTo) == 1:
    tagFrom = elFrom[0].LookupParameter('TAG').AsString()
    tagTo = elTo[0].LookupParameter('TAG')
    t = Transaction(doc, 'TransferTag')
    t.Start()
    tagTo.Set(tagFrom)
    t.Commit()
  else:
    MessageBox.Show("Выберите 1 элемент из модели и соответствующий ему элемент узла из принципиальной схемы", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)
