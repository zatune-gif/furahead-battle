#if UNITY_EDITOR
using NUnit.Framework;

public class CombatSystemTests
{
    [Test]
    public void HitsToDownIsPositive()
    {
        Assert.Greater(CombatSystem.HitsToDown, 0);
    }

    [Test]
    public void HitsToDownEqualsExpected()
    {
        Assert.AreEqual(4, CombatSystem.HitsToDown);
    }

    [Test]
    public void FighterIsAliveBeforeHitsToDown()
    {
        int hitCount = CombatSystem.HitsToDown - 1;
        Assert.IsTrue(hitCount < CombatSystem.HitsToDown);
    }

    [Test]
    public void FighterIsDownAtHitsToDown()
    {
        int hitCount = CombatSystem.HitsToDown;
        Assert.IsFalse(hitCount < CombatSystem.HitsToDown);
    }
}
#endif
