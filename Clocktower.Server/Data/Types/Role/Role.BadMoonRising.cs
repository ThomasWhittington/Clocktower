namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static Role BadMoonRisingTownsfolk(string name, string description)
        => Townsfolk(name, description, Edition.BadMoonRising);

    private static Role BadMoonRisingOutsider(string name, string description)
        => Outsider(name, description, Edition.BadMoonRising);

    private static Role BadMoonRisingMinion(string name, string description)
        => Minion(name, description, Edition.BadMoonRising);

    private static Role BadMoonRisingDemon(string name, string description)
        => Demon(name, description, Edition.BadMoonRising);

    private static Role BadMoonRisingTraveller(string name, string description)
        => Traveller(name, description, Edition.BadMoonRising);

    public static Role GrandMother => BadMoonRisingTownsfolk("Grandmother", "You start knowing a good player & their character. If the Demon kills them, you die too.")
        .FirstNight(40, "Show the grandmother a player then show what character that player is.")
        .OtherNight(51, "If the Grandmother's grandchild was killed by the Demon tonight: The Grandmother dies.")
        .WithReminders("Grandchild", "Dead");

    public static Role Sailor => BadMoonRisingTownsfolk("Sailor", "Each night, choose an alive player: either you or they are drunk until dusk. You cannot die. (When you are drunk, your ability doesn't work and therefore you can die.)")
        .FirstNight(11, "The Sailor chooses a living player. Either the Sailor, or the chosen player, is drunk.")
        .OtherNight(4, "The previously drunk player is no longer drunk. The Sailor chooses a living player. Either the Sailor, or the chosen player, is drunk.")
        .WithReminders("Drunk");

    public static Role Chambermaid => BadMoonRisingTownsfolk("Chambermaid", "Each night, choose 2 alive players (not yourself): you learn how many woke tonight due to their ability.")
        .EachNight(51, 70, "The chambermaid selects 2 players, show them (0,1,2) for how many of those players wake tonight for their ability.");

    public static Role Exorcist => BadMoonRisingTownsfolk("Exorcist", "Each night*, choose a player (different to last night): the Demon, if chosen, learns who you are then doesn't wake tonight.")
        .OtherNight(21, "The Exorcist chooses a player (different to last night). If that player is a Demon, Wake the demon and tell them who the exorcist is. The Demon does not act tonight.")
        .WithReminders("Chosen");

    public static Role Innkeeper => BadMoonRisingTownsfolk("Innkeeper", "Each night*, choose 2 players: they can't die tonight, but 1 is drunk until dusk.")
        .OtherNight(9, "The previously protected and drunk players are no longer protected and drunk. The innkeeper chooses 2 players, Those players are projected. 1 is drunk.")
        .WithReminders("Safe", "Drunk");

    public static Role Gambler => BadMoonRisingTownsfolk("Gambler", "Each night*, choose a player & guess their character: if you guess wrong, you die.")
        .OtherNight(10, "The gambler chooses a player and a character. If the chosen player is not the chosen character, the Gambler dies.")
        .WithReminders("Dead");

    public static Role Gossip => BadMoonRisingTownsfolk("Gossip", "Each day, you may make a public statement. Tonight, if it was true, a player dies.")
        .OtherNight(38, "If the Gossip's public statement was true: Choose a player not protected from dying tonight. That player dies.")
        .WithReminders("Dead");

    public static Role Courtier => BadMoonRisingTownsfolk("Courtier", "Once per game, at night, choose a character: they are drunk for 3 nights & 3 days.")
        .FirstNight(19, "Ask if they would like to use their ability. If yes, they select a character: If that character is in play, that player is drunk.")
        .OtherNight(8, "Reduce the remaining number of days the marked player is poisoned. If the Courtier has not yet used their ability: Ask if they would like to use their ability. If yes, they select a character: If that character is in play, that player is drunk.")
        .WithReminders("Drunk 3", "Drunk 2", "Drunk 1", "No Ability");

    public static Role Professor => BadMoonRisingTownsfolk("Professor", "Once per game, at night*, choose a dead player: if they are a Townsfolk, they are resurrected.")
        .OtherNight(43, "If the Professor has not yet used their ability: Ask if they would like to use their ability. If yes, they select a a dead player: If that player is a townsfolk, that player is resurrected.")
        .WithReminders("Alive", "No Ability");

    public static Role Minstrel => BadMoonRisingTownsfolk("Minstrel", "When a minion dies by execution, all other players (except Travellers) are drunk until dusk tomorrow.")
        .WithReminders("Everyone is Drunk");

    public static Role TeaLady => BadMoonRisingTownsfolk("Tea Lady", "If both your alive neighbours are good, they cannot die.")
        .WithReminders("Cannot die");

    public static Role Pacifist => BadMoonRisingTownsfolk("Pacifist", "Executed good players might not die.");

    public static Role Fool => BadMoonRisingTownsfolk("Fool", "The first time you die, you don't.")
        .WithReminders("No Ability");


    public static Role Tinker => BadMoonRisingOutsider("Tinker", "You might die at any time.")
        .OtherNight(49, "The Tinker might die.")
        .WithReminders("Dead");

    public static Role Moonchild => BadMoonRisingOutsider("Moonchild", "When you learn you died, publicly choose 1 alive player. Tonight, if it was a good character, they die.")
        .OtherNight(50, "If the Moonchild used their ability to target a player today: If that player is good, they die.")
        .WithReminders("Dead");

    public static Role Goon => BadMoonRisingOutsider("Goon", "Each night, the 1st player to choose you with their ability is drunk until dusk. You become their alignment.")
        .WithReminders("Drunk");

    public static Role Lunatic => BadMoonRisingOutsider("Lunatic", "You think you are a Demon, but you are not. The Demon knows who you are & who you choose at night.")
        .FirstNight(8, "If 7 or more players: Show the Lunatic a number of arbitrary 'Minions', players equal to the number of Minions in play. Show 3 character tokens of arbitrary good characters. If the token received by the Lunatic is a Demon that would wake tonight: Allow the Lunatic to do the Demon actions. Place their 'chosen' markers. Wake the Demon. Show the Demon's real character token. Show them the Lunatic player. If the Lunatic attacked players: Show the real demon each marked player. Remove any Lunatic 'chosen' markers.")
        .OtherNight(20, "Allow the Lunatic to do the actions of the Demon. Place their 'attack' markers. If the Lunatic selected players: Wake the Demon. Show the 'chosen' marker, then point to each marked player. Remove any Lunatic 'chosen' markers.")
        .WithReminders("Chosen");
}