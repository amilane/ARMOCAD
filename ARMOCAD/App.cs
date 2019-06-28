using System;
using System.Collections.Generic;
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
      DateTime ControlDate = new DateTime(2200, 11, 29, 12, 0, 0); // Формируем контрольную дату в формате: год, месяц, день, часы, минуты, секунды
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

    //словарь для ссылок на F1
    public static Dictionary<string, ContextualHelp> helpButtonsDictionary = new Dictionary<string, ContextualHelp>
    {
      ["cmdLinkEquipmentSS"] = new ContextualHelp(ContextualHelpType.ChmFile, @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT-BIMnet\СС_Размещение оборудования из связи.docx"),
      ["cmdLinkEquipmentLoc"] = new ContextualHelp(ContextualHelpType.ChmFile, @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT-BIMnet\СС_Проверка в проекте оборудования.docx"),
      ["cmdDetailLinesLength"] = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/DetailLinesLength_Help.html")),
      ["cmdSimilarParamsClass"] = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/SimilarParameters_Help.html")),
      ["cmdParamtextreplaceClass"] = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/Paramtextreplace_Help.html")),
      ["cmdOpeningElevation"] = new ContextualHelp(ContextualHelpType.Url, Path.Combine(GetExeDirectory(), "Help/OpeningElevation_Help.html")),
      ["cmdCreateMepSpaces"] = new ContextualHelp(ContextualHelpType.ChmFile, @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT-BIMnet\СТП-Расстановка пространств по связанной модели АР.docx"),
      ["cmdSpreadEvenly"] = new ContextualHelp(ContextualHelpType.ChmFile, @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT-BIMnet\СТП-Расстановка семейств массивом SpreadEvenly.docx"),
      ["cmdConPoints"] = new ContextualHelp(ContextualHelpType.Url, @"https://drive.google.com/file/d/1Dyy2vsOLukdFzAEIVvzUv_tu3tWKD1je/view"),
      ["cmdConPointLocation"] = new ContextualHelp(ContextualHelpType.Url, @"https://drive.google.com/file/d/1J5HuVRW80eRR3kKp4TenDmcRmpm66oaE/view"),
      ["cmdSKUDControlPlacementEx"] = new ContextualHelp(ContextualHelpType.ChmFile, @"\\arena\ARMO-GROUP\ИПУ\ЛИЧНЫЕ\САПРомания\RVT-BIMnet\СТП-СКУД Расстановка точек доступа и оборудования.docx"),




    };

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

      RibbonPanel electricRibbonPanel = application.CreateRibbonPanel(tabName, "Электрические системы");

      RibbonPanel agpzTagRibbonPanel = application.CreateRibbonPanel(tabName, "AGPZ TAG");

      RibbonPanel ribbonPanel2 = application.CreateRibbonPanel(tabName, "Механические системы");

      RibbonPanel ribbonPanel3 = application.CreateRibbonPanel(tabName, "Слаботочные системы");

      // Get dll assembly path
      string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;


      #region Common Buttons
      // create push button
      PushButtonData b1Data = new PushButtonData(
        "cmdDetailLinesLength",
        "DL" + System.Environment.NewLine + "length",
        thisAssemblyPath,
        "DetailLinesLength.DetailLinesLengthClass");

      b1Data.SetContextualHelp(helpButtonsDictionary["cmdDetailLinesLength"]);
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
      pb6.SetContextualHelp(helpButtonsDictionary["cmdSimilarParamsClass"]);


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
      pb7.SetContextualHelp(helpButtonsDictionary["cmdParamtextreplaceClass"]);

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
      pb9.SetContextualHelp(helpButtonsDictionary["cmdOpeningElevation"]);

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
      PushButtonData b25Data = new PushButtonData(
        "cmdCreateMepSpaces",
        "Пространства" + System.Environment.NewLine + "по АР",
        thisAssemblyPath,
        "ARMOCAD.CreateMepSpaces");
      b25Data.SetContextualHelp(helpButtonsDictionary["cmdCreateMepSpaces"]);

      PushButton pb25 = ribbonPanel.AddItem(b25Data) as PushButton;
      pb25.ToolTip = "Ставит пространства по модели АР";
      pb25.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/spaceIcon.png"));
      pb25.LargeImage = pb25.Image;



      #endregion Common Buttons

      #region Electric Buttons

      // create push button
      PushButtonData elB1Data = new PushButtonData(
        "cmdSpreadEvenly",
        "Spread" + System.Environment.NewLine + "Evenly",
        thisAssemblyPath,
        "SpreadEvenly.SpreadEvenlyClass");

      PushButton elPB1 = electricRibbonPanel.AddItem(elB1Data) as PushButton;
      elPB1.ToolTip = "Нажмите чтобы расставить семейства";
      elPB1.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/SpreadEvenly_icon.png"));
      elPB1.LargeImage = elPB1.Image;
      elPB1.SetContextualHelp(helpButtonsDictionary["cmdSpreadEvenly"]);

      //DenisButtons
      // create push button
      PushButtonData elB2Data = new PushButtonData(
        "cmdConPoints",
        "Размещение\n ТП",
        thisAssemblyPath,
        "ARMOCAD.ConPoint");

      elB2Data.SetContextualHelp(helpButtonsDictionary["cmdConPoints"]);
      elB2Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tpointsIcon.png"));
      elB2Data.ToolTip = "Размещение точек подключения из связи";
      elB2Data.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/Screenshot_1.png"));
      elB2Data.LongDescription =
        "Автоматическая расстановка электровыводов для подключения оборудования из связанных моделей с загрузкой технических параметров \n";
      // create push button
      PushButtonData elB3Data = new PushButtonData(
        "cmdConPointLocation",
        "Проверка\n ТП",
        thisAssemblyPath,
        "ARMOCAD.ConPointLocation");

      elB3Data.SetContextualHelp(helpButtonsDictionary["cmdConPointLocation"]);
      elB3Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/checkIcon.png"));
      elB3Data.ToolTip = "Проверка размещенных точек подключения";
      elB3Data.LongDescription = "Проверка размещенных электровыводов оборудования из связанных моделей на количество и правильность размещения \n";
      elB3Data.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/Screenshot_2.png"));
      // Создаём выпадающие кнопки
      SplitButtonData sbElData1 = new SplitButtonData("ConPoints", "ConPointLocation");
      SplitButton sbEl1 = electricRibbonPanel.AddItem(sbElData1) as SplitButton;
      sbEl1.AddPushButton(elB2Data);
      sbEl1.AddPushButton(elB3Data);
      //sbEl1.CurrentButton.Name



      #endregion Electric Buttons

      #region AGPZ Tag Buttons

      // create push button
      PushButtonData tagB1Data = new PushButtonData(
          "cmdTagOVDucts",
          "TAG\n Комплект.",
          thisAssemblyPath,
          "ARMOCAD.TagOVDucts");
      
      tagB1Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));
      tagB1Data.ToolTip = "Тагирование воздуховодов и соединительных деталей";

      // create push button
      PushButtonData tagB2Data = new PushButtonData(
        "cmdTagOVEquipPhase1",
        "TAG Ф1\n Оборуд.",
        thisAssemblyPath,
        "ARMOCAD.TagOVEquipPhase1");

      tagB2Data.ToolTip = "Тагирование (Фаза 1) оборудования, арматуры воздуховодов, воздухораспределителей, арматуры труб";
      tagB2Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));

      // create push button
      PushButtonData tagB3Data = new PushButtonData(
        "cmdTagOVEquip",
        "TAG Ф2+\n Оборуд.",
        thisAssemblyPath,
        "ARMOCAD.TagOVEquip");

      tagB3Data.ToolTip = "Тагирование (Фаза 2 и сл.) оборудования, арматуры воздуховодов, воздухораспределителей, арматуры труб";
      tagB3Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));

      // create push button
      PushButtonData tagB4Data = new PushButtonData(
        "cmdTagOVEquipReplace",
        "TAG Ф2+\n Замена",
        thisAssemblyPath,
        "ARMOCAD.TagOVEquipReplace");

      tagB4Data.ToolTip = "Тагирование (Фаза 2 и сл.) заменяет часть тэга на код системы 0001А => 9901А, не создавая новых тэгов";
      tagB4Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIcon.png"));

      // Создаём выпадающие кнопки
      SplitButtonData sbTagData1 = new SplitButtonData("AGPZ", "TAGButtons");
      SplitButton sbTag1 = agpzTagRibbonPanel.AddItem(sbTagData1) as SplitButton;
      sbTag1.AddPushButton(tagB1Data);
      sbTag1.AddPushButton(tagB2Data);
      sbTag1.AddPushButton(tagB3Data);
      sbTag1.AddPushButton(tagB4Data);


      // create push button
      PushButtonData tagB5Data = new PushButtonData(
        "cmdTagsFromSheetsEx",
        "TAG" + System.Environment.NewLine + "на листах",
        thisAssemblyPath,
        "ARMOCAD.TagsFromSheetsEx");

      PushButton tagPB5 = agpzTagRibbonPanel.AddItem(tagB5Data) as PushButton;
      tagPB5.ToolTip = "Собирает информацию о тэгах и листах, на которых они расположены";
      tagPB5.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagIconGreen.png"));
      tagPB5.LargeImage = tagPB5.Image;

      // create push button
      PushButtonData tagB6Data = new PushButtonData(
        "cmdTBCommand",
        "TAG\n bridge",
        thisAssemblyPath,
        "ARMOCAD.TBCommand");

      PushButton tagPB6 = agpzTagRibbonPanel.AddItem(tagB6Data) as PushButton;
      tagPB6.ToolTip = "Сцепляет элементы модели и схем";
      tagPB6.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/tagTransferIcon.png"));
      tagPB6.LargeImage = tagPB6.Image;


      #endregion AGPZ Tag Buttons

      #region MEP Buttons
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

      #endregion MEP Buttons

      #region SS Buttons

      // create push button
      PushButtonData b21Data = new PushButtonData(
        "cmdSKSSocketsToShelfsExCommand",
        "Розетки по Шкафам",
        thisAssemblyPath,
        "ARMOCAD.SKSSocketsToShelfsExCommand");
      b21Data.ToolTip = "Заполняет в розетках параметр \"Розетка - Шкаф\"";
      b21Data.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/socketIcon.png"));

      // create push button
      PushButtonData b22Data = new PushButtonData(
        "cmdSKSFasadExCommand",
        "Схемы и Фасады",
        thisAssemblyPath,
        "ARMOCAD.SKSFasadExCommand");
      b22Data.ToolTip = "Маркирует розетки и создает чертежные виды со схемами и фасадами";
      b22Data.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/schemeIcon.png"));

      List<RibbonItem> ssButtons1 = new List<RibbonItem>();
      ssButtons1.AddRange(ribbonPanel3.AddStackedItems(b21Data, b22Data));

      // create push button
      PushButtonData b23Data = new PushButtonData(
        "cmdSKUDControlPlacementEx",
        "Точки Доступа",
        thisAssemblyPath,
        "ARMOCAD.SKUDControlPlacementEx");
      b23Data.ToolTip = "Размещение точек доступа по дверям из связанной модели АР";
      b23Data.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/arrowIcon.png"));
      b23Data.SetContextualHelp(helpButtonsDictionary["cmdSKUDControlPlacementEx"]);

      // create push button
      PushButtonData b24Data = new PushButtonData(
        "cmdSKUDPlaceEquipmentEx",
        "ТД -> Оборудование",
        thisAssemblyPath,
        "ARMOCAD.SKUDPlaceEquipmentEx");
      b24Data.ToolTip = "Размещение оборудования СКУД по точкам доступа";
      b24Data.Image = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/equipIcon.png"));
      b24Data.SetContextualHelp(helpButtonsDictionary["cmdSKUDControlPlacementEx"]);

      List<RibbonItem> skudButtons1 = new List<RibbonItem>();
      skudButtons1.AddRange(ribbonPanel3.AddStackedItems(b23Data, b24Data));

      //========================= DenisButtons
      // create push button
      PushButtonData b26Data = new PushButtonData("cmdLinkEquipmentSS", "Размещение\n оборудования",thisAssemblyPath, "ARMOCAD.LinkEquipmentSS");

      b26Data.SetContextualHelp(helpButtonsDictionary["cmdLinkEquipmentSS"]);
      b26Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/valveIcon.png"));
      b26Data.ToolTip = "Размещение оборудования из связной модели";
      b26Data.LongDescription = "Автоматическая расстановка электровыводов для подключения оборудования из связанных моделей с загрузкой технических параметров \n";
      b26Data.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/Screenshot_3.png"));
      // create push button
      PushButtonData b27Data = new PushButtonData("cmdLinkEquipmentLoc", "Проверка\n оборудования",thisAssemblyPath, "ARMOCAD.LinkEquipmentLoc");

      b27Data.SetContextualHelp(helpButtonsDictionary["cmdLinkEquipmentLoc"]);
      b27Data.LargeImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/checkIcon.png"));
      b27Data.ToolTip = "Проверка оборудования размещенного из связной модели \n";
      b27Data.ToolTipImage = new BitmapImage(new Uri("pack://application:,,,/ARMOCAD;component/Resources/Screenshot_2.png"));
      // Создаём выпадающие кнопки
      SplitButtonData sbdata2 = new SplitButtonData("LinkEquipmentSS", "SS LinkEquipmentSS");
      SplitButton sb2 = ribbonPanel3.AddItem(sbdata2) as SplitButton;
      sb2.AddPushButton(b26Data);
      sb2.AddPushButton(b27Data);
      PushButton curbutt = sb2.CurrentButton;
      ContextualHelp h = curbutt.GetContextualHelp();
      sb2.SetContextualHelp(h);
      







      #endregion SS Buttons










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
