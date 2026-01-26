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

    public static Role Clockmaker => SectsAndVioletsTownsfolk("Clockmaker", "You start knowing how many steps from the demon to it's nearest minion.")
        .FirstNight(41, "Give the Clockmaker the number of steps from the demon to it's nearest minion. A step is a the space between players so a demon with a single player between them and a minion will result in a 2.");

    public static Role Dreamer => SectsAndVioletsTownsfolk("Dreamer", "Each night, choose a player (not yourself or Travellers): you learn 1 good & 1 evil character, 1 of which is correct")
        .EachNight(42, 56, "The dreamer selects a player. Show 1 good and 1 evil character token; one of these is correct");

    public static Role SnakeCharmer => SectsAndVioletsTownsfolk("Snake Charmer", "Each night, choose an alive player: a chosen Demon swaps characters & alignments with you & is then poisoned.")
        .EachNight(20, 11, "The Snake Charmer selects a player. If that player is the Demon: swap the Demon and Snake Charmer character and alignments. Wake each player to inform them of their new role and alignment. The new Snake Charmer is poisoned.")
        .WithReminders("Poisoned");

    public static Role Mathematician => SectsAndVioletsTownsfolk("Mathematician", "Each night, you learn how many players's abilities worked abnormally (since dawn) due to another character's ability.")
        .EachNight(52, 71, "Give the Mathematician the number of players whose ability malfunctioned due to other abilities.")
        .WithReminders("Abnormal");

    public static Role FlowerGirl => SectsAndVioletsTownsfolk("Flowergirl", "Each night*, you learn if a Demon voted today.")
        .OtherNight(57, "Give a Yes or No for if the Demon voted today.")
        .WithReminders("Demon Voted", "Demon Not Voted");

    public static Role TownCrier => SectsAndVioletsTownsfolk("Town Crier", "Each night*, you learn if a Minion nominated today.")
        .OtherNight(58, "Give a Yes or No for if a Minion nominated today.")
        .WithReminders("Minion Nominated", "Minions Not Nominated");

    public static Role Oracle => SectsAndVioletsTownsfolk("Oracle", "Each night*, you learn how many dead players are evil.")
        .OtherNight(59, "Give the Oracle the number players that are evil.");

    public static Role Savant => SectsAndVioletsTownsfolk("Savant", "Each day, you may visit the Storyteller to learn 2 things in private: 1 is true & 1 is false.");

    public static Role Seamstress => SectsAndVioletsTownsfolk("Seamstress", "Once per game, at night, choose 2 players (not yourself): you learn if they are the same alignment.")
        .EachNight(43, 60, "Ask the seamstress if they would like to use their ability. If yes, they choose two players and learn if they are the same alignment.")
        .WithReminders("No Ability");

    public static Role Philosopher => SectsAndVioletsTownsfolk("Philosopher", "Once per game, at night, choose a good character: gain that ability. If this character is in play, they become drunk.")
        .EachNight(2, 2, "As the Philosopher if they would like to use their ability. If yes, they pick a good character and gain that ability (they are still the philosopher). If the selected role is already in play, the existing player becomes drunk.")
        .WithReminders("Is the Philosopher", "Drunk");

    public static Role Artist => SectsAndVioletsTownsfolk("Artist", "Once per game, during the day, privately ask the Storyteller any yes/no question.")
        .WithReminders("No Ability");

    public static Role Juggler => SectsAndVioletsTownsfolk("Juggler", "On your 1st day, publicly guess up to 5 players' characters. That night, you learn how many you got correct.")
        .OtherNight(61, "If today was the jugglers first day: Give them the number of correct guesses they made.")
        .WithReminders("Correct");

    public static Role Sage => SectsAndVioletsTownsfolk("Sage", "If the Demon kills you, you learn 2 players, one of which is the Demon.")
        .OtherNight(42, "If the sage was killed by a Demon: Give them 2 players, 1 of which is the Demon.");
}