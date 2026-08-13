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

    public static Role Goblin => CarouselMinion("Goblin", "If you public claim to be the Goblin when nominated & are executed that day, your team wins.")
        .WithReminders("Claimed");

    public static Role Wizard => CarouselMinion("Wizard", "Once per game, choose to make a wish. If granted, it might have a price & leave a clue as to it's nature.")
        .EachNight(75, 80, "If the wizard can still make a wish, ask if they wish to do so. Accept or decline the wish.")
        .WithReminders("?");

    public static Role Mezepheles => CarouselMinion("Mezepheles", "You start knowing a secret word / phrase. The 1st good player to say this word becomes evil that night.")
        .FirstNight(19, "Pick a secret word / phrase and tell the Mezepheles what it is.")
        .OtherNight(12, "If a good player said the secret word / phrase today: Wake that player. Tell them they are now evil. (Optionally, wake the Mezepheles to tell them who turned evil)")
        .WithReminders("Turns Evil", "No Ability");

    public static Role OrganGrinder => CarouselMinion("Organ Grinder", "All players keep their eyes closed when voting and the vote tally is secret. Each night, choose if you are drunk until dusk.")
        .EachNight(24, 10, "Ask the Organ Grinder if they wish to be Drunk or Sober the following day.")
        .WithReminders("Drunk", "About To Die");

    public static Role Fearmonger => CarouselMinion("Fearmonger", "Each night, choose a player: if you nominate & execute them, their team loses. All players know if you choose a new player.")
        .FirstNight(18, "Ask the Fearmonger to pick a player. Place the FEAR marker on them. Announce to the town today that a Fearmonger is in play and has chosen a player.")
        .OtherNight(11, "Ask the Fearmonger to pick a player. Move the FEAR marker if needed. Announce to the town today that the Fearmonger chose a player.")
        .WithReminders("Fear");

    public static Role Widow => CarouselMinion("Widow", "On your 1st night, look at the Grimoire & choose a player: they are poisoned. 1 good player knows a Widow is in play.")
        .FirstNight(15, "Show the Widow the Grimoire. Ask the Widow to pick a player. That player is poisoned for the remainder of the game (does not end on Widow death).")
        .WithReminders("Poisoned", "Known");

    public static Role Xaan => CarouselMinion("Xaan", "On night X, all Townsfolk are poisoned until dusk. [X Outsiders]")
        .FirstNight(14, "Show the Xaan the number X (1, 2, or 3) based on setup.")
        .WithReminders("Night 1", "Night 2", "Night 3", "X")
        .AffectsSetup();

    public static Role Summoner => CarouselMinion("Summoner", "You get 3 bluffs. On the 3rd night, choose a player: they become an evil Demon of your choice. [No Demon]")
        .FirstNight(80, "Show the Summoner 3 unassigned good character tokens as bluffs.")
        .OtherNight(22, "If this is Night 3: Ask the Summoner to point to a player and a Demon character on the character sheet. That player becomes that Demon and turns Evil.")
        .WithReminders("Night 1", "Night 2", "Night 3")
        .AffectsSetup();

    public static Role Harpy => CarouselMinion("Harpy", "Each night, choose 2 players: tomorrow, the 1st player is mad that the 2nd is evil, or one or both might die.")
        .EachNight(28, 16, "Ask the Harpy to select 2 players. Mark 1st 'Mad' and 2nd '2nd'. Put Harpy to sleep. Wake 1st player: show 'This Character Selected You', show Harpy token, point to 2nd player.")
        .WithReminders("Mad", "2nd");
}