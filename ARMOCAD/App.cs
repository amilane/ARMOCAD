using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;


namespace ARMOCAD
{
  class App : IExternalApplication
  {
    public static int Security() // Метод по защите программы в зависимости от контрольной даты
    {
      DateTime ControlDate = new DateTime(2020, 11, 29, 12, 0, 0); // Формируем контрольную дату в формате: год, месяц, день, часы, минуты, секунды
      DateTime dt = DateTime.Today; // получаем сегодняшнюю дату в таком же формате
      TimeSpan deltaDate = ControlDate - dt; // вычисляем разницу между контрольной и текущей датой
      string deltaDateDays = deltaDate.Days.ToString();
      int deltaDateDaysInt = Convert.ToInt32(deltaDateDays);
      //string curDate = dt.ToShortDateString(); //Результат: 06.03.2014
      if (deltaDateDaysInt >= 1 && deltaDateDaysInt <= 7)
      {
        TaskDialog.Show("Истекает срок действия программы ARMOCAD", String.Concat("До окончания срока действия программы ARMOCAD осталось дней: ", deltaDateDays, ".\r\nЧтобы избежать появления данного предупреждения обратитесь к разработчику\r\nили удалите программу ARMOCAD в панели управления - программы и компоненты."));
      }
      if (deltaDateDaysInt < 1)
      {
        TaskDialog.Show("Истёк срок действия программы ARMOCAD", String.Concat("Срок действия программы ARMOCAD истёк", ".\r\nЧтобы избежать появления данного предупреждения обратитесь к разработчику\r\nили удалите программу ARMOCAD в панели управления - программы и компоненты.\r\nСпасибо за использование программы."));
      }
      return deltaDateDaysInt;
    }


    public static string GetExeDirectory() // Метод получает путь к папке где лежит исполняемая библиотека dll
    {
      string codeBase = Assembly.GetExecutingAssembly().CodeBase;
      UriBuilder uri = new UriBuilder(codeBase);
      string path = Uri.UnescapeDataString(uri.Path);
      path = Path.GetDirectoryName(path);
      return path;
    }

    // Подгрузка библиотек dll из ресурсов самой сборки
    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      var assemblyName = new AssemblyName(args.Name).Name;
      if (assemblyName == "IronPython")
      {
        using (var stream = typeof(App).Assembly.GetManifestResourceStream(
            "SuElProgs" + assemblyName + ".dll"))
        {
          byte[] assemblyData = new byte[stream.Length];
          stream.Read(assemblyData, 0, assemblyData.Length);
          return Assembly.Load(assemblyData);
        }
      }
      else
      {
        return null;
      }

    }



    // define a method that will create our tab and button
    static void AddRibbonPanel(UIControlledApplication application)
    {
      // Проверяем не истёк ли срок действия программы
      int deltaDateDaysInt1 = Security();
      if (deltaDateDaysInt1 < 1)
      {
        return; // Выходим из программы. То есть риббон не создастся
      }

      // Create a custom ribbon tab
      String tabName = "ARMOCAD";
      application.CreateRibbonTab(tabName);

      // Add a new ribbon panel
      RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Программки общего назначения");

      RibbonPanel ribbonPanel1 = application.CreateRibbonPanel(tabName, "Электрические системы");

      RibbonPanel ribbonPanelAgpzTag = application.CreateRibbonPanel(tabName, "AGPZ TAG");

      RibbonPanel ribbonPanel2 = application.CreateRibbonPanel(tabName, "Механические системы");

      RibbonPanel ribbonPanel3 = application.CreateRibbonPanel(tabName, "Слаботочные системы");

      // Get dll assembly path
      string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

      // create push button
      PushButtonData b1Data = new PushButtonData(
          "cmdDetailLinesLength",
          "DL" + System.Environment.NewLine + "length",
          thisAssemblyPath,
          "DetailLinesLength.DetailLinesLengthClass");

      ContextualHelp contextHelp1 = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/DetailLinesLength_Help.html"));
      b1Data.SetContextualHelp(contextHelp1);
      b1Data.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/DetailLinesLength_icon.png"));
      b1Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/DetailLinesLength_icon.png"));
      b1Data.ToolTip = "Нажмите чтобы измерить длину линий детализации";

      // create push button
      PushButtonData b2Data = new PushButtonData(
          "cmdALength",
          "MEP" + System.Environment.NewLine + "length",
          thisAssemblyPath,
          "ALength.OverLength");

      b2Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/measureIcon.png"));
      b2Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/measureIcon.png"));
      b2Data.ToolTip = "Нажмите чтобы измерить длину инженерных коммуникаций и линий";


      // Создаём выпадающие кнопки
      SplitButtonData sbdata1 = new SplitButtonData("Ara", "MEP Length");
      SplitButton sb1 = ribbonPanel.AddItem(sbdata1) as SplitButton;
      sb1.AddPushButton(b2Data);
      sb1.AddPushButton(b1Data);


      // create push button for RotateAll
      PushButtonData b3Data = new PushButtonData(
          "cmdRotateAll",
          "Rotate" + System.Environment.NewLine + "Elements",
          thisAssemblyPath,
          "RotateAll.RotateAllClass");

      PushButton pb3 = ribbonPanel.AddItem(b3Data) as PushButton;
      pb3.ToolTip = "Select Elements to rotate them";
      BitmapImage pb3Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/rotateIcon.png"));
      pb3.LargeImage = pb3Image;

      // create push button
      PushButtonData b4Data = new PushButtonData(
          "cmdScheduleToExcel",
          "Export" + System.Environment.NewLine + "Schedule",
          thisAssemblyPath,
          "ScheduleToExcel.ScheduleToExcelClass");

      PushButton pb4 = ribbonPanel.AddItem(b4Data) as PushButton;
      pb4.ToolTip = "Select Schedules to export them";
      BitmapImage pb4Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/excelIcon.png"));
      pb4.LargeImage = pb4Image;


      // create push button
      PushButtonData b5Data = new PushButtonData(
          "cmdCopySheet",
          "Copy" + System.Environment.NewLine + "sheet",
          thisAssemblyPath,
          "CopySheet.CopySheetClass");

      PushButton pb5 = ribbonPanel.AddItem(b5Data) as PushButton;
      pb5.ToolTip = "Copy Sheet";
      BitmapImage pb5Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/copySheetIcon.png"));
      pb5.LargeImage = pb5Image;


      // create push button
      PushButtonData b6Data = new PushButtonData(
          "cmdSimilarParamsClass",
          "Similar" + System.Environment.NewLine + "Parameters",
          thisAssemblyPath,
          "SimilarParams.SimilarParamsClass");

      PushButton pb6 = ribbonPanel.AddItem(b6Data) as PushButton;
      pb6.ToolTip = "Запись одинкаовых параметров в разные семейства";
      pb6.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/SimilarParams_icon.png"));
      pb6.LargeImage = pb6.Image;
      ContextualHelp contextHelp6 = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/SimilarParameters_Help.html"));
      pb6.SetContextualHelp(contextHelp6);


      // create push button
      PushButtonData b7Data = new PushButtonData(
          "cmdParamtextreplaceClass",
          "Param" + System.Environment.NewLine + "text replace",
          thisAssemblyPath,
          "Paramtextreplace.ParamtextreplaceClass");

      PushButton pb7 = ribbonPanel.AddItem(b7Data) as PushButton;
      pb7.ToolTip = "Поиск и замена выбранных параметров в выбранных семействах";
      pb7.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/Paramtextreplace_icon.png"));
      pb7.LargeImage = pb7.Image;
      ContextualHelp contextHelp7 = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/Paramtextreplace_Help.html"));
      pb7.SetContextualHelp(contextHelp7);


      // create push button
      PushButtonData b8Data = new PushButtonData(
          "cmdSwapDuct",
          "Swap" + System.Environment.NewLine + "Duct",
          thisAssemblyPath,
          "SwapDuct.SwapDuctClass");

      PushButton pb8 = ribbonPanel2.AddItem(b8Data) as PushButton;
      pb8.ToolTip = "Нажмите чтобы развернуть воздуховод";
      pb8.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/swapIcon.png"));
      pb8.LargeImage = pb8.Image;


      // create push button
      PushButtonData b9Data = new PushButtonData(
          "cmdOpeningElevation",
          "Opening" + System.Environment.NewLine + "Elevation",
          thisAssemblyPath,
          "OpeningElevation.OpeningElevationClass");

      PushButton pb9 = ribbonPanel.AddItem(b9Data) as PushButton;
      pb9.ToolTip = "Нажмите чтобы обработать отверстия Cut Opening";
      pb9.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/openElevIcon.png"));
      pb9.LargeImage = pb9.Image;
      ContextualHelp contextHelp9 = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/OpeningElevation_Help.html"));
      pb9.SetContextualHelp(contextHelp9);


      // create push button
      PushButtonData b10Data = new PushButtonData(
          "cmdSpreadEvenly",
          "Spread" + System.Environment.NewLine + "Evenly",
          thisAssemblyPath,
          "SpreadEvenly.SpreadEvenlyClass");

      PushButton pb10 = ribbonPanel1.AddItem(b10Data) as PushButton;
      pb10.ToolTip = "Нажмите чтобы расставить семейства";
      pb10.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/SpreadEvenly_icon.png"));
      pb10.LargeImage = pb10.Image;
      ContextualHelp contextHelp10 = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/SpreadEvenly_Help.html"));
      pb10.SetContextualHelp(contextHelp10);


      // create push button
      PushButtonData b11Data = new PushButtonData(
          "cmdSectionByElement",
          "Section" + System.Environment.NewLine + "by Element",
          thisAssemblyPath,
          "SectionByElement.SectionByElementClass");

      PushButton pb11 = ribbonPanel.AddItem(b11Data) as PushButton;
      pb11.ToolTip = "Выберите элемент(ы) и нажмите, чтобы создать разрез(ы) вдоль них";
      pb11.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/section_icon.png"));
      pb11.LargeImage = pb11.Image;


      // create push button
      PushButtonData b12Data = new PushButtonData(
          "cmdThiDuct",
          "Толщина" + System.Environment.NewLine + "воздуховодов",
          thisAssemblyPath,
          "ThiDuct.ThiDuctClass");

      PushButton pb12 = ribbonPanel2.AddItem(b12Data) as PushButton;
      pb12.ToolTip = "Толщина металла воздуховодов и соединительных деталей";
      pb12.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/thicknessIcon.png"));
      pb12.LargeImage = pb12.Image;



      // create push button
      PushButtonData b13_1Data = new PushButtonData(
          "cmdTagOVDucts",
          "TAG" + System.Environment.NewLine + "Комплект.",
          thisAssemblyPath,
          "ARMOCAD.TagOVDucts");

      PushButton pb13_1 = ribbonPanelAgpzTag.AddItem(b13_1Data) as PushButton;
      pb13_1.ToolTip = "Тагирование воздуховодов и соединительных деталей";
      pb13_1.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));
      pb13_1.LargeImage = pb13_1.Image;

      // create push button
      PushButtonData b13_2Data = new PushButtonData(
        "cmdTagOVEquipPhase1",
        "TAG Ф1" + System.Environment.NewLine + "Оборуд.",
        thisAssemblyPath,
        "ARMOCAD.TagOVEquipPhase1");

      PushButton pb13_2 = ribbonPanelAgpzTag.AddItem(b13_2Data) as PushButton;
      pb13_2.ToolTip = "Тагирование (Фаза 1) оборудования, арматуры воздуховодов, воздухораспределителей, арматуры труб";
      pb13_2.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));
      pb13_2.LargeImage = pb13_2.Image;

      // create push button
      PushButtonData b13_3Data = new PushButtonData(
        "cmdTagOVEquip",
        "TAG Ф2+" + System.Environment.NewLine + "Оборуд.",
        thisAssemblyPath,
        "ARMOCAD.TagOVEquip");

      PushButton pb13_3 = ribbonPanelAgpzTag.AddItem(b13_3Data) as PushButton;
      pb13_3.ToolTip = "Тагирование (Фаза 2 и сл.) оборудования, арматуры воздуховодов, воздухораспределителей, арматуры труб";
      pb13_3.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));
      pb13_3.LargeImage = pb13_3.Image;


      // create push button
      PushButtonData b14Data = new PushButtonData(
          "cmdTagTransfer",
          "TAG" + System.Environment.NewLine + "transfer",
          thisAssemblyPath,
          "TagTransfer.TagTransferClass");

      PushButton pb14 = ribbonPanel2.AddItem(b14Data) as PushButton;
      pb14.ToolTip = "Перенос TAG с элемента модели в элемент узла на чертежном виде (выберите оба элемента и нажмите кнопку)";
      pb14.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagTransferIcon.png"));
      pb14.LargeImage = pb14.Image;


      // create push button
      PushButtonData b15Data = new PushButtonData(
          "cmdCompareModelAndScheme",
          "Model" + System.Environment.NewLine + "Scheme",
          thisAssemblyPath,
          "CompareModelAndScheme.CompareModelAndSchemeClass");

      PushButton pb15 = ribbonPanel2.AddItem(b15Data) as PushButton;
      pb15.ToolTip = "Сравнение однолинейки и модели";
      pb15.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/compareIcon.png"));
      pb15.LargeImage = pb15.Image;

      // create push button
      PushButtonData b16Data = new PushButtonData(
          "cmdLengthOfTransition",
          "Длина" + System.Environment.NewLine + "перехода",
          thisAssemblyPath,
          "LengthOfTransition.LengthOfTransitionClass");

      PushButton pb16 = ribbonPanel2.AddItem(b16Data) as PushButton;
      pb16.ToolTip = "Редактирует длину перехода по ВСН 353-86 Таблица 7";
      pb16.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/transIcon.png"));
      pb16.LargeImage = pb16.Image;


      // create push button
      PushButtonData b17Data = new PushButtonData(
          "cmdMEPSystemScheme",
          "Аксон." + System.Environment.NewLine + "схемы",
          thisAssemblyPath,
          "MEPSystemScheme.MEPSystemSchemeClass");

      PushButton pb17 = ribbonPanel2.AddItem(b17Data) as PushButton;
      pb17.ToolTip = "Создает 3D виды по механическим системам";
      pb17.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/3dIcon.png"));
      pb17.LargeImage = pb17.Image;

      /*
      // create push button
      PushButtonData b18Data = new PushButtonData(
          "cmdMTO",
          "AGPZ" + System.Environment.NewLine + "MTO",
          thisAssemblyPath,
          "MTO.MTOClass");

      PushButton pb18 = ribbonPanel2.AddItem(b18Data) as PushButton;
      pb18.ToolTip = "Генерирует МТО для проектов AGPZ";
      pb18.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/mIcon.png"));
      pb18.LargeImage = pb18.Image;
      */

      // create push button
      PushButtonData b19Data = new PushButtonData(
          "cmdChangeUnitExCommand",
          "AGPZ" + System.Environment.NewLine + "Copy Model",
          thisAssemblyPath,
          "ARMOCAD.ChangeUnitExCommand");

      PushButton pb19 = ribbonPanel2.AddItem(b19Data) as PushButton;
      pb19.ToolTip = "Заменяет параметры для новой модели AGPZ";
      pb19.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/copyIcon.png"));
      pb19.LargeImage = pb19.Image;

      // create push button
      PushButtonData b20Data = new PushButtonData(
        "cmdFilterExCommand",
        "Filter" + System.Environment.NewLine + "by Params",
        thisAssemblyPath,
        "ARMOCAD.FilterExCommand");

      PushButton pb20 = ribbonPanel.AddItem(b20Data) as PushButton;
      pb20.ToolTip = "Выбирает элементы в модели по условиям";
      pb20.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/filterIcon.png"));
      pb20.LargeImage = pb20.Image;

      // create push button
      PushButtonData b21Data = new PushButtonData(
        "cmdSKSSocketsToShelfsExCommand",
        "Розетки" + System.Environment.NewLine + "по Шкафам",
        thisAssemblyPath,
        "ARMOCAD.SKSSocketsToShelfsExCommand");

      PushButton pb21 = ribbonPanel3.AddItem(b21Data) as PushButton;
      pb21.ToolTip = "Заполняет в розетках параметр \"Розетка - Шкаф\"";
      pb21.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/socketIcon.png"));
      pb21.LargeImage = pb21.Image;


      // create push button
      PushButtonData b22Data = new PushButtonData(
        "cmdSKSFasadExCommand",
        "Схемы" + System.Environment.NewLine + "и Фасады",
        thisAssemblyPath,
        "ARMOCAD.SKSFasadExCommand");

      PushButton pb22 = ribbonPanel3.AddItem(b22Data) as PushButton;
      pb22.ToolTip = "Маркирует розетки и создает чертежные виды со схемами и фасадами";
      pb22.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/schemeIcon.png"));
      pb22.LargeImage = pb22.Image;

      //// create push button
      //PushButtonData b12Data = new PushButtonData(
      //	"cmdTestara",
      //	"ara" + System.Environment.NewLine + "vasa",
      //	thisAssemblyPath,
      //	"Testara.TestaraClass");

      //PushButton pb12 = ribbonPanel1.AddItem(b12Data) as PushButton;
      //pb12.ToolTip = "ararar";
      //pb12.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/SpreadEvenly_icon.png"));
      //pb12.LargeImage = pb12.Image;


    }

    public Result OnShutdown(UIControlledApplication application)
    {
      // do nothing
      return Result.Succeeded;
    }

    public Result OnStartup(UIControlledApplication application)
    {
      // call our method that will load up our toolbar
      AddRibbonPanel(application);
      return Result.Succeeded;
    }
  }
}
