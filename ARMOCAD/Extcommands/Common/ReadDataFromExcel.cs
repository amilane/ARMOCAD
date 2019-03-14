using System.Collections.Generic;
using System.Data;
using System.IO;
using ExcelDataReader;

namespace ARMOCAD
{
  class ReadDataFromExcel
  {
    public List<List<string>> readDataTable(string path, string sheetName)
    {
      using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
      {
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
          var result = reader.AsDataSet();
          DataTableCollection table = result.Tables;
          DataTable resultTable = table[sheetName];

          List<List<string>> dataList = new List<List<string>>();
          foreach (DataRow row in resultTable.Rows)
          {
            List<string> rowList = new List<string>();
            foreach (var item in row.ItemArray)
            {
              rowList.Add(item.ToString());
            }
            dataList.Add(rowList);
          }
          return dataList;
          
        }
      }
    }
  }
}
