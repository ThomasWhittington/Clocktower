namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static Role SectsAndVioletsTownsfolk(string name, string description)
        => Townsfolk(name, description, Edition.SectsAndViolets);

    private static Role SectsAndVioletsOutsider(string name, string description)
        => Outsider(name, description, Edition.SectsAndViolets);

    private static Role SectsAndVioletsMinion(string name, string description)
        => Minion(name, description, Edition.SectsAndViolets);

    private static Role SectsAndVioletsDemon(string name, string description)
        => Demon(name, description, Edition.SectsAndViolets);

    private static Role SectsAndVioletsTraveller(string name, string description)
        => Traveller(name, description, Edition.SectsAndViolets);

    public static Role Clockmaker => SectsAndVioletsTownsfolk("Clockmaker", "You start knowing how many steps from the demon to it's nearest minion.")
        .FirstNight(41, "Give the Clockmaker the number of steps from the demon to its nearest minion. A step is a the space between players so a demon with a single player between them and a minion will result in a 2.");

    public static Role Dreamer => SectsAndVioletsTownsfolk("Dreamer", "Each night, choose a player (not yourself or Travellers): you learn 1 good & 1 evil character, 1 of which is correct")
        .EachNight(42, 56, "The dreamer selects a player. Show 1 good and 1 evil character token; one of these is correct");

    public static Role SnakeCharmer => SectsAndVioletsTownsfolk("Snake Charmer", "Each night, choose an alive player: a chosen Demon swaps characters & alignments with you & is then poisoned.")
        .EachNight(20, 11, "The Snake Charmer selects a player. If that player is the Demon: swap the Demon and Snake Charmer character and alignments. Wake each player to inform them of their new role and alignment. The new Snake Charmer is poisoned.")
        .WithReminders("Poisoned");

    public static Role Mathematician => SectsAndVioletsTownsfolk("Mathematician", "Each night, you learn how many players' abilities worked abnormally (since dawn) due to another character's ability.")
        .EachNight(52, 71, "Give the Mathematician the number of players whose ability malfunctioned due to other abilities.")
        .WithReminders("Abnormal");

    public static Role Flowergirl => SectsAndVioletsTownsfolk("Flowergirl", "Each night*, you learn if a Demon voted today.")
        .OtherNight(57, "Give a Yes or No for if the Demon voted today.")
        .WithReminders("Demon Voted", "Demon Not Voted");

    public static Role TownCrier => SectsAndVioletsTownsfolk("Town Crier", "Each night*, you learn if a Minion nominated today.")
        .OtherNight(58, "Give a Yes or No for if a Minion nominated today.")
        .WithReminders("Minion Nominated", "Minions Not Nominated");

    public static Role Oracle => SectsAndVioletsTownsfolk("Oracle", "Each night*, you learn how many dead players are evil.")
        .OtherNight(59, "Give the Oracle the number of players that are evil.");

    public static Role Savant => SectsAndVioletsTownsfolk("Savant", "Each day, you may visit the Storyteller to learn 2 things in private: 1 is true & 1 is false.");

    public static Role Seamstress => SectsAndVioletsTownsfolk("Seamstress", "Once per game, at night, choose 2 players (not yourself): you learn if they are the same alignment.")
        .EachNight(43, 60, "Ask the seamstress if they would like to use their ability. If yes, they choose two players and learn if they are the same alignment.")
        .WithReminders(NoAbilityText);

    public static Role Philosopher => SectsAndVioletsTownsfolk("Philosopher", "Once per game, at night, choose a good character: gain that ability. If this character is in play, they become drunk.")
        .EachNight(2, 2, "Ask the Philosopher if they would like to use their ability. If yes, they pick a good character and gain that ability (they are still the philosopher). If the selected role is already in play, the existing player becomes drunk.")
        .WithReminders("Is the Philosopher", "Drunk");

    public static Role Artist => SectsAndVioletsTownsfolk("Artist", "Once per game, during the day, privately ask the Storyteller any yes/no question.")
        .WithReminders(NoAbilityText);

    public static Role Juggler => SectsAndVioletsTownsfolk("Juggler", "On your 1st day, publicly guess up to 5 players' characters. That night, you learn how many you got correct.")
        .OtherNight(61, "If today was the Juggler's first day: Give them the number of correct guesses they made.")
        .WithReminders("Correct");

    public static Role Sage => SectsAndVioletsTownsfolk("Sage", "If the Demon kills you, you learn 2 players, one of which is the Demon.")
        .OtherNight(42, "If the sage was killed by a Demon: Give them 2 players, 1 of which is the Demon.");

    public static Role Mutant => SectsAndVioletsOutsider("Mutant", "If you are \"mad\" about being an Outsider, you might be EXECUTED");

    public static Role Barber => SectsAndVioletsOutsider("Barber", "If you died today or tonight, the Demon may choose 2 players (not another Demon) to swap characters.")
        .OtherNight(40, "If the barber died today, tell the Demon the barber has died. As the demon if they wish to use the ability given by the barber. If yes, they choose 2 players (not another Demon) to swap characters. Wake those players and inform them of their new role. Their alignments are not affected.")
        .WithReminders("Haircuts tonight");

    public static Role Sweetheart => SectsAndVioletsOutsider("Sweetheart", "When you die, 1 player is drunk from now on.")
        .OtherNight(41, "If the Sweetheart died, choose a player to be drunk from now on.")
        .WithReminders("Drunk");

    public static Role Klutz => SectsAndVioletsOutsider("Klutz", "When you learn you died, publicly choose 1 alive player: if they are evil, your team loses.");

    public static Role Witch => SectsAndVioletsMinion("Witch", "Each night, choose a player: if they nominate tomorrow, they die (not executed). If just 3 players live, you lose this ability.")
        .FirstNight(24, "The Witch chooses a player. Mark them as 'Cursed'")
        .OtherNight(14, "If there are 4 or more players alive: The Witch chooses a player. Mark them as 'Cursed'")
        .WithReminders("Cursed");

    public static Role Cerenovus => SectsAndVioletsMinion("Cerenovus", "Each night, choose a player & a good character: they are \"mad\" they are this character tomorrow, or might be executed (even if already dead).")
        .EachNight(25, 15, "The Cerenovus chooses a player and a good character. Mark them as 'Mad'. Wake the selected player and inform them that the cerenovus selected them. Inform them of their madness. If they are not \"Mad\" tomorrow, they can be executed.")
        .WithReminders("Mad");

    public static Role PitHag => SectsAndVioletsMinion("Pit-Hag", "Each night*, choose a player and a character they become (if not in play). If a Demon is made, deaths tonight are arbitrary.")
        .OtherNight(16, "The pithag chooses a player and a character. If this character is not in play, wake the selected player and inform them of their new role. Their alignments are not affected.");

    public static Role EvilTwin => SectsAndVioletsMinion("Evil Twin", "You & an opposing player know each other. If the good player is executed, evil wins. Good can't win if you both live.")
        .FirstNight(23, "Wake the evil twin and inform them of the good twin and the good twin's role. Wake the good twin, inform them that they are the good twin and inform them who the evil twin is.")
        .WithReminders("Twin");

    public static Role FangGu => SectsAndVioletsDemon("Fang Gu", "Each night*, choose a player, they die. The 1st outsider this kills becomes an evil Fang Gu and you die instead.")
        .OtherNight(29, "The Fang Gu chooses a player. If this player is not an outsider: they die. If this player is an outsider, the Fang Gu dies then wake them and inform them that they are now an evil Fang Gu.")
        .WithReminders(DeadText, "Once")
        .AffectsSetup("+1 Outsider");

    public static Role Vigormortis => SectsAndVioletsDemon("Vigormortis", "Each night*, choose a player: they die. Minions you kill keep their ability and poison 1 Townsfolk neighbour.")
        .OtherNight(32, "The Vigormortis chooses a player: they die. If this player is a minion: they keep their ability and one of their Townsfolk neighbours is poisoned")
        .WithReminders(DeadText, "Has Ability", "Poisoned")
        .AffectsSetup("-1 Outsider");

    public static Role NoDashii => SectsAndVioletsDemon("No Dashii", "Each night*, choose a player: they die. Your 2 Townsfolk neighbours are poisoned.")
        .OtherNight(30, "The No Dashii chooses a player: they die. Their 2 Townsfolk neighbours are poisoned.")
        .WithReminders(DeadText, "Poisoned");

    public static Role Vortox => SectsAndVioletsDemon("Vortox", "Each night*, choose a player: they die. Townsfolk abilities yield objectively false information. Each day, if no-one is executed, evil wins. (Executing dead players counts as an execution).")
        .OtherNight(31, "The Vortox chooses a player: they die.")
        .WithReminders(DeadText);

    public static Role Barista => SectsAndVioletsTraveller("Barista", "Each night, until dusk, 1) a player becomes sober, healthy and gets true info, or 2) their ability works twice. The selected player learns which.")
        .EachNight(1, 1, "Choose a player, wake them and tell them which Barista power is affecting them. Treat them accordingly (sober/healthy/true info or activate their ability twice).")
        .WithReminders("Sober & Healthy", "Ability Twice");

    public static Role Harlot => SectsAndVioletsTraveller("Harlot", "Each night*, choose a living player: if they agree, you learn their character, but you both might die.")
        .OtherNight(1, "The Harlot chooses a player. Wake the chosen player, Inform them that the Harlot selected them. Ask if they wish to share their character, If yes: wake the Harlot and show them the chosen player's character token. Then, you may decide that both players die.")
        .WithReminders(DeadText);

    public static Role Butcher => SectsAndVioletsTraveller("Butcher", "Each day, after the first execution, you may nominate again.");

    public static Role BoneCollector => SectsAndVioletsTraveller("Bone Collector", "Once per game, at night*, choose a dead player, they regain their ability until dusk.")
        .OtherNight(1, "Ask the Bone Collector if they wish to use their ability. If yes: They choose a dead player, 'Has Ability' reminder token. (They may now wake this night if their ability is in the order)")
        .WithReminders(NoAbilityText, "Has Ability");

    public static Role Deviant => SectsAndVioletsTraveller("Deviant", "If you were funny today, you cannot die by exile.");
}