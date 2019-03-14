# -*- coding: utf-8 -*-
import clr
import System

import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

from Autodesk.Revit.DB import ViewSchedule, ViewScheduleExportOptions
from Autodesk.Revit.DB import ExportColumnHeaders, ExportTextQualifier
from Autodesk.Revit.DB import BuiltInCategory, ViewSchedule

import os
import subprocess

desktop = os.path.expandvars('%temp%\\')
vseop = ViewScheduleExportOptions()

selected_ids = uidoc.Selection.GetElementIds()
if not selected_ids.Count:
    '''If nothing is selected, use Active View'''
    selected_ids=[ doc.ActiveView.Id ]
for element_id in selected_ids:
    element = doc.GetElement(element_id)
    if not isinstance(element, ViewSchedule):
        print('No schedule in Selection. Skipping...')
        continue
    filename = "".join(x for x in element.ViewName if x not in ['*']) + '.txt'
    element.Export(desktop, filename, vseop)
    print('EXPORTED: {0}\n      TO: {1}\n'.format(element.ViewName, filename))
    #EXCEL = r"C:\Program Files (x86)\Microsoft Office\Office14\EXCEL.EXE"
    EXCEL = excelPath
    if os.path.exists(EXCEL):
        print('Excel Found. Trying to open...')
        print('Filename is: ', filename)
        try:
            full_filepath = os.path.join(desktop, filename)
            os.system('start excel \"{path}\"'.format(path=full_filepath))
        except:
            print('Sorry, something failed:')
            print('Filepath: {}'.filename)
            print('EXCEL Path: {}'.format(EXCEL))
    else:
        print('Could not find excel. EXCEL: {}'.format(EXCEL))
print('Done')
