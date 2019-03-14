# -*- coding: utf-8 -*-

import clr
import System
import math

import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")
from operator import itemgetter
from itertools import groupby

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

from System.Collections.Generic import List


cats = []
cats.Add(ElementId(BuiltInCategory.OST_DuctCurves))
cats.Add(ElementId(BuiltInCategory.OST_FlexDuctCurves))
cats.Add(ElementId(BuiltInCategory.OST_DuctAccessory))
cats.Add(ElementId(BuiltInCategory.OST_DuctFitting))
cats.Add(ElementId(BuiltInCategory.OST_DuctTerminal))
cats.Add(ElementId(BuiltInCategory.OST_MechanicalEquipment))
cats.Add(ElementId(BuiltInCategory.OST_DuctInsulations))
cats.Add(ElementId(BuiltInCategory.OST_PipeCurves))
cats.Add(ElementId(BuiltInCategory.OST_FlexPipeCurves))
cats.Add(ElementId(BuiltInCategory.OST_PipeFitting))
cats.Add(ElementId(BuiltInCategory.OST_PipeAccessory))
cats.Add(ElementId(BuiltInCategory.OST_PlumbingFixtures))
cats.Add(ElementId(BuiltInCategory.OST_Sprinklers))
cats.Add(ElementId(BuiltInCategory.OST_PipeInsulations))


ICats = List[ElementId](cats)

def filterBySysName(sysName):
  bip = BuiltInParameter.RBS_SYSTEM_NAME_PARAM
  rules = []
  rules.Add(ParameterFilterRuleFactory.CreateNotEqualsRule(ElementId(bip), sysName, True))
  filName = 'system_' + sysName
  if filName in filterNames:
    f = filter(lambda i: i.Name == filName, filters)[0]
  else:
    f = ParameterFilterElement.Create(doc, filName, ICats, rules)
  return f

def filterByShortSysName(shortSysName):
  bip = BuiltInParameter.RBS_SYSTEM_NAME_PARAM
  rules = []
  rules.Add(ParameterFilterRuleFactory.CreateNotContainsRule(ElementId(bip), shortSysName, True))
  filName = 'system_' + shortSysName
  if filName in filterNames:
    f = filter(lambda i: i.Name == filName, filters)[0]
  else:
    f = ParameterFilterElement.Create(doc, filName, ICats, rules)
  return f

filters = FilteredElementCollector(doc).OfClass(ParameterFilterElement).ToElements()
filterNames = [f.Name for f in filters]

ductSystems = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsNotElementType().ToElements()
pipeSystems = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipingSystem).WhereElementIsNotElementType().ToElements()


viewFamilyTypes = FilteredElementCollector(doc).OfClass(ViewFamilyType).ToElements()
viewFamilyTypes3D = filter(lambda x: x.ViewFamily == ViewFamily.ThreeDimensional, viewFamilyTypes)
viewsInDoc = FilteredElementCollector(doc).OfClass(View3D).ToElements()
viewsInDocNames = [i.Name for i in viewsInDoc]

t = Transaction(doc, 'Create Schemes')
t.Start()

# duct systems
for system in ductSystems:
  sysName = system.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString()
  f = filterBySysName(sysName)
  viewNameValue = 'Аксонометрия ' + sysName
  if viewNameValue not in viewsInDocNames:
    view3D = View3D.CreateIsometric(doc, viewFamilyTypes3D[0].Id)
    parViewName = view3D.get_Parameter(BuiltInParameter.VIEW_NAME)
    parViewName.Set(viewNameValue)
    view3D.SaveOrientationAndLock()
    view3D.AddFilter(f.Id)
    view3D.SetFilterVisibility(f.Id, False)
    view3D.DetailLevel = ViewDetailLevel.Fine

# pipe systems
shortSysNames = []
for system in pipeSystems:
  sysType = doc.GetElement(system.GetTypeId())
  shortSysName = sysType.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM)
  if shortSysName != None:
    shortSysNameValue = shortSysName.AsString()
    if shortSysNameValue.strip() != '' and shortSysNameValue not in shortSysNames:
      shortSysNames.append(shortSysNameValue)

for ssn in shortSysNames:
  f = filterByShortSysName(ssn)
  viewNameValue = 'Аксонометрия ' + ssn
  if viewNameValue not in viewsInDocNames:
    view3D = View3D.CreateIsometric(doc, viewFamilyTypes3D[0].Id)
    parViewName = view3D.get_Parameter(BuiltInParameter.VIEW_NAME)
    parViewName.Set(viewNameValue)
    view3D.SaveOrientationAndLock()
    view3D.AddFilter(f.Id)
    view3D.SetFilterVisibility(f.Id, False)
    view3D.DetailLevel = ViewDetailLevel.Fine

t.Commit()