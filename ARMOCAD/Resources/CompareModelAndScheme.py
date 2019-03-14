# -*- coding: utf-8 -*-
import clr
import System
import sys
sys.path.append("C:/Program Files (x86)/IronPython 2.7/Lib")

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

from operator import itemgetter
from itertools import groupby

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *

phase = filter(lambda p: p.Name == 'Стадия 1', doc.Phases)[0]

NONELIST = [None, '', ' ']
def getElemsByCat(bic):
  el = FilteredElementCollector(doc).OfCategory(bic).WhereElementIsNotElementType().ToElements()
  return filter(lambda e: e.LookupParameter('TAG').AsString() not in NONELIST, el)

def findElems(l):
  out = ''
  notInScheme = [e for e in l if e.LookupParameter('TAG').AsString() not in tagsDetails]
  inSpace = filter(lambda e: e.Space[phase] != None, notInScheme)
  outSpace = filter(lambda e: e.Space[phase] == None, notInScheme)
  if inSpace:
    elByRoomSort = sorted(inSpace, key = lambda e: e.Space[phase].Number)
    elByRoomGroup = [[x for x in g] for k,g in groupby(elByRoomSort, lambda e: e.Space[phase].Number)]
    out += l[0].Category.Name + ': \r\n'
    for group in elByRoomGroup:
      out += 'Space ' + str(group[0].Space[phase].Number) + ': \r\n' + '\r\n'.join([i.LookupParameter('TAG').AsString() for i in group]) + '\r\n'
  if outSpace:
    out += 'Not in space: \r\n' + '\r\n'.join([i.LookupParameter('TAG').AsString() for i in outSpace]) + '\r\n'
  return out


dTerm = getElemsByCat(BuiltInCategory.OST_DuctTerminal)
dAcc = getElemsByCat(BuiltInCategory.OST_DuctAccessory)
pAcc = getElemsByCat(BuiltInCategory.OST_PipeAccessory)
equip = getElemsByCat(BuiltInCategory.OST_MechanicalEquipment)
details = getElemsByCat(BuiltInCategory.OST_DetailComponents)

tagsElems = [i.LookupParameter('TAG').AsString() for i in dTerm] +\
  [i.LookupParameter('TAG').AsString() for i in dAcc] +\
  [i.LookupParameter('TAG').AsString() for i in pAcc] +\
  [i.LookupParameter('TAG').AsString() for i in equip]

tagsDetails = [i.LookupParameter('TAG').AsString() for i in details]

notEnough = 'Нет на схемах (но есть в модели): \r\n' + findElems(dTerm) + findElems(dAcc) +  findElems(pAcc) + findElems(equip)
excess = 'Лишнее на схемах(нет в модели): \r\n' + ','.join(list(set(tagsDetails)-set(tagsElems)))

class MainForm(Form):
  def __init__(self):
    self.InitializeComponent()
  def InitializeComponent(self):
    self._textBox1 = System.Windows.Forms.TextBox()
    self._textBox2 = System.Windows.Forms.TextBox()
    self.SuspendLayout()
    # 
    # textBox1
    # 
    self._textBox1.Location = System.Drawing.Point(10, 10)
    self._textBox1.Multiline = True
    self._textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    self._textBox1.Name = "textBox1"
    self._textBox1.Size = System.Drawing.Size(300, 200)
    # 
    # textBox2
    # 
    self._textBox2.Location = System.Drawing.Point(10, 220)
    self._textBox2.Multiline = True
    self._textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
    self._textBox2.Name = "textBox1"
    self._textBox2.Size = System.Drawing.Size(300, 200)
    # 
    # MainForm
    # 
    self.ClientSize = System.Drawing.Size(320, 430)
    self.Controls.Add(self._textBox1)
    self.Controls.Add(self._textBox2)
    self.Name = "MainForm"
    self.Text = "Compare model and scheme"
    self.ResumeLayout(False)
    self.PerformLayout()

    self._textBox1.Text = notEnough
    self._textBox2.Text = excess

MainForm().ShowDialog()
