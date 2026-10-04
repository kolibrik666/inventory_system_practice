using System;

using InventorySystem;

namespace HaulSystem
{
    public enum HaulRoundState
    {
        Collecting,
        Results,
        Restarting
    }

    public enum HaulAward
    {
        None,
        Bronze,
        Silver,
        Gold
    }

    public readonly struct HaulResult
    {
        public long Score { get; }
        public HaulAward Award { get; }

        internal HaulResult(long score, HaulAward award)
        {
            Score = score;
            Award = award;
        }
    }

    public sealed class HaulRoundModel
    {
        private readonly InventoryModel _inventory;
        private readonly long _bronzeScore;
        private readonly long _silverScore;
        private readonly long _goldScore;

        public HaulRoundState State { get; private set; }
        public HaulResult Result { get; private set; }

        public event Action<HaulResult> Completed;

        public HaulRoundModel(InventoryModel inventory, long bronzeScore, long silverScore, long goldScore)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            if (bronzeScore <= 0) throw new ArgumentOutOfRangeException(nameof(bronzeScore));
            if (silverScore <= bronzeScore) throw new ArgumentOutOfRangeException(nameof(silverScore));
            if (goldScore <= silverScore) throw new ArgumentOutOfRangeException(nameof(goldScore));

            _bronzeScore = bronzeScore;
            _silverScore = silverScore;
            _goldScore = goldScore;
        }

        public bool TryComplete()
        {
            if (State != HaulRoundState.Collecting) return false;

            long score = 0;
            checked
            {
                foreach (InventoryEntry entry in _inventory.Entries)
                {
                    score += (long)entry.Item.ItemMoneyValue * entry.Quantity;
                }
            }

            HaulAward award = score >= _goldScore ? HaulAward.Gold
                : score >= _silverScore ? HaulAward.Silver
                : score >= _bronzeScore ? HaulAward.Bronze : HaulAward.None;
            Result = new HaulResult(score, award);
            State = HaulRoundState.Results;
            Completed?.Invoke(Result);
            return true;
        }

        public bool TryBeginRestart()
        {
            if (State != HaulRoundState.Results) return false;
            State = HaulRoundState.Restarting;
            return true;
        }
    }
}
