# -*- coding: utf-8 -*-
'''
Программа расставляет светильники по трем точкам, описывающим угол. 
'''

#закрывает консоль которая иначе вылетает после работы программы
#__window__.Close()

import clr
from math import *
import System

import sys
pyt_path = r'C:\Program Files (x86)\IronPython 2.7\Lib'
sys.path.append(pyt_path)


clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Structure import StructuralType
#clr.AddReference('RevitServices')
#import RevitServices
from Autodesk.Revit.Creation import *
#from RevitServices.Persistence import DocumentManager
#from RevitServices.Transactions import TransactionManager

#библиотеки для окошек
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *

#doc = __revit__.ActiveUIDocument.Document
#uidoc = __revit__.ActiveUIDocument

if doc.ActiveView.ViewType != ViewType.FloorPlan and doc.ActiveView.ViewType != ViewType.CeilingPlan:
	MessageBox.Show("Перейдите на план этажа или план потолка", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()

# Собираем нужные нам категории семейств с которыми может работать программа
Categories = ['Осветительные приборы', 'Датчики', 'Системы пожарной сигнализации', 'Спринклеры']

# Собираем светильники из диспетчера, вносим их имена в список lightNames
lights = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_LightingFixtures).WhereElementIsElementType().ToElements()
lightsNames = ['{0}: {1}'.format(i.Family.Name, i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) for i in lights]

# Собираем уровни из проекта, их имена и Id
levels = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements()
levelsNames, levelsIds = [], []
for i in levels:
	levelsNames.append(i.Name)
	levelsIds.append(i.Id)


# Фильтрация элементов по значению параметра (применена на уровнях)
def FilterElementByName(bip, item):
	nameParamId = ElementId(bip)
	pvp = ParameterValueProvider(nameParamId)
	evaluator = FilterStringContains()
	rule = FilterStringRule(pvp, evaluator, item, False)
	paramFilter = ElementParameterFilter(rule)
	selectElement = FilteredElementCollector(doc).WherePasses(paramFilter).ToElements()
	return selectElement

# Выбор точек на плане
A = uidoc.Selection.PickPoint("Выберите первую точку")
D = uidoc.Selection.PickPoint("Выберите угловую точку")
C = uidoc.Selection.PickPoint("Выберите последнюю точку")

'''
Если хочется подредакировать вид окна в SharpDevelop, то код окна нужно выделить и скопировать туда начиная со строки: 'class MainForm(Form):' и до объявления функций.
Но SharpDevelop может не создать окошко по скопированному коду, а ругаться что что0то не так. Это происходит потому что мы уже в Visual Studio Code сами вставили какие-то куски кода в чистый код окна.
Поэтому нужно их удалить, отредакировать окошко, а потом вставить на место, чтобы программа заработала.
В нашем случае такие строки, которые нужно убирать в SharpDevelop, но возвращать в Visual Studio Code приведены ниже:

Эти строчки нужно вставить в соответствующие блоки кода главного окна программы. 
Они отвечают за первоначальное заполнение списков семейств светильников и уровней из проекта.
self._lightBox.DataSource = lightsNames - относится к блоку # lightBox
self._CategorycheckedListBox1.DataSource = Categories - относится к блоку # _CategorycheckedListBox1

Это события, которые происходят при уходе фокуса (Tab) из данных текстовых полей.
self._lamps.Leave += self.LampsFocusLeave - относится к блоку # lamps
self._rows.Leave += self.RowsFocusLeave  - относится к блоку # rows
'''

'''
Полезные сайты по Винформам:
https://technet.microsoft.com/ru-ru/library/system.windows.forms.textbox(v=vs.100).aspx
https://msdn.microsoft.com/ru-ru/library/system.windows.forms.textbox_events(v=vs.110).aspx
'''

# Винформа
class MainForm(Form):
	def __init__(self):
		self.InitializeComponent()
	
	def InitializeComponent(self):
		self._components = System.ComponentModel.Container()
		self._lightBox = System.Windows.Forms.ComboBox()
		self._button1 = System.Windows.Forms.Button()
		self._label1 = System.Windows.Forms.Label()
		self._offset = System.Windows.Forms.TextBox()
		self._label7 = System.Windows.Forms.Label()
		self._label6 = System.Windows.Forms.Label()
		self._rotation = System.Windows.Forms.TextBox()
		self._label8 = System.Windows.Forms.Label()
		self._rows = System.Windows.Forms.TextBox()
		self._label9 = System.Windows.Forms.Label()
		self._lamps = System.Windows.Forms.TextBox()
		self._groupBox1 = System.Windows.Forms.GroupBox()
		self._Cancel_button = System.Windows.Forms.Button()
		self._errorProvider1 = System.Windows.Forms.ErrorProvider(self._components)
		self._errorProvider2 = System.Windows.Forms.ErrorProvider(self._components)
		self._CategorycheckedListBox1 = System.Windows.Forms.CheckedListBox()
		self._label2 = System.Windows.Forms.Label()
		self._textBox1 = System.Windows.Forms.TextBox()
		self._groupBox1.SuspendLayout()
		self._errorProvider1.BeginInit()
		self._errorProvider2.BeginInit()
		self.SuspendLayout()
		# 
		# lightBox
		# 
		self._lightBox.DropDownWidth = 300
		self._lightBox.FormattingEnabled = True
		self._lightBox.Location = System.Drawing.Point(7, 304)
		self._lightBox.Name = "lightBox"
		self._lightBox.Size = System.Drawing.Size(286, 21)
		self._lightBox.TabIndex = 4
		self._lightBox.SelectedIndexChanged += self.LightBoxSelectedIndexChanged
		self._lightBox.DataSource = lightsNames
		# 
		# button1
		# 
		self._button1.Location = System.Drawing.Point(6, 352)
		self._button1.Name = "button1"
		self._button1.Size = System.Drawing.Size(75, 23)
		self._button1.TabIndex = 6
		self._button1.Text = "Расставить"
		self._button1.UseVisualStyleBackColor = True
		self._button1.Click += self.Button1Click
		# 
		# label1
		# 
		self._label1.Location = System.Drawing.Point(5, 275)
		self._label1.Name = "label1"
		self._label1.Size = System.Drawing.Size(76, 26)
		self._label1.TabIndex = 5
		self._label1.Text = "Семейство"
		self._label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		# 
		# offset
		# 
		self._offset.AcceptsTab = True
		self._offset.Location = System.Drawing.Point(6, 252)
		self._offset.Name = "offset"
		self._offset.Size = System.Drawing.Size(120, 20)
		self._offset.TabIndex = 3
		self._offset.Text = "2500"
		self._offset.TextChanged += self.OffsetTextChanged
		# 
		# label7
		# 
		self._label7.Location = System.Drawing.Point(170, 218)
		self._label7.Name = "label7"
		self._label7.Size = System.Drawing.Size(135, 23)
		self._label7.TabIndex = 13
		self._label7.Text = "Вращение вокруг оси Z"
		self._label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		# 
		# label6
		# 
		self._label6.Location = System.Drawing.Point(5, 218)
		self._label6.Name = "label6"
		self._label6.Size = System.Drawing.Size(129, 31)
		self._label6.TabIndex = 12
		self._label6.Text = "Высота установки от пола"
		self._label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		# 
		# rotation
		# 
		self._rotation.AcceptsTab = True
		self._rotation.Location = System.Drawing.Point(171, 252)
		self._rotation.Name = "rotation"
		self._rotation.Size = System.Drawing.Size(121, 20)
		self._rotation.TabIndex = 5
		self._rotation.Text = "0"
		self._rotation.TextChanged += self.RotationTextChanged
		# 
		# label8
		# 
		self._label8.Location = System.Drawing.Point(7, 157)
		self._label8.Name = "label8"
		self._label8.Size = System.Drawing.Size(130, 35)
		self._label8.TabIndex = 15
		self._label8.Text = "Количесво рядов"
		self._label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		# 
		# rows
		# 
		self._rows.AcceptsTab = True
		self._rows.Location = System.Drawing.Point(7, 195)
		self._rows.Name = "rows"
		self._rows.Size = System.Drawing.Size(119, 20)
		self._rows.TabIndex = 1
		self._rows.Text = "1"
		self._rows.TextChanged += self.RowsTextChanged
		self._rows.Leave += self.RowsFocusLeave
		# 
		# label9
		# 
		self._label9.Location = System.Drawing.Point(171, 159)
		self._label9.Name = "label9"
		self._label9.Size = System.Drawing.Size(135, 33)
		self._label9.TabIndex = 17
		self._label9.Text = "Общее количество"
		self._label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
		# 
		# lamps
		# 
		self._lamps.AcceptsTab = True
		self._lamps.Location = System.Drawing.Point(171, 195)
		self._lamps.Name = "lamps"
		self._lamps.Size = System.Drawing.Size(119, 20)
		self._lamps.TabIndex = 2
		self._lamps.Text = "1"
		self._lamps.TextChanged += self.LampsTextChanged
		self._lamps.Leave += self.LampsFocusLeave
		# 
		# groupBox1
		# 
		self._groupBox1.Controls.Add(self._textBox1)
		self._groupBox1.Controls.Add(self._label2)
		self._groupBox1.Controls.Add(self._Cancel_button)
		self._groupBox1.Controls.Add(self._CategorycheckedListBox1)
		self._groupBox1.Controls.Add(self._button1)
		self._groupBox1.Controls.Add(self._label1)
		self._groupBox1.Controls.Add(self._lightBox)
		self._groupBox1.Controls.Add(self._offset)
		self._groupBox1.Controls.Add(self._label6)
		self._groupBox1.Controls.Add(self._rotation)
		self._groupBox1.Controls.Add(self._lamps)
		self._groupBox1.Controls.Add(self._label7)
		self._groupBox1.Controls.Add(self._label9)
		self._groupBox1.Controls.Add(self._rows)
		self._groupBox1.Controls.Add(self._label8)
		self._groupBox1.Location = System.Drawing.Point(7, 11)
		self._groupBox1.Name = "groupBox1"
		self._groupBox1.Size = System.Drawing.Size(310, 381)
		self._groupBox1.TabIndex = 19
		self._groupBox1.TabStop = False
		self._groupBox1.Text = "Введите данные:"
		# 
		# Cancel_button
		# 
		self._Cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel
		self._Cancel_button.Location = System.Drawing.Point(217, 352)
		self._Cancel_button.Name = "Cancel_button"
		self._Cancel_button.Size = System.Drawing.Size(75, 23)
		self._Cancel_button.TabIndex = 7
		self._Cancel_button.Text = "Cancel"
		self._Cancel_button.UseVisualStyleBackColor = True
		self._Cancel_button.Click += self.Cancel_buttonClick
		# 
		# errorProvider1
		# 
		self._errorProvider1.ContainerControl = self
		# 
		# errorProvider2
		# 
		self._errorProvider2.ContainerControl = self
		# 
		# CategorycheckedListBox1
		# 
		self._CategorycheckedListBox1.CheckOnClick = True
		self._CategorycheckedListBox1.FormattingEnabled = True
		self._CategorycheckedListBox1.HorizontalScrollbar = True
		self._CategorycheckedListBox1.Location = System.Drawing.Point(5, 47)
		self._CategorycheckedListBox1.Name = "CategorycheckedListBox1"
		self._CategorycheckedListBox1.Size = System.Drawing.Size(286, 109)
		self._CategorycheckedListBox1.TabIndex = 18
		self._CategorycheckedListBox1.SelectedIndexChanged += self.CategorycheckedListBox1SelectedIndexChanged
		self._CategorycheckedListBox1.DataSource = Categories
		# 
		# label2
		# 
		self._label2.Location = System.Drawing.Point(5, 26)
		self._label2.Name = "label2"
		self._label2.Size = System.Drawing.Size(129, 18)
		self._label2.TabIndex = 19
		self._label2.Text = "Категория семейств"
		# 
		# textBox1
		# 
		self._textBox1.Location = System.Drawing.Point(120, 352)
		self._textBox1.Name = "textBox1"
		self._textBox1.Size = System.Drawing.Size(61, 20)
		self._textBox1.TabIndex = 20
		self._textBox1.Visible = False
		# 
		# MainForm
		# 
		self.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit
		self.CancelButton = self._Cancel_button
		self.ClientSize = System.Drawing.Size(324, 404)
		self.Controls.Add(self._groupBox1)
		self.Name = "MainForm"
		self.Text = "Расстановка семейств"
		self.Load += self.MainFormLoad
		self._groupBox1.ResumeLayout(False)
		self._groupBox1.PerformLayout()
		self._errorProvider1.EndInit()
		self._errorProvider2.EndInit()
		self.ResumeLayout(False)


	def MainFormLoad(self, sender, e):
		self._CategorycheckedListBox1.SetItemChecked(0, True) # При загрузке формы выствляем флажок по умолчанию
		self._CategorycheckedListBox1.Select() # Устанавливаем фокус в этот лист бокс (нужно в дальнейшем в проге)

	def CategorycheckedListBox1SelectedIndexChanged(self, sender, e):
		# Следующий код следит за тем чтобы в чэк боксе можно было выбрать только одну галочку
		if (self._CategorycheckedListBox1.CheckedItems.Count > 1):
			a = 0
			while a < self._CategorycheckedListBox1.Items.Count:
				self._CategorycheckedListBox1.SetItemChecked(a, False)
				a = a + 1
			self._CategorycheckedListBox1.SetItemChecked(self._CategorycheckedListBox1.SelectedIndex, True)
			# self._CategorycheckedListBox1.SelectedIndex - индекс выбранного элемента в списке значений начиная с 0. Т.е. первая строчка - 0, вторая - 1 и т.д.
		# Следующий код заполняет список с семействами которые нужно расставить в зависимости от того какую категорию семейств выбрал пользователь
		if self._CategorycheckedListBox1.GetItemChecked(0):
			self._lightBox.DataSource = lightsNames
		elif self._CategorycheckedListBox1.GetItemChecked(1): # Собираем остальные категории с которыми работает программа
			self._lightBox.DataSource = ['{0}: {1}'.format(i.Family.Name, i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) for i in FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DataDevices).WhereElementIsElementType().ToElements()]
		elif self._CategorycheckedListBox1.GetItemChecked(2): # Собираем остальные категории с которыми работает программа
			self._lightBox.DataSource = ['{0}: {1}'.format(i.Family.Name, i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) for i in FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_FireAlarmDevices).WhereElementIsElementType().ToElements()]
		elif self._CategorycheckedListBox1.GetItemChecked(3): # Собираем остальные категории с которыми работает программа
			self._lightBox.DataSource = ['{0}: {1}'.format(i.Family.Name, i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString()) for i in FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sprinklers).WhereElementIsElementType().ToElements()]


	def CategorycheckedListBox1FocusLeave(self, sender, e):
		pass

	def LevelBoxSelectedIndexChanged(self, sender, e):
		pass

	def LightBoxSelectedIndexChanged(self, sender, e):
		pass

	def OffsetTextChanged(self, sender, e):
		pass

	def RotationTextChanged(self, sender, e):
		pass

# функция при изменении текстового значения в поле
	def RowsTextChanged(self, sender, e):
		if int(self._rows.Text) <= 0:
			self._errorProvider1.SetError(self._rows, 'Количество рядов не может быть меньше или равно нулю')
		elif int(self._rows.Text) > 0:
			self._errorProvider1.Clear()

# функция действия при уходе фокуса (по Tab) из данного текстового поля
	def RowsFocusLeave(self, sender, e):
		if int(self._rows.Text) <= 0:
			self._errorProvider1.SetError(self._rows, 'Количество рядов не может быть меньше или равно нулю')
		elif int(self._rows.Text) > 0:
			self._errorProvider1.Clear()

# функция при изменении текстового значения в поле
	def LampsTextChanged(self, sender, e):
		if int(self._rows.Text) > int(self._lamps.Text):
			self._errorProvider1.SetError(self._rows, 'Количество рядов не может быть больше чем количество семейств')
		if int(self._rows.Text) <= int(self._lamps.Text):
			self._errorProvider1.Clear()
		if (not (float(self._lamps.Text) / float(self._rows.Text)).is_integer()):
			self._errorProvider2.SetError(self._lamps, 'Количество семейств должно быть кратным количеству рядов')
		if (float(self._lamps.Text) / float(self._rows.Text)).is_integer():
			self._errorProvider2.Clear()
		
# функция действия при уходе фокуса (по Tab) из данного текстового поля
	def LampsFocusLeave(self, sender, e):
		if int(self._rows.Text) > int(self._lamps.Text):
			self._errorProvider1.SetError(self._rows, 'Количество рядов не может быть больше чем количество семейств')
		if int(self._rows.Text) <= int(self._lamps.Text):
			self._errorProvider1.Clear()
		if (not (float(self._lamps.Text) / float(self._rows.Text)).is_integer()):
			self._errorProvider2.SetError(self._lamps, 'Количество семейств должно быть кратным количеству рядов')
		if (float(self._lamps.Text) / float(self._rows.Text)).is_integer():
			self._errorProvider2.Clear()


	def Cancel_buttonClick(self, sender, e):
		#self._textBox1.Text = str(self._CategorycheckedListBox1.SelectedIndex)
		self.Close()

	def Button1Click(self, sender, e):

		#selectLevel = FilterElementByName(BuiltInParameter.DATUM_TEXT, self._levelBox.SelectedItem)[0]

		# Определяем какую категорию семейств выбрал пользователь
		if self._CategorycheckedListBox1.GetItemChecked(0):
			famselected = None
			famselected = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_LightingFixtures).WhereElementIsElementType().ToElements()
		elif self._CategorycheckedListBox1.GetItemChecked(1):
			famselected = None
			famselected = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DataDevices).WhereElementIsElementType().ToElements()
		elif self._CategorycheckedListBox1.GetItemChecked(2): # Собираем остальные категории с которыми работает программа
			famselected = None
			famselected = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_FireAlarmDevices).WhereElementIsElementType().ToElements()
		elif self._CategorycheckedListBox1.GetItemChecked(3): # Собираем остальные категории с которыми работает программа
			famselected = None
			famselected = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sprinklers).WhereElementIsElementType().ToElements()

		# Вытаскиваем семейство из ранее собранных по имени из combobox'a 
		for i in famselected:
			selectName = '{0}: {1}'.format(i.Family.Name, i.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString())
			if selectName == self._lightBox.SelectedItem:
				selectLight = i
							

		lampPoints = []
		finalLamps = []
	
	
		# Количество рядов вдоль длинной стороны, количество рядов вдоль короткой
		maxrow = int(self._rows.Text) * 2
		minrow = int(self._lamps.Text) / int(self._rows.Text) * 2

		
		# Определяем, какая из сторон длинная, а какая короткая
		l1 = sqrt((A.X-D.X)**2+(A.Y-D.Y)**2)
		l2 = sqrt((C.X-D.X)**2+(C.Y-D.Y)**2)
		if l1 > l2:
			maxline = [A, D, l1]
			minline = [C, D, l2]
		else:
			maxline = [C, D, l2]
			minline = [A, D, l1]

		# Вектора, образованные линиями
		vectorLong = XYZ(maxline[0].X - maxline[1].X, maxline[0].Y - maxline[1].Y, 0)
		vectorShort = XYZ(minline[0].X - minline[1].X, minline[0].Y - minline[1].Y, 0)
		
		# Определяем точки, лежащие на линиях по кол-ву рядов
		minpoints = [XYZ(minline[1].X + (minline[0].X - minline[1].X)/maxrow * 2 * i + (minline[0].X - minline[1].X)/maxrow, minline[1].Y + (minline[0].Y - minline[1].Y)/maxrow * 2 * i + (minline[0].Y - minline[1].Y)/maxrow, 0) for i in range(0, maxrow/2)]	
		maxpoints = [XYZ(maxline[1].X + (maxline[0].X - maxline[1].X)/minrow * 2 * i + (maxline[0].X - maxline[1].X)/minrow, maxline[1].Y + (maxline[0].Y - maxline[1].Y)/minrow * 2 * i + (maxline[0].Y - maxline[1].Y)/minrow, 0) for i in range(0, minrow/2)]

		# Рассчитываем точки внутри 'прямоугольника' образованного линиями (по ним будут ставиться светильники)
		#zEnd = selectLevel.Elevation + int(self._offset.Text)*0.0032808398950131
		zEnd = int(self._offset.Text)*0.0032808398950131 # здесь zEnd это просто пользовательская высота
		for a in minpoints:
			for c in maxpoints:
				x0 = (a.X + c.X)/2
				y0 = (a.Y + c.Y)/2
				xP = 2 * x0 - D.X
				yP = 2 * y0 - D.Y
				P = XYZ(xP, yP, zEnd)
				lampPoints.append(P)

		if selectLight.Family.FamilyPlacementType == FamilyPlacementType.WorkPlaneBased:
			# Создание опорных плоскостей, определяющих высоту светильников от уровня
			zEnd = doc.ActiveView.GenLevel.Elevation + int(self._offset.Text)*0.0032808398950131 # здесь zEnd это текущий уровень плюс пользовательская высота
			bubbleEnd = XYZ(0, 1, zEnd)
			freeEnd = XYZ(0, -5, zEnd)   
			cutVec = XYZ(1, 0, 0)
			refPlanes = []
			t = Transaction(doc, 'CreateRefPlane')
			t.Start()
			refPlane = doc.Create.NewReferencePlane(bubbleEnd, freeEnd, cutVec, doc.ActiveView)
			refPlanes.append(refPlane)
			t.Commit()

			# Ставим светильники по точкам, с поворотом параллельно одной из сторон (в данном случае по короткой стороне)		
			Dir = XYZ(vectorShort.X, vectorShort.Y , 0)
			t = Transaction(doc, 'Create Lamps')
			t.Start()
			for p in lampPoints:
				if not selectLight.IsActive:
					selectLight.Activate()
					doc.Regenerate()
				newLight = doc.Create.NewFamilyInstance(Reference(refPlane), p, Dir, selectLight)
				finalLamps.append(newLight)
			t.Commit()

			# Удаляем Опорные плоскости
			t = Transaction(doc, 'Delete RefPlanes')
			t.Start()
			for e in refPlanes:
				doc.Delete(e.Id)
			t.Commit()

		elif selectLight.Family.FamilyPlacementType == FamilyPlacementType.OneLevelBased:
			t = Transaction(doc, 'Create Lamps')
			t.Start()
			for p in lampPoints:
				if not selectLight.IsActive:
					selectLight.Activate()
					doc.Regenerate()
				newLight = doc.Create.NewFamilyInstance(p, selectLight, doc.ActiveView.GenLevel, StructuralType.NonStructural)
				finalLamps.append(newLight)
			t.Commit()			

		# Угол пользовательского поворота светильников
		angle = radians(int(self._rotation.Text))

		# Поворачиваем созданные светильники на пользовательский угол
		t=Transaction(doc, 'Rotate lamps')
		t.Start()
		for e in finalLamps:
			ref_location = e.Location.Point
			rot_axis = Line.CreateBound(ref_location, XYZ(ref_location.X, ref_location.Y, ref_location.Z+1.0))
			ElementTransformUtils.RotateElement(doc, e.Id, rot_axis, angle)
		t.Commit()

		# Проверяем видны ли созданные светильники на текущем виде
		if finalLamps[0].Id not in FilteredElementCollector(doc, doc.ActiveView.Id).ToElementIds():
			MessageBox.Show("Ни один из созданных элементов не является видимым на текущем виде. Проверьте активный вид, его параметры и настройку видимости, а также все фрагменты планов и их параметры.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Information)


		self.Close()
		
MainForm().ShowDialog()

