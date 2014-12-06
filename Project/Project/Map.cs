﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonCrawler;

namespace DungeonCrawler
{
    public class Map
    {   ///All Map and player variables
        Program prog = new Program();
        GameOver end = new GameOver();
        Player hero = new Player();
        Random rand = new Random();
        #region David's code
        ConsoleKeyInfo input;
        static string[,] mask, events, unknown;
        static int pRow = 0, pCol = 0, _pRow, _pCol, numFount = 5;
        static bool onLvl_1 = true, onLvl_2 = false, onLvl_3 = false, fleed = false, gameOver = false;
        static string player = " YOU ", unexplored = " [ ] ", explored = "     ", bound = "▓▓▓▓▓",
                            enemy = " {E} ", onEnemy = "{ E }", enemyDef = "-{E}-", onEnemyDef = "{-E-}",
                            boss = " {B} ", onBoss = "{ B }", bossDef = "-{B}-", onBossDef = "{-B-}",
                            trapdoor = " {T} ", onTrap = "{ T }", trapDef = "-{T}-", onTrapDef = "{-T-}",
                            fountain = " {F} ", onFount = "{ F }", fountUsed = "-{F}-", onFountUsed = "{-F-}",
                            door = " {D} ", onDoor = "{ D }", undiscovered = " ??? ";
        public Map()
        {   ///Creates new instances of Map arrays and sets player position, weapon and health
            mask = new string[17, 11];      ///"masks" the contents of the level until explored by the player
            events = new string[17, 11];    ///the contents of the level
            unknown = new string[17, 11];   ///"masks" any chests, doors and trapdoors until explored by the player
            generate();
            if (onLvl_1) setLvl(1);  ///decides what level to create based on player progress 
            else if (onLvl_2) setLvl(2);
            else if (onLvl_3) setLvl(3);
            reveal(pRow, pCol);             ///reveals the map around the player in a 1-block radius
            weaponSelect();
            InfoPane();                     ///sets info pane to display player information
            refresh();                      ///displays arrays to reflect the player's current progress
        }
        public void setPlayer(int r, int c)
        {   ///Sets player coordinates and displays it on the Mask array
            pRow = r; pCol = c; mask[r, c] = player;
            prog.ClearTextbox();                        ///clears textbox area
        }
        static void generate()
        {   ///Fills null arrays with "blank" spaces
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    mask[i, j] = unexplored;
                    events[i, j] = unexplored;
                    unknown[i, j] = unexplored;
                }
            }
        }
        public void setLvl(int l)
        {   ///Updates player progress when switching levels
            switch (l)
            {
                case 1: onLvl_1 = true; onLvl_2 = false; onLvl_3 = false; lvl_1(); break;
                case 2: onLvl_1 = false; onLvl_2 = true; onLvl_3 = false; lvl_2(); break;
                case 3: onLvl_3 = false; onLvl_2 = false; onLvl_3 = true; lvl_3(); break;
            } refresh();
        }
        public void movement()
        {   ///Reads user input for player movement
            _pRow = pRow; _pCol = pCol;                                     ///saves previous player coordinates
            mask[pRow, pCol] = explored;                                    ///sets players previous coordinates as "explored" in Mask array
            reveal(pRow, pCol);                                             ///reveals Events content on the Mask array in a one block radius of the player
            input = Console.ReadKey();
            switch (input.Key)
            {
                case ConsoleKey.Escape: Environment.Exit(1); break;         ///quits the game upon pressing Escape (testing purposes)
                case ConsoleKey.UpArrow:                                    ///sets player location one block upwards
                    try
                    {
                        pRow -= 1;
                        if (checkBound(pRow, pCol))                     ///if the player hits a "wall," set the player one space in the opposite direction
                        { pRow += 1; setPlayer(pRow, pCol); }
                        else { setPlayer(pRow, pCol); }
                    }             ///set the player in its new position if within bounds
                    catch { pRow += 1; setPlayer(pRow, pCol); } break;      ///in case of IndexOutOfBounds exception, set the player one space in the opposite direction
                case ConsoleKey.LeftArrow:                                  ///sets player location one block to the left
                    try
                    {
                        pCol -= 1;
                        if (checkBound(pRow, pCol))
                        { pCol += 1; setPlayer(pRow, pCol); }
                        else { setPlayer(pRow, pCol); }
                    }
                    catch { pCol += 1; setPlayer(pRow, pCol); } break;
                case ConsoleKey.RightArrow:                                 ///sets player location one block to the right
                    try
                    {
                        pCol += 1;
                        if (checkBound(pRow, pCol))
                        { pCol -= 1; setPlayer(pRow, pCol); }
                        else { setPlayer(pRow, pCol); }
                    }
                    catch { pCol -= 1; mask[pRow, pCol] = player; } break;
                case ConsoleKey.DownArrow:                                  ///sets player location one block downwards
                    try
                    {
                        pRow += 1;
                        if (checkBound(pRow, pCol))
                        { pRow -= 1; setPlayer(pRow, pCol); }
                        else { setPlayer(pRow, pCol); }
                    }
                    catch { pRow -= 1; mask[pRow, pCol] = player; } break;
                default: setPlayer(pRow, pCol); break;
            } reveal(pRow, pCol);
            onEvent(pRow, pCol);                                            ///checks if the player matches coordinates with Events content
        }
        static bool checkBound(int r, int c)
        {   ///returns true if the player is on the same coordinates as the "boundary"
            if (mask[r, c].Equals(bound))
            { return true; }
            else { return false; }
        }
        static void postEvent(int r, int c)
        {   ///keeps all discovered Events content on the Mask array
            try
            {
                switch (events[r, c])
                {
                    case "▓▓▓▓▓": mask[r, c] = bound; break;
                    case " [ ] ": mask[r, c] = explored; break;
                    case " {E} ": mask[r, c] = enemy; break;
                    case "-{E}-": mask[r, c] = enemyDef; break;
                    case " {B} ": mask[r, c] = boss; break;
                    case "-{B}-": mask[r, c] = bossDef; break;
                    case " {T} ": if (unknown[r, c].Equals(undiscovered))     ///displays trapdoors as ??? if shown via reveal() and not discovered by the player
                        { mask[r, c] = undiscovered; }
                        else { mask[r, c] = trapdoor; } break;
                    case "-{T}-": mask[r, c] = trapDef; break;
                    case " {F} ": if (unknown[r, c].Equals(undiscovered))     ///displays fountains as ??? if shown via reveal() and not discovered by the player
                        { mask[r, c] = undiscovered; }
                        else { mask[r, c] = fountain; } break;
                    case "-{F}-": mask[r, c] = fountUsed; break;
                    case " {D} ": if (unknown[r, c].Equals(undiscovered))     ///displays doors as ??? if shown via reveal() and not discovered by the player
                        { mask[r, c] = undiscovered; }
                        else { mask[r, c] = door; } break;
                }
            }
            catch { }
        }
        static void reveal(int r, int c)
        {   ///reveals all Event content in a one block radius of the player
            postEvent(r - 1, c);
            postEvent(r - 1, c + 1);
            postEvent(r, c + 1);
            postEvent(r + 1, c);
            postEvent(r + 1, c + 1);
            postEvent(r + 1, c - 1);
            postEvent(r, c - 1);
            postEvent(r - 1, c - 1);
        }
        public void onEvent(int r, int c)
        {   ///execute appropriate code to their respective Events
            try
            {
                switch (events[r, c])
                {
                    case " {E} ":
                        mask[r, c] = onEnemy;
                        refresh();
                        atEnemy(r, c);
                        break;
                    case "-{E}-":
                        mask[r, c] = onEnemyDef;
                        refresh();
                        prog.WriteTextBox(" You look at your foe's corpse and laugh. \n \n Press the Arrow Keys to Move");
                        break;
                    case " {B} ":
                        mask[r, c] = onBoss;
                        refresh();
                        atBoss(r, c);
                        break;
                    case "-{B}-":
                        mask[r, c] = onBossDef;
                        refresh();
                        prog.WriteTextBox(" It was a tough battle, but then you compare injuries with the Boss and laugh. \n \n Press the Arrow Keys to Move");
                        break;
                    case " {T} ":
                        mask[r, c] = onTrap;
                        refresh();
                        int trapDmg = rand.Next(4, 11);
                        hero.HealthBehaviour.subHealth(trapDmg);
                        InfoPane();
                        prog.WriteTextBox(" As you hear a clicking sound, you immediately fall down a trapdoor and " + randomInjury() + "! (-" + trapDmg + " DMG) \n \n Press Any Key to Continue");
                        Console.ReadKey();
                        events[r, c] = trapDef; setPlayer(_pRow, _pCol);
                        postEvent(r, c);
                        break;
                    case "-{T}-":
                        mask[r, c] = onTrapDef;
                        prog.WriteTextBox(" The sight of the trapdoor makes you sweaty already. Mom's spaghetti. \n \n Press the Arrow Keys to Move");
                        break;
                    case " {F} ":
                        mask[r, c] = onFount;
                        refresh();
                        int fount = rand.Next(3, 8);
                        int randFount = rand.Next(1, 5);
                        numFount += randFount;
                        hero.HealthBehaviour.addHealth(fount);
                        InfoPane();
                        prog.WriteTextBox(" You take a sip of the fountain's water. \n \n The filtered taste is so good that it restores (" + fount + " HP), " +
                            "and so you use the remaining water to fill up " + randFount + " empty flask(s)! \n \n Press Any Key to Continue");
                        Console.ReadKey();
                        unknown[r, c] = events[r, c];
                        events[r, c] = fountUsed;
                        setPlayer(_pRow, _pCol);
                        postEvent(r, c);
                        break;
                    case "-{F}-":
                        mask[r, c] = onFountUsed;
                        refresh();
                        prog.WriteTextBox(" Looking at the empty fountain makes you thirsty. \n \n Press the Arrow Keys to Move");
                        break;
                    case " {D} ":
                        unknown[r, c] = events[r, c];
                        mask[r, c] = onDoor;
                        atDoor(r, c);
                        break;
                } refresh();
            }
            catch { }
        }
        public void atEnemy(int r, int c)
        {   ///Event triggered when the player encounters an enemy
            triggerFight(getEnemy(), r, c);
        }
        public void atBoss(int r, int c)
        {   ///Event triggered when the player encounters the boss
            triggerFight(getBoss(), r, c);
        }
        public void atDoor(int r, int c)
        {   ///Event triggered when the player discovers a door
            refresh();
            prog.WriteTextBox(" You discover a large metal door that you hope leads to the next floor. Will you open it? \n \n [1] Yes \n [2] No");
            input = Console.ReadKey();
            switch (input.Key)
            {
                case ConsoleKey.D1:
                    if (onLvl_1)
                    {
                        if (checkBoss(16, 8))
                        {
                            setLvl(2);
                            prog.WriteTextBox(" You continue deeper into the dungeon, complaining at their lack of elevators. \n \n" +
                                " The staircase takes you to an even dirtier dungeon floor, ripe with the smell of mold, rotting flesh and a Troll's gym bag. \n \n" +
                                " As you're about to gag, you notice an enemy with its back turned... \n \n Press the Arrow Keys to Move");
                        }
                        else
                        {
                            prog.WriteTextBox(" For some strange reason, you must defeat the Boss to unlock the door. \n \n Press Any Key to Continue");
                            Console.ReadKey();
                            setPlayer(_pRow, _pCol);
                        }
                    }
                    else if (onLvl_2)
                    {
                        if (checkBoss(3, 1))
                        {
                            setLvl(3);
                            prog.WriteTextBox(" You finally reach the Final Boss' lair, which is actually kind of cozy compared to the rest of the dungeon. \n \n" + 
                                " With the couches and water coolers lining the hall, you deduce this floor serves as a Lounge for the Dungeon's Bosses and henchmen. \n \n" +
                                " Before you decide to take a break yourself, however, you hear the Boss taunting you further down the hall... \n \n Press the Arrow Keys to Move");
                        }
                        else
                        {
                            prog.WriteTextBox(" For some strange reason, you must defeat the Boss to unlock the door. \n \n Press Any Key to Continue");
                            Console.ReadKey();
                            setPlayer(_pRow, _pCol);
                        }
                    }
                    else if (onLvl_3)
                    {
                        GameOver end = new GameOver();
                        gameOver = true;
                        end.Victory();
                    }
                    break;
                case ConsoleKey.D2: case ConsoleKey.Escape:
                    prog.WriteTextBox(" Despite your curiosity, you decide to explore the dungeon floor like a woman in a shopping mall. \n \n Press the Arrow Keys to Move");
                    setPlayer(_pRow, _pCol); postEvent(r, c); break;
                default: atDoor(r, c); break;
            }
        }
        public bool checkBoss(int r, int c)
        {   ///returns true if the Boss has been defeated
            if (events[r, c].Equals(bossDef)) { return true; }
            else { return false; }
        }
        public string randomInjury()
        {   ///returns a random body part that is injured during the Trapdoor event... because why not?
            if (rand.Next(0, 8) == 0) { return "scrape your elbow"; }
            else if (rand.Next(0, 6) == 1) { return "bruise your shoulder"; }
            else if (rand.Next(0, 6) == 2) { return "hit your funnybone"; }
            else if (rand.Next(0, 6) == 3) { return "scrape your knee"; }
            else if (rand.Next(0, 6) == 4) { return "bruise your shin"; }
            else if (rand.Next(0, 6) == 5) { return "pull a muscle"; }
            else if (rand.Next(0, 7) == 6) { return "break a finger nail"; }
            else                           { return "cut your beautiful face"; }
        }
        static void put(int r, int c, string type)
        {   ///Adds specific Event types on a specific coordinate on the Events array
            events[r, c] = type;
            if (type.Equals(fountain) || type.Equals(trapdoor) || type.Equals(door))
                unknown[r, c] = undiscovered;   ///Chests, doors and trapdoors will also be recorded in the Unknown array
        }
        public void lvl_1()
        {   ///The stupidest bit of code ever
            put(0, 0, unexplored); put(0, 1, unexplored); put(0, 2, unexplored); put(0, 3, unexplored); put(0, 4, unexplored); put(0, 5, bound); put(0, 6, bound); put(0, 7, unexplored); put(0, 8, enemy); put(0, 9, unexplored); put(0, 10, bound);
            put(1, 0, bound); put(1, 1, unexplored); put(1, 2, unexplored); put(1, 3, bound); put(1, 4, unexplored); put(1, 5, unexplored); put(1, 6, bound); put(1, 7, unexplored); put(1, 8, bound); put(1, 9, bound); put(1, 10, bound);
            put(2, 0, bound); put(2, 1, unexplored); put(2, 2, unexplored); put(2, 3, bound); put(2, 4, bound); put(2, 5, unexplored); put(2, 6, unexplored); put(2, 7, enemy); put(2, 8, unexplored); put(2, 9, unexplored); put(2, 10, fountain);
            put(3, 0, bound); put(3, 1, unexplored); put(3, 2, unexplored); put(3, 3, bound); put(3, 4, fountain); put(3, 5, unexplored); put(3, 6, unexplored); put(3, 7, bound); put(3, 8, bound); put(3, 9, bound); put(3, 10, bound);
            put(4, 0, bound); put(4, 1, bound); put(4, 2, enemy); put(4, 3, bound); put(4, 4, bound); put(4, 5, unexplored); put(4, 6, unexplored); put(4, 7, bound); put(4, 8, bound); put(4, 9, unexplored); put(4, 10, bound);
            put(5, 0, trapdoor); put(5, 1, bound); put(5, 2, unexplored); put(5, 3, bound); put(5, 4, unexplored); put(5, 5, unexplored); put(5, 6, unexplored); put(5, 7, bound); put(5, 8, unexplored); put(5, 9, unexplored); put(5, 10, bound);
            put(6, 0, unexplored); put(6, 1, bound); put(6, 2, unexplored); put(6, 3, unexplored); put(6, 4, unexplored); put(6, 5, bound); put(6, 6, bound); put(6, 7, bound); put(0, 8, unexplored); put(6, 9, unexplored); put(6, 10, fountain);
            put(7, 0, enemy); put(7, 1, unexplored); put(7, 2, unexplored); put(7, 3, bound); put(7, 4, unexplored); put(7, 5, bound); put(7, 6, fountain); put(7, 7, unexplored); put(7, 8, enemy); put(7, 9, bound); put(7, 10, bound);
            put(8, 0, unexplored); put(8, 1, bound); put(8, 2, unexplored); put(8, 3, bound); put(8, 4, unexplored); put(8, 5, bound); put(8, 6, bound); put(8, 7, bound); put(8, 8, unexplored); put(8, 9, bound); put(8, 10, bound);
            put(9, 0, fountain); put(9, 1, bound); put(9, 2, unexplored); put(9, 3, bound); put(9, 4, unexplored); put(9, 5, unexplored); put(9, 6, unexplored); put(9, 7, unexplored); put(9, 8, unexplored); put(9, 9, unexplored); put(9, 10, unexplored);
            put(10, 0, bound); put(10, 1, bound); put(10, 2, unexplored); put(10, 3, bound); put(10, 4, bound); put(10, 5, unexplored); put(10, 6, bound); put(10, 7, bound); put(10, 8, bound); put(10, 9, bound); put(10, 10, trapdoor);
            put(11, 0, unexplored); put(11, 1, unexplored); put(11, 2, unexplored); put(11, 3, bound); put(11, 4, bound); put(11, 5, unexplored); put(11, 6, bound); put(11, 7, fountain); put(11, 8, bound); put(11, 9, bound); put(11, 10, bound);
            put(12, 0, unexplored); put(12, 1, bound); put(12, 2, unexplored); put(12, 3, unexplored); put(12, 4, unexplored); put(12, 5, enemy); put(12, 6, unexplored); put(12, 7, unexplored); put(12, 8, unexplored); put(12, 9, enemy); put(12, 10, unexplored);
            put(13, 0, unexplored); put(13, 1, bound); put(13, 2, bound); put(13, 3, bound); put(13, 4, bound); put(13, 5, bound); put(13, 6, unexplored); put(13, 7, unexplored); put(13, 8, unexplored); put(13, 9, bound); put(13, 10, unexplored);
            put(14, 0, enemy); put(14, 1, unexplored); put(14, 2, unexplored); put(14, 3, unexplored); put(14, 4, bound); put(14, 5, bound); put(14, 6, unexplored); put(14, 7, unexplored); put(14, 8, unexplored); put(14, 9, bound); put(14, 10, unexplored);
            put(15, 0, unexplored); put(15, 1, bound); put(15, 2, unexplored); put(15, 3, unexplored); put(15, 4, trapdoor); put(15, 5, bound); put(15, 6, bound); put(15, 7, bound); put(15, 8, bound); put(15, 9, bound); put(15, 10, unexplored);
            put(16, 0, fountain); put(16, 1, bound); put(16, 2, unexplored); put(16, 3, enemy); put(16, 4, bound); put(16, 5, bound); put(16, 6, door); put(16, 7, unexplored); put(16, 8, boss); put(16, 9, unexplored); put(16, 10, unexplored);
            setPlayer(0, 0); reveal(pRow, pCol); refresh();
        }
        public void lvl_2()
        {
            generate();
            put(0, 0, fountain); put(0, 1, unexplored); put(0, 2, bound); put(0, 3, unexplored); put(0, 4, fountain); put(0, 5, bound); put(0, 6, fountain); put(0, 7, unexplored); put(0, 8, bound); put(0, 9, unexplored); put(0, 10, trapdoor);
            put(1, 0, unexplored); put(1, 1, unexplored); put(1, 2, bound); put(1, 3, unexplored); put(1, 4, bound); put(1, 5, bound); put(1, 6, bound); put(1, 7, trapdoor); put(1, 8, bound); put(1, 9, unexplored); put(1, 10, unexplored);
            put(2, 0, bound); put(2, 1, unexplored); put(2, 2, bound); put(2, 3, unexplored); put(2, 4, bound); put(2, 5, door); put(2, 6, bound); put(2, 7, unexplored); put(2, 8, bound); put(2, 9, unexplored); put(2, 10, bound);
            put(3, 0, bound); put(3, 1, boss); put(3, 2, unexplored); put(3, 3, unexplored); put(3, 4, bound); put(3, 5, unexplored); put(3, 6, bound); put(3, 7, unexplored); put(3, 8, unexplored); put(3, 9, enemy); put(3, 10, bound);
            put(4, 0, bound); put(4, 1, bound); put(4, 2, bound); put(4, 3, unexplored); put(4, 4, bound); put(4, 5, unexplored); put(4, 6, bound); put(4, 7, unexplored); put(4, 8, bound); put(4, 9, bound); put(4, 10, bound);
            put(5, 0, trapdoor); put(5, 1, unexplored); put(5, 2, bound); put(5, 3, enemy); put(5, 4, unexplored); put(5, 5, unexplored); put(5, 6, unexplored); put(5, 7, enemy); put(5, 8, bound); put(5, 9, unexplored); put(5, 10, fountain);
            put(6, 0, bound); put(6, 1, unexplored); put(6, 2, bound); put(6, 3, bound); put(6, 4, bound); put(6, 5, unexplored); put(6, 6, bound); put(6, 7, bound); put(6, 8, bound); put(6, 9, unexplored); put(6, 10, bound);
            put(7, 0, trapdoor); put(7, 1, unexplored); put(7, 2, unexplored); put(7, 3, bound); put(7, 4, unexplored); put(7, 5, unexplored); put(7, 6, unexplored); put(7, 7, bound); put(7, 8, unexplored); put(7, 9, unexplored); put(7, 10, trapdoor);
            put(8, 0, bound); put(8, 1, bound); put(8, 2, enemy); put(8, 3, unexplored); put(8, 4, unexplored); put(8, 5, fountain); put(8, 6, unexplored); put(8, 7, unexplored); put(8, 8, enemy); put(8, 9, bound); put(8, 10, bound);
            put(9, 0, fountain); put(9, 1, unexplored); put(9, 2, unexplored); put(9, 3, bound); put(9, 4, unexplored); put(9, 5, unexplored); put(9, 6, unexplored); put(9, 7, bound); put(9, 8, unexplored); put(9, 9, unexplored); put(9, 10, trapdoor);
            put(10, 0, bound); put(10, 1, unexplored); put(10, 2, bound); put(10, 3, bound); put(10, 4, bound); put(10, 5, unexplored); put(10, 6, bound); put(10, 7, bound); put(10, 8, bound); put(10, 9, unexplored); put(10, 10, bound);
            put(11, 0, trapdoor); put(11, 1, unexplored); put(11, 2, bound); put(11, 3, enemy); put(11, 4, unexplored); put(11, 5, unexplored); put(11, 6, unexplored); put(11, 7, enemy); put(11, 8, bound); put(11, 9, unexplored); put(11, 10, trapdoor);
            put(12, 0, bound); put(12, 1, bound); put(12, 2, bound); put(12, 3, unexplored); put(12, 4, bound); put(12, 5, enemy); put(12, 6, bound); put(12, 7, unexplored); put(12, 8, bound); put(12, 9, bound); put(12, 10, bound);
            put(13, 0, bound); put(13, 1, enemy); put(13, 2, unexplored); put(13, 3, unexplored); put(13, 4, bound); put(13, 5, unexplored); put(13, 6, bound); put(13, 7, unexplored); put(13, 8, unexplored); put(13, 9, enemy); put(13, 10, bound);
            put(14, 0, bound); put(14, 1, unexplored); put(14, 2, bound); put(14, 3, unexplored); put(14, 4, bound); put(14, 5, fountain); put(14, 6, bound); put(14, 7, unexplored); put(14, 8, bound); put(14, 9, unexplored); put(14, 10, bound);
            put(15, 0, unexplored); put(15, 1, unexplored); put(15, 2, bound); put(15, 3, fountain); put(15, 4, bound); put(15, 5, bound); put(15, 6, bound); put(15, 7, unexplored); put(15, 8, bound); put(15, 9, unexplored); put(15, 10, unexplored);
            put(16, 0, fountain); put(16, 1, unexplored); put(16, 2, bound); put(16, 3, unexplored); put(16, 4, trapdoor); put(16, 5, bound); put(16, 6, unexplored); put(16, 7, unexplored); put(16, 8, bound); put(16, 9, unexplored); put(16, 10, fountain);
            setPlayer(16, 6); reveal(pRow, pCol); refresh();
        }
        public void lvl_3()
        {
            generate();
            put(0, 0, bound); put(0, 1, bound); put(0, 2, bound); put(0, 3, bound); put(0, 4, bound); put(0, 5, bound); put(0, 6, bound); put(0, 7, bound); put(0, 8, bound); put(0, 9, bound); put(0, 10, bound);
            put(1, 0, bound); put(1, 1, bound); put(1, 2, bound); put(1, 3, bound); put(1, 4, bound); put(1, 5, fountain); put(1, 6, bound); put(1, 7, bound); put(1, 8, bound); put(1, 9, bound); put(1, 10, bound);
            put(2, 0, bound); put(2, 1, bound); put(2, 2, bound); put(2, 3, bound); put(2, 4, unexplored); put(2, 5, unexplored); put(2, 6, unexplored); put(2, 7, bound); put(2, 8, bound); put(2, 9, bound); put(2, 10, bound);
            put(3, 0, bound); put(3, 1, bound); put(3, 2, bound); put(3, 3, bound); put(3, 4, unexplored); put(3, 5, unexplored); put(3, 6, unexplored); put(3, 7, bound); put(3, 8, bound); put(3, 9, bound); put(3, 10, bound);
            put(4, 0, bound); put(4, 1, bound); put(4, 2, bound); put(4, 3, bound); put(4, 4, bound); put(4, 5, unexplored); put(4, 6, bound); put(4, 7, bound); put(4, 8, bound); put(4, 9, bound); put(4, 10, bound);
            put(5, 0, bound); put(5, 1, bound); put(5, 2, bound); put(5, 3, fountain); put(5, 4, bound); put(5, 5, unexplored); put(5, 6, bound); put(5, 7, enemy); put(5, 8, bound); put(5, 9, bound); put(5, 10, bound);
            put(6, 0, bound); put(6, 1, bound); put(6, 2, bound); put(6, 3, unexplored); put(6, 4, bound); put(6, 5, unexplored); put(6, 6, bound); put(6, 7, unexplored); put(6, 8, bound); put(6, 9, bound); put(6, 10, bound);
            put(7, 0, bound); put(7, 1, bound); put(7, 2, bound); put(7, 3, unexplored); put(7, 4, bound); put(7, 5, unexplored); put(7, 6, bound); put(7, 7, unexplored); put(7, 8, bound); put(7, 9, bound); put(7, 10, bound);
            put(8, 0, bound); put(8, 1, bound); put(8, 2, bound); put(8, 3, unexplored); put(8, 4, enemy); put(8, 5, unexplored); put(8, 6, enemy); put(8, 7, unexplored); put(8, 8, bound); put(8, 9, bound); put(8, 10, bound);
            put(9, 0, bound); put(9, 1, bound); put(9, 2, bound); put(9, 3, trapdoor); put(9, 4, bound); put(9, 5, unexplored); put(9, 6, bound); put(9, 7, trapdoor); put(9, 8, bound); put(9, 9, bound); put(9, 10, bound);
            put(10, 0, bound); put(10, 1, bound); put(10, 2, bound); put(10, 3, unexplored); put(10, 4, bound); put(10, 5, unexplored); put(10, 6, bound); put(10, 7, unexplored); put(10, 8, bound); put(10, 9, bound); put(10, 10, bound);
            put(11, 0, bound); put(11, 1, bound); put(11, 2, bound); put(11, 3, fountain); put(11, 4, bound); put(11, 5, unexplored); put(11, 6, bound); put(11, 7, fountain); put(11, 8, bound); put(11, 9, bound); put(11, 10, bound);
            put(12, 0, bound); put(12, 1, bound); put(12, 2, bound); put(12, 3, bound); put(12, 4, bound); put(12, 5, unexplored); put(12, 6, bound); put(12, 7, bound); put(12, 8, bound); put(12, 9, bound); put(12, 10, bound);
            put(13, 0, bound); put(13, 1, bound); put(13, 2, bound); put(13, 3, bound); put(13, 4, unexplored); put(13, 5, boss); put(13, 6, unexplored); put(13, 7, bound); put(13, 8, bound); put(13, 9, bound); put(13, 10, bound);
            put(14, 0, bound); put(14, 1, bound); put(14, 2, bound); put(14, 3, bound); put(14, 4, unexplored); put(14, 5, unexplored); put(14, 6, unexplored); put(14, 7, bound); put(14, 8, bound); put(14, 9, bound); put(14, 10, bound);
            put(15, 0, bound); put(15, 1, bound); put(15, 2, bound); put(15, 3, bound); put(15, 4, bound); put(15, 5, door); put(15, 6, bound); put(15, 7, bound); put(15, 8, bound); put(15, 9, bound); put(15, 10, bound);
            put(16, 0, bound); put(16, 1, bound); put(16, 2, bound); put(16, 3, bound); put(16, 4, bound); put(16, 5, bound); put(16, 6, bound); put(16, 7, bound); put(16, 8, bound); put(16, 9, bound); put(16, 10, bound);
            setPlayer(4, 5); reveal(pRow, pCol); refresh();
        }
        #endregion
        #region Nolan's code
        public Monsters getEnemy()
        {     ///is called when going over an enemy on the map and randomly picked one of 3 monsters to fight.
            int randMonster = rand.Next(0, 9);
            if (randMonster == 0)
            {
                Ogre enemy1 = new Ogre();
                return enemy1;
            }
            else if (randMonster == 1)
            {
                Troll enemy2 = new Troll();
                return enemy2;
            }
            else if (randMonster == 2)
            {
                Spirit enemy3 = new Spirit();
                return enemy3;
            }
            else if (randMonster == 3)
            {
                Mercenary enemy4 = new Mercenary();
                return enemy4;
            }
            else if (randMonster == 4)
            {
                Wraith enemy5 = new Wraith();
                return enemy5;
            }
            else if (randMonster == 5)
            {
                Skeleton enemy6 = new Skeleton();
                return enemy6;
            }
            else if (randMonster == 6)
            {
                Bandit enemy7 = new Bandit();
                return enemy7;
            }
            else if (randMonster == 7)
            {
                Goblin enemy8 = new Goblin();
                return enemy8;
            }
            else
            {
                Zombie enemy9 = new Zombie();
                return enemy9;
            }
        }
        public Monsters getBoss()
        {   //is triggered when standing on a boss on the map
            if (rand.Next(0, 5) == 0)
            {
                Swamphag enemyBoss = new Swamphag();
                return enemyBoss;
            }
            else if (rand.Next(0, 5) == 1)
            {
                Demon enemyBoss = new Demon();
                return enemyBoss;
            }
            else if (rand.Next(0, 5) == 2)
            {
                SpiderQueen enemyBoss = new SpiderQueen();
                return enemyBoss;
            }
            else if (rand.Next(0, 5) == 3)
            {
                BloodOgre enemyBoss = new BloodOgre();
                return enemyBoss;
            }
            else
            {
                BlackKnight enemyBoss = new BlackKnight();
                return enemyBoss;
            }
        }
#endregion
        #region David and Nolan's code
        public void triggerFight(Monsters monster, int r, int c)
        {
            prog.WriteTextBox(" You have encountered a " + monster.HealthBehaviour.getName() + " armed with a " + monster.WeaponBehaviour.getName() + ". Will you fight? \n \n [1] Yes \n [2] No");
            MonsterPane(monster);
            input = Console.ReadKey();
            switch (input.Key)
            {                                       ///Determines actions based of of the fight trigger
                case ConsoleKey.D1:
                    fleed = false;
                    while (monster.HealthBehaviour.getHealth() > 0 && hero.HealthBehaviour.getHealth() > 0)
                    {
                        InfoPane();                 ///refreshes the infopane with updated values
                        MonsterPane(monster);       ///refreshes the monsterpane with updated values
                        
                        fightMenu(monster, r, c);   ///initiates the fight.
                        if (fleed)
                        {
                            break;                  ///If the flee option is chosen ends the combat loop.
                        }
                    }
                    if (fleed)
                    {
                        setPlayer(_pRow, _pCol); postEvent(r, c); refresh();    ///resets the character position after fleeing.
                        InfoPane();
                        break;
                    }
                    if (monster.HealthBehaviour.getHealth() < 1)                ///checks to see if the monster was slain when the loop parameters werent met
                    {
                        if (events[r, c].Equals(enemy))
                        {
                            events[r, c] = enemyDef;                            ///updates the maps display value
                        }
                        else if (events[r, c].Equals(boss))
                        {
                            events[r, c] = bossDef;                             ///updates the maps display value
                        }
                        MonsterPane(monster);                                   ///updates the monsterpane with correct values (without it monster death will not show monster health at or below 0)
                        prog.WriteTextBox(" You have defeated the " + monster.HealthBehaviour.getName() + "! \n \n Press Any Key to Continue");
                        Console.ReadKey();
                        setPlayer(_pRow, _pCol); postEvent(r, c); refresh();
                    }
                    InfoPane();
                    break;
                case ConsoleKey.D2: case ConsoleKey.Escape:      ///resets character position and leaves the monster able to be fought.
                    prog.WriteTextBox(" You find it in your heart to prolonge the " + monster.HealthBehaviour.getName() + "'s demise... \n \n Press Any Key to Continue");
                    Console.ReadKey();
                    setPlayer(_pRow, _pCol); postEvent(r, c); refresh();
                    InfoPane();
                    break;
                default: triggerFight(monster, r, c); break;
            }
        }
        public void fightMenu(Monsters monster, int r, int c)
        {
            prog.WriteTextBox(" What will you do? \n \n [1] Use Weapon \n [2] Defend \n [3] Use HP Potion \n [4] Flee");
            input = Console.ReadKey();
            switch (input.Key)
            {                                       ///Takes values of inputs for what to apply for the combat, applies the action
                case ConsoleKey.D1:
                    ///Hero gives damage to monster, hero recieves damage from monster.
                    int damage = hero.WeaponBehaviour.useWeapon();
                    int monsterDamage = monster.WeaponBehaviour.useWeapon();
                    monster.HealthBehaviour.subHealth(damage);
                    MonsterPane(monster);
                    prog.WriteTextBox(hero.WeaponBehaviour.heroMessage(damage, damage));
                    System.Threading.Thread.Sleep(1250);
                    if (monster.HealthBehaviour.getHealth() < 1)
                    {
                        break;
                    }
                    else
                    {
                        hero.HealthBehaviour.subHealth(monsterDamage);
                        InfoPane(); MonsterPane(monster);
                        prog.WriteTextBox(monster.WeaponBehaviour.monsterMessage(monster.HealthBehaviour.getName(), monsterDamage, monsterDamage));
                        Console.ReadKey();
                    }
                    break;
                case ConsoleKey.D2:         
                    int blockedDamage;              ///Hero blocks 50% of the monsters damage.
                    int blockedAmount;  
                    prog.WriteTextBox(" You brace yourself against the " + monster.HealthBehaviour.getName() + "'s attack!");
                    System.Threading.Thread.Sleep(1250);
                    blockedAmount = monster.WeaponBehaviour.damage() / 2;
                    blockedDamage = monster.WeaponBehaviour.damage() - (blockedAmount);
                    hero.HealthBehaviour.subHealth(blockedDamage);
                    InfoPane(); MonsterPane(monster);
                    prog.WriteTextBox(" You soften the " + monster.HealthBehaviour.getName() + "'s attack! (-" + blockedDamage +" DMG)");
                    Console.ReadKey();
                    break;
                case ConsoleKey.D3:                 ///Hero restores some HP, hero recieves damage from monster.
                    if (numFount != 0)
                    {
                        int monsterDamage2 = monster.WeaponBehaviour.useWeapon();
                        numFount--;
                        prog.WriteTextBox(" You drink a HP potion, restoring (+15 HP)!");
                        hero.HealthBehaviour.addHealth(15);
                        InfoPane(); MonsterPane(monster);
                        System.Threading.Thread.Sleep(1250);
                        hero.HealthBehaviour.subHealth(monsterDamage2);
                        InfoPane(); MonsterPane(monster);
                        prog.WriteTextBox(monster.WeaponBehaviour.monsterMessage(monster.HealthBehaviour.getName(), monsterDamage2, monsterDamage2));
                        Console.ReadKey();
                    }
                    else
                    {
                        prog.WriteTextBox(" You do not have any potions left! \n \n Press Any Key to Continue");
                        Console.ReadKey();
                    }
                    break;  
                case ConsoleKey.D4: case ConsoleKey.Escape:               ///hero attempts to flee, if flee = false, cannot flee, restarts choice.
                    if (monster.HealthBehaviour.getFlee())
                    {
                        fleed = true;
                        prog.WriteTextBox(" You flee from the " + monster.HealthBehaviour.getName() + "! \n \n Press Any Key to Continue");
                        Console.ReadKey();
                    }
                    else
                    {
                        prog.WriteTextBox(" You cannot flee from the " + monster.HealthBehaviour.getName() + "! \n \n Press Any Key to Continue");
                        Console.ReadKey();
                    }
                    break;
                default:
                    fightMenu(monster, r, c);
                    break;
            }
        }
        
        #endregion
        #region Evan's code
        static void refresh()
        {   ///
            if (!gameOver)
            {
                Console.SetCursorPosition(0, 7);
                for (int i = 0; i < 40; i++)
                {
                    Console.WriteLine("");
                } Console.SetCursorPosition(0, 7);
                for (int i = 0; i < 17; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        Console.Write(mask[i, j]);
                    } Console.WriteLine(Environment.NewLine);
                } Console.SetCursorPosition(7, 59);
            }
            else { }
        }
        public void InfoPane()
        {   ///
            Console.SetCursorPosition(59, 7);
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(58, 7 + i);
                Console.WriteLine("                                 ");
            }
            ///Deathcheck
            if (hero.HealthBehaviour.getHealth() < 1)
            {
                GameOver end = new GameOver();
                gameOver = true;
                end.Died();
                //continueDialog();
            }
            Console.SetCursorPosition(59, 7);
            Console.WriteLine("Health: {0}", hero.HealthBehaviour.getHealth());
            Console.SetCursorPosition(59, 8);
            Console.WriteLine("HP Potions: {0}", numFount);
            Console.SetCursorPosition(59, 9);
            Console.WriteLine("Current Weapon: {0}", hero.WeaponBehaviour.getName().ToUpper());
            Console.SetCursorPosition(59, 10);
            Console.WriteLine("Current Damage: {0}", hero.WeaponBehaviour.damage());
        }
        public void MonsterPane(Monsters monster)               ///displays the information about the monsters.
        {
            prog.MonsterHealthBox("Health: " + monster.HealthBehaviour.getHealth());
            prog.MonsterNameBox("Enemy: " + monster.HealthBehaviour.getName());
            prog.MonsterWeaponBox("Weapon: " + monster.WeaponBehaviour.getName());
        }
        #endregion
        #region Jordan's code
        public void weaponSelect()
        {
            ///Select weapon 
            prog.WriteTextBox(" Welcome to Dungeon C#rawler! \n \n Please Select a Weapon: \n \n [1] Sword \t DMG = 5 \t ACC = 75% \n [2] Axe \t 8 DMG \n [3] Knife \t 2 DMG " + 
                " \n [4] Mace \t 4 DMG \n [5] Fish \t 3 DMG \n [6] Bowstaff \t 6 DMG \n [7] Caestus \t 1 DMG \n [8] Halberd \t 8 DMG");

            ///Takes key input and assigns to weapon
            input = Console.ReadKey();
            switch (input.Key)
            {
                case ConsoleKey.D1:
                    hero.WeaponBehaviour = new Sword();
                    break;
                case ConsoleKey.D2:
                    hero.WeaponBehaviour = new Axe();
                    break;
                case ConsoleKey.D3:
                    hero.WeaponBehaviour = new Knife();
                    break;
                case ConsoleKey.D4:
                    hero.WeaponBehaviour = new Mace();
                    break;
                case ConsoleKey.D5:
                    hero.WeaponBehaviour = new Fish();
                    break;
                case ConsoleKey.D6:
                    hero.WeaponBehaviour = new Bowstaff();
                    break;
                case ConsoleKey.D7:
                    hero.WeaponBehaviour = new Caestus();
                    break;
                case ConsoleKey.D8:
                    hero.WeaponBehaviour = new Quarterstaff();
                    break;
                    ///possible numpad support
                //case ConsoleKey.NumPad1:
                //    hero.WeaponBehaviour = new Sword();
                //    break;
                //case ConsoleKey.NumPad2:
                //    hero.WeaponBehaviour = new Axe();
                //    break;
                //case ConsoleKey.NumPad3:
                //    hero.WeaponBehaviour = new Knife();
                //    break;
                //case ConsoleKey.NumPad4:
                //    hero.WeaponBehaviour = new Mace();
                //    break;
                //case ConsoleKey.NumPad5:
                //    hero.WeaponBehaviour = new Fish();
                //    break;
                //case ConsoleKey.NumPad6:
                //    hero.WeaponBehaviour = new Bowstaff();
                //    break;
                //case ConsoleKey.NumPad7:
                //    hero.WeaponBehaviour = new Caestus();
                //    break;
                //case ConsoleKey.NumPad8:
                //    hero.WeaponBehaviour = new Quarterstaff();
                //    break;
                default:
                    weaponSelect();
                    break;
            }
            prog.WriteTextBox(" You have selected: " + hero.WeaponBehaviour.getName().ToUpper() + " \n \n Press the Arrow Keys to Move");
        }
        #endregion
    }
}