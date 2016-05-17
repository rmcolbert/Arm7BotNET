using System;
using Arm7BotNET;

namespace Arm7Bot_IK_simple_Xsteps
{
    class IK_simple_Xsteps
    {
        private static Arm7Bot arm;

        static void Main(string[] args)
        {
            // The Arm7Bot constructor with no parameters checks all COM ports for a 7Bot controller.
            Console.WriteLine("Detecting 7Bot");
            arm = new Arm7Bot();

            // The Arm7Bot constrcutor accepts up to two possible parameters, SerialPort name and reboot delay.
            // arm = new Arm7Bot("COM5");
            // arm = new Arm7Bot("COM5", 3000);

            // If we didn't detect a 7Bot, exit.
            if (!arm.deviceFound) return;

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

            arm.setServoAngles(Arm7Bot.INITIAL_POSE);
            while (!arm.isAllConverged) arm.Wait(100);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            arm.Close();
        }
    }
}
