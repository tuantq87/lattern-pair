using LanternPair.Core.Balance;
using UnityEngine;

namespace LanternPair.Data
{
    /// <summary>
    /// Inspector-editable tuning for a run. Wraps Core's <see cref="BalanceConfig"/> so values can be tweaked without code.
    /// The project's instance lives at Assets/_Data/GameBalance.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalance", menuName = "Lantern Pair/Game Balance")]
    public class GameBalance : ScriptableObject
    {
        [SerializeField] private BalanceConfig values = new BalanceConfig();

        public BalanceConfig Values => values;
    }
}
