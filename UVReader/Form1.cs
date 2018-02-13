using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UVReader
{
    public partial class Form1 : Form
    {

        String[] m_PortNames = SerialPort.GetPortNames();
        SerialPort serialPort = new SerialPort();
        List<Tube> tubes = new List<Tube>(8);
        CheckBox[] enabled;
        NumericUpDown[] step;
        NumericUpDown[] vibro;
        NumericUpDown[] pauseLed;
        NumericUpDown[] number;
        NumericUpDown[] gain;
        NumericUpDown[] pause;
        CheckBox[] ledOff;
        Boolean resultOk = false;
        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < m_PortNames.Length; i++)
            {
                comboBox1.Items.Add(m_PortNames[i]);
            }
            enabled = new CheckBox[] {Prob1,Prob2, Prob3, Prob4, Prob5, Prob6, Prob7, Prob8 };
            step = new NumericUpDown[] { step1, step2, step3, step4, step5, step6, step7, step8 };
            vibro = new NumericUpDown[] { vibro1, vibro2, vibro3, vibro4, vibro5, vibro6, vibro7, vibro8 };
            pauseLed = new NumericUpDown[] { pauseLed1, pauseLed2, pauseLed3, pauseLed4, pauseLed5, pauseLed6, pauseLed7, pauseLed8 };
            number = new NumericUpDown[] { number1, number2, number3, number4, number5, number6, number7, number8 };       
            gain = new NumericUpDown[] { gain1, gain2, gain3, gain4, gain5, gain6, gain7, gain8 };
            pause = new NumericUpDown[] { pause1, pause2, pause3, pause4, pause5, pause6, pause7, pause8 };
            ledOff = new CheckBox[] { ledOff1, ledOff2, ledOff3, ledOff4, ledOff5, ledOff6, ledOff7, ledOff8 };
        }

        private void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            createList();

            foreach (Tube t in tubes)
            {
                if (t.Enable)
                {
                    writeToReader("step "+t.Step);  
                }
            }
        }

        private void createList()
        {
            for (int i = 0; i < 8; i++)
            {
                tubes.Add(new Tube()
                {
                    Enable = enabled[i].Checked,
                    Step = Convert.ToInt32(step[i].Value),
                    Vibro = Convert.ToInt32(vibro[i].Value),
                    PauseLed = Convert.ToInt32(pauseLed[i].Value),
                    Num = Convert.ToInt32(number[i].Value),
                    Pause = Convert.ToInt32(pause[i].Value),
                    LedOff = ledOff[i].Checked
                });
            }
        }

        private void writeToReader(string command)
        {
            resultOk = false;
            serialPort.Write(command);
            while (!resultOk)
            {
                Thread.Sleep(100);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                serialPort.PortName = comboBox1.SelectedItem.ToString();
                serialPort.BaudRate = 9600;
                serialPort.DtrEnable = true;
                serialPort.Open();
                serialPort.DataReceived += serialPort_DataReceived;
                com_port.Text = "Подключенно к COM порту";
                com_port.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                com_port.Text = "Не удалось подключиться к порту";
                com_port.ForeColor = Color.Red;
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string line = serialPort.ReadLine();
            if (line.Contains("OK")) resultOk = true;         
            this.Invoke((MethodInvoker)delegate
            {
                richTextBox1.Text += line;
            });
        }
    }
    class Tube
    {
        public bool Enable { get; set; }
        public int Step { get; set; }
        public int Vibro { get; set; }
        public int PauseLed { get; set; }
        public int Num { get; set; }
        public int Pause { get; set; }
        public bool LedOff { get; set; }

    }
}
