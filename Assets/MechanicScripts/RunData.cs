using System;

[Serializable]
public class RunData
{
    public int totalZombieKills;
    public int deathByHealth;
    public int deathByHunger;
    public int deathByThirst;
    
    // Rekor sure saniye cinsinden, 0 ise hic bitirilmedi anlamina gelir
    public float fastestWinTime;

    public RunData()
    {
        totalZombieKills = 0;
        deathByHealth = 0;
        deathByHunger = 0;
        deathByThirst = 0;
        fastestWinTime = 0f;
    }
}