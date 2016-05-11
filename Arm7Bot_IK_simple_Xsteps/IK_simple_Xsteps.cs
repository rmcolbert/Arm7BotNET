using System;
using Arm7BotNET;

namespace Arm7Bot_IK_simple_Xsteps
{
    class IK_simple_Xsteps
    {
        private static Arm7Bot arm;

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

            int Xtgt = -200, Ytgt = 200, Ztgt = 60; // target coordinates

            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            int[] speeds_1 = { 50, 50, 50, 50, 50, 50, 50 };
            arm.setSpeed(fluentEnabled, speeds_1); // set speed

            int loop = 0;

            bool forward = true;
            while (loop < 5)
            {
                PVector vec56 = new PVector(0, 0, -1);
                PVector vec67 = new PVector(1, 0, 0);
                float theta6 = 55;

                if (forward)    Xtgt += 10;
                else            Xtgt -= 10;

                PVector j6 = new PVector(Xtgt, Ytgt, Ztgt);
                arm.setIK(j6, vec56, vec67, theta6);
                while (!arm.isAllConverged) { arm.Wait(33); }

                if (Xtgt > 200) forward = false;
                if (Xtgt < -200) { forward = true; loop++; }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            arm.Close();
        }
    }
}
