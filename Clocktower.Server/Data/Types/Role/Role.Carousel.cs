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

    #region Townsfolk

    public static Role Magician => CarouselTownsfolk("Magician", "The Demon thinks you are a Minion. Minions think yuo are a Demon.");
    public static Role Shugenja => CarouselTownsfolk("Shugenja", "You start knowing if your closest evil player is clockwise or anti-clockwise. If equidistant, this info is arbitrary.");

    public static Role Princess => CarouselTownsfolk("Princess", "On your 1st day, if you nominated & executed a player, the Demon doesn't kill tonight.")
        .WithReminders("Doesn't Kill");

    public static Role Preacher => CarouselTownsfolk("Preacher", "Each night, choose a player: a Minion, if chosen, learns this. All chosen Minions have no ability.")
        .EachNight(70, 13, "Ask the Preacher to choose a player. If that player is a Minion, wake them and tell them they have been Preached and mark them as No Ability")
        .WithReminders("No Ability");

    public static Role PoppyGrower => CarouselTownsfolk("Poppy Grower", "Minions & Demons do not know each other. If you die, they learn who each other are that night.")
        .EachNight(2, 30, "If the Poppy Grower has died, wake the Evil team and inform them of who their teammates are.")
        .WithReminders("Evil Wakes");

    public static Role Nightwatchman => CarouselTownsfolk("Nightwatchman", "Once per game, at night, choose a player: they learn you are the Nightwatchman.")
        .EachNight(45, 58, "If the Nightwatchman still has their ability, ask if they wish to use it. If they pick a player, tell that player who the Nightwatchman is. Mark as No Ability")
        .WithReminders("No Ability");

    public static Role Noble => CarouselTownsfolk("Noble", "You start knowing 3 players, 1 and only 1 of which is evil.")
        .FirstNight(48, "Show the Noble 3 players, 1 and only 1 of which is evil.")
        .WithReminders("Know");

    public static Role Pixie => CarouselTownsfolk("Pixie", "You start knowing 1 in-play Townsfolk. If you were mad that you were this character, you gain their ability when they die.")
        .FirstNight(50, "Show the Pixie 1 in-play Townsfolk character.")
        .WithReminders("Has Ability");

    public static Role Steward => CarouselTownsfolk("Steward", "You start knowing 1 good player.")
        .FirstNight(55, "Show the Steward 1 good player (not their character).")
        .WithReminders("Know");

    public static Role VillageIdiot => CarouselTownsfolk("Village Idiot", "Each night, choose a player: you learn their alignment. [+0 to +2 Village Idiots. 1 of the extras is drunk].")
        .EachNight(72, 70, "For each Village Idiot, wake in turn. They pick a player and get a good or evil. The drunk one gets arbitrary info.")
        .WithReminders("Drunk")
        .AffectsSetup();

    #endregion

    #region Outsiders

    public static Role Damsel => CarouselOutsider("Damsel", "All Minions know a Damsel is in play. If a Minion publicly guesses you (once), your team loses.")
        .FirstNight(5, "Inform the Minions that a Damsel is in play.")
        .WithReminders("Guess Used");

    public static Role Golem => CarouselOutsider("Golem", "You may only nominate once per game. When you do, if the nominee is not the Demon, they die")
        .WithReminders("May Not Nominate");

    public static Role Hatter => CarouselOutsider("Hatter", "If you died today or tonight, the Minion & Demon players may choose new Minion & Demon characters to be.")
        .OtherNight(28, "If the Hatter died today or tonight, allow the Minions and Demons to change their characters if they wish.")
        .WithReminders("Tea Party Tonight");

    public static Role Heretic => CarouselOutsider("Heretic", "Whoever wins, loses & whoever loses, wins, even if you are dead.");

    public static Role Hermit => CarouselOutsider("Hermit", "You have all Outsider abilities. [-0 or -1 Outsiders]")
        .WithReminders("1", "2", "3")
        .AffectsSetup();

    public static Role Ogre => CarouselOutsider("Ogre", "On your first night, choose a player (not yourself): you become their alignment (you don't know which) even if drunk or poisoned.")
        .FirstNight(29, "The Ogre chooses a player. The Ogre becomes the alignment of the selected player.")
        .WithReminders("Friend");

    public static Role PlagueDoctor => CarouselOutsider("Plague Doctor", "When you die, the Storyteller gains a Minion ability.")
        .WithReminders("Storyteller Ability");

    public static Role Politician => CarouselOutsider("Politician", "If you were the player most responsible for you team losing, you change alignment & win, even if dead.");

    public static Role PuzzleMaster => CarouselOutsider("Puzzle Master", "1 player is drunk, even if you die. If you guess (once) who it is, learn the Demon player, but guess wrong & get false info.")
        .WithReminders("Drunk", "Guess Used");

    public static Role Snitch => CarouselOutsider("Snitch", "Each Minion gets 3 bluffs.");

    public static Role Zealot => CarouselOutsider("Zealot", "If there are more 5 or more players alive, you must vote for every nomination.");

    #endregion

    #region Minions

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

    #endregion

    #region Demons

    public static Role LordOfTyphon => CarouselDemon("Lord Of Typhon", "Each night*, choose a player: they die. [Evil characters are in a line. You are in the middle. +1 Minion. -? to +? Outsiders.]")
        .OtherNight(24, "The Lord Of Typhon picks a player. That player dies.")
        .WithReminders("Dead")
        .AffectsSetup();

    public static Role Lleech => CarouselDemon("Lleech", "Each night*, choose a player: they die. You start by choosing a player: they are poisoned. You die if & only if they are dead.")
        .FirstNight(1, "The Lleech picks a player to be their host.")
        .OtherNight(24, "The Lleech picks a player. That player dies.")
        .WithReminders("Host & Poisoned", "Dead");

    public static Role Ojo => CarouselDemon("Ojo", "Each night*, choose a character, they die. If they are not in play, the Storyteller chooses who dies.")
        .OtherNight(24, "The Ojo picks a character. If that character is in play, they die. If not, the Storyteller chooses who dies.")
        .WithReminders("Dead");

    public static Role Leviathan => CarouselDemon("Leviathan", "If more than 1 good player is executed, evil wins. All players know you are in play. After day 5, evil wins.")
        .OtherNight(100, "Increment day counter")
        .WithReminders("Day 1", "Day 2", "Day 3", "Day 4", "Day 5", "Good Player Executed");

    public static Role Yaggababble => CarouselDemon("Yaggababble", "You start knowing a secret phrase. For each timr you said it publicly today, a player might die.")
        .FirstNight(1, "Wake the Yaggababble and show them their secret phrase. Put them to sleep.")
        .OtherNight(24, "Do not wake. Choose and mark players to die based on how many times the Yaggababble said their secret phrase today.")
        .WithReminders("Dead");

    public static Role Legion => CarouselDemon("Legion", "Each night*, a player might die. Executions fail if only evil voted. You register as a Minion too. [Most players are Legion].")
        .OtherNight(24, "The Storyteller may kill 1 player.")
        .WithReminders("Dead", "About To Die")
        .AffectsSetup();

    public static Role Riot => CarouselDemon("Riot", "On day 3, Minions become Riot & nominees die but nominate an alive player immediately. This must happen.")
        .OtherNight(100, "Increment day counter")
        .WithReminders("Day 1", "Day 2", "Day 3");

    public static Role Kazali => CarouselDemon("Kazali", "Each night*, choose a player: they die. [You choose which players are which Minions. -? to +? Outsiders")
        .FirstNight(1, "Allow the Kazali to pick which players are which Minions. Inform the new Minions of the change.")
        .OtherNight(24, "The Kazali picks a player. That player dies.")
        .WithReminders("Dead")
        .AffectsSetup();

    public static Role AlHadikhia => CarouselDemon("Al-Hadikhia", "Each night*, you may choose 3 players (all players learn who): each silently chooses to live or die, but if all live, all die.")
        .OtherNight(24, "Wake the Al-Hadikhia to choose 3 players; announce the group to be silent and name each chosen player, waking them individually to ask if they live—if all 3 choose life, all die, otherwise only those who chose death die.")
        .WithReminders("1", "2", "3");

    public static Role LilMonsta => CarouselDemon("Lil' Monsta", "Each night, Minions choose who babysits Lil' Monsta & \"is the Demon\". Each night *, a player might die. [+1 Minion, no player is \"Lil' Monsta\"]")
        .FirstNight(24, "Wake all Minions together. They select at a player to babysit Lil' Monsta.")
        .OtherNight(24, "Wake all Minions together. They select a player to babysit Lil' Monsta. A player might die.")
        .WithReminders("Dead", "Is The Demon")
        .AffectsSetup();

    #endregion
}