namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static Role TroubleBrewingTownsfolk(string name, string description) => Townsfolk(name, description, Edition.TroubleBrewing);
    private static Role TroubleBrewingOutsider(string name, string description) => Outsider(name, description, Edition.TroubleBrewing);
    private static Role TroubleBrewingMinion(string name, string description) => Minion(name, description, Edition.TroubleBrewing);
    private static Role TroubleBrewingDemon(string name, string description) => Demon(name, description, Edition.TroubleBrewing);
    private static Role TroubleBrewingTraveller(string name, string description) => Traveller(name, description, Edition.TroubleBrewing);

    public static Role Chef() => TroubleBrewingTownsfolk("Chef", "Each night, you learn how many pairs of evil players are sitting next to each other.");
    public static Role Empath() => TroubleBrewingTownsfolk("Empath", "Each night, you learn how many of your 2 alive neighbors are evil.");
    public static Role FortuneTeller() => TroubleBrewingTownsfolk("Fortune Teller", "Each night, choose 2 players: you learn if either is a Demon. There is a good player who registers as a Demon to you.");
    public static Role Investigator() => TroubleBrewingTownsfolk("Investigator", "On your first night, you learn that 1 of 2 players is a particular Minion.");
    public static Role Librarian() => TroubleBrewingTownsfolk("Librarian", "On your first night, you learn that 1 of 2 players is a particular Outsider.");
    public static Role Mayor() => TroubleBrewingTownsfolk("Mayor", "If only 3 players live & no execution occurs, your team wins. If you die at night, another player might die instead.");
    public static Role Monk() => TroubleBrewingTownsfolk("Monk", "Each night*, choose a player (not yourself): they are safe from the Demon tonight.");
    public static Role Ravenkeeper() => TroubleBrewingTownsfolk("Ravenkeeper", "If you die at night, you are woken to choose a player: you learn their role.");
    public static Role Slayer() => TroubleBrewingTownsfolk("Slayer", "Once per game, during the day, publicly choose a player: if they are the Demon, they die.");
    public static Role Soldier() => TroubleBrewingTownsfolk("Soldier", "You are safe from the Demon.");
    public static Role Undertaker() => TroubleBrewingTownsfolk("Undertaker", "Each night*, you learn which player died by execution today.");
    public static Role Virgin() => TroubleBrewingTownsfolk("Virgin", "The 1st time you are nominated, if the nominator is a Townsfolk, they are executed immediately.");
    public static Role Washerwoman() => TroubleBrewingTownsfolk("Washerwoman", "On your first night, you learn that 1 of 2 players is a particular Townsfolk.");
    public static Role Butler() => TroubleBrewingOutsider("Butler", "Each night, choose a player (not yourself): tomorrow, you may only vote if they vote.");
    public static Role Drunk() => TroubleBrewingOutsider("Drunk", "You think you are a Townsfolk, but you are not. (You are shown a random Townsfolk token at setup)");
    public static Role Recluse() => TroubleBrewingOutsider("Recluse", "You might register as evil & as a Minion or Demon, even if dead.");
    public static Role Saint() => TroubleBrewingOutsider("Saint", "If you die by execution, your team loses.");
    public static Role Baron() => TroubleBrewingMinion("Baron", "There are 2 extra Outsiders in play.");
    public static Role Poisoner() => TroubleBrewingMinion("Poisoner", "Each night, choose a player: they are poisoned tonight and tomorrow day.");
    public static Role ScarletWoman() => TroubleBrewingMinion("Scarlet Woman", "If there are 5 or more players alive & the Demon dies, you become the Demon.");
    public static Role Spy() => TroubleBrewingMinion("Spy", "Each night, you see the Grimoire. You might register as good & as a Townsfolk or Outsider, even if dead.");
    public static Role Imp() => TroubleBrewingDemon("Imp", "Each night*, choose a player: they die. If you kill yourself this way, a Minion becomes the Imp.");
    public static Role Scapegoat() => TroubleBrewingTraveller("Scapegoat", "If a player of your alignment is executed, you might be executed instead.");
    public static Role Gunslinger() => TroubleBrewingTraveller("Gunslinger", "Each day, after the 1st vote has been tallied, you may choose a player that voted: they die.");
    public static Role Beggar() => TroubleBrewingTraveller("Beggar", "You must use a vote token to vote. If you die, you lose all your vote tokens. Each dawn, gain 1 vote token.");
    public static Role Bureaucrat() => TroubleBrewingTraveller("Bureaucrat", "Each night*, choose a player: tomorrow, they must nominate & may nominate any number of times.");
    public static Role Thief() => TroubleBrewingTraveller("Thief", "Each night*, choose a player (not yourself) & a character: their vote counts as that character's alignment.");
}