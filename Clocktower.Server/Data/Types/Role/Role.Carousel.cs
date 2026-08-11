namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static Role CarouselTownsfolk(string name, string description)
        => Townsfolk(name, description, Edition.Carousel);

    private static Role CarouselOutsider(string name, string description)
        => Outsider(name, description, Edition.Carousel);

    private static Role CarouselMinion(string name, string description)
        => Minion(name, description, Edition.Carousel);

    private static Role CarouselDemon(string name, string description)
        => Demon(name, description, Edition.Carousel);

    private static Role CarouselTraveller(string name, string description)
        => Traveller(name, description, Edition.Carousel);

    public static Role Marionette => CarouselMinion("Marionette", "You think you are a good character, but you are not. The Demon knows who you are. [You neighbour the demon]")
        .WithGlobalReminders("Is The Marionette")
        .FirstNight(5, "Mark a good player neighbouring the demon with the \"Is The Marionette\" reminder. Inform the demon of the Marionette.")
        .AffectsSetup();

    public static Role Boffin => CarouselMinion("Boffin", "The Demon (even if drunk or poisoned) has a not-in-play good character's ability. You both know which.")
        .FirstNight(18, "The demon gains the ability of a not-in-play good character. Show both the Demon and the Boffin which");

    public static Role Psychopath => CarouselMinion("Psychopath", "Each day, before nominations, you may publicly choose a player: they die. If executed, you only die if you loose roshambo.");
    public static Role Boomdandy => CarouselMinion("Boomdandy", "If you are executed, all but 3 players die. After a 10 to 1 countdown, the player with the most players pointing at them, dies.");

    public static Role Wraith => CarouselMinion("Wraith", "You may choose to open your eyes at night. You wake when other evil players do.")
        .EachNight(21, 20, "Wake the wraith whenever evil players wake.");

    public static Role Vizier => CarouselMinion("Vizier", "All players know you are the Vizier. You cannot die during the day. If good voted, you may choose the execute immediately.")
        .FirstNight(90, "Inform the whole town who the Vizier is.");
}