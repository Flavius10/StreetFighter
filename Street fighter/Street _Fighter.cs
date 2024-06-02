using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Street_fighter
{
    public partial class Form1 : Form
    {
        Image player;
        Image background;
        Image drum;
        Image fireball;

        int playerX = 0;
        int playerY = 300;

        int drumX = 450;
        int drumY = 335;

        int fireballX;
        int fireballY;

        int drumMoveTime = 0;
        int actionStrength = 0;
        int endFrame = 0;
        int backgroundPosition = 0;
        int totalFrame = 0;
        int bg_number = 0;

        float num;
        bool goLeft, goRight;
        bool directionPressed;
        bool playingAction;
        bool shotFireBall;

        List<string> background_images = new List<string>();


        public Form1()
        {
            InitializeComponent();
            SetUpForm();
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && !directionPressed)
            {
                MovePlayerAnimation("left");
            }
            if (e.KeyCode == Keys.Right && !directionPressed)
            {
                MovePlayerAnimation("right");
            }
        }

        private void KeysIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.X)
            {
                if (bg_number < background_images.Count - 1)
                {
                    bg_number++;
                }
                else
                {
                    bg_number = 0;
                }

                background = Image.FromFile(background_images[bg_number]);
                backgroundPosition = 0;
                drumMoveTime = 0;
                drumX = 300;

            }

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                goLeft = false;
                goRight = false;
                directionPressed = false;
                ResetPlayer();
            }

            if (e.KeyCode == Keys.A && !playingAction && !goLeft && !goRight)
            {
                SetPlayerAction("punch2.gif", 2);
            }

            if (e.KeyCode == Keys.S && !playingAction && !goLeft && !goRight)
            {
                SetPlayerAction("punch1.gif", 5);
            }

            if (e.KeyCode == Keys.D && !playingAction && !goLeft && !goRight && !shotFireBall)
            {
                SetPlayerAction("fireball.gif", 30);
            }
        }

        private void FormPaintEvent(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(background, new Point(backgroundPosition, 0));
            e.Graphics.DrawImage(player, new Point(playerX, playerY));
            e.Graphics.DrawImage(drum, new Point(drumX, drumY));

            if (shotFireBall)
            {
                e.Graphics.DrawImage(fireball, new Point(fireballX, fireballY));
            }
        }

        private void GameTimerEvent(object sender, EventArgs e)
        {
            this.Invalidate();
            ImageAnimator.UpdateFrames();
            MovePlayerBackground();
            CheckPunchHit();

            if (playingAction)
            {
                if (num < totalFrame)
                {
                    num += 0.5f;
                }

                if (num == totalFrame)
                {
                    ResetPlayer();
                }
            }

            if (shotFireBall)
            {
                fireballX += 10;
                CheckFireballHit();
            }

            if (fireballX > this.ClientSize.Width)
            {
                shotFireBall = false;
            }

            if (!shotFireBall && num > endFrame && drumMoveTime == 0 && actionStrength == 30)
            {
                ShotFireBall();
            }

            if(drumMoveTime > 0)
            {
                drumMoveTime--;
                drumX += 10;
                drum = Image.FromFile("hitdrum.png");
            }
            else
            {
                drum = Image.FromFile("drum.png");
                drumMoveTime = 0;
            } 
            
            if(drumX > this.ClientSize.Width)
            {
                drumX = 300;
                drumMoveTime = 0;
            }
        }

        private void SetUpForm()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);

            background_images = Directory.GetFiles("background", "*.jpg").ToList();
            background = Image.FromFile(background_images[bg_number]);
            player = Image.FromFile("standing.gif");
            drum = Image.FromFile("drum.png");
            SetUpAnimation();
        }

        private void SetUpAnimation()
        {
            ImageAnimator.Animate(player, this.OnFrameChangeHandler);
            FrameDimension dimentions = new FrameDimension(player.FrameDimensionsList[0]);
            totalFrame = player.GetFrameCount(dimentions);
            endFrame = totalFrame - 3;
        }

        private void OnFrameChangeHandler(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void MovePlayerBackground()
        {
            if (goLeft)
            {
                if (playerX > 0)
                {
                    playerX -= 6;
                }

                if (backgroundPosition < 0 && playerX < 100)
                {
                    backgroundPosition += 5;
                    drumX += 5;
                }
            }
            if (goRight)
            {
                if (playerX + player.Width < this.ClientSize.Width)
                {
                    playerX += 6;
                }

                if (backgroundPosition + background.Width > this.ClientSize.Width + 5 && playerX > 650)
                {
                    backgroundPosition -= 5;
                    drumX -= 5;
                }
            }
        }

        private void MovePlayerAnimation(string direction)
        {
            if (direction == "left")
            {
                goLeft = true;
                player = Image.FromFile("backwards.gif");
            }

            if (direction == "right")
            {
                goRight = true;
                player = Image.FromFile("forwards.gif");
            }

            directionPressed = true;
            playingAction = false;
            SetUpAnimation();
        }

        private void ResetPlayer()
        {
            player = Image.FromFile("standing.gif");
            SetUpAnimation();
            num = 0;
            playingAction = false;

        }

        private void SetPlayerAction(string animation, int strength)
        {
            player = Image.FromFile(animation);
            actionStrength = strength;
            SetUpAnimation();
            playingAction = true;
        }

        private void ShotFireBall()
        {
            fireball = Image.FromFile("FireBallFinal.gif");
            ImageAnimator.Animate(fireball, this.OnFrameChangeHandler);
            fireballX = playerX + player.Width - 50;
            fireballY = playerY - 33;
            shotFireBall = true;
        }

        private void CheckFireballHit()
        {
            bool colision = DetectColision(fireballX, fireballY, fireball.Width, fireball.Height,
                drumX, drumY, drum.Width, drum.Height);


            if (colision)
            {
                drumMoveTime = actionStrength;
                shotFireBall = false;   
            }
        }

        private void CheckPunchHit()
        {
            bool colision = DetectColision(playerX, playerY, player.Width, player.Height,
                drumX, drumY, drum.Width, drum.Height);

            if (colision && playingAction && num > endFrame)
            {
                drumMoveTime = actionStrength;
            }
        }

        private bool DetectColision(int object1X, int object1Y, int object1Width, int object1Height,
        int object2X, int object2Y, int object2Width, int object2Height)
        {
            if (object1X + object1Width <= object2X || object1X >= object2Width + object2X ||
                object1Y + object1Height <= object2Y || object1Y >= object2Height + object2Y)
                return false;
            return true;
        }
    }
}
