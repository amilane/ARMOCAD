'''
Запись одинаковых параметров.
Программа заменяет значение выбранного параметра, если он встречается у всех выбранных семейств.
Работает также и для одного выбранного семейства. Так что можно просто записывать значения его параметров из окошка данной программы. 
Это порой удобнее чем писать его значение в стандартной панели 'Свойства', где иногда слетает то, что ты только что написал.

Например удобно записывать параметр 'Принадлежность щиту' для разных семейств составляющих схему щита.

Работает только с параметрами, у которых тип данных - текст, число, целое число, логическое (в этом случае значение параметра в окошке программы должно быть 1 или 0)

'''


#закрывает консоль которая иначе вылетает после работы программы
#__window__.Close()


# сайт со всеми перекодировками текста https://2cyr.com/decode/?lang=ru
# Возможно перекодировка лучше всего из KOI-7

# О сортировке списков https://habrahabr.ru/post/138535/

#подгружаем нужные библиотеки
import clr
import System
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
clr.AddReference('RevitAPI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.ApplicationServices import Application
from System.Windows.Forms import *
from System.Drawing import *
import sys
import itertools
from operator import itemgetter
from datetime import datetime, date
from math import ceil
from itertools import groupby

# Объявляем глобальные переменные!
global Param_selected_name_100
global Param_selected_value_100
Param_selected_name_100 = None
Param_selected_value_100 = None

#doc = __revit__.ActiveUIDocument.Document
#uidoc = __revit__.ActiveUIDocument

''' создаём выборку. Пользователь выбирает нужные элементы'''
ids = uidoc.Selection.GetElementIds()

idd = [str(i) for i in ids]




# функция получения индексов одинаковых элементов в списке
# на входе: элемент который ищем, список в котором ищем. На выходе список с индексами найденных элементов. Например: [2, 4]. Если совпадений не найдено - на выходе пустой список: []
def Get_coincidence_in_list (search_element, search_list):
	index_list = []
	for n, i in enumerate(search_list):
		if i == search_element:
			index_list.append(n)
	return index_list


# функция удаляет элементы из списка по указанным индексам
# на входе: список нужных индексов (например: [2, 4],) и список из которого их будем удалять (например [1, 2, 3, 4, 5]). 
# На выходе: список без удалённых элементов (например [1, 2, 4]). 
# Внимание! Входящий список deleting_list переобъявляется!! 
# То есть то, что мы подали на вход, после работы этой функции уже не будет содержать удалённых элементов.
def Delete_indexed_elements_in_list (indexes_list, deleting_list):
	a = (len(deleting_list)-1)
	while a >= 0:
		for i in indexes_list:
			if a == i:
				deleting_list.pop(a)
		a = a - 1

	return deleting_list


'''функция для записи нужных данных в чертёж
обращение:
Transaction_sukhov (doc, 'Py', Py_sum, elems_calculation_table_for_scheme, 0)
где:
doc - текущий документ (объявлен в начале программы)
changing_parametr - изменяемый параметр в формате String. То есть тот параметр который нужно искать в выбранном элементе
element_to_write_down - элемент для записи. То есть данные которые нужно записать. Например число 20.
element_in_which_to_write_down - элемент в который будем записывать. То есть семейство. 
'''
def Transaction_sukhov_1 (doc, changing_parametr, element_to_write_down, element_in_which_to_write_down):
	t = Transaction(doc, 'Change changing_parametr')
	t.Start()
	element_in_which_to_write_down.LookupParameter(changing_parametr).Set(element_to_write_down)
	t.Commit()



#сообщение об ошибке которое должно вывестись в следующем модуле
error_text_in_window = 'Ничего не выбрано. Пожалуйста выберите семейства в которых Вы хотите изменить значения параметра.'
#если ничего не выбрано, выйти из программы
if idd == []: 
	MessageBox.Show(error_text_in_window, 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()

#если пользователь что-то выбрал, продолжаем
if isinstance(idd, list) == True:
	elems = [doc.GetElement(ElementId(int(i))) for i in idd]
else:
	elems = doc.GetElement(ElementId(int(idd)))

# Создадим список с уникальными семействами. То есть выбрано может быть много одинаковых семейств, но мы сделаем список только с неповторяющимися семействами
unique_families_names = []
Copy_unique_families_names = []
for i in elems:
	unique_families_names.append(i.Name)
	Copy_unique_families_names.append(i.Name)



Help_unique_families_names = []
for i in unique_families_names:
	for j in Copy_unique_families_names:
		if i == j:
			cur_indx = Get_coincidence_in_list (j, Copy_unique_families_names) # получаем индексы совпавших элементов
			Help_unique_families_names.append(j)	
			Delete_indexed_elements_in_list (cur_indx, Copy_unique_families_names) # удаляем совпавшие элементы из списка

# Получили список unique_families_names вида: ['Семейство 2 автоматический выключатель 4', 'Резервный автомат для щитков', 'Резервный автомат для ВРУ', 'Вводной автомат для щитков']
unique_families_names = []
for i in Help_unique_families_names:
	unique_families_names.append(i)


# Создадим список имён всех параметров выбранных элементов
all_params_names = []
Copy_all_params_names = []
Copy_elems = []
for i in elems:
	Copy_elems.append(i)

for i in unique_families_names:
	for j in Copy_elems:
		if i == j.Name:
			Copy_all_params_names.append([k.Definition.Name for k in j.Parameters])
			cur_indx = Get_coincidence_in_list (j.Name, [l.Name for l in Copy_elems]) # получаем индексы совпавших элементов
			Delete_indexed_elements_in_list (cur_indx, Copy_elems) # удаляем совпавшие элементы из списка

all_params_names = [item for sublist in Copy_all_params_names for item in sublist] # раскатываем список, чтобы не было подсписков



# Достанем из этого спсика all_params_names только повторяющиеся несколько раз имена параметров
unique_params_names = []
Copy_all_params_names = []
for i in all_params_names:
	Copy_all_params_names.append(i)

for i in all_params_names:
	for j in Copy_all_params_names:
		if i == j:
			cur_indx = Get_coincidence_in_list (j, Copy_all_params_names) # получаем индексы совпавших элементов
			if len(cur_indx) == len(unique_families_names): # найденный параметр должен повторяться столько раз, сколько разных семейств выбрано. Другими словами, это и будет значить, что такой параметр присутствует у каждого из выбранных семейств.
				unique_params_names.append(j)
			Delete_indexed_elements_in_list (cur_indx, Copy_all_params_names) # удаляем совпавшие элементы из списка

unique_params_names.sort() # сортируем список

'''
Эту строчку нужно вставить в соответствующий блок кода главного окна программы. 
Она отвечают за первоначальное заполнение списка имён щитков из проекта.
self._comboBox_unique_params_names.Items.AddRange(System.Array[System.Object](unique_params_names)) - относится к блоку # comboBox_unique_params_names
'''


class Change_common_params(Form):
	def __init__(self):
		self.InitializeComponent()
	
	def InitializeComponent(self):
		self._Write_button = System.Windows.Forms.Button()
		self._Cancel_button = System.Windows.Forms.Button()
		self._comboBox_unique_params_names = System.Windows.Forms.ComboBox()
		self._label1 = System.Windows.Forms.Label()
		self._textBox_ParamValue = System.Windows.Forms.TextBox()
		self._label2 = System.Windows.Forms.Label()
		self.SuspendLayout()
		# 
		# Write_button
		# 
		self._Write_button.Location = System.Drawing.Point(27, 250)
		self._Write_button.Name = 'Write_button'
		self._Write_button.Size = System.Drawing.Size(75, 23)
		self._Write_button.TabIndex = 0
		self._Write_button.Text = 'Записать'
		self._Write_button.UseVisualStyleBackColor = True
		self._Write_button.Click += self.Write_buttonClick
		# 
		# Cancel_button
		# 
		self._Cancel_button.Location = System.Drawing.Point(186, 250)
		self._Cancel_button.Name = 'Cancel_button'
		self._Cancel_button.Size = System.Drawing.Size(75, 23)
		self._Cancel_button.TabIndex = 1
		self._Cancel_button.Text = 'Cancel'
		self._Cancel_button.UseVisualStyleBackColor = True
		self._Cancel_button.Click += self.Cancel_buttonClick
		# 
		# comboBox_unique_params_names
		# 
		self._comboBox_unique_params_names.FormattingEnabled = True
		self._comboBox_unique_params_names.Location = System.Drawing.Point(27, 89)
		self._comboBox_unique_params_names.Name = 'comboBox_unique_params_names'
		self._comboBox_unique_params_names.Size = System.Drawing.Size(234, 21)
		self._comboBox_unique_params_names.TabIndex = 2
		self._comboBox_unique_params_names.SelectedIndexChanged += self.ComboBox_unique_params_namesSelectedIndexChanged
		self._comboBox_unique_params_names.Items.AddRange(System.Array[System.Object](unique_params_names))
		# 
		# label1
		# 
		self._label1.Location = System.Drawing.Point(27, 22)
		self._label1.Name = 'label1'
		self._label1.Size = System.Drawing.Size(234, 64)
		self._label1.TabIndex = 3
		self._label1.Text = 'У выбранных элементов совпадают следующие параметры. Выберите среди них один, в который Вы хотите записать новое значение:'
		# 
		# textBox_ParamValue
		# 
		self._textBox_ParamValue.Location = System.Drawing.Point(27, 157)
		self._textBox_ParamValue.Multiline = True
		self._textBox_ParamValue.Name = 'textBox_ParamValue'
		self._textBox_ParamValue.Size = System.Drawing.Size(234, 69)
		self._textBox_ParamValue.TabIndex = 5
		self._textBox_ParamValue.TextChanged += self.TextBox_ParamValueTextChanged
		# 
		# label2
		# 
		self._label2.Location = System.Drawing.Point(27, 124)
		self._label2.Name = 'label2'
		self._label2.Size = System.Drawing.Size(234, 30)
		self._label2.TabIndex = 6
		self._label2.Text = 'Введите новое значение для выбранного параметра:'
		# 
		# Change_common_params
		# 
		self.ClientSize = System.Drawing.Size(287, 289)
		self.Controls.Add(self._label2)
		self.Controls.Add(self._textBox_ParamValue)
		self.Controls.Add(self._label1)
		self.Controls.Add(self._comboBox_unique_params_names)
		self.Controls.Add(self._Cancel_button)
		self.Controls.Add(self._Write_button)
		self.Name = 'Change_common_params'
		self.Text = 'Заменить параметры'
		self.ResumeLayout(False)
		self.PerformLayout()




	def Write_buttonClick(self, sender, e):
		global Param_selected_name_100
		global Param_selected_value_100
		Param_selected_name_100 = self._comboBox_unique_params_names.Text
		Param_selected_value_100 = self._textBox_ParamValue.Text
		self.Close()

		
	def Cancel_buttonClick(self, sender, e):
		self.Close()

	def ComboBox_unique_params_namesSelectedIndexChanged(self, sender, e):
		pass

	def TextBox_ParamValueTextChanged(self, sender, e):
		pass

Change_common_params().ShowDialog()


# записываем значение параметра в выбранные элементы
er_hlp = [] # вспомогательный список для вывода или не вывода предупреждения пользователю

if Param_selected_name_100 is not None and Param_selected_value_100 is not None: # это проверка на то, что пользователь в окне нажал именно кнопку 'Записать'
	for i in elems:
		for j in [k.Definition for k in i.Parameters]:
			if j.Name == Param_selected_name_100: # если совпало имя параметра
				if j.ParameterType == ParameterType.Number: # если тип данных параметра число
					Transaction_sukhov_1 (doc, Param_selected_name_100, float(Param_selected_value_100), i)
				elif j.ParameterType == ParameterType.Text: # если тип данных параметра текст
					Transaction_sukhov_1 (doc, Param_selected_name_100, Param_selected_value_100, i)
				elif j.ParameterType == ParameterType.Integer: # если тип данных параметра целое
					Transaction_sukhov_1 (doc, Param_selected_name_100, int(Param_selected_value_100), i)
				elif j.ParameterType == ParameterType.YesNo: # если тип данных параметра логическое (в этом случе нужно вписывать целое число 1 или 0)
					Transaction_sukhov_1 (doc, Param_selected_name_100, int(Param_selected_value_100), i)
				else:
					er_hlp.append(1)
					
if len(er_hlp) > 0:
	MessageBox.Show('Данные не были записаны! Скорее всего проблема с типом данных выбранного параметра. Программа работает только с данными типа: текст, число, целое, логическое (в этом случае значение параметра в окошке программы должно быть 1 или 0).', 'Предупреждение', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)



'''
i.LookupParameter('Макс.откл.способность (кА)')
<Autodesk.Revit.DB.Parameter object at 0x000000000000014A [Autodesk.Revit.DB.Parameter]>
ara = i.LookupParameter('Макс.откл.способность (кА)')

ara.Definition.UnitType
Autodesk.Revit.DB.UnitType.UT_Number

ara.Definition.ParameterType
Autodesk.Revit.DB.ParameterType.Number

ara1 = i.LookupParameter('Принадлежность щиту')
Autodesk.Revit.DB.ParameterType.Text

Если брать тип данных таким макаром:
ara1.Definition.ParameterType.value__
То на текст прога выдаст
1
А на целое число
2
А на дробное число
3
А на флажок да/нет
10

i.LookupParameter('Кол-во полюсов').Definition.ParameterType.value__
2
i.LookupParameter('3-фазный автомат').Definition.ParameterType.value__
10

Во как надо проверять тип данных:
i.LookupParameter('Кол-во полюсов').Definition.ParameterType
Autodesk.Revit.DB.ParameterType.Integer

i.LookupParameter('3-фазный автомат').Definition.ParameterType
Autodesk.Revit.DB.ParameterType.YesNo

i.LookupParameter('Принадлежность щиту').Definition.ParameterType == ParameterType.Text
True

 i.LookupParameter('Макс.откл.способность (кА)').Definition.ParameterType
Autodesk.Revit.DB.ParameterType.Number
'''


'''
Вытаскивает значение из-под класса
https://pythonworld.ru/osnovy/obektno-orientirovannoe-programmirovanie-obshhee-predstavlenie.html

class A:
	def g(self): # self - обязательный аргумент, содержащий в себе экземпляр
		return 'hello world'

a = A()
a.g()

ara2 = Change_common_params()

'''


'''

	def Vasa(self):
		ara = self._comboBox_unique_params_names.Text
		ara1 = ['aaa']
		return ara

'''

'''
Param_selected_name = 'Принадлежность щиту'
Param_selected_value = 'qqqqqq'
'''