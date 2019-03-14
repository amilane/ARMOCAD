'''
Программа заменяет текстовые значения выбранных параметров выбранных семейств.
Например:
На 3-м этаже здания расставлены щитки. Нужно скопировать их на 2-й этаж. Допустим на 3-м этаже имена щитков были ЩК-3.1, ЩК-3.2 и т.д.
При простом копировании на другой этаж эти имена сохранятся, а у Ревита нет стандартной команды 'Найти и заменить' для параметров семейств.
Поэтому выбираем щитки в которых мы хотим поменять имя (параметр 'Имя панели'), запускаем эту прогу, вписываем что мы хотим найти, на что заменить и - готово!
'''

#закрывает консоль которая иначе вылетает после работы программы  (убирать при вставке этого кода в C#)
# также не забыть при вставке через C# поменять кодировку на UTF8 with BOM
#__window__.Close()

# -*- coding: utf-8 -*-
# скрипт заменяет часть текста в параметрах
import clr
import System

from operator import itemgetter
from itertools import groupby

clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.DB.Electrical import *

from Autodesk.Revit.Creation import *

clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
from System.Drawing import *
from System.Windows.Forms import *
import sys

# (убирать при вставке этого кода в C#)
#doc = __revit__.ActiveUIDocument.Document
#uidoc = __revit__.ActiveUIDocument


# выбранные семейств
ids = uidoc.Selection.GetElementIds()
fams = []
idd = [str(i) for i in ids]
if idd == []:
	MessageBox.Show('Ничего не выбрано. Пожалуйста, выберите семейства в которых нужно провести поиск и замену значения параметра и перезапустите программу.', 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()
elif isinstance(idd, list) == True:
	fams = [doc.GetElement(ElementId(int(i))) for i in idd]
else:
	fams = doc.GetElement(ElementId(int(idd)))

# взять все параметры из выбранных семейств
params = [i.Parameters for i in fams]

# взять строковые параметры
paramTextNames = []
for i in params:
	for p in i:
		if str(p.StorageType) == 'String':
			paramTextNames.append([p, p.Definition.Name])
paramTextNames.sort(key=itemgetter(1))
paramTextNamesGroup = [[x for x in g] for k, g in groupby(paramTextNames, key=itemgetter(1))]

# взять только те параметры, которые есть во всех выбранных семействах
correctParams = []
for i in paramTextNamesGroup:
	if len(i) == len(fams):
		correctParams.append(i)
paramNames = [i[0][1] for i in correctParams]

'''
После редактирования окна в программе Sharp Develop не забыть вставить эту строчку в блок # selElements
self._selElements.DataSource = paramNames
'''

# Винформа
class MainForm(Form):
	def __init__(self):
		self.InitializeComponent()

	def InitializeComponent(self):
		self._selElements = System.Windows.Forms.ComboBox()
		self._CreateButt = System.Windows.Forms.Button()
		self._textBoxFrom = System.Windows.Forms.TextBox()
		self._textBoxTo = System.Windows.Forms.TextBox()
		self._Cancel_button = System.Windows.Forms.Button()
		self._label1 = System.Windows.Forms.Label()
		self._label2 = System.Windows.Forms.Label()
		self._label3 = System.Windows.Forms.Label()
		self.SuspendLayout()
		# 
		# selElements
		# 
		self._selElements.FormattingEnabled = True
		self._selElements.Location = System.Drawing.Point(13, 60)
		self._selElements.Name = 'selElements'
		self._selElements.Size = System.Drawing.Size(150, 21)
		self._selElements.TabIndex = 0
		self._selElements.SelectedIndexChanged += self.ElemsSelectedIndexChanged
		self._selElements.DataSource = paramNames
		# 
		# CreateButt
		# 
		self._CreateButt.Location = System.Drawing.Point(12, 221)
		self._CreateButt.Name = 'CreateButt'
		self._CreateButt.Size = System.Drawing.Size(120, 23)
		self._CreateButt.TabIndex = 3
		self._CreateButt.Text = 'Заменить и закрыть'
		self._CreateButt.UseVisualStyleBackColor = True
		self._CreateButt.Click += self.CreateButtClick
		# 
		# textBoxFrom
		# 
		self._textBoxFrom.Location = System.Drawing.Point(12, 117)
		self._textBoxFrom.Name = 'textBoxFrom'
		self._textBoxFrom.Size = System.Drawing.Size(120, 20)
		self._textBoxFrom.TabIndex = 4
		# 
		# textBoxTo
		# 
		self._textBoxTo.Location = System.Drawing.Point(13, 174)
		self._textBoxTo.Name = 'textBoxTo'
		self._textBoxTo.Size = System.Drawing.Size(120, 20)
		self._textBoxTo.TabIndex = 4
		# 
		# Cancel_button
		# 
		self._Cancel_button.Location = System.Drawing.Point(237, 221)
		self._Cancel_button.Name = 'Cancel_button'
		self._Cancel_button.Size = System.Drawing.Size(75, 23)
		self._Cancel_button.TabIndex = 5
		self._Cancel_button.Text = 'Cancel'
		self._Cancel_button.UseVisualStyleBackColor = True
		self._Cancel_button.Click += self.Cancel_buttonClick
		# 
		# label1
		# 
		self._label1.Location = System.Drawing.Point(13, 13)
		self._label1.Name = 'label1'
		self._label1.Size = System.Drawing.Size(148, 44)
		self._label1.TabIndex = 6
		self._label1.Text = 'Выберите параметр в котором будет произведёт поиск и замена:'
		# 
		# label2
		# 
		self._label2.Location = System.Drawing.Point(12, 97)
		self._label2.Name = 'label2'
		self._label2.Size = System.Drawing.Size(149, 17)
		self._label2.TabIndex = 7
		self._label2.Text = 'Найти:'
		# 
		# label3
		# 
		self._label3.Location = System.Drawing.Point(12, 154)
		self._label3.Name = 'label3'
		self._label3.Size = System.Drawing.Size(149, 17)
		self._label3.TabIndex = 8
		self._label3.Text = 'Заменить на:'
		# 
		# MainForm
		# 
		self.ClientSize = System.Drawing.Size(326, 261)
		self.Controls.Add(self._label3)
		self.Controls.Add(self._label2)
		self.Controls.Add(self._label1)
		self.Controls.Add(self._Cancel_button)
		self.Controls.Add(self._selElements)
		self.Controls.Add(self._textBoxFrom)
		self.Controls.Add(self._textBoxTo)
		self.Controls.Add(self._CreateButt)
		self.Name = 'MainForm'
		self.Text = 'Найти и заменить текст в параметре'
		self.ResumeLayout(False)
		self.PerformLayout()

	def ElemsSelectedIndexChanged(self, sender, e):
		pass

	def Cancel_buttonClick(self, sender, e):
		self.Close()
	
	def CreateButtClick(self, sender, e):

		# составляем список со значениями параметров
		paramValues = [i.LookupParameter(self._selElements.Text).AsString() for i in fams]
		# производим замену значений параметров
		newParamValues = [i.replace(self._textBoxFrom.Text, self._textBoxTo.Text) for i in paramValues]

		strnotfound = [] # вспомогательная переменная. Останется пустым списком, если строка везде найдена, или непустым списком, если где-то не найдено совпадений
		for i in newParamValues:
			if i.find(self._textBoxFrom.Text) == -1:
				strnotfound.append(1)
		# выдадим предупреждение:
		if len(strnotfound) <> 0:
			MessageBox.Show('Искомая строка не найдена у некоторых выбранных семейств. Замена была произведена только для семейств у которых совпадение было найдено.', 'предупреждение', MessageBoxButtons.OK, MessageBoxIcon.Asterisk)

		# Выдадим предупреждение, если пользователь ничего не ввёл в поле 'Найти:', если что-то ввёл - производим замену
		if self._textBoxFrom.Text == '':
			MessageBox.Show('Введите значение в поле Найти:', 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
		elif self._textBoxTo.Text == '':
			MessageBox.Show('Введите значение в поле Заменить на:', 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
		else:
			# записываем результаты замены в чертёж
			for i, k in zip(fams, newParamValues):
				t = Transaction(doc, 'Rename')
				t.Start()
				p = i.LookupParameter(self._selElements.Text)
				p.Set(k)
				t.Commit()
				self.Close()

		
MainForm().ShowDialog()