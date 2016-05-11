using System;
using Arm7BotNET;

namespace Arm7Bot_IK_simple_XYcircle
{
    class IK_simple_XYcircle
    {
        private static Arm7Bot arm;

        private static double Radians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Available ports: " + String.Join(", ", Arm7Bot.list()));
            if (Arm7Bot.list().Length == 0)
            {
                Console.WriteLine("No ports available.  Please connect 7Bot.");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Connecting to 7Bot on " + Arm7Bot.list()[0]);
            arm = new Arm7Bot(Arm7Bot.list()[0]);

            Console.WriteLine("Setting 7Bot motors to SERVO mode");
            arm.setForceStatus((int)SERVO_MODE.NORMAL);

            int Xtgt = 0, Ytgt = 160, Ztgt = 80;
            float r = 0F;

            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            int[] speeds_1 = { 50, 50, 50, 50, 50, 50, 50 };
            arm.setSpeed(fluentEnabled, speeds_1); // set speed

            int loop = 0;

            while (loop < 10)
            {
                r += 1.0F; if (r > 720F) { r = 0F; arm.Wait(10000); loop++; }// 2 circles and 10s pause
                float rad = (float)Radians(r + 180);
                Xtgt = (int)(0 + Math.Sin(rad) * 60);
                Ytgt = (int)(240 + Math.Cos(rad) * 60);

                PVector j6 = new PVector(Xtgt, Ytgt, Ztgt);
                PVector vec56 = new PVector(0, 0, -1);
                PVector vec67 = new PVector(1, 0, 0);
                float theta6 = 55; // Pump Off

                arm.setIK(j6, vec56, vec67, theta6);
                Console.WriteLine(r + " " + Xtgt + " " + Ytgt);
               // while (!arm.isAllConverged) { arm.Wait(5); }  // wait motion converge
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            arm.Close();
        }
    }
}
