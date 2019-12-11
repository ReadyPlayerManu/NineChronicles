using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bencodex.Types;
using Nekoyume.EnumType;
using Nekoyume.State;

namespace Nekoyume.Game
{
    // todo: `Stats`나 `StatModifier`로 대체되어야 함.
    [Serializable]
    public class StatsMap : IStats, IAdditionalStats, IState
    {
        public int HP => HasHP ? _statMaps[StatType.HP].TotalValueAsInt : 0;
        public int ATK => HasATK ? _statMaps[StatType.ATK].TotalValueAsInt : 0;
        public int DEF => HasDEF ? _statMaps[StatType.DEF].TotalValueAsInt : 0;
        public int CRI => HasCRI ? _statMaps[StatType.CRI].TotalValueAsInt : 0;
        public int DOG => HasDOG ? _statMaps[StatType.DOG].TotalValueAsInt : 0;
        public int SPD => HasSPD ? _statMaps[StatType.SPD].TotalValueAsInt : 0;

        public bool HasHP => _statMaps.ContainsKey(StatType.HP) &&
                             (_statMaps[StatType.HP].HasValue || _statMaps[StatType.HP].HasAdditionalValue);

        public bool HasATK => _statMaps.ContainsKey(StatType.ATK) &&
                              (_statMaps[StatType.ATK].HasValue || _statMaps[StatType.ATK].HasAdditionalValue);

        public bool HasDEF => _statMaps.ContainsKey(StatType.DEF) &&
                              (_statMaps[StatType.DEF].HasValue || _statMaps[StatType.DEF].HasAdditionalValue);

        public bool HasCRI => _statMaps.ContainsKey(StatType.CRI) && 
                              (_statMaps[StatType.CRI].HasValue || _statMaps[StatType.CRI].HasAdditionalValue);

        public bool HasDOG => _statMaps.ContainsKey(StatType.DOG) && 
                              (_statMaps[StatType.DOG].HasValue || _statMaps[StatType.DOG].HasAdditionalValue);

        public bool HasSPD => _statMaps.ContainsKey(StatType.SPD) &&
                              (_statMaps[StatType.SPD].HasValue || _statMaps[StatType.SPD].HasAdditionalValue);

        public int AdditionalHP => HasAdditionalHP ? _statMaps[StatType.HP].AdditionalValueAsInt : 0;
        public int AdditionalATK => HasAdditionalATK ? _statMaps[StatType.ATK].AdditionalValueAsInt : 0;
        public int AdditionalDEF => HasAdditionalDEF ? _statMaps[StatType.DEF].AdditionalValueAsInt : 0;
        public int AdditionalCRI => HasAdditionalCRI ? _statMaps[StatType.CRI].AdditionalValueAsInt : 0;
        public int AdditionalDOG => HasAdditionalDOG ? _statMaps[StatType.DOG].AdditionalValueAsInt : 0;
        public int AdditionalSPD => HasAdditionalSPD ? _statMaps[StatType.SPD].AdditionalValueAsInt : 0;

        public bool HasAdditionalHP => _statMaps.ContainsKey(StatType.HP) && _statMaps[StatType.HP].HasAdditionalValue;

        public bool HasAdditionalATK =>
            _statMaps.ContainsKey(StatType.ATK) && _statMaps[StatType.ATK].HasAdditionalValue;

        public bool HasAdditionalDEF =>
            _statMaps.ContainsKey(StatType.DEF) && _statMaps[StatType.DEF].HasAdditionalValue;

        public bool HasAdditionalCRI =>
            _statMaps.ContainsKey(StatType.CRI) && _statMaps[StatType.CRI].HasAdditionalValue;

        public bool HasAdditionalDOG =>
            _statMaps.ContainsKey(StatType.DOG) && _statMaps[StatType.DOG].HasAdditionalValue;

        public bool HasAdditionalSPD =>
            _statMaps.ContainsKey(StatType.SPD) && _statMaps[StatType.SPD].HasAdditionalValue;

        public bool HasAdditionalStats => HasAdditionalHP || HasAdditionalATK || HasAdditionalDEF || HasAdditionalCRI ||
                                          HasAdditionalDOG || HasAdditionalSPD;

        private readonly Dictionary<StatType, StatMapEx> _statMaps =
            new Dictionary<StatType, StatMapEx>(StatTypeComparer.Instance);

        public StatsMap()
        {
        }

        protected bool Equals(StatsMap other)
        {
            return Equals(_statMaps, other._statMaps);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StatsMap) obj);
        }

        public override int GetHashCode()
        {
            return (_statMaps != null ? _statMaps.GetHashCode() : 0);
        }

        public void AddStatValue(StatType key, decimal value)
        {
            if (!_statMaps.ContainsKey(key))
            {
                _statMaps.Add(key, new StatMapEx(key));
            }

            _statMaps[key].Value += value;
            PostStatValueChanged(key);
        }

        public void AddStatAdditionalValue(StatType key, decimal additionalValue)
        {
            if (!_statMaps.ContainsKey(key))
            {
                _statMaps.Add(key, new StatMapEx(key));
            }

            _statMaps[key].AdditionalValue += additionalValue;
            PostStatValueChanged(key);
        }

        public void AddStatAdditionalValue(StatModifier statModifier)
        {
            AddStatAdditionalValue(statModifier.StatType, statModifier.Value);
        }

        public void SetStatAdditionalValue(StatType key, decimal additionalValue)
        {
            if (!_statMaps.ContainsKey(key))
            {
                _statMaps.Add(key, new StatMapEx(key));
            }

            _statMaps[key].AdditionalValue = additionalValue;
            PostStatValueChanged(key);
        }

        private void PostStatValueChanged(StatType key)
        {
            if (!_statMaps.ContainsKey(key))
                return;

            var statMap = _statMaps[key];
            if (statMap.HasValue ||
                statMap.HasAdditionalValue)
                return;

            _statMaps.Remove(key);
        }

        public string GetInformation()
        {
            var sb = new StringBuilder();
            foreach (var pair in _statMaps)
            {
                var information = pair.Value.GetInformation();
                if (string.IsNullOrEmpty(information))
                {
                    continue;
                }

                sb.AppendLine(information);
            }

            return sb.ToString().Trim();
        }

        public IValue Serialize() =>
            new Bencodex.Types.Dictionary(
                _statMaps.Select(kv =>
                    new KeyValuePair<IKey, IValue>(
                        kv.Key.Serialize(),
                        kv.Value.Serialize()
                    )
                )
            );

        public void Deserialize(Bencodex.Types.Dictionary serialized)
        {
            foreach (KeyValuePair<IKey, IValue> kv in serialized)
            {
                _statMaps[StatTypeExtension.Deserialize((Binary) kv.Key)] =
                    new StatMapEx((Bencodex.Types.Dictionary) kv.Value);
            }
        }

        public IEnumerable<(StatType statType, int value)> GetStats(bool ignoreZero = false)
        {
            if (ignoreZero)
            {
                if (HasHP)
                    yield return (StatType.HP, HP);
                if (HasATK)
                    yield return (StatType.ATK, ATK);
                if (HasDEF)
                    yield return (StatType.DEF, DEF);
                if (HasCRI)
                    yield return (StatType.CRI, CRI);
                if (HasDOG)
                    yield return (StatType.DOG, DOG);
                if (HasSPD)
                    yield return (StatType.SPD, SPD);
            }
            else
            {
                yield return (StatType.HP, HP);
                yield return (StatType.ATK, ATK);
                yield return (StatType.DEF, DEF);
                yield return (StatType.CRI, CRI);
                yield return (StatType.DOG, DOG);
                yield return (StatType.SPD, SPD);
            }
        }

        public IEnumerable<(StatType statType, int additionalValue)> GetAdditionalStats(bool ignoreZero = false)
        {
            if (ignoreZero)
            {
                if (HasAdditionalHP)
                    yield return (StatType.HP, AdditionalHP);
                if (HasAdditionalATK)
                    yield return (StatType.ATK, AdditionalATK);
                if (HasAdditionalDEF)
                    yield return (StatType.DEF, AdditionalDEF);
                if (HasAdditionalCRI)
                    yield return (StatType.CRI, AdditionalCRI);
                if (HasAdditionalDOG)
                    yield return (StatType.DOG, AdditionalDOG);
                if (HasAdditionalSPD)
                    yield return (StatType.SPD, AdditionalSPD);
            }
            else
            {
                yield return (StatType.HP, AdditionalHP);
                yield return (StatType.ATK, AdditionalATK);
                yield return (StatType.DEF, AdditionalDEF);
                yield return (StatType.CRI, AdditionalCRI);
                yield return (StatType.DOG, AdditionalDOG);
                yield return (StatType.SPD, AdditionalSPD);
            }
        }

        public IEnumerable<(StatType statType, int baseValue, int additionalValue)> GetBaseAndAdditionalStats(bool ignoreZero = false)
        {
            //var levelStats = LevelStats.GetStats();
            //var additionalStats = GetAdditionalStats();

            //var enumerable =
            //    levelStats.Join(additionalStats,
            //                    levelStat => levelStat.statType,
            //                    additionalStat => additionalStat.statType,
            //                    (levelStat, additionalStat)
            //                        => (levelStat.statType, levelStat.value, additionalStat.additionalValue));

            //foreach (var row in eumerable)
            //    yield return row;

            var hp = _statMaps[StatType.HP];
            var atk = _statMaps[StatType.ATK];
            var def = _statMaps[StatType.DEF];
            var cri = _statMaps[StatType.CRI];
            var dog = _statMaps[StatType.DOG];
            var spd = _statMaps[StatType.SPD];

            if (ignoreZero)
            {
                if (hp.HasValue && hp.HasAdditionalValue)
                    yield return (StatType.HP, hp.ValueAsInt, hp.AdditionalValueAsInt);
                if (atk.HasValue && atk.HasAdditionalValue)
                    yield return (StatType.ATK, atk.ValueAsInt, atk.AdditionalValueAsInt);
                if (def.HasValue && def.HasAdditionalValue)
                    yield return (StatType.DEF, def.ValueAsInt, def.AdditionalValueAsInt);
                if (cri.HasValue && cri.HasAdditionalValue)
                    yield return (StatType.CRI, cri.ValueAsInt, cri.AdditionalValueAsInt);
                if (dog.HasValue && dog.HasAdditionalValue)
                    yield return (StatType.DOG, dog.ValueAsInt, dog.AdditionalValueAsInt);
                if (spd.HasValue && spd.HasAdditionalValue)
                    yield return (StatType.SPD, spd.ValueAsInt, spd.AdditionalValueAsInt);

            }
            else
            {
                yield return (StatType.HP, hp.ValueAsInt, hp.AdditionalValueAsInt);
                yield return (StatType.ATK, atk.ValueAsInt, atk.AdditionalValueAsInt);
                yield return (StatType.DEF, def.ValueAsInt, def.AdditionalValueAsInt);
                yield return (StatType.CRI, cri.ValueAsInt, cri.AdditionalValueAsInt);
                yield return (StatType.DOG, dog.ValueAsInt, dog.AdditionalValueAsInt);
                yield return (StatType.SPD, spd.ValueAsInt, spd.AdditionalValueAsInt);
            }
        }

        public IEnumerable<StatMapEx> GetStats()
        {
            if (HasHP)
                yield return _statMaps[StatType.HP];
            if (HasATK)
                yield return _statMaps[StatType.ATK];
            if (HasDEF)
                yield return _statMaps[StatType.DEF];
            if (HasCRI)
                yield return _statMaps[StatType.CRI];
            if (HasDOG)
                yield return _statMaps[StatType.DOG];
            if (HasSPD)
                yield return _statMaps[StatType.SPD];
        }

        public IEnumerable<StatMapEx> GetAdditionalStats()
        {
            if (HasAdditionalHP)
                yield return _statMaps[StatType.HP];
            if (HasAdditionalATK)
                yield return _statMaps[StatType.ATK];
            if (HasAdditionalDEF)
                yield return _statMaps[StatType.DEF];
            if (HasAdditionalCRI)
                yield return _statMaps[StatType.CRI];
            if (HasAdditionalDOG)
                yield return _statMaps[StatType.DOG];
            if (HasAdditionalSPD)
                yield return _statMaps[StatType.SPD];
        }

        public int GetStatValue(StatType statType, bool ignoreAdditional = false)
        {
            switch (statType)
            {
                case StatType.HP:
                    return ignoreAdditional ? HP - AdditionalHP : HP;
                case StatType.ATK:
                    return ignoreAdditional ? ATK - AdditionalATK : ATK;
                case StatType.DEF:
                    return ignoreAdditional ? DEF - AdditionalDEF : DEF;
                case StatType.CRI:
                    return ignoreAdditional ? CRI - AdditionalCRI : CRI;
                case StatType.DOG:
                    return ignoreAdditional ? DOG - AdditionalDOG : DOG;
                case StatType.SPD:
                    return ignoreAdditional ? SPD - AdditionalSPD : SPD;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        public int GetAdditionalStatValue(StatType statType)
        {
            switch (statType)
            {
                case StatType.HP:
                    return AdditionalHP;
                case StatType.ATK:
                    return AdditionalATK;
                case StatType.DEF:
                    return AdditionalDEF;
                case StatType.CRI:
                    return AdditionalCRI;
                case StatType.DOG:
                    return AdditionalDOG;
                case StatType.SPD:
                    return AdditionalSPD;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        public void ClearAdditionalStats()
        {
            if (HasAdditionalHP)
            {
                SetStatAdditionalValue(StatType.HP, 0);
            }

            if (HasAdditionalATK)
            {
                SetStatAdditionalValue(StatType.ATK, 0);
            }

            if (HasAdditionalCRI)
            {
                SetStatAdditionalValue(StatType.CRI, 0);
            }

            if (HasAdditionalDEF)
            {
                SetStatAdditionalValue(StatType.DEF, 0);
            }

            if (HasAdditionalDOG)
            {
                SetStatAdditionalValue(StatType.DOG, 0);
            }

            if (HasAdditionalSPD)
            {
                SetStatAdditionalValue(StatType.SPD, 0);
            }
        }
    }
}
