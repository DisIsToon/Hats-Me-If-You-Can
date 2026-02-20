public static class TutorialEvents
{
    public static bool Jumped;
    public static bool OpenedInventory;
    public static bool OpenedHatalogue;
    public static bool BrewedPotion;
    public static bool ThrewPotion;
    public static bool HatCaptured;

    public static void Reset()
    {
        Jumped = false;
        OpenedInventory = false;
        OpenedHatalogue = false;
        BrewedPotion = false;
        ThrewPotion = false;
        HatCaptured = false;
    }
}
