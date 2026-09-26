using Cairo;
using Gdk;
using Gtk;
using NAudio.Wave;
using Color = Cairo.Color;
using Key = Gdk.Key;
using Timeout = GLib.Timeout;
using System.Diagnostics;

// class for movement and collision of ghost and player sprites
class Sprite {

    public int x, y;
    public int speed;  // adjustable by power-up
    public bool collision;
    public bool isPhase;  // attribute triggered by power-up
    bool[] prevMov = new bool[4];  // stores the previous direction of the sprite as a bool
                                   // ranging from indices 0 to 3 (left, right, up, down)
    int direction;  // stores current direction as a number corresponding to the index in the prevMov array

    public Sprite(int x, int y) {
        this.x = x;
        this.y = y;
        speed = 4;
        isPhase = false;
    }

    public void tick(bool moveLeft, bool moveRight, bool moveUp, bool moveDown, bool collision) {
        this.collision = collision;  // collision parameter added to be controlled by other methods later

        if (x < -30) {
            x = 830;  // moving beyond the left edge of the screen moves the sprite to the right edge
        }
        if (x > 830) {
            x = -30; // moving beyond the right edge of the screen moves the sprite to the left edge
        }

        if (!collision || isPhase) { // can only move when there is no collision or when the
                                     // the phase power is activated disabling collision effect entirely
            for (int i = 0 ; i < prevMov.Length ; ++i) {
                prevMov[i] = false;  // initializes previous direction to false
            }

            // in each case:
            // change the x or y coordinate of sprite based on speed and direction
            // store the current direction as the previous direction
            // store the current direction corresponding to the direction index in the prevMov array
            if (moveLeft) {
                x -= speed;
                prevMov[0] = true;
                direction = 0;
            }
            else if (moveRight) {
                x += speed;
                prevMov[1] = true;
                direction = 1;
            }
            
            if (moveUp) {
                prevMov[2] = true;
                y -= speed;
                direction = 2;
            }
            else if (moveDown) {
                prevMov[3] = true;
                y += speed;
                direction = 3;
            }
        }

        // when collision does happen
        // the movement is stopped as it is a condition above
        // we go through the below 4 cases based on our previous direction
        // on each call of this method only one previous direction is set to true
        // the for loop detects this direction and pushes back against it in the opposite direction
        // until there is no more collision
        if (collision) {
            for (int i = 0 ; i < prevMov.Length ; ++i) {
                if (prevMov[i]) {
                    switch (i) {
                        case 0:
                            x += 1;
                            break;
                        case 1:
                            x -= 1;
                            break;
                        case 2:
                            y += 1;
                            break;
                        case 3:
                            y -= 1;
                            break;
                    }
                }
            }
        }
    }

    // gets the curent direction
    public int getDirection() {
        return direction;
    }
}

// class for individual ghost AI. initially intended to be different for each ghost
class ghostAi {
    Sprite ghost;
    Gdk.Rectangle ghostRect;
    Gdk.Rectangle badDirectionArea = new Gdk.Rectangle(390, 340, 45, 45); // area just above the box from which 
                                                                          // the ghosts are released
    bool[] movDirection = new bool[4];  // array determining current direction of ghost using indices
    int prevDirection;

    public ghostAi(Sprite ghost) {
        this.ghost = ghost;
        ghostRect = new Gdk.Rectangle(ghost.x, ghost.y, 40, 40);
    }

    // method to get new random direction after collision
    int getRandDirection() {
        Random rng = new Random();
        int newDirection = prevDirection;

        // randomize until the new direction is not the same as the old one
        while (newDirection == prevDirection) {
            newDirection = rng.Next(4);
        }
        return newDirection;
    }

    public int blinkyMovement(Area gameArea) {
        int newDirection = prevDirection;
        ghostRect.X = ghost.x;
        ghostRect.Y = ghost.y;

        // initial collision is always with the wall above the ghost area
        bool initialChange = ghostRect.IntersectsWith(badDirectionArea) ? true : false;

        // setting the original direction to upwards
        if (ghost.x == 385 && ghost.y == 350) {
            movDirection = new bool[4];
            movDirection[2] = true;
            prevDirection = 2;
        }

        if (gameArea.collision("blinky")) {
            newDirection = getRandDirection();
            // if the initial direction change is back into the box
            // keep getting a new direction until it is not back into the box
            while (initialChange && newDirection == 3) {
                newDirection = getRandDirection(); 
            }
            // only one direction must remain
            movDirection[prevDirection] = false;
            movDirection[newDirection] = true;
            prevDirection = newDirection;
        }

        ghost.tick(movDirection[0], movDirection[1], movDirection[2], movDirection[3], gameArea.collision("blinky"));
        return newDirection;
    }

    public int inkyMovement(Area gameArea, long time) {
        int newDirection = prevDirection;
        ghostRect.X = ghost.x;
        ghostRect.Y = ghost.y;

        bool initialChange = ghostRect.IntersectsWith(badDirectionArea) ? true : false;

        if (ghost.x == 385 && ghost.y == 420) {
            movDirection = new bool[4];
            movDirection[2] = true;
            prevDirection = 2;
        }

        if (gameArea.collision("inky")) {
            newDirection = getRandDirection();
            while (initialChange && newDirection == 3) {
                newDirection = getRandDirection(); 
            }
            movDirection[prevDirection] = false;
            movDirection[newDirection] = true;
            prevDirection = newDirection;
        }

        if (time >= 5000) ghost.tick(movDirection[0], movDirection[1], movDirection[2], movDirection[3], gameArea.collision("inky"));
        return newDirection;
    }

    public int clydeMovement(Area gameArea, long time) {
        int newDirection = prevDirection;
        ghostRect.X = ghost.x;
        ghostRect.Y = ghost.y;

        bool initialChange = ghostRect.IntersectsWith(badDirectionArea) ? true : false;

        if (ghost.x == 385 && ghost.y == 420) {
            movDirection = new bool[4];
            movDirection[2] = true;
            prevDirection = 2;
        }

        if (gameArea.collision("clyde")) {
            newDirection = getRandDirection();
            while (initialChange && newDirection == 3) {
                newDirection = getRandDirection(); 
            }
            movDirection[prevDirection] = false;
            movDirection[newDirection] = true;
            prevDirection = newDirection;
        }
        
        if (time > 14000) ghost.tick(movDirection[0], movDirection[1], movDirection[2], movDirection[3], gameArea.collision("clyde"));
        return newDirection;
    }

    public int pinkyMovement(Area gameArea) {
        int newDirection = prevDirection;
        ghostRect.X = ghost.x;
        ghostRect.Y = ghost.y;

        bool initialChange = ghostRect.IntersectsWith(badDirectionArea) ? true : false;

        if (ghost.x == 385 && ghost.y == 420) {
            movDirection = new bool[4];
            movDirection[2] = true;
            prevDirection = 2;
        }

        if (gameArea.collision("pinky")) {
            newDirection = getRandDirection();
            while (initialChange && newDirection == 3) {
                newDirection = getRandDirection(); 
            }
            movDirection[prevDirection] = false;
            movDirection[newDirection] = true;
            prevDirection = newDirection;
        }

        ghost.tick(movDirection[0], movDirection[1], movDirection[2], movDirection[3], gameArea.collision("pinky"));
        return newDirection;
    }
    
}

// class for power-up objects except the special devil juice power up
// handles power-up image, rectangle, collision, activation, time of activation
class powerUp {
    public ImageSurface? powerImg;
    public Gdk.Rectangle powerRect; 
    public int x;
    public int y;
    public long time;
    public bool powerActive;

    public powerUp(ImageSurface powerImg, int x, int y) {
        this.powerImg = powerImg;
        this.x = x;
        this.y = y;
        powerRect = new Gdk.Rectangle(x, y, 35, 35);
        time = 0;
    }

    public bool powerActivate(Gdk.Rectangle spriteRect, long time) {
        if (spriteRect.IntersectsWith(powerRect) && powerImg != null) {
            this.time = time;
            powerActive = true;
            return true;
        }
        return false;
    }
}

class Area : DrawingArea {
    // sound setup
    IWavePlayer? waveOut;
    AudioFileReader? audioFileReader;
    
    // sprites
    Sprite pacMan;
    Sprite blinky;
    Sprite inky;
    Sprite clyde;
    Sprite pinky;

    // sprite images
    ImageSurface pacManImgRight = new ImageSurface("graphics/sprites/pacman/pacManRight.png");
    ImageSurface pacManImgLeft = new ImageSurface("graphics/sprites/pacman/pacManLeft.png");
    ImageSurface pacManImgUp = new ImageSurface("graphics/sprites/pacman/pacManUp.png");
    ImageSurface pacManImgDown = new ImageSurface("graphics/sprites/pacman/pacManDown.png");

    ImageSurface blinkyRightImg = new ImageSurface("graphics/sprites/blinky/blinkyRight.png");
    ImageSurface blinkyLeftImg = new ImageSurface("graphics/sprites/blinky/blinkyLeft.png");
    ImageSurface blinkyUpImg = new ImageSurface("graphics/sprites/blinky/blinkyUp.png");
    ImageSurface blinkyDownImg = new ImageSurface("graphics/sprites/blinky/blinkyDown.png");

    ImageSurface inkyRightImg = new ImageSurface("graphics/sprites/inky/inkyRight.png");
    ImageSurface inkyLeftImg = new ImageSurface("graphics/sprites/inky/inkyLeft.png");
    ImageSurface inkyUpImg = new ImageSurface("graphics/sprites/inky/inkyUp.png");
    ImageSurface inkyDownImg = new ImageSurface("graphics/sprites/inky/inkyDown.png");
    
    ImageSurface clydeRightImg = new ImageSurface("graphics/sprites/clyde/clydeRight.png");
    ImageSurface clydeLeftImg = new ImageSurface("graphics/sprites/clyde/clydeLeft.png");
    ImageSurface clydeUpImg = new ImageSurface("graphics/sprites/clyde/clydeUp.png");
    ImageSurface clydeDownImg = new ImageSurface("graphics/sprites/clyde/clydeDown.png");

    ImageSurface pinkyRightImg = new ImageSurface("graphics/sprites/pinky/pinkyRight.png");
    ImageSurface pinkyLeftImg = new ImageSurface("graphics/sprites/pinky/pinkyLeft.png");
    ImageSurface pinkyUpImg = new ImageSurface("graphics/sprites/pinky/pinkyUp.png");
    ImageSurface pinkyDownImg = new ImageSurface("graphics/sprites/pinky/pinkyDown.png");

    ImageSurface ghostFearImg = new ImageSurface("graphics/sprites/ghostAfraid.png");

    // powerUp objects
    powerUp speedUp1 = new powerUp(new ImageSurface("graphics/powerUps/speedUp.png"), 100, 415);
    powerUp speedUp2 = new powerUp(new ImageSurface("graphics/powerUps/speedUp.png"), 680, 415);
    powerUp slowDown1 = new powerUp(new ImageSurface("graphics/powerUps/slowDown.png"), 255, 415);
    powerUp slowDown2 = new powerUp(new ImageSurface("graphics/powerUps/slowDown.png"), 515, 415);
    powerUp phase = new powerUp(new ImageSurface("graphics/powerUps/phase.png"), 385, 420);
    powerUp teleporter1 = new powerUp(new ImageSurface("graphics/powerUps/teleporter.png"), 380, 225);
    powerUp teleporter2 = new powerUp(new ImageSurface("graphics/powerUps/teleporter.png"), 380, 745);

    // game backgrounds
    ImageSurface gameStartImg = new ImageSurface("graphics/backgrounds/pacManTitle.png");
    ImageSurface gameWonImg = new ImageSurface("graphics/backgrounds/gameWon.png");
    ImageSurface gameOverImg = new ImageSurface("graphics/backgrounds/gameOver.png");

    // game rectangles
    Gdk.Rectangle pacManRect;
    Gdk.Rectangle inkyRect;
    Gdk.Rectangle blinkyRect;
    Gdk.Rectangle clydeRect;
    Gdk.Rectangle pinkyRect;

    // gameplay variables
    int lives;
    int pacManDirection;
    int[] ghostDirections;
    long time;
    int score;
    bool gameStarted;
    long wallActiveTime;
    long wallDisableTime;
    bool devilJuiceActive;
    long juiceActiveTime;
    bool gameWon;
    bool gameOver;

    Color black = new Color(0, 0, 0),  // background color
          blue = new Color(0, 0, 1),  // wall color
          white = new Color(1, 1, 1), // text color
          beige = new Color(0.851, 0.659, 0.561); // score circle color

    // lists for drawing the game
    // makes detecting collisions with the rectanges and circles much easier
    List<Gdk.Rectangle> walls = new List<Gdk.Rectangle> {
        // borders
        new Gdk.Rectangle(0, 780, 800, 10), // bottom wall
        new Gdk.Rectangle(0, 550, 10, 230), // left lower wall
        new Gdk.Rectangle(790, 550, 10, 230), // right lower wall
        new Gdk.Rectangle(0, 540, 180, 10), // left center wall 1
        new Gdk.Rectangle(620, 540, 180, 10), // right center wall 1
        new Gdk.Rectangle(170, 470, 10, 70), // left vertical centre wall 1
        new Gdk.Rectangle(620, 470, 10, 70), // right vertical centre wall 1
        new Gdk.Rectangle(0, 460, 180, 10), // left center wall 2
        new Gdk.Rectangle(620, 460, 180, 10), // right center wall 2
        new Gdk.Rectangle(0, 400, 180, 10), // left center wall 3
        new Gdk.Rectangle(620, 400, 180, 10), // right center wall 3
        new Gdk.Rectangle(170, 330, 10, 70),
        new Gdk.Rectangle(620, 330, 10, 70),
        new Gdk.Rectangle(0, 320, 180, 10), // left center wall 4
        new Gdk.Rectangle(620, 320, 180, 10),
        new Gdk.Rectangle(0, 140, 10, 180),
        new Gdk.Rectangle(790, 140, 10, 180),
        new Gdk.Rectangle(0, 130, 800, 10),

        // mid rectangle
        new Gdk.Rectangle(295, 470, 210, 10), // mid rect bottom
        new Gdk.Rectangle(295, 400, 10, 70), // mid rect left
        new Gdk.Rectangle(295, 390, 75, 10), new Gdk.Rectangle(430, 390, 75, 10), // mid rect top
        new Gdk.Rectangle(495, 400, 10, 70), // mid rect top

        // central lower side-rects
        new Gdk.Rectangle(225, 460, 20, 90),
        new Gdk.Rectangle(555, 460, 20, 90),

        // -|-
        new Gdk.Rectangle(295, 535, 210, 15),
        new Gdk.Rectangle(390, 550, 20, 65),

        // lower horizontal rects
        new Gdk.Rectangle(225, 600, 120, 15),
        new Gdk.Rectangle(455, 600, 120, 15),

        // -|-
        new Gdk.Rectangle(295, 655, 210, 15),
        new Gdk.Rectangle(390, 670, 20, 65),

        // upper left rects
        new Gdk.Rectangle(55, 185, 125, 30),
        new Gdk.Rectangle(55, 260, 125, 15),

        // upper right rects
        new Gdk.Rectangle(620, 185, 125, 30),
        new Gdk.Rectangle(620, 260, 125, 15),

        //upper rect left
        new Gdk.Rectangle(225, 185, 120, 30),

        // upper rect right
        new Gdk.Rectangle(455, 185, 120, 30),

        // upper wall protusion
        new Gdk.Rectangle(390, 140, 20, 75),

        // |-
        new Gdk.Rectangle(225, 260, 20, 150),
        new Gdk.Rectangle(245, 320, 100, 20),

        // -|-
        new Gdk.Rectangle(290, 260, 220, 15),
        new Gdk.Rectangle(390, 275, 20, 65),

        // -|
        new Gdk.Rectangle(555, 260, 20, 150),
        new Gdk.Rectangle(455, 320, 100, 20),

        // left L
        new Gdk.Rectangle(55, 600, 125, 15),
        new Gdk.Rectangle(160, 615, 20, 55),

        // Right L
        new Gdk.Rectangle(620, 600, 125, 15),
        new Gdk.Rectangle(620, 615, 20, 55),

        // left wall protusion
        new Gdk.Rectangle(10, 660, 105, 15),

        // Right wall protusion
        new Gdk.Rectangle(686, 660, 105, 15),

        // left _|_
        new Gdk.Rectangle(55, 720, 290, 15),
        new Gdk.Rectangle(230, 655, 20, 65),

        // right _|_
        new Gdk.Rectangle(455, 720, 290, 15),
        new Gdk.Rectangle(550, 655, 20, 65)
    };
    List<(int x, int y)> scoreCircles = new List<(int x, int y)> {
        /* intersection */ (32, 162), (66, 162), (100, 162), (134, 162), (168, 162), /* intersection */ (202, 162), (234, 162), (267, 162), (300, 162), (333, 162), /* intersection */ (366, 162),             /* intersection */ (432, 162), (466, 162), (499, 162), (532, 162), (564, 162), /*intersection */ (596, 162),  (630, 162), (664, 162), (698, 162), (732, 162), /* intersection */ (766, 162),
                                                                                                        (202, 197),                                                                    (366, 197),                                (432, 197),                                                                   (596, 197),
        /* intersection */ (32, 238), (66, 238), (100, 238), (134, 238), (168, 238), /* intersection */ (202, 238), (234, 238), (267, 238), (300, 238), (333, 238), /* intersection */ (366, 238), (399, 238), /* intersection */ (432, 238), (466, 238), (499, 238), (532, 238), (564, 238), /*intersection */ (596, 238),  (630, 238), (664, 238), (698, 238), (732, 238), /* intersection */ (766, 238),
                           (32, 267),                                                                   (202, 267),             (267, 267),                                                                                                                           (532, 267),                               (596, 267),                                                                     (766, 267),
        /* intersection */ (32, 297), (66, 297), (100, 297), (134, 297), (168, 297), /* intersection */ (202, 297),             (267, 297), (300, 297), (333, 297), /* intersection */ (366, 297),             /* intersection */ (432, 297), (466, 297), (499, 297), (532, 297),             /*intersection */ (596, 297),  (630, 297), (664, 297), (698, 297), (732, 297), /* intersection */ (766, 297),
                                                                                                        (202, 324),                                                                                                                                                                                             (596, 324),
                                                                                                        (202, 351),                                                                                                                                                                                             (596, 351),
                                                                                                        (202, 378),                                                                                                                                                                                             (596, 378),
                                                                                                        (202, 405),                                                                                                                                                                                             (596, 405),
                                                                                                        (202, 432),                                                                                                                                                                                             (596, 432),
                                                                                                        (202, 459),                                                                                                                                                                                             (596, 459),
                                                                                                        (202, 486),                                                                                                                                                                                             (596, 486),
                                                                                                        (202, 513),                                                                                                                                                                                             (596, 513),
                                                                                                        (202, 540),                                                                                                                                                                                             (596, 540),                 
        /* intersection */ (32, 575), (66, 575), (100, 575), (134, 575), (168, 575), /* intersection */ (202, 575), (234, 575), (267, 575), (300, 575), (333, 575), /* intersection */ (366, 575),             /* intersection */ (432, 575), (466, 575), (499, 575), (532, 575), (564, 575), /*intersection */ (596, 575),  (630, 575), (664, 575), (698, 575), (732, 575), /* intersection */ (766, 575), 
        /* intersection */ (32, 607),                                                /* intersection */ (202, 607),                                                 /* intersection */ (366, 607),             /* intersection */                                                             /*intersection */ (596, 607),                                                  /* intersection */ (766, 607),   
        /* intersection */            (66, 637), (100, 637), (134, 637),             /* intersection */ (202, 637), (234, 637), (267, 637), (300, 637), (333, 637), /* intersection */ (366, 637),             /* intersection */ (432, 637), (466, 637), (499, 637), (532, 637), (564, 637), /*intersection */ (596, 637),              (664, 637), (698, 637), (732, 637),
                                                             (134, 667),             /* intersection */ (202, 667),             (267, 667),                                                                                                                           (532, 667),             /*intersection */ (596, 667),              (664, 667),               
        /* intersection */ (32, 697), (66, 697), (100, 697), (134, 697), (168, 697), /* intersection */ (202, 697),             (267, 697), (300, 697), (333, 697), /* intersection */ (366, 697),             /* intersection */ (432, 697), (466, 697), (499, 697), (532, 697),             /*intersection */ (596, 697),  (630, 697), (664, 697), (698, 697), (732, 697), /* intersection */ (766, 697),
        /* intersection */ (32, 727),                                                                                                                                                  (366, 727),             /* intersection */ (432, 727),                                                                                                                                                   (766, 727),
        /* intersection */ (32, 757), (66, 757), (100, 757), (134, 757), (168, 757), /* intersection */ (202, 757), (234, 757), (267, 757), (300, 757), (333, 757), /* intersection */ (366, 757), (399, 757), /* intersection */ (432, 757), (466, 757), (499, 757), (532, 757), (564, 757), /*intersection */ (596, 757),  (630, 757), (664, 757), (698, 757), (732, 757), /* intersection */ (766, 757),    
    };

    List<(int x, int y)> devilJuice = new List<(int x, int y)> {
        (33, 200), (767, 200), (32, 638), (769, 638)
    };

    public Area(Sprite pacMan, Sprite blinky, Sprite inky, Sprite clyde, Sprite pinky) {
        this.pacMan = pacMan;
        this.blinky = blinky;
        this.clyde = clyde;
        this.pinky = pinky;
        this.inky = inky;

        pacManRect = new Gdk.Rectangle(pacMan.x, pacMan.y, 37, 37);  // pac-man rect smaller for better manuevrability
        // ghost rect bigger for more realistic collision
        blinkyRect = new Gdk.Rectangle(blinky.x, blinky.y, 40, 40);
        inkyRect = new Gdk.Rectangle(blinky.x, blinky.y, 40, 40); 
        clydeRect = new Gdk.Rectangle(clyde.x, clyde.y, 40, 40);
        pinkyRect = new Gdk.Rectangle(pinky.x, pinky.y, 40, 40);

        lives = 2;
        pacManDirection = 0;
        ghostDirections = new int[4];
        time = 0;
        score = 0;
        gameStarted = false;
        devilJuiceActive = false;
        gameWon = false;
        gameOver = false;
    }

    private void playSound(string filePath) {
        waveOut = new WaveOutEvent();
        audioFileReader = new AudioFileReader(filePath);
        waveOut.Init(audioFileReader);
        waveOut.Play();
    }
    
    // detects collision of any sprite string entered in the paramter using switch
    public bool collision(string sprite) {
        Gdk.Rectangle spriteRect = new Gdk.Rectangle();

        switch(sprite) {
            case "pacMan":
                spriteRect = pacManRect;
                break;
            case "blinky":
                spriteRect = blinkyRect;
                break;
            case "inky":
                spriteRect = inkyRect;
                break;
            case "clyde":
                spriteRect = clydeRect;
                break;
            case "pinky":
                spriteRect = pinkyRect;
                break;
        }

        foreach (var wall in walls) {
            if (spriteRect.IntersectsWith(wall)) {
                return true;
            }
        }
        return false;
    }

    // method for pac-man's eating of the score circles
    public void eating () {
        List<(int x, int y)> eatenCircles = new();  // creates new list to store eaten circles in

        // goes through each circle in the score circles list to detect eaten circles
        foreach (var (x, y) in scoreCircles) {
            Gdk.Rectangle circleRect = new Gdk.Rectangle (x, y, 4, 4);

            // if collision, then circle is eaten, a sound is played, and the score is incremented
            if (pacManRect.IntersectsWith(circleRect)) {
                playSound("sounds/pacManChomp.wav");
                eatenCircles.Add((x, y));
                score += 10;
            }
        }

        // remove eaten circles for the score circles list so that they are not drawn anymore
        foreach (var circle in eatenCircles) {
            scoreCircles.Remove(circle);
        }
    }

    // method for drinking and activating the special devil-juice power-up
    // similar to eating score circles but the drinking time is recorded and
    // the devil juice bool is set to true activating the power
    public void drinkingDevilJuice() {
        List<(int x, int y)> drunkDevilJuice = new();

        foreach (var (x, y) in devilJuice) {
            Gdk.Rectangle devilJuiceRect = new Gdk.Rectangle (x, y, 10, 10);

            if (pacManRect.IntersectsWith(devilJuiceRect)) {
                juiceActiveTime = time;
                playSound("sounds/pacManChomp.wav");
                drunkDevilJuice.Add((x, y));
                devilJuiceActive = true;
            }
        }

        foreach (var juice in drunkDevilJuice) {
            devilJuice.Remove(juice);
        }
    }

    // useful when the devil juice is activated
    public void eatingGhost(Sprite ghost, Gdk.Rectangle ghostRect) {
        if (devilJuiceActive && pacManRect.IntersectsWith(ghostRect)) {
            playSound("sounds/eatGhost.wav");
            score += 100; // score to encourage eating ghosts
            // send ghost to box
            ghost.x = 385;
            ghost.y = 420;
        }
    }

    // methods to get different variable values for the gameplay from the window class
    public void getLives(int lives) {
        this.lives = lives;
    }

    public void getPacManDirection(int direction) {
        pacManDirection = direction;
    }

    public void getDirections(int blinky, int inky, int clyde, int pinky) {
        ghostDirections = [blinky, inky, clyde, pinky];
    }

    // method for the implementation of two dynamic walls in the game
    public void dynamicWall() {
        // wall should be there at the start of the game
        if (wallActiveTime == 0 || wallDisableTime - wallActiveTime > 5000) {
            // adding walls to the walls list
            walls.Add(new Gdk.Rectangle(170, 410, 10, 50));
            walls.Add(new Gdk.Rectangle(620, 410, 10, 50));
        }

        // updating wall active time or disable time
        if (walls.Contains(new Gdk.Rectangle(170, 410, 10, 50))) wallActiveTime = time;
        else wallDisableTime = time;

        // disable wall after a 5 second interval
        if (wallActiveTime - wallDisableTime > 5000) {
            // removing walls from the list so they are no longer drawn
            walls.Remove(new Gdk.Rectangle(170, 410, 10, 50));
            walls.Remove(new Gdk.Rectangle(620, 410, 10, 50));
        }
    }

    // get time form window class
    public void getTime(long time) {
        this.time = time;
    }

    // cause death when pac-man touches any ghost
    // or is touching a wall when phase power is disabled
    // and reset game variables
    public bool death() {
        if (pacManRect.IntersectsWith(blinkyRect) || pacManRect.IntersectsWith(inkyRect) || pacManRect.IntersectsWith(clydeRect)
        || pacManRect.IntersectsWith(pinkyRect) || (time - phase.time > 8000 && phase.powerActive && pacMan.collision)) {
            if (!devilJuiceActive){
                speedUp1.powerActive = false;
                speedUp2.powerActive = false;
                slowDown1.powerActive = false;
                slowDown2.powerActive = false;
                phase.powerActive = false;
                pacMan.speed = 4;
                pacMan.isPhase = false;
                time = 0;
                wallActiveTime = 0;
                wallDisableTime = 0;
                teleporter1.time = 0;
                teleporter2.time = 0;
                return true;
            }
        }
        return false;
    }

    public void isGameStarted(bool gameStarted) {
        this.gameStarted = gameStarted;
    }

    public void isGameOver(bool gameOver) {
        this.gameOver = gameOver;
    }

    protected override bool OnDrawn (Context c) {
        // draw background
        c.SetSourceColor(black);
        c.Rectangle(x: 0, y: 0, width: 800, height: 800);
        c.Fill();

        // draw start background if the game is not started
        if (!gameStarted) {
            c.SetSourceSurface(gameStartImg, 132, 168);
            c.Paint();

            // drawing instruction to start game
            c.SetSourceColor(white);
            c.SelectFontFace("Lucida Console", FontSlant.Normal, FontWeight.Normal);
            c.SetFontSize(30);
            c.MoveTo(200, 600);
            c.ShowText("Press Spacebar to Play");
        }

        // draw gameplay if game is not on start menu or hasn't ended (won or lost)
        if (!gameOver && !gameWon && gameStarted) {

            c.SetSourceColor(blue);

            // iterate through each wall and draw them
            foreach (var wall in walls) {
                c.Rectangle(wall.X, wall.Y, wall.Width, wall.Height);
                c.Stroke();
            }

            // iterate through each circle and draw them
            c.SetSourceColor(beige);
            foreach (var (x,y) in scoreCircles) {
                c.Arc(x, y, 4, 0.0, 2 * Math.PI);
                c.Fill();
            }

            // iterate through each devil juice and draw them
            foreach (var (x, y) in devilJuice) {
                c.Arc(x, y, 12, 0.0, 2 * Math.PI);
                c.Fill();
            }

            // dynaminc wall implementation
            dynamicWall();

            // speedUp implementation
            void speedingUp(powerUp speedUp) {
                // image is null when power up is used and so should not be drawn to avoid errors
                if (speedUp.powerImg != null) {
                    c.SetSourceSurface(speedUp.powerImg, speedUp.x, speedUp.y);
                    c.Paint();
                }
                // run powerActivate method of the powerUp class to detect power activation
                if (speedUp.powerActivate(pacManRect, time)) {
                    pacMan.speed = 7; // increase pac-man's speed
                    speedUp.powerImg?.Dispose(); // dispose image to avoid memory leaks
                    speedUp.powerImg = null; // make it null to fulfill condition of not drawing image
                    playSound("sounds/speedUp.wav"); // play power-up sound
                }
                // reset pac-man's speed after 7 seconds
                if (time - speedUp.time > 7000 && speedUp.powerActive){
                    pacMan.speed = 4;
                    speedUp.powerActive = false;
                }
            }
            // implement for each speedUp power-up object
            speedingUp(speedUp1);
            speedingUp(speedUp2);

            // slowDown implementation
            // similar to speedUp implementation except that we slow down speed instead
            void slowingDown(powerUp slowDown) {
                if (slowDown.powerImg != null) {
                    c.SetSourceSurface(slowDown.powerImg, slowDown.x, slowDown.y);
                    c.Paint();
                }

                if (slowDown.powerActivate(pacManRect, time)) {
                    pacMan.speed = 2;
                    slowDown.powerImg?.Dispose();
                    slowDown.powerImg = null;
                    playSound("sounds/slowDown.wav");
                }

                if (time - slowDown.time > 7000 && slowDown.powerActive) {
                    pacMan.speed = 4;
                    slowDown.powerActive = false; 
                }
            }

            slowingDown(slowDown1);
            slowingDown(slowDown2);

            // phase implementation
            // similar to slowDown and speedUp
            // set pacMan's phase attribute to true instead of lowering or increeasing speed
            if (phase.powerImg != null) {
                c.SetSourceSurface(phase.powerImg, phase.x, phase.y);
                c.Paint();
            }

            if (phase.powerActivate(pacManRect, time)) {
                playSound("sounds/phase.wav");
                pacMan.isPhase = true;
                phase.powerImg?.Dispose();
                phase.powerImg = null;
            }
            
            if (time - phase.time > 8000 && phase.powerActive) {
                phase.powerActive = false;
                pacMan.isPhase = false;
            }

            // teleporter implementation
            void teleporting(powerUp teleporter_1, powerUp teleporter_2) {
                // spawn teleporters after 10 seconds have passed since last activation of either teleporter
                if (time - teleporter_1.time > 10000) {
                    c.SetSourceSurface(teleporter_1.powerImg, teleporter_1.x, teleporter_1.y);
                    c.Paint();
                    c.SetSourceSurface(teleporter_2.powerImg, teleporter_2.x, teleporter_2.y);
                    c.Paint();

                    // if pac-man intersects withone teleporter
                    // change pac-man's position to the other teleporter
                    if (teleporter_1.powerActivate(pacManRect, time)) {

                        pacMan.x = teleporter_2.x;
                        pacMan.y = teleporter_2.y - 5;
                        teleporter_2.time = time; // set activation time
                        playSound("sounds/portal.wav");
                    }
                    if (teleporter_2.powerActivate(pacManRect, time)) {
                        pacMan.x = teleporter_1.x;
                        pacMan.y = teleporter_1.y - 5;
                        teleporter_1.time = time;
                        playSound("sounds/portal.wav");
                    }
                }
            }
            teleporting(teleporter1, teleporter2);

            // devil Juice implementation
            drinkingDevilJuice();
            // if 7 seconds have passed since activation, disable
            if (devilJuiceActive && (time - juiceActiveTime) > 7000) devilJuiceActive = false;
            // eating ghost implementation for each ghost
            eatingGhost(blinky, blinkyRect);
            eatingGhost(inky, inkyRect);
            eatingGhost(clyde, clydeRect);
            eatingGhost(pinky, pinkyRect);

            // return image according to pac-man's current direction
            ImageSurface setPacManImg() {
                switch (pacManDirection) {
                    case 0:
                        return pacManImgLeft;
                    case 1:
                        return pacManImgRight;
                    case 2:
                        return pacManImgUp;
                    case 3:
                        return pacManImgDown;
                }
                return null!;
            }

            // Player
            // paints image and sets rectangel position to sprite position
            c.SetSourceSurface(setPacManImg(), pacMan.x, pacMan.y);
            c.Paint();
            pacManRect.X = pacMan.x;
            pacManRect.Y = pacMan.y;

            // return image according to ghost and ghost direction
            ImageSurface setGhostImg(int ghost) {
                if (devilJuiceActive) return ghostFearImg;

                switch (ghost) {
                    case 0:
                        if (ghostDirections[ghost] == 0) return blinkyLeftImg;
                        if (ghostDirections[ghost] == 1) return blinkyRightImg;
                        if (ghostDirections[ghost] == 2) return blinkyUpImg;
                        if (ghostDirections[ghost] == 3) return blinkyDownImg;
                        break;
                    case 1:
                        if (ghostDirections[ghost] == 0) return inkyLeftImg;
                        if (ghostDirections[ghost] == 1) return inkyRightImg;
                        if (ghostDirections[ghost] == 2) return inkyUpImg;
                        if (ghostDirections[ghost] == 3) return inkyDownImg;
                        break;
                    case 2:
                        if (ghostDirections[ghost] == 0) return clydeLeftImg;
                        if (ghostDirections[ghost] == 1) return clydeRightImg;
                        if (ghostDirections[ghost] == 2) return clydeUpImg;
                        if (ghostDirections[ghost] == 3) return clydeDownImg;
                        break;
                    case 3:
                        if (ghostDirections[ghost] == 0) return pinkyLeftImg;
                        if (ghostDirections[ghost] == 1) return pinkyRightImg;
                        if (ghostDirections[ghost] == 2) return pinkyUpImg;
                        if (ghostDirections[ghost] == 3) return pinkyDownImg;
                        break;
                }
                return null!;
            }

            // painting each ghost image and setting rectagle coordinates to spirte coordinates
            // Blinky
            c.SetSourceSurface(setGhostImg(0), blinky.x, blinky.y);
            c.Paint();
            blinkyRect.X = blinky.x;
            blinkyRect.Y = blinky.y;

            // Inky
            c.SetSourceSurface(setGhostImg(1), inky.x, inky.y);
            c.Paint();
            inkyRect.X = inky.x;
            inkyRect.Y = inky.y;

            // clyde
            c.SetSourceSurface(setGhostImg(2), clyde.x, clyde.y);
            c.Paint();
            clydeRect.X = clyde.x;
            clydeRect.Y = clyde.y;

            // pinky
            c.SetSourceSurface(setGhostImg(3), pinky.x, pinky.y);
            c.Paint();
            pinkyRect.X = pinky.x;
            pinkyRect.Y = pinky.y;
            
            // drawing score at the top of the screen
            c.SetSourceColor(white);
            c.SelectFontFace("Lucida Console", FontSlant.Normal, FontWeight.Normal);
            c.SetFontSize(30);
            c.MoveTo(355, 60);
            c.ShowText("Score");
            c.MoveTo(365, 100);
            c.ShowText($"{score}");

            // drawing countdown for game start or when pac-man dies and spawns again
            c.MoveTo(395, 520);
            // one second intervals
            if (time >= 0 && time < 1000) c.ShowText("3");
            if (time >= 1000 && time < 2000) c.ShowText("2");
            if (time >= 2000 && time < 3000) c.ShowText("1");

            // drawing remaining lives at the top left of screen
            if (lives == 2) { // draw if 2 lives
                c.SetSourceSurface(pacManImgLeft, 40, 50);
                c.Paint();
            }
            if (lives >= 1) { // draw if 1 or two lives
                c.SetSourceSurface(pacManImgLeft, 80, 50);
                c.Paint();
            }
            
        }

        // game end
        // game ends if all scorecircles are eaten
        if (scoreCircles.Count == 0) {
            if (!gameWon) playSound("sounds/gameWon.wav"); // plays sound once before setting gameWon = true
            gameWon = true;
            c.SetSourceSurface(gameWonImg, 0, 0);
            c.Paint();
        }

        if (gameOver) {
            c.SetSourceSurface(gameOverImg, 0, 0);
            c.Paint();
        }

        return true;
    }
}

class MyWindow : Gtk.Window {
    IWavePlayer? waveOut;
    AudioFileReader? audioFileReader;

    // game sprites
    static Sprite pacMan = new Sprite(385, 615);
    static Sprite blinky = new Sprite(385, 350);
    static Sprite inky = new Sprite(385, 420);
    static Sprite clyde = new Sprite(385, 420);
    static Sprite pinky = new Sprite(385, 420);

    // ghost Ai
    ghostAi blinkyAi = new ghostAi(blinky);
    ghostAi inkyAi = new ghostAi(inky);
    ghostAi clydeAi = new ghostAi(clyde);
    ghostAi pinkyAi = new ghostAi(pinky);

    Stopwatch timer = new Stopwatch();
    HashSet<Key> keys = new HashSet<Key>();
    Area gameArea;
    int lives = 2;
    bool gameStarted;
    bool gameOver;

    // method to play sound
    private void playSound(string filePath) {
        waveOut = new WaveOutEvent();
        audioFileReader = new AudioFileReader(filePath);
        waveOut.Init(audioFileReader);
        waveOut.Play();
    }

    // setting game window
    public MyWindow() : base("Pac-Man") {
        Resize(800, 800);
        SetPosition(WindowPosition.Center);
        Add(gameArea = new Area(pacMan, blinky, inky, clyde, pinky));
        gameOver = false;
        Timeout.Add(30, onTimeout);
    }

    bool onTimeout() { // on each tick

        if (gameStarted) {
            timer.Start(); // start timer
            if (timer.ElapsedMilliseconds < 1) { // play sound in the beginning of game start
                playSound("sounds/pacManBeginning.wav");
            }
            gameArea.getTime(timer.ElapsedMilliseconds);  // send recorded time to gameArea
        }

        if (timer.ElapsedMilliseconds > 3000 && !gameOver) {  // wait three seconds after game start to start movement
            // pac-man movement
            pacMan.tick(keys.Contains(Key.Left), keys.Contains(Key.Right), keys.Contains(Key.Up), keys.Contains(Key.Down), gameArea.collision("pacMan"));
            gameArea.eating(); // pac-man eating
            gameArea.getPacManDirection(pacMan.getDirection()); // send pac-man's direction to gameArea

            if (gameArea.death()) {
                // reset positions and timer when pac-man dies
                pacMan.x = 385; pacMan.y = 615;
                blinky.x = 385; blinky.y = 350;
                inky.x = 385; inky.y = 420;
                clyde.x = 385; clyde.y = 420;
                pinky.x = 385; pinky.y = 420;
                timer = new Stopwatch();
                --lives; // -1 life
            }

            // send directions for approapriate image to gameArea
            // also calls on AI to do it's thing
            gameArea.getDirections(
                                  blinkyAi.blinkyMovement(gameArea),
                                  inkyAi.inkyMovement(gameArea, timer.ElapsedMilliseconds), 
                                  clydeAi.clydeMovement(gameArea, timer.ElapsedMilliseconds),
                                  pinkyAi.pinkyMovement(gameArea)
                                  );
        }

        // starts game is spacebar pressed
        if (keys.Contains(Key.space)) {
            gameStarted = true;
            gameArea.isGameStarted(true);
        }

        gameArea.getLives(lives); // send current lives to gameArea

        if (lives < 0) gameOver = true;  // game is lost if lives zero before collecting all circles

        if (gameOver) gameArea.isGameOver(true);  // tell gameArea that game is over if it is

        QueueDraw();  // draw on each tick

        return true;
    }


    // key events, pressed, released
    protected override bool OnKeyPressEvent(EventKey e) {
        keys.Add(e.Key);
        return true;
    }

    protected override bool OnKeyReleaseEvent(EventKey e) {
        keys.Remove(e.Key);
        return true;
    }

    // quit game when 'X' is clicked on window
    protected override bool OnDeleteEvent(Event e) {
        Application.Quit();
        return true;
    }
}

class Hello {
    static void Main() {
        Application.Init();
        MyWindow w = new MyWindow();
        w.ShowAll();
        Application.Run();
    }
}
