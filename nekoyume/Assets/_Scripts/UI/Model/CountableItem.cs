using System;
using Nekoyume.EnumType;
using Nekoyume.Game.Item;
using UniRx;

namespace Nekoyume.UI.Model
{
    public class CountableItem : Item
    {
        public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>(0);
        public readonly ReactiveProperty<bool> CountEnabled = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<bool> Dimmed = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<Func<CountableItem, bool>> CountEnabledFunc = new ReactiveProperty<Func<CountableItem, bool>>();
        
        public CountableItem(ItemBase item, int count) : base(item)
        {
            Count.Value = count;
            CountEnabledFunc.Value = CountEnabledFuncDefault;

            CountEnabledFunc.Subscribe(func =>
            {
                if (CountEnabledFunc.Value == null)
                {
                    CountEnabledFunc.Value = CountEnabledFuncDefault;
                }

                CountEnabled.Value = CountEnabledFunc.Value(this);
            });
        }
        
        public override void Dispose()
        {
            base.Dispose();
            
            Count.Dispose();
            CountEnabledFunc.Dispose();
            Dimmed.Dispose();
            CountEnabledFunc.Dispose();
        }

        private bool CountEnabledFuncDefault(CountableItem countableItem)
        {
            if (countableItem.ItemBase.Value == null)
            {
                return false;
            }
            
            return countableItem.ItemBase.Value.Data.ItemType == ItemType.Material;
        }
    }
}
