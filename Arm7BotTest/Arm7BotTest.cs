using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arm7BotNET;

namespace Arm7BotTest
{
    class Arm7BotTest
    {
        private static Arm7Bot arm;

        static void Main(string[] args)
        {
            Console.WriteLine("Connecting to 7Bot");
            arm = new Arm7Bot();

            ////////////////////////////////////////////////////////////////////////
            // 1- change force status

            //Console.WriteLine("Setting 7Bot motors to FORCELESS mode");
            //arm.setForceStatus(Arm7Bot.FORCELESS);
            //arm.Wait(2000);

            //Console.WriteLine("Setting 7Bot motors to SERVO mode");
            //arm.setForceStatus(Arm7Bot.SERVO);
            //arm.Wait(2000);

            //Console.WriteLine("Setting 7Bot motors to PROTECTION mode");
            //arm.setForceStatus(Arm7Bot.PROTECTION);
            //arm.Wait(2000);

            ////////////////////////////////////////////////////////////////////////
            // 2- speed & pose setting 
            Console.WriteLine("Setting 7Bot motors to SERVO mode");
            arm.setForceStatus((int)SERVO_MODE.NORMAL);

            //Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            //int[] speeds_1 = { 50, 50, 50, 200, 200, 200, 200 };
            //Console.WriteLine("Setting to 7Bot's speed to {0} {1} {2} {3} {4} {5} {6}",
            //    speeds_1[0], speeds_1[1], speeds_1[2], speeds_1[3], speeds_1[4], speeds_1[5], speeds_1[6]);
            //arm.setSpeed(fluentEnabled, speeds_1); // set speed

            //float[] angles_1 = { 45, 115, 65, 90, 90, 90, 80 };
            //Console.WriteLine("Setting to 7Bot's angles to {0} {1} {2} {3} {4} {5} {6}",
            //    angles_1[0], angles_1[1], angles_1[2], angles_1[3], angles_1[4], angles_1[5], angles_1[6]);
            //arm.setServoAngles(angles_1);  // set pose
            //while (!arm.isAllConverged)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            //    arm.Wait(100);
            //}  // wait motion converge

            //float[] angles_2 = { 135, 115, 65, 90, 90, 90, 80 };
            //Console.WriteLine("Setting to 7Bot's angles to {0} {1} {2} {3} {4} {5} {6}",
            //    angles_2[0], angles_2[1], angles_2[2], angles_2[3], angles_2[4], angles_2[5], angles_2[6]);
            //arm.setServoAngles(angles_2);
            //while (!arm.isAllConverged)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            //    arm.Wait(100);
            //}  // wait motion converge

            //int[] speeds_2 = { 100, 100, 100, 200, 200, 200, 200 };
            //Console.WriteLine("Setting to 7Bot's speed to {0} {1} {2} {3} {4} {5} {6}",
            //    speeds_2[0], speeds_2[1], speeds_2[2], speeds_2[3], speeds_2[4], speeds_2[5], speeds_2[6]);
            //arm.setSpeed(fluentEnabled, speeds_2);  // change speed 

            //float[] angles_3 = { 45, 135, 65, 90, 90, 90, 80 };
            //Console.WriteLine("Setting to 7Bot's angles to {0} {1} {2} {3} {4} {5} {6}",
            //    angles_3[0], angles_3[1], angles_3[2], angles_3[3], angles_3[4], angles_3[5], angles_3[6]);
            //arm.setServoAngles(angles_3);
            //while (!arm.isAllConverged)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            //    arm.Wait(100);
            //}  // wait motion converge

            //float[] angles_4 = { 135, 135, 65, 90, 90, 90, 80 };
            //Console.WriteLine("Setting to 7Bot's angles to {0} {1} {2} {3} {4} {5} {6}",
            //    angles_4[0], angles_4[1], angles_4[2], angles_4[3], angles_4[4], angles_4[5], angles_4[6]);
            //arm.setServoAngles(angles_4);
            //while (!arm.isAllConverged)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            //    arm.Wait(100);
            //}  // wait motion converge

            //Console.WriteLine("Setting 7Bot's angles to initial speed & position");
            //arm.setInitialSpeed();
            //arm.setInitialPosition();

            //PVector j6 = new PVector(-100, 250, 50);
            //PVector vec56 = new PVector(0, 0, -1);
            //PVector vec67 = new PVector(1, 0, 0);
            //float theta6 = 10; // pump on

            //Console.WriteLine("Using 7Bot's IK3 - pump on");
            //arm.setIK(j6, vec56, vec67, theta6);
            //while (!arm.isAllConverged) { arm.Wait(200); }  // wait motion converge

            //j6 = new PVector(0, 250, 150);
            //vec56 = new PVector(0, 1, 0);
            //vec67 = new PVector(1, 0, 0);
            //theta6 = 55; // pump off

            //Console.WriteLine("Using 7Bot's IK3 - pump off");
            //arm.setIK(j6, vec56, vec67, theta6);
            //while (!arm.isAllConverged) { arm.Wait(200); }  // wait motion converge

//            IK_simple_XY();
            IK_simple_XYZrandom();

            //Console.WriteLine("Setting 7Bot motors to PROTECTION mode");
            //arm.setForceStatus((int)SERVO_MODE.NORMAL);
            //arm.setInitialPosition();


            //arm.setForceStatus((int)SERVO_MODE.PROTECT);
            //int loop = 0;
            //while (loop < 50)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2] + " loop# " + loop);
            //    arm.Wait(100);
            //    loop++;
            //}

            //// Go back to home position
            //arm.setInitialPosition();
            //while (!arm.isAllConverged)
            //{
            //    Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            //    arm.Wait(100);
            //}  // wait motion converge

            Console.WriteLine("7Bot: x=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
            Console.WriteLine("7Bot: a=" + arm.posD[3] + " b=" + arm.posD[4] + " c=" + arm.posD[5]);

            arm.setServoAngles(Arm7Bot.INITIAL_POSE);
            while (!arm.isAllConverged) arm.Wait(100);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            arm.Close();
        }

        private static double Radians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private static void IK_simple_XY()
        {
            int Xtgt = 0, Ytgt = 160, Ztgt = 80;
            float r = 0;


            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            int[] speeds_1 = { 150, 150, 150, 200, 200, 200, 200 };
            arm.setSpeed(fluentEnabled, speeds_1); // set speed
            int loop = 0;

            while (loop < 2)
            {
                r += (float)1.0;
                if (r > 720) { r = 0; arm.Wait(5000); loop++; } // 2 circles and 10s pause

                float rad = (float)Radians(r + 180);
                Xtgt = (int)(0 + Math.Sin(rad) * 60);
                Ytgt = (int)(240 + Math.Cos(rad) * 60);

                PVector j6 = new PVector(Xtgt, Ytgt, Ztgt);
                PVector vec56 = new PVector(0, 0, -1);
                PVector vec67 = new PVector(0, 0, 0);

                float theta6 = 55; // Pump off
                arm.setIK(j6, vec56, vec67, theta6);
                Console.WriteLine(loop + ": " + r + " " + Xtgt + " " + Ytgt);
               // while (!arm.isAllConverged) { arm.Wait(5); }  // wait motion converge
            }

        }


        private static void IK_simple_XYZrandom()
        {
            int Xtgt = -200, Ytgt = 200, Ztgt = 60; // target coordinates
            Random random = new Random();

            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            int[] speeds_1 = { 50, 50, 50, 50, 50, 50, 50 };
            arm.setSpeed(fluentEnabled, speeds_1); // set speed

            int loop = 0;

            while (loop < 10)
            {
                Xtgt = (int)random.Next(-200, 200);
                Ytgt = (int)random.Next(100, 300);
                Ztgt = (int)random.Next(100, 200);
                PVector j6 = new PVector(Xtgt, Ytgt, Ztgt);
                PVector vec56 = new PVector(0, 0, -1);
                PVector vec67 = new PVector(1, 0, 0);
                float theta6 = 55;
                arm.setIK(j6, vec56, vec67, theta6);
                while (!arm.isAllConverged) {
                    Console.WriteLine("7Bot: Loop #" + loop + "\tx=" + arm.posD[0] + " y=" + arm.posD[1] + " z=" + arm.posD[2]);
                    arm.Wait(100);
                }
                loop++;
            }
        }


    }
}
