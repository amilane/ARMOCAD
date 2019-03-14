# -*- coding: utf-8 -*-
import clr
import System

import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

def run():
  elems = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting).WhereElementIsNotElementType().ToElements()
  transitions = filter(lambda e: e.MEPModel.PartType == PartType.Transition, elems)
  t = Transaction(doc, "Set Transition Length")
  t.Start()
  for e in transitions:
    sizetext = e.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString()
    size2 = sizetext.split('-')
    size3 = [i.split('x') for i in size2]
    size4 = [[int(i[0]), int(i[1])] for i in size3]

    a = set(size4[0])
    b = set(size4[1])
    intersection = list(a & b)
    if intersection:
      B = intersection[0]
    else:
      _ = list(a ^ b)
      _.sort()
      B = _[int(len(_)/2)]
    difference = list(a ^ b)
    if len(difference) == 1:
      A = min(difference[0], B)
      A1 = max(difference[0], B)
    else:
      A = min(difference)
      A1 = max(difference)
    
    l = A1-A
    if l <= 200:
      L = 300
    else:
      L = l + 100

    try:
      parLength = e.LookupParameter('Длина воздуховода')
      parLength.Set(L/304.8)
    except: pass
  t.Commit()

run()