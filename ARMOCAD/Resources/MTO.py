# -*- coding: utf-8 -*-

import clr
import System

import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

from itertools import groupby

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

NONELIST = [None, '', ' ']
projInfo = doc.ProjectInformation
tag1Value = projInfo.LookupParameter('TagCode1').AsString()

valueMainEquipTag = tag1Value + '.1'
valueSecondary1 = tag1Value + '.2.1'
valueSecondary2 = tag1Value + '.2.2'
valueSecondary3 = tag1Value + '.2.3'
valueSecondary4 = tag1Value + '.2.4'

ducts = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctCurves).WhereElementIsNotElementType().ToElements()
flexDuct = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_FlexDuctCurves).WhereElementIsNotElementType().ToElements()
ductFitings = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting).WhereElementIsNotElementType().ToElements()
ductAccessory = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctAccessory).WhereElementIsNotElementType().ToElements()
ductTerminal = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctTerminal).WhereElementIsNotElementType().ToElements()
ductIsol = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctInsulations).WhereElementIsNotElementType().ToElements()
equipment = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_MechanicalEquipment).WhereElementIsNotElementType().ToElements()
pipes = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeCurves).WhereElementIsNotElementType().ToElements()
pipeFitings = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeFitting).WhereElementIsNotElementType().ToElements()
pipeAccessory = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeAccessory).WhereElementIsNotElementType().ToElements()
flexPipe = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_FlexPipeCurves).WhereElementIsNotElementType().ToElements()
pipeIsol = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeInsulations).WhereElementIsNotElementType().ToElements()


t = Transaction(doc, 'Set MTO')
t.Start()

# основное оборудование
mainEquipment = []
for e in equipment:
  parTagcode4 = e.LookupParameter('TagCode4')
  parTagcode5 = e.LookupParameter('TagCode5')
  if parTagcode4 != None and parTagcode5 != None:
    if parTagcode4.AsString() not in NONELIST and parTagcode5.AsString() not in NONELIST and parTagcode4.AsString() != 'HZ':
      mainEquipment.append(e)

mainEquipmentSortTG4_TG5 = sorted(mainEquipment, key = lambda e: e.LookupParameter('TagCode4').AsString() + e.LookupParameter('TagCode5').AsString())
mainEquipmentGroupTG4_TG5 = [[x for x in g] for k,g in groupby(mainEquipmentSortTG4_TG5, lambda e: e.LookupParameter('TagCode4').AsString() + e.LookupParameter('TagCode5').AsString())]

for k, group in enumerate(mainEquipmentGroupTG4_TG5):
  for e in group:
    parMTO = e.LookupParameter('AG_Spc_МТО')
    parMTO.Set(valueMainEquipTag + '.' + str(k+1))

# неосновное 1
secondary1 = list(ducts) + list(flexDuct) + list(ductFitings) + list(ductIsol)
for e in secondary1:
  parMTO = e.LookupParameter('AG_Spc_МТО')
  parMTO.Set(valueSecondary1)

# неосновное 2
secondary2 = list(pipes) + list(flexPipe) + list(pipeFitings) + list(pipeAccessory) + list(pipeIsol)
for e in secondary2:
  parMTO = e.LookupParameter('AG_Spc_МТО')
  parMTO.Set(valueSecondary2)

# неосновное 3
secondary3 = list(ductAccessory) + list(ductTerminal)
for e in secondary3:
  parMTO = e.LookupParameter('AG_Spc_МТО')
  parMTO.Set(valueSecondary3)

# неосновное 4
secondary4 = []
for e in pipeAccessory:
  parTagcode3 = e.LookupParameter('TagCode3')
  if parTagcode3 != None:
    parTagcode3Value = parTagcode3.AsString()
    if parTagcode3Value == 'I':
      secondary4.append(e)

for e in secondary4:
  parMTO = e.LookupParameter('AG_Spc_МТО')
  parMTO.Set(valueSecondary4)

t.Commit()
