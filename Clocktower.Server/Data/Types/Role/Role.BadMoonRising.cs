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

    public static Role Grandmother => BadMoonRisingTownsfolk("Grandmother", "You start knowing a good player & their character. If the Demon kills them, you die too.")
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
        .OtherNight(9, "The previously protected and drunk players are no longer protected and drunk. The innkeeper chooses 2 players, Those players are protected. 1 is drunk.")
        .WithReminders("Safe", "Drunk");

    public static Role Gambler => BadMoonRisingTownsfolk("Gambler", "Each night*, choose a player & guess their character: if you guess wrong, you die.")
        .OtherNight(10, "The gambler chooses a player and a character. If the chosen player is not the chosen character, the Gambler dies.")
        .WithReminders("Dead");

    public static Role Gossip => BadMoonRisingTownsfolk("Gossip", "Each day, you may make a public statement. Tonight, if it was true, a player dies.")
        .OtherNight(38, "If the Gossip's public statement was true: Choose a player not protected from dying tonight. That player dies.")
        .WithReminders("Dead");

    public static Role Courtier => BadMoonRisingTownsfolk("Courtier", "Once per game, at night, choose a character: they are drunk for 3 nights & 3 days.")
        .FirstNight(19, "Ask if they would like to use their ability. If yes, they select a character: If that character is in play, that player is drunk.")
        .OtherNight(8, "Reduce the remaining number of days the marked player is drunk. If the Courtier has not yet used their ability: Ask if they would like to use their ability. If yes, they select a character: If that character is in play, that player is drunk.")
        .WithReminders("Drunk 3", "Drunk 2", "Drunk 1", NoAbilityText);

    public static Role Professor => BadMoonRisingTownsfolk("Professor", "Once per game, at night*, choose a dead player: if they are a Townsfolk, they are resurrected.")
        .OtherNight(43, "If the Professor has not yet used their ability: Ask if they would like to use their ability. If yes, they select a dead player: If that player is a townsfolk, that player is resurrected.")
        .WithReminders("Alive", NoAbilityText);

    public static Role Minstrel => BadMoonRisingTownsfolk("Minstrel", "When a minion dies by execution, all other players (except Travellers) are drunk until dusk tomorrow.")
        .WithReminders("Everyone is Drunk");

    public static Role TeaLady => BadMoonRisingTownsfolk("Tea Lady", "If both your alive neighbours are good, they cannot die.")
        .WithReminders("Cannot die");

    public static Role Pacifist => BadMoonRisingTownsfolk("Pacifist", "Executed good players might not die.");

    public static Role Fool => BadMoonRisingTownsfolk("Fool", "The first time you die, you don't.")
        .WithReminders(NoAbilityText);

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
        .OtherNight(20, "Allow the Lunatic to do the actions of the Demon. Place their 'attack' markers. If the Lunatic selected players: Wake the Demon. Show them who the lunatic selected. Remove any Lunatic 'chosen' markers.")
        .WithReminders("Chosen");

    public static Role Godfather => BadMoonRisingMinion("Godfather", "You start knowing which Outsiders are in play. If 1 died today, choose a player tonight: they die.")
        .FirstNight(21, "Show the Godfather the tokens of all Outsiders in play.")
        .OtherNight(37, "If an Outsider died today: The Godfather selects a player. That player dies.")
        .WithReminders("Died Today", "Dead")
        .AffectsSetup("-1 or +1 Outsider");

    public static Role DevilsAdvocate => BadMoonRisingMinion("Devil's Advocate", "Each night, choose a living player (different to last night): if executed tomorrow, they don't die.")
        .FirstNight(22, "The Devil's Advocate selects a living player. That player survives execution tomorrow.")
        .OtherNight(13, "The Devil's Advocate selects a living player, different from the previous night. That player survives execution tomorrow.")
        .WithReminders("Survives Execution");

    public static Role Assassin => BadMoonRisingMinion("Assassin", "Once per game, at night*, choose a player: they die, even if for some reason they could not.")
        .OtherNight(36, "If the Assassin has not yet used their ability: The Assassin is asked if they want to use their ability, if yes, they select a player. That player dies.")
        .WithReminders("Dead", NoAbilityText);

    public static Role Mastermind => BadMoonRisingMinion("Mastermind", "If the demon dies by execution (ending the game), play for 1 more day. If a player is then executed, their team loses.");

    public static Role Pukka => BadMoonRisingDemon("Pukka", "Each night, choose a player: they are poisoned. The previously poisoned player dies then becomes healthy.")
        .FirstNight(28, "The Pukka selects a player. That player is poisoned.")
        .OtherNight(26, "The Pukka selects a player. That player is poisoned. The previously poisoned player dies.")
        .WithReminders("Poisoned", "Dead");

    public static Role Shabaloth => BadMoonRisingDemon("Shabaloth", "Each night*, choose 2 players: they die. A dead player you chose last night might be regurgitated.")
        .OtherNight(27, "One player that the Shabaloth chose the previous night might be resurrected. The Shabaloth selects two players. Those players die.")
        .WithReminders("Dead", "Alive");

    public static Role Po => BadMoonRisingDemon("Po", "Each night*, you may choose a player: they die. If your last choice was no-one, choose 3 players tonight.")
        .OtherNight(28, "If the Po chose no-one the previous night: The Po selects three players. Otherwise: The Po either selects a player or declines to use ability. Chosen players die")
        .WithReminders("Dead", "3 Attacks");

    public static Role Zombuul => BadMoonRisingDemon("Zombuul", "Each night*, if no-one died today, choose a player: they die. The 1st time you die, you live but register as dead.")
        .OtherNight(25, "If no-one died during the day: The Zombuul selects a player. That player dies.")
        .WithReminders("Dead", "Died Today");

    public static Role Matron => BadMoonRisingTraveller("Matron", "Each day, you may choose up to 3 sets of 2 players to swap seats. Players may not leave their seats to talk in private.");

    public static Role Judge => BadMoonRisingTraveller("Judge", "Once per game, if another player nominated, you may choose to force the current execution to pass or fail.")
        .WithReminders(NoAbilityText);

    public static Role Apprentice => BadMoonRisingTraveller("Apprentice", "On your 1st night, you gain a Townsfolk ability (if good), or a Minion ability (if evil).")
        .FirstNight(1, "Assign and show the Apprentice a Townsfolk or Minion role (where appropriate). From now on they wake at night if their ability would.")
        .WithReminders("Is the Apprentice");

    public static Role Bishop => BadMoonRisingTraveller("Bishop", "Only the Storyteller can nominate. At least 1 opposing player must be nominated each day.")
        .WithReminders("Nominate Good", "Nominate Evil");

    public static Role Voudon => BadMoonRisingTraveller("Voudon", "Only you & the dead can vote. They don't need a vote token to do so. A 50% majority isn't required.");
}