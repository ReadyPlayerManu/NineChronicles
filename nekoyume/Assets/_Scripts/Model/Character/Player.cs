using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Battle;
using Nekoyume.EnumType;
using Nekoyume.Game.Item;
using Nekoyume.State;
using Nekoyume.TableData;

namespace Nekoyume.Model
{
    [Serializable]
    public class Player : CharacterBase, ICloneable
    {
        [Serializable]
        public class ExpData : ICloneable
        {
            public long Max { get; private set; }
            public long Need { get; private set; }
            public long Current { get; set; }

            public ExpData()
            {
            }

            protected ExpData(ExpData value)
            {
                Max = value.Max;
                Need = value.Need;
                Current = value.Current;
            }

            public void Set(LevelSheet.Row row)
            {
                Max = row.Exp + row.ExpNeed;
                Need = row.ExpNeed;
            }

            public object Clone()
            {
                return new ExpData(this);
            }
        }

        public readonly ExpData Exp = new ExpData();
        public readonly Inventory Inventory;
        public WorldInformation worldInformation;
        
        public Weapon weapon;
        public Armor armor;
        public Belt belt;
        public Necklace necklace;
        public Ring ring;
        public Helm helm;
        public SetItem set;
        public CollectionMap monsterMap;
        public CollectionMap eventMap;

        public int hairIndex;
        public int lensIndex;
        public int earIndex;
        public int tailIndex;

        private List<Equipment> Equipments { get; set; }

        public Player(AvatarState avatarState, Simulator simulator) : base(simulator, avatarState.characterId, avatarState.level)
        {
            // FIXME 중복 코드 제거할 것
            Exp.Current = avatarState.exp;
            Inventory = avatarState.inventory;
            worldInformation = avatarState.worldInformation;
            hairIndex = avatarState.hair;
            lensIndex = avatarState.lens;
            earIndex = avatarState.ear;
            tailIndex = avatarState.tail;
            monsterMap = new CollectionMap();
            eventMap = new CollectionMap();
            PostConstruction(simulator?.TableSheets);
        }

        public Player(AvatarState avatarState, TableSheets tableSheets) : base (null, avatarState.characterId, avatarState.level)
        {
            // FIXME 중복 코드 제거할 것
            Exp.Current = avatarState.exp;
            Inventory = avatarState.inventory;
            worldInformation = avatarState.worldInformation;
            hairIndex = avatarState.hair;
            lensIndex = avatarState.lens;
            earIndex = avatarState.ear;
            tailIndex = avatarState.tail;
            monsterMap = new CollectionMap();
            eventMap = new CollectionMap();
            PostConstruction(tableSheets);
        }

        public Player(int level, TableSheets tableSheets) : base(null, GameConfig.DefaultAvatarCharacterId, level)
        {
            Exp.Current = 0;
            Inventory = new Inventory();
            worldInformation = null;
            PostConstruction(tableSheets);
        }

        protected Player(Player value) : base(value)
        {
            Exp = (ExpData) value.Exp.Clone();
            Inventory = value.Inventory;
            worldInformation = value.worldInformation;
            hairIndex = value.hairIndex;
            lensIndex = value.lensIndex;
            earIndex = value.earIndex;
            tailIndex = value.tailIndex;
            weapon = value.weapon;
            armor = value.armor;
            belt = value.belt;
            necklace = value.necklace;
            ring = value.ring;
            helm = value.helm;
            set = value.set;

            Equipments = value.Equipments;
        }

        private void PostConstruction(TableSheets sheets)
        {
            UpdateExp(sheets);
            Equip(Inventory.Items);
        }

        private void UpdateExp(TableSheets sheets)
        {
            sheets.LevelSheet.TryGetValue(Level, out var row, true);
            Exp.Set(row);
        }

        public void RemoveTarget(Enemy enemy)
        {
            monsterMap.Add(new KeyValuePair<int, int>(enemy.RowData.Id, 1));
            Targets.Remove(enemy);
            Simulator.Characters.TryRemove(enemy);
        }

        public void RemoveTarget(EnemyPlayer enemy)
        {
            Targets.Remove(enemy);
            Simulator.Characters.TryRemove(enemy);
        }

        protected override void OnDead()
        {
            base.OnDead();
            eventMap.Add(new KeyValuePair<int, int>((int) QuestEventType.Die, 1));
            Simulator.Lose = true;
        }
        
        private void Equip(IEnumerable<Inventory.Item> items)
        {
            Equipments = items.Select(i => i.item)
                .OfType<Equipment>()
                .Where(e => e.equipped)
                .ToList();
            foreach (var equipment in Equipments)
            {
                switch (equipment.Data.ItemSubType)
                {
                    case ItemSubType.Weapon:
                        weapon = equipment as Weapon;
                        break;
                    case ItemSubType.RangedWeapon:
                        weapon = equipment as RangedWeapon;
                        break;
                    case ItemSubType.Armor:
                        armor = equipment as Armor;
                        defElementType = equipment.Data.ElementalType;
                        break;
                    case ItemSubType.Belt:
                        belt = equipment as Belt;
                        break;
                    case ItemSubType.Necklace:
                        necklace = equipment as Necklace;
                        break;
                    case ItemSubType.Ring:
                        ring = equipment as Ring;
                        break;
                    case ItemSubType.Helm:
                        helm = equipment as Helm;
                        break;
                    case ItemSubType.Set:
                        set = equipment as SetItem;
                        break;
                    default:
                        throw new InvalidEquipmentException();
                }
            }
            
            Stats.SetEquipments(Equipments);

            foreach (var skill in Equipments.SelectMany(equipment => equipment.Skills))
            {
                Skills.Add(skill);
            }
            
            foreach (var buffSkill in Equipments.SelectMany(equipment => equipment.BuffSkills))
            {
                Skills.Add(buffSkill);
            }
        }

        public void GetExp(long waveExp, bool log = false)
        {
            Exp.Current += waveExp;

            if (log)
            {
                var getExp = new GetExp((CharacterBase) Clone(), waveExp);
                Simulator.Log.Add(getExp);
            }

            if (Exp.Current < Exp.Max)
                return;

            var level = Level;
            Level = Simulator.TableSheets.LevelSheet.GetLevel(Exp.Current);
            // UI에서 레벨업 처리시 NRE 회피
            if (level < Level)
            {
                eventMap?.Add(new KeyValuePair<int, int>((int) QuestEventType.Level, Level - level));
            }
            UpdateExp(Simulator.TableSheets);
        }

        // ToDo. 지금은 스테이지에서 재료 아이템만 주고 있음. 추후 대체 불가능 아이템도 줄 경우 수정 대상.
        public CollectionMap GetRewards(List<ItemBase> items)
        {
            var map = new CollectionMap();
            foreach (var item in items)
            {
                map.Add(Inventory.AddItem(item));
            }

            return map;
        }

        public virtual void Spawn()
        {
            InitAI();
            var spawn = new SpawnPlayer((CharacterBase) Clone());
            Simulator.Log.Add(spawn);
        }

        public IEnumerable<string> GetOptions()
        {
            var atkOptions = atkElementType.GetOptions(StatType.ATK);
            foreach (var atkOption in atkOptions)
            {
                yield return atkOption;
            }

            var defOptions = defElementType.GetOptions(StatType.DEF);
            foreach (var defOption in defOptions)
            {
                yield return defOption;
            }
        }

        public void Use(List<Consumable> foods)
        {
            Stats.SetConsumables(foods);
            foreach (var food in foods)
            {
                foreach (var skill in food.Skills)
                {
                    Skills.Add(skill);
                }

                foreach (var buffSkill in food.BuffSkills)
                {
                    BuffSkills.Add(buffSkill);
                }
                
                Inventory.RemoveNonFungibleItem(food);
            }
        }

        public void OverrideSkill(Game.Skill skill)
        {
            Skills.Clear();
            Skills.Add(skill);
        }

        public override object Clone()
        {
            return new Player(this);
        }
    }

    public class InvalidEquipmentException : Exception
    {
    }
}
