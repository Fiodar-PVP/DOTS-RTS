public enum AnimationType
{
    None,
    SoldierIdle,
    SoldierWalk,
    ZombieIdle,
    ZombieWalk,
    SoldierShoot,
    SoldierAim,
    ZombieAttack,
    ScoutIdle,
    ScoutWalk,
    ScoutShoot,
    ScoutAim,
}

public static class AnimationTypeExtension
{
    public static bool IsInterruptable(this AnimationType animationType)
    {
        switch(animationType)
        {
            default:
                return true;
            case AnimationType.ScoutShoot:
            case AnimationType.SoldierShoot:
            case AnimationType.ZombieAttack:
                return false;
        }
    }
}