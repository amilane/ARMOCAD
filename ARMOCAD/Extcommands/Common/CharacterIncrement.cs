using System;

namespace ARMOCAD
{
  class CharacterIncrement
  {
    public string СharacterIncrement(int colCount)
    {
      int TempCount = 0;
      string returnCharCount = string.Empty;

      if (colCount < 26)
      {
        TempCount = colCount;
        char CharCount = Convert.ToChar((Convert.ToInt32('A') + TempCount));
        returnCharCount += CharCount;
        return returnCharCount;
      }
      else
      {
        var rev = 0;

        while (colCount >= 26)
        {
          colCount = colCount - 26;
          rev++;
        }

        returnCharCount += СharacterIncrement(rev - 1);
        returnCharCount += СharacterIncrement(colCount);
        return returnCharCount;
      }
    }
  }
}
