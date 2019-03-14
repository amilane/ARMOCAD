# -*- coding: utf-8 -*-
import clr
import System
from math import radians

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *

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
elemsUser = []
for e in elems:
	try:
		a = e.Symbol.Family.IsUserCreated
	except:
		a = False
	if a:
		elemsUser.append(e)


class MainForm(Form):
	def __init__(self):
		self.InitializeComponent()
		
	def InitializeComponent(self):
		self._button1 = System.Windows.Forms.Button()
		self._textBox1 = System.Windows.Forms.TextBox()
		self.SuspendLayout()

		# button1
		self._button1.Location = System.Drawing.Point(140, 12)
		self._button1.Name = "button1"
		self._button1.Size = System.Drawing.Size(120, 23)
		self._button1.TabIndex = 2
		self._button1.Text = "Rotate"
		self._button1.UseVisualStyleBackColor = True
		self._button1.Click += self.Button1Click

		# textBox1
		self._textBox1.Location = System.Drawing.Point(12, 12)
		self._textBox1.Name = "textBox1"
		self._textBox1.Size = System.Drawing.Size(120, 23)
		self._textBox1.TabIndex = 1

		# MainForm
		self.ClientSize = System.Drawing.Size(280, 50)
		self.Controls.Add(self._textBox1)
		self.Controls.Add(self._button1)
		self.Name = "MainForm"
		self.Text = "Rotate Elements"
		self.TopMost = True
		self.ResumeLayout(False)
		self.PerformLayout()

	def Button1Click(self, sender, e):
		angle = -radians(int(self._textBox1.Text))
		t = Transaction(doc, "Rotate Elements")
		t.Start()
		for e in elemsUser:
			ref_location = e.Location.Point
			rot_axis = Line.CreateBound(ref_location, XYZ(ref_location.X, ref_location.Y, ref_location.Z+1.0))
			ElementTransformUtils.RotateElement(doc, e.Id, rot_axis, angle)
		t.Commit()

MainForm().ShowDialog()