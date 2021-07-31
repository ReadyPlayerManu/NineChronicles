using Nekoyume.Model.Stat;
using Nekoyume.TableData;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nekoyume.Helper
{
    public static class RecipeHelper
    {
        public static EquipmentItemSheet.Row GetResultItem(this EquipmentItemRecipeSheet.Row recipeRow)
        {
            return Game.Game.instance.TableSheets
                .EquipmentItemSheet[recipeRow.ResultEquipmentId];
        }

        public static ConsumableItemSheet.Row GetResultItem(this ConsumableItemRecipeSheet.Row recipeRow)
        {
            return Game.Game.instance.TableSheets
                .ConsumableItemSheet[recipeRow.ResultConsumableItemId];
        }

        public static DecimalStat GetUniqueStat(this EquipmentItemSheet.Row row)
        {
            return row.Stat ?? new DecimalStat(StatType.NONE);
        }

        public static StatMap GetUniqueStat(this ConsumableItemSheet.Row row)
        {
            return row.Stats.Any() ? row.Stats[0] : new StatMap(StatType.NONE);
        }

        public static StatMap GetUniqueStat(this ConsumableItemRecipeSheet.Row recipeRow)
        {
            var resultItem = GetResultItem(recipeRow);
            return GetUniqueStat(resultItem);
        }
    }
}