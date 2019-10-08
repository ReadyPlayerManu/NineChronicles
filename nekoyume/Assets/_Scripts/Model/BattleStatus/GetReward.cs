using System;
using System.Collections;
using System.Collections.Generic;
using Nekoyume.Game.Item;

namespace Nekoyume.Model
{
    [Serializable]
    public class GetReward : EventBase
    {
        public readonly List<ItemBase> Rewards;
        
        public GetReward(CharacterBase character, List<ItemBase> rewards) : base(character)
        {
            Rewards = rewards;
        }
        
        public override IEnumerator CoExecute(IStage stage)
        {
            yield return stage.CoGetReward(Rewards);
        }
    }
}
