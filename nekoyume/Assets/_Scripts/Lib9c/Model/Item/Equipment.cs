using System;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Nekoyume.Model.Item
{
    [Serializable]
    public class Equipment : ItemUsable
    {
        public bool equipped = false;
        public int level;

        public new EquipmentItemSheet.Row Data { get; }
        public StatType UniqueStatType => Data.Stat.Type;

        public decimal GetIncrementAmountOfEnhancement()
        {
            return StatsMap.GetStat(UniqueStatType, true) * 0.1m;
        }

        public Equipment(EquipmentItemSheet.Row data, Guid id, long requiredBlockIndex)
            : base(data, id, requiredBlockIndex)
        {
            Data = data;
        }

        public override IValue Serialize() =>
            new Dictionary(new Dictionary<IKey, IValue>
            {
                [(Text) "equipped"] = new Bencodex.Types.Boolean(equipped),
                [(Text) "level"] = (Integer) level,
            }.Union((Dictionary) base.Serialize()));

        public bool Equip()
        {
            equipped = true;
            return true;
        }

        public bool Unequip()
        {
            equipped = false;
            return true;
        }

        // FIXME: 기본 스탯을 복리로 증가시키고 있는데, 단리로 증가시켜야 한다.
        // 이를 위해서는 기본 스탯을 유지하면서 추가 스탯에 더해야 하는데, UI 표현에 문제가 생기기 때문에 논의 후 개선한다.
        // 장비가 보유한 스킬의 확률과 수치 강화가 필요한 상태이다.
        public void LevelUp()
        {
            level++;
            StatsMap.AddStatValue(UniqueStatType, GetIncrementAmountOfEnhancement());
            if (new[] {4, 7, 10}.Contains(level) &&
                GetOptionCount() > 0)
            {
                UpdateOptions();
            }
        }

        public List<object> GetOptions()
        {
            var options = new List<object>();
            options.AddRange(Skills);
            options.AddRange(BuffSkills);
            foreach (var statMapEx in StatsMap.GetAdditionalStats())
            {
                options.Add(new StatModifier(
                    statMapEx.StatType,
                    StatModifier.OperationType.Add,
                    statMapEx.AdditionalValueAsInt));
            }

            return options;
        }

        private void UpdateOptions()
        {
            foreach (var statMapEx in StatsMap.GetAdditionalStats())
            {
                StatsMap.SetStatAdditionalValue(
                    statMapEx.StatType,
                    statMapEx.AdditionalValue * 1.3m);
            }

            var skills = new List<Skill.Skill>();
            skills.AddRange(Skills);
            skills.AddRange(BuffSkills);
            foreach (var skill in skills)
            {
                var chance = decimal.ToInt32(skill.Chance * 1.3m);
                var power = decimal.ToInt32(skill.Power * 1.3m);
                skill.Update(chance, power);
            }
        }
    }
}
