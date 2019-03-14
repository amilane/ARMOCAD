# -*- coding: utf-8 -*-

import clr
import System
import math

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
if idd == []:
    MessageBox.Show("Ничего не выбрано!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)
else:
    if isinstance(idd, list) == True:
        elems = [doc.GetElement(ElementId(int(i))) for i in idd]
    else:
        elems = doc.GetElement(ElementId(int(idd)))

    _vft = FilteredElementCollector(doc).OfClass(ViewFamilyType).ToElements()
    vft = filter(lambda x: x.ViewFamily == ViewFamily.Section, _vft)[0]

    t = Transaction(doc, "Section")
    t.Start()
    for e in elems:
        lc = e.Location
        line = lc.Curve

        p = line.GetEndPoint(0)
        q = line.GetEndPoint(1)
        v = q - p

        bb = e.get_BoundingBox(None)
        minZ = bb.Min.Z
        maxZ = bb.Max.Z

        w = v.GetLength()
        h = maxZ - minZ
        offset = 0.1*w
        
        min = XYZ( -w, minZ - offset, -offset)
        max = XYZ( w, maxZ + offset, offset)

        midpoint = XYZ((bb.Min.X + bb.Max.X) * 0.5, (bb.Min.Y + bb.Max.Y) * 0.5, 0)
        linedir = v.Normalize()
        up = XYZ.BasisZ
        viewdir = linedir.CrossProduct(up)
        
        tr = Transform.Identity
        tr.Origin = midpoint
        tr.BasisX = linedir
        tr.BasisY = up
        tr.BasisZ = viewdir
        
        sectionBox = BoundingBoxXYZ()
        sectionBox.Transform = tr
        sectionBox.Min = min
        sectionBox.Max = max
        
        ViewSection.CreateSection( doc, vft.Id, sectionBox)
        
    t.Commit()

#MessageBox.Show(str(q), "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)