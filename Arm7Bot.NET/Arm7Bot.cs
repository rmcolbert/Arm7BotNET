using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace Arm7BotNET
{
    public enum SERVO_MODE
    {
        FORCELESS = 0,
        NORMAL = 1,
        PROTECT = 2
    }

    public enum COMMAND
    {
        SETVER = 0xF1,
        GETVER = 0xF2,
        STOREPOS = 0xF3,
        LOADPOS = 0xF4,

        SETMODE = 0xF5,
        GETMODE = 0xF6,

        SETSPEED = 0xF7,
        GETSPEED = 0xF8,

        GETPOS = 0xF9,
        SETPOS = 0xF9,

        SETIK6 = 0xFA,
        SETIK5 = 0xFB,
        SETIK3 = 0xFC,

        START = 0xFE
    }

    public class Arm7Bot
    {
        public static double Radians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private const int MAX_DATA_BYTES = 60;

        public const int SERVO_NUM = 7;
        private static readonly int[] INITIAL_SPEED = { 15, 20, 20, 30, 30, 30, 30 };
        private static readonly int[] RUNTIME_SPEED = { 80, 100, 100, 200, 200, 200, 200 };

        public static readonly float[] INITIAL_POSE  = { 90, 115, 65, 90, 90, 90, 80 };
        private static readonly int[] maxSpeedInit = { 110, 110, 110, 200, 200, 200, 200 };

        bool[] isFluentInit = { true, true, true, true, true, true, true };

        private bool[] isConverged = new bool[SERVO_NUM];
        private bool[] isFluent = new bool[SERVO_NUM];
        private int[] fluentRange = new int[SERVO_NUM];
        private int[] maxSpeed = new int[SERVO_NUM];

        public float[] posG = new float[SERVO_NUM];
        public float[] posD = new float[SERVO_NUM];
        public int[] force = new int[SERVO_NUM];
        public bool isAllConverged = false;

        private SerialPort _serialPort;
        private const int BAUDRATE = 115200;
        private int delay;

        private int[] storedInputData = new int[MAX_DATA_BYTES];

        private int majorVersion = 0;
        private int minorVersion = 0;
        private Thread readThread = null;
        private object locker = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        /// <param name="autoStart">Determines whether the serial port should be opened automatically.
        ///                     use the Open() method to open the connection manually.</param>
        /// <param name="delay">Time delay that may be required to allow some Arm7Bot models
        ///                     to reboot after opening a serial connection. The delay will only activate
        ///                     when autoStart is true.</param>
        public Arm7Bot(string serialPortName, bool autoStart, int delay)
        {
            _serialPort = new SerialPort(serialPortName, BAUDRATE);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;

            for (int i = 0; i < SERVO_NUM; i++) {
                posG[i] = INITIAL_POSE[i];
                maxSpeed[i] = maxSpeedInit[i];
                isFluent[i] = isFluentInit[i];
                isConverged[i] = false;
            }

            this.delay = delay;

            if (autoStart) this.Open();
        }

        /// <summary>
        /// Creates an instance of the Arm7Bot object, based on a user-specified serial port.
        /// Assumes default values for baud rate (115200) and reboot delay (2 seconds)
        /// and automatically opens the specified serial connection.
        /// </summary>
        /// <param name="serialPortName">String specifying the name of the serial port. eg COM4</param>
        public Arm7Bot(string serialPortName) : this(serialPortName, true, 1000) { }

        /// <summary>
        /// Creates an instance of the Arm7Bot object using default arguments.
        /// Assumes the Arm7Bot is connected as the HIGHEST serial port on the machine,
        /// default baud rate (115200), and a reboot delay (2 seconds).
        /// and automatically opens the specified serial connection.
        /// </summary>
        public Arm7Bot() : this(Arm7Bot.list().ElementAt(list().Length - 1), true, 1000) { }

        /// <summary>
        /// Opens the serial port connection, should it be required. By default the port is
        /// opened when the object is first created.
        /// </summary>
        public void Open()
        {
            _serialPort.Open();
            Wait(delay);

            // Start serial reader thread
            if (readThread == null)
            {
                readThread = new Thread(processInput);
                readThread.Start();
            }

            setForceStatus((int)SERVO_MODE.NORMAL);

            setInitialSpeed();
            setServoAngles(posG);
            Wait(50);
            while (!isAllConverged) { Wait(200); }  // wait motion converge

            setRuntimeSpeed();
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        public void Close()
        {
            if (readThread != null)
            {
                readThread.Join(500);
                readThread = null;
            }

            _serialPort.Close();
        }

        /// <summary>
        /// Lists all available serial ports on current system.
        /// </summary>
        /// <returns>An array of strings containing all available serial ports.</returns>
        public static string[] list()
        {
            return SerialPort.GetPortNames();
        }

        private void setVersion(int majorVersion, int minorVersion)
        {
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        private int available()
        {
            return _serialPort.BytesToRead;
        }

        private static int Constrain(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private static double Constrain(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public void Wait(int value)
        {
            Thread.Sleep(value);
        }

        public void reqStoredPose(int poseNum)
        {
            byte[] message = new byte[3];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)0xF2;
            message[2] = (byte)(poseNum);
            _serialPort.Write(message, 0, message.Length);
        }

        public void storePose(float[] servoAngles)
        {
            // 1- Process Data
            int[] sendData = new int[SERVO_NUM];
            for (int i = 0; i < SERVO_NUM; i++)
            {
                sendData[i] = (int)(servoAngles[i] * 50 / 9);
            }

            // 2- Send Data
            sendMoveCommand((int)COMMAND.STOREPOS, sendData);
        }
        
        // set motor force status: 0-forceless, 1-normal servo, 2-protection
        public void setForceStatus(int status)
        {
            byte[] message = new byte[3];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)COMMAND.SETMODE;
            message[2] = (byte)(status & 0x7F);
            _serialPort.Write(message, 0, message.Length);
        }

        public void reqForceStatus()
        {
            byte[] message = new byte[2];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)COMMAND.GETMODE;
            _serialPort.Write(message, 0, message.Length);
        }

        public void setSpeed(bool[] fluentEnabled, int[] speeds)
        {
            // 1- Process Data
            int[] sendData = new int[SERVO_NUM];
            for (int i = 0; i < SERVO_NUM; i++)
            {
                sendData[i] = Constrain(speeds[i], 0, 250) / 10;
                if (fluentEnabled[i]) sendData[i] += 64;
            }

            // 2- Send Data
            byte[] message = new byte[2 + SERVO_NUM];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)COMMAND.SETSPEED;

            for (int i = 0; i < SERVO_NUM; i++)
            {
                message[i + 2] = (byte)(sendData[i] & 0x7F);
            }
            _serialPort.Write(message, 0, message.Length);
        }

        public void reqSpeed()
        {
            byte[] message = new byte[2];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)COMMAND.GETSPEED;
            _serialPort.Write(message, 0, message.Length);
        }

        public void setInitialSpeed()
        {
            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            setSpeed(fluentEnabled, INITIAL_SPEED); // set speed
        }

        public void setRuntimeSpeed()
        {
            Boolean[] fluentEnabled = { true, true, true, true, true, true, true };
            setSpeed(fluentEnabled, RUNTIME_SPEED); // set speed
        }

        private void sendMoveCommand(int Command, int[] sendData)
        {
            // 2- Send Data
            byte[] message = new byte[2 + (2 * sendData.Length)];
            message[0] = (byte)COMMAND.START;
            message[1] = (byte)(Command);

            for (int i = 0; i < sendData.Length; i++)
            {
                message[2 + (2 * i)] = (byte)((sendData[i] / 128) & 0x7F);
                message[3 + (2 * i)] = (byte)(sendData[i] & 0x7F);
            }
            _serialPort.Write(message, 0, message.Length);
            this.isAllConverged = false;
            this.Wait(33); // Wait 33ms to ensure 1 position update has been sent

            // 3 - Wait for arm to complete move
            //while (! this.isAllConverged) { this.Wait(100); }  // wait motion converge
        }

        // set Servo angles
        public void setServoAngles(float[] servoAngles)
        {
            isAllConverged = false;

            // 1- Process Data
            int[] sendData = new int[SERVO_NUM];
            for (int i = 0; i < SERVO_NUM; i++)
            {
                sendData[i] = (int)(servoAngles[i] * 50 / 9);
            }

            // 2- Send Data
            sendMoveCommand((int)COMMAND.SETPOS, sendData);
        }

        // IK6(6 angles)
        // j6:mm(-500~500), vec:(-1.0~1.0)--->(-500~500), theta:Degrees
        public void setIK(PVector j6, PVector vec56, PVector vec67, float theta6)
        {
            isAllConverged = false;
            // 1- Process Data
            PVector j6_c = new PVector(Constrain(j6.x, -500, 500), Constrain(j6.y, -500, 500), Constrain(j6.z, -500, 500));
            PVector vec56_c = vec56;
            vec56_c.normalize();
            vec56_c.mult(500);
            PVector vec67_c = vec67;
            vec67_c.normalize();
            vec67_c.mult(500);
            //
            int[] sendData = new int[10];
            sendData[0] = (int)Math.Abs(j6_c.x);
            if (j6_c.x < 0) sendData[0] += 1024;
            sendData[1] = (int)Math.Abs(j6_c.y);
            if (j6_c.y < 0) sendData[1] += 1024;
            sendData[2] = (int)Math.Abs(j6_c.z);
            if (j6_c.z < 0) sendData[2] += 1024;
            //
            sendData[3] = (int)Math.Abs(vec56_c.x);
            if (vec56_c.x < 0) sendData[3] += 1024;
            sendData[4] = (int)Math.Abs(vec56_c.y);
            if (vec56_c.y < 0) sendData[4] += 1024;
            sendData[5] = (int)Math.Abs(vec56_c.z);
            if (vec56_c.z < 0) sendData[5] += 1024;
            //
            sendData[6] = (int)Math.Abs(vec67_c.x);
            if (vec67_c.x < 0) sendData[6] += 1024;
            sendData[7] = (int)Math.Abs(vec67_c.y);
            if (vec67_c.y < 0) sendData[7] += 1024;
            sendData[8] = (int)Math.Abs(vec67_c.z);
            if (vec67_c.z < 0) sendData[8] += 1024;
            //
            sendData[9] = (int)(theta6 * 50 / 9);

            // 2- Send Data
            sendMoveCommand((int)COMMAND.SETIK6, sendData);
        }

        // IK5(5 angles)
        // j6:mm(-500~500), vec:(-1.0~1.0)--->(-500~500),  theta:Degrees
        public void setIK(PVector j6, PVector vec56, float theta5, float theta6)
        {
            isAllConverged = false;
            // 1- Process Data
            PVector j6_c = new PVector(Constrain(j6.x, -500, 500), Constrain(j6.y, -500, 500), Constrain(j6.z, -500, 500));
            PVector vec56_c = vec56;
            vec56_c.normalize();
            vec56_c.mult(500);
            //
            int[] sendData = new int[8];
            sendData[0] = (int)Math.Abs(j6_c.x);
            if (j6_c.x < 0) sendData[0] += 1024;
            sendData[1] = (int)Math.Abs(j6_c.y);
            if (j6_c.y < 0) sendData[1] += 1024;
            sendData[2] = (int)Math.Abs(j6_c.z);
            if (j6_c.z < 0) sendData[2] += 1024;
            //
            sendData[3] = (int)Math.Abs(vec56_c.x);
            if (vec56_c.x < 0) sendData[3] += 1024;
            sendData[4] = (int)Math.Abs(vec56_c.y);
            if (vec56_c.y < 0) sendData[4] += 1024;
            sendData[5] = (int)Math.Abs(vec56_c.z);
            if (vec56_c.z < 0) sendData[5] += 1024;
            //
            sendData[6] = (int)(theta5 * 50 / 9);
            sendData[7] = (int)(theta6 * 50 / 9);

            // 2- Send Data
            sendMoveCommand((int)COMMAND.SETIK5, sendData);
        }

        // IK3(3 angles)
        // j5:mm(-500~500),   theta:Degrees
        public void setIK(PVector j5, float theta3, float theta4, float theta5, float theta6)
        {
            isAllConverged = false;
            // 1- Process Data
            PVector j5_c = new PVector(Constrain(j5.x, -500, 500), Constrain(j5.y, -500, 500), Constrain(j5.z, -500, 500));
            //
            int[] sendData = new int[7];
            sendData[0] = (int)Math.Abs(j5_c.x);
            if (j5_c.x < 0) sendData[0] += 1024;
            sendData[1] = (int)Math.Abs(j5_c.y);
            if (j5_c.y < 0) sendData[1] += 1024;
            sendData[2] = (int)Math.Abs(j5_c.z);
            if (j5_c.z < 0) sendData[2] += 1024;
            //
            sendData[3] = (int)(theta3 * 50 / 9);
            sendData[4] = (int)(theta4 * 50 / 9);
            sendData[5] = (int)(theta5 * 50 / 9);
            sendData[6] = (int)(theta6 * 50 / 9);

            // 2- Send Data
            sendMoveCommand((int)COMMAND.SETIK3, sendData);
        }









        bool beginFlag = false;
        int instruction = 0;
        int cnt = 0;
        public void processInput()
        {
            while (_serialPort.IsOpen)
            {
                if (_serialPort.BytesToRead > 0)
                {
                    lock (this)
                    {
                        int inputData = _serialPort.ReadByte();

                        if (!beginFlag) beginFlag = (inputData == (int)COMMAND.START) ? true : false;
                        else
                        {
                            if (instruction == 0)
                            {
                                instruction = inputData - 240;
                            }
                            else
                            {
                                switch (instruction)
                                {
                                    case 2:
                                        storedInputData[cnt++] = inputData;
                                        if (this.cnt >= 2)
                                        {
                                            this.beginFlag = false;
                                            this.instruction = 0;
                                            this.cnt = 0;
                                            Console.WriteLine("ID:" + storedInputData[0] + "  Data:" + storedInputData[1] );
                                        }
                                        break;

                                    case 4:
                                        storedInputData[cnt++] = inputData;
                                        if (this.cnt >= 2)
                                        {
                                            this.beginFlag = false;
                                            this.instruction = 0;
                                            this.cnt = 0;
                                            Console.WriteLine("ID:" + storedInputData[0] + "  Data:" + storedInputData[1]);
                                        }
                                        break;

                                    case 6:
                                        storedInputData[cnt++] = inputData;
                                        if (this.cnt >= 1)
                                        {
                                            this.beginFlag = false;
                                            this.instruction = 0;
                                            this.cnt = 0;
                                            Console.WriteLine("Force Status: " + (SERVO_MODE)storedInputData[0]);
                                        }
                                        break;

                                    case 8: // Servo Speed & isFluent
                                        storedInputData[cnt++] = inputData;
                                        if (cnt >= SERVO_NUM)
                                        {
                                            beginFlag = false;
                                            instruction = 0;
                                            cnt = 0;
                                            for (int i = 0; i < SERVO_NUM; i++)
                                            {
                                                if (storedInputData[i] >= 64)
                                                {
                                                    isFluent[i] = true;
                                                    storedInputData[i] -= 64;
                                                }
                                                else isFluent[i] = false;
                                                maxSpeed[i] = storedInputData[i] * 10;

                                                Console.WriteLine("Server {0}: Fluent={1}, Speed={2}", i, isFluent[i], maxSpeed[i]);
                                            }
                                        }
                                        break;


                                    case 9:
                                        storedInputData[cnt++] = inputData;
                                        if (cnt >= (SERVO_NUM * 2) + 1)
                                        {
                                            beginFlag = false;
                                            instruction = 0;
                                            cnt = 0;
                                            for (int i = 0; i < SERVO_NUM; i++)
                                            {
                                                int posCode = storedInputData[i * 2] * 128 + storedInputData[i * 2 + 1];
                                                force[i] = posCode % 16384 / 1024;

                                                if (posCode / 16384 > 0) force[i] = -force[i];

                                                posD[i] = (posCode % 1024) * 9 / 50; // convert 0~1000 code to 0~180 degree(accuracy 0.18 degree)
                                            }
                                        }
                                        // Set isAllConverged = false until all servo data is received
                                        isAllConverged = (storedInputData[(SERVO_NUM - 1) * 2 + 2] == 1) ? true : false;
                                        break;

                                    case 10:
                                    case 11:
                                    case 12:
                                        beginFlag = false;
                                        instruction = 0;
                                        cnt = 0;
                                        Console.WriteLine("IK out of range");
                                        break;

                                    case 3:
                                    case 5:
                                    case 7:
                                    default:
                                        beginFlag = false;
                                        instruction = 0;
                                        cnt = 0;
                                        storedInputData = new int[60];
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
