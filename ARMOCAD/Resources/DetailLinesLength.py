'''
Программа собирает длины выбранных пользователем линий детализации и суммирует их.
'''

# Не забыть пересохранить в кодировке UTF-8 with BOM и закомментить __window__.Close() при запуске через PyRevit


#закрывает консоль которая иначе вылетает после работы программы
#__window__.Close()


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
from datetime import date

#doc = __revit__.ActiveUIDocument.Document
#uidoc = __revit__.ActiveUIDocument

# Ставим защиту. Если текущая дата позже контрольной даты когда программма должна перестать работать, то вывести
# предупреждение и выкинуть пользователя из программы
'''
control_date = date(2018, 12, 28) # формат: год, месяц, день
if 5 <= (control_date - date.today()).days <= 7:
	MessageBox.Show('Срок действия программы истекает через ' + str((control_date - date.today()).days) + ' дней. Обратитесь к разработчику. paarsu@mail.ru', 'Срок лицензии истёкает', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
elif 2 <= (control_date - date.today()).days <= 4:
	MessageBox.Show('Срок действия программы истекает через ' + str((control_date - date.today()).days) + ' дня. Обратитесь к разработчику. paarsu@mail.ru', 'Срок лицензии истёкает', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
elif (control_date - date.today()).days == 1:
	MessageBox.Show('Срок действия программы истекает послезавтра. Обратитесь к разработчику. paarsu@mail.ru', 'Срок лицензии истёкает', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
elif (control_date - date.today()).days == 0:
	MessageBox.Show('Срок действия программы истекает завтра. Обратитесь к разработчику. paarsu@mail.ru', 'Срок лицензии истёкает', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
elif date.today() > control_date:
	MessageBox.Show('Истёк срок действия программы. Обратитесь к разработчику. paarsu@mail.ru', 'Срок лицензии истёк', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()
'''

''' создаём выборку. Пользователь выбирает нужные элементы'''
ids = uidoc.Selection.GetElementIds()

idd = [str(i) for i in ids]




#сообщение об ошибке которое должно вывестись в следующем модуле
error_text_in_window = 'Ничего не выбрано. Пожалуйста выберите линии детализации, сумму длин которых Вы хотите получить'
#если ничего не выбрано, выйти из программы
if idd == []: 
	MessageBox.Show(error_text_in_window, 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()

#если пользователь что-то выбрал, продолжаем
if isinstance(idd, list) == True:
	elems = [doc.GetElement(ElementId(int(i))) for i in idd]
else:
	elems = doc.GetElement(ElementId(int(idd)))

'''Фильтруем общую выборку'''	


detail_lines_selected = []

for element in elems:
	if element.Name == 'Линии детализации': detail_lines_selected.append(element)
	

'''
OST_Lines
ara = elems[0]
ara.Category.GetCategory(doc, ara.Category.Id)
<Autodesk.Revit.DB.Category object at 0x000000000000018B [Autodesk.Revit.DB.Category]>
сайт со всеми перекодировками текста https://2cyr.com/decode/?lang=ru

ara.Category.Name
ara.Category.Name == 'Линии'
True
ara.Category.BuiltInCategory
'''


#сообщение об ошибке которое должно вывестись в следующем модуле
error_text_in_window = 'Вы не выбрали линии детализации. Программа собирает длины только с линий детализации. Выберите их плз и перезапустите программу.'
#если выбраны не линии детализации, выйти из программы
if detail_lines_selected == []: 
	MessageBox.Show(error_text_in_window, 'Ошибка', MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
	sys.exit()


''' вытаскиваем параметр длины линии'''
LengthOfLines = [i.LookupParameter('Длина').AsValueString() for i in detail_lines_selected]

#сразу суммируем все значения для последующей записи в итоговую табличку
LengthOfLines_sum = 0
for i in LengthOfLines:
	LengthOfLines_sum = LengthOfLines_sum + float(i)


MessageBox.Show('Общая длина выбранных линий - ' + str(round((LengthOfLines_sum / 1000) , 1)) + ' м', 'Результат', MessageBoxButtons.OK, MessageBoxIcon.Asterisk)