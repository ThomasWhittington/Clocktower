namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static Role TroubleBrewingTownsfolk(string name, string description)
        => Townsfolk(name, description, Edition.TroubleBrewing);

    private static Role TroubleBrewingOutsider(string name, string description)
        => Outsider(name, description, Edition.TroubleBrewing);

    private static Role TroubleBrewingMinion(string name, string description)
        => Minion(name, description, Edition.TroubleBrewing);

    private static Role TroubleBrewingDemon(string name, string description)
        => Demon(name, description, Edition.TroubleBrewing);

    private static Role TroubleBrewingTraveller(string name, string description)
        => Traveller(name, description, Edition.TroubleBrewing);

    public static Role Chef => TroubleBrewingTownsfolk("Chef", "You start knowing how many pairs of evil players there are.")
        .FirstNight(36, "Show (0, 1, 2, etc) for the number of pairs of neighbouring evil players.");

    public static Role Empath => TroubleBrewingTownsfolk("Empath", "Each night, you learn how many of your 2 alive neighbours are evil.")
        .EachNight(37, 53, "Show (0, 1, 2, etc) for the number of evil alive neighbours of the Empath.");

    public static Role FortuneTeller => TroubleBrewingTownsfolk("Fortune Teller", "Each night, choose 2 players: you learn if either is a Demon. There is a good player that registers as a Demon to you.")
        .EachNight(38, 54, "The Fortune Teller chooses two players. Show YES or NO for whether either is a Demon or the Red Herring.")
        .WithReminder("Red Herring");

    public static Role Investigator => TroubleBrewingTownsfolk("Investigator", "You start knowing that 1 of 2 players is a particular Minion.")
        .FirstNight(35, "Show the investigator a Minion character and 2 players. One of the players is the Minion.")
        .WithReminders(["Minion", "Wrong"]);

    public static Role Librarian => TroubleBrewingTownsfolk("Librarian", "You start knowing that 1 of 2 players is a particular Outsider. (Or that zero are in play.)")
        .FirstNight(34, "Show the librarian an Outsider character and 2 players. One of the players is the Outsider.")
        .WithReminders(["Outsider", "Wrong"]);

    public static Role Mayor => TroubleBrewingTownsfolk("Mayor", "If only 3 players live & no execution occurs, your team wins. If you die at night, another player might die instead.");

    public static Role Monk => TroubleBrewingTownsfolk("Monk", "Each night*, choose a player (not yourself): they are safe from the Demon tonight.")
        .OtherNight(12, "The previously protected player is no longer safe. The monk selects a player (not themself). Mark that player as 'Safe'.")
        .WithReminder("Safe");

    public static Role Ravenkeeper => TroubleBrewingTownsfolk("Ravenkeeper", "If you die at night, you are woken to choose a player: you learn their character.")
        .OtherNight(52, "If the Ravenkeeper died tonight: The Ravenkeeper chooses a player. Show them that players Character.");

    public static Role Slayer => TroubleBrewingTownsfolk("Slayer", "Once per game, during the day, publicly choose a player: if they are the Demon, they die.")
        .WithReminder("No Ability");

    public static Role Soldier => TroubleBrewingTownsfolk("Soldier", "You are safe from the Demon.");

    public static Role Undertaker => TroubleBrewingTownsfolk("Undertaker", "Each night*, you learn which character died by execution today.")
        .OtherNight(55, "If a player was EXECUTED today: Show that player's character token.")
        .WithReminder("Died Today");

    public static Role Virgin => TroubleBrewingTownsfolk("Virgin", "The 1st time you are nominated, if the nominator is a Townsfolk, they are EXECUTED immediately.")
        .WithReminder("No Ability");

    public static Role Washerwoman => TroubleBrewingTownsfolk("Washerwoman", "On your first night, you learn that 1 of 2 players is a particular Townsfolk.")
        .FirstNight(34, "Show the Washerwoman a Townsfolk character and 2 players. One of the players is the Townsfolk.")
        .WithReminders(["Townsfolk", "Wrong"]);

    public static Role Butler => TroubleBrewingOutsider("Butler", "Each night, choose a player (not yourself): tomorrow, you may only vote if they are voting too.")
        .EachNight(39, 67, "The Butler chooses a player. Mark them as 'Master'")
        .WithReminder("Master");

    public static Role Drunk => TroubleBrewingOutsider("Drunk", "You do not know you are the Drunk. You think you are a Townsfolk, but you are not.")
        .WithGlobalReminder("Is the Drunk")
        .AffectsSetup();

    public static Role Recluse => TroubleBrewingOutsider("Recluse", "You might register as evil & as a Minion or Demon, even if dead.");

    public static Role Saint => TroubleBrewingOutsider("Saint", "If you die by execution, your team loses.");

    public static Role Baron => TroubleBrewingMinion("Baron", "There are extra Outsiders in play. [+2 Outsiders]")
        .AffectsSetup();

    public static Role Poisoner => TroubleBrewingMinion("Poisoner", "Each night, choose a player: they are poisoned tonight and tomorrow day.")
        .EachNight(17, 7, "The previously poisoned player is no longer poisoned. The Poisoner selects a player. Mark that player as 'Poisoned'.")
        .WithReminder("Poisoned");

    public static Role ScarletWoman => TroubleBrewingMinion("Scarlet Woman", "If there are 5 or more players alive & the Demon dies, you become the Demon. (Travellers don't count.)")
        .OtherNight(19, "If the Scarlet Woman became the Demon today: change their character to the Demon.")
        .WithReminder("Is the Demon");

    public static Role Spy => TroubleBrewingMinion("Spy", "Each night, you see the Grimoire. You might register as good & as a Townsfolk or Outsider, even if dead.")
        .EachNight(49, 68, "Show the Grimoire to the Spy for as long as they need.");

    public static Role Imp => TroubleBrewingDemon("Imp", "Each night*, choose a player: they die. If you kill yourself this way, a Minion becomes the Imp.")
        .OtherNight(24, "The Imp picks a player. That player dies. If the Imp chose themselves: Replace the character of 1 alive minion with the Imp token.")
        .WithReminder("Dead");

    public static Role Scapegoat => TroubleBrewingTraveller("Scapegoat", "If a player of your alignment is executed, you might be executed instead.");
    public static Role Gunslinger => TroubleBrewingTraveller("Gunslinger", "Each day, after the 1st vote has been tallied, you may choose a player that voted: they die.");
    public static Role Beggar => TroubleBrewingTraveller("Beggar", "You must use a vote token to vote. If a dead player gives you theirs, you learn their alignment. You are sober and healthy.");

    public static Role Bureaucrat => TroubleBrewingTraveller("Bureaucrat", "Each night, choose a player (not yourself): tomorrow, their vote counts as 3 votes.")
        .EachNight(1, 1, "The Bureaucrat picks a player (not themselves). Mark that player with the '3 Votes' reminder.")
        .WithReminder("3 Votes");

    public static Role Thief => TroubleBrewingTraveller("Thief", "Each night, choose a player (not yourself): tomorrow, their vote counts negatively.")
        .EachNight(1, 1, "The Thief picks a player (not themselves). Mark that player with the 'Negative Vote' reminder.")
        .WithReminder("Negative Vote");
}