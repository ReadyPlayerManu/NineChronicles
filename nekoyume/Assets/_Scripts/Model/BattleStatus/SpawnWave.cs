using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nekoyume.Model
{
    [Serializable]
    public class SpawnWave : EventBase
    {
        public readonly List<Enemy> Enemies;
        public readonly bool IsBoss;
        
        public SpawnWave(CharacterBase character, List<Enemy> enemies, bool isBoss) : base(character)
        {
            Enemies = enemies;
            IsBoss = isBoss;
        }
        
        public override IEnumerator CoExecute(IStage stage)
        {
            yield return stage.CoSpawnWave(Enemies, IsBoss);
        }
    }
}
