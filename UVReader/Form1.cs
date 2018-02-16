using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        List<int[]> counts = new List<int[]>(8);
        ArrayList s = new ArrayList();
        String line;
        int num;
        int position = 0;
        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < m_PortNames.Length; i++)
            {
                comboBox1.Items.Add(m_PortNames[i]);
            }
            enabled = new CheckBox[] { Prob1, Prob2, Prob3, Prob4, Prob5, Prob6, Prob7, Prob8 };
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
            Thread sendCmd = new Thread(sendCommand);
            sendCmd.Start();
        }

        private void sendCommand()
        {
            foreach (Tube t in tubes)
            {
                int[] count = new int[10];
                if (t.Enable)
                {

                    writeToReader("step " + t.Step);
                    if (t.Vibro > 0)
                    {
                        writeToReader("vibro " + t.Vibro);
                    }
                    Thread.Sleep(t.PauseLed * 1000);
                    writeToReader("gain " + t.Gain);
                    for (int i = 0; i < t.Num; i++)
                    {
                        writeToReader("ledON");
                        writeToReader("read");
                        count[i] = num;
                        Thread.Sleep(t.Pause * 1000);
                    }
                    writeToReader("ledOFF");
                    s.Add(count);
                }
            }
            Thread.Sleep(1);
            this.Invoke((MethodInvoker)delegate {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionColor = Color.White;
                richTextBox1.SelectedText = "Success" + Environment.NewLine;
                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont.FontFamily, richTextBox1.SelectionFont.Size, FontStyle.Bold);
                drawgraphs(0);
            });
        }
        private void createList()
        {
            tubes.Clear();
            s.Clear();
            for (int i = 0; i < 8; i++)
            {
                tubes.Add(new Tube()
                {
                    Enable = enabled[i].Checked,
                    Step = Convert.ToInt32(step[i].Value),
                    Vibro = Convert.ToInt32(vibro[i].Value),
                    PauseLed = Convert.ToInt32(pauseLed[i].Value),
                    Num = Convert.ToInt32(number[i].Value),
                    Gain = Convert.ToInt32(gain[i].Value),
                    Pause = Convert.ToInt32(pause[i].Value),
                    LedOff = ledOff[i].Checked
                });
            }
        }

        private void writeToReader(string command)
        {
            resultOk = false;
            serialPort.Write(command);
            this.Invoke((MethodInvoker)delegate
            {
                //richTextBox1.Text += command + " ";
                richTextBox1.AppendText(command + " ");

            });
            while (!resultOk)
            {
                Thread.Sleep(100);
            }
        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            try
            {
                serialPort.PortName = comboBox1.SelectedItem.ToString();
                serialPort.BaudRate = 9600;
                serialPort.DtrEnable = true;
                serialPort.Open();
                serialPort.DataReceived += serialPort_DataReceived;
                com_port.Text = "Подключенно к COM порту";
                com_port.ForeColor = Color.Green;
                serialPort.Write("help");
            }
            catch (Exception)
            {
                com_port.Text = "Не удалось подключиться к порту";
                com_port.ForeColor = Color.Red;
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+\r");
            line = serialPort.ReadLine();
            MatchCollection c = regex.Matches(line);
            if (c.Count != 0)
            {
                num = Convert.ToInt32(line);
            }
            if (line.Contains("OK")) resultOk = true;
            this.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(line);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            });
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Focus();
            //richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // richTextBox1.ScrollToCaret();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder csv = new StringBuilder();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            int i = 1;
            int num = 0;
            csv.AppendLine("Пробирка; Шаг; Вибрация; Пауза(LED); Количество измерений; Коэффициент усиление; Пауза между измерениями; ");
            foreach (Tube t in tubes)
            {
                if (t.Enable)
                {
                    csv.AppendLine(i + ";" + t.Step + ";" + t.Vibro + ";" + t.PauseLed + ";" + t.Num + ";" + t.Gain + ";" + t.Pause);
                    if (t.Num > num)
                    {
                        num = t.Num;
                    }

                }
                i++;
            }
            csv.AppendLine();

            csv.Append("Измерение ;");
            for (int z = 1; z < num + 1; z++)
            {
                csv.Append(z + ";");
            }
            csv.AppendLine();

            for (int j = 0; j < s.Count; j++)
            {
                int[] a = (int[])s[j];
                csv.Append(" ;");
                for (int f = 0; f < num; f++)
                {
                    csv.Append(a[f] + ";");
                }
                csv.AppendLine();
            }
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.AppendAllText(saveFileDialog1.FileName, csv.ToString(), Encoding.UTF8);
                System.Diagnostics.Process.Start(saveFileDialog1.FileName);               
                drawgraphs(0);
            }
        }

        private void drawgraphs(int index)
        {
            tabControl1.SelectedIndex = 1;
            chart1.Series[0].Points.Clear();
            Tube curent_tube = tubes.ElementAt(index);
            int time =0;
            int[] a = (int[])s[index];
        
            chart1.ChartAreas[0].AxisX.Interval = curent_tube.Pause;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != 0)
                {
                    chart1.Series[0].Points.AddXY(time, a[i]);
                    time += curent_tube.Pause;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (position>0)
            {
                position--;
                drawgraphs(position);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (s.Count>position+1)
            {
                position++;
                drawgraphs(position);
            }
        }
    }
    class Tube
    {
        public bool Enable { get; set; }
        public int Step { get; set; }
        public int Vibro { get; set; }
        public int PauseLed { get; set; }
        public int Num { get; set; }
        public int Gain { get; set; }
        public int Pause { get; set; }
        public bool LedOff { get; set; }

    }
}
