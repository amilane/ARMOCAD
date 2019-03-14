# -*- coding: utf-8 -*-
import clr
import System

import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

from operator import itemgetter
from itertools import groupby

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

from Autodesk.Revit.Creation import *

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *

def findLevel(z, levelsSort):
    n = 0
    while n < len(levels)-1:
        if z < levelsSort[0].Elevation:
            return levelsSort[0]
            break
        elif levelsSort[n].Elevation <= z <= levelsSort[n+1].Elevation:
            return levelsSort[n]
            break
        elif z > levelsSort[len(levelsSort)-1].Elevation:
            return levelsSort[len(levelsSort)-1]
            break
        n += 1

generics = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsNotElementType().ToElements()
openings = filter(lambda e: e.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString()=="M_Rectangular Face Opening Solid", generics)

levels = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements()
levelsSort = sorted(levels, key = lambda i: i.Elevation)

t = Transaction(doc, "Level")
t.Start()
for e in openings:
    z = e.Location.Point.Z
    lvl = findLevel(z, levelsSort)
    opLev = e.LookupParameter("Opening Level")
    opLev.Set(lvl.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsValueString())
    offset = z - lvl.Elevation - e.LookupParameter("Height_True").AsDouble()/2
    opElev = e.LookupParameter("Opening Elevation")
    opElev.Set(offset)
    
t.Commit()
MessageBox.Show("ОК", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)
