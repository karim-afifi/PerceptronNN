using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Excel;

namespace Neural_Project
{
    public partial class Form1 : Form
    {
        //List<Tuple<List<double>, string>> dataSet = new List<Tuple<List<double>, string>>();
        List<Tuple<List<double>, double>> dataSet = new List<Tuple<List<double>, double>>();
        uint no_hidden_nodes = 0;
        uint no_iterations = 0;
        double learning_rate = 0;
        double mse = 0;
        double momentum = 0;
        double error_squared = 0;
        int number_of_xs = 0;
        //layer, to, from
        Dictionary<Tuple<int, int, int>, double> weights = new Dictionary<Tuple<int, int, int>, double>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private double class_to_float(string class_name)
        {
            if(class_name == "Iris-setosa")
            {
                //return (double)1.0;
                return (double)0.33333;
            }
            else if (class_name == "Iris-versicolor")
            {
                //return (double)2.0;
                return (double)0.66666;
            }
            else if (class_name == "Iris-virginica")
            {
                //return (double)3.0;
                return (double)1.00000;
            }
            else
            {
                //return (double)101.0;
                return (double)2.00000;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Excel File (*xlsx*)|*xlsx*";
            file_dialog.FilterIndex = 1;
            file_dialog.Multiselect = false;

            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = file_dialog.FileName;
                string[] words = sFileName.Split('\\');
                textBox4.Text = words[words.Length - 1];

                FileStream fs = File.Open(sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                reader.IsFirstRowAsColumnNames = true;

                bool not_first_row = false;
                bool checked_no_cols = false;

                while (reader.Read())
                {
                    if (not_first_row == true)
                    {
                        if (checked_no_cols == false)
                        {
                            number_of_xs = (reader.FieldCount) - 1;
                            checked_no_cols = true;
                        }

                        List<double> xs = new List<double>();

                        int i;

                        for(i = 0; i < number_of_xs; i++)
                        {
                            xs.Add(reader.GetDouble(i));
                        }

                        double class_value_dble = 0;

                        string class_value = reader.GetString(i);

                        if (number_of_xs == 4) // for the given neural data set and classification
                        {
                            class_value_dble = class_to_float(class_value);

                            if (class_value_dble > 1.1)
                            {
                                MessageBox.Show("Error: class name to value was not found");
                                System.Windows.Forms.Application.Exit();
                            }
                        }
                        else
                        {
                            class_value_dble = double.Parse(class_value); // for any normal regression or a classification problem with numbers
                        }

                        dataSet.Add(Tuple.Create(xs, class_value_dble));
                    }
                    else
                    {
                        not_first_row = true;
                    }
                }

                fs.Close();
            }
        }

        private void predict(List<double> data_entry, ref List<double> net, ref List<double> snet)
        {

            for (int j = 0; j < no_hidden_nodes; j++)
            {
                net.Add(0);
                snet.Add(0);
            }

            for (int j = 0; j < no_hidden_nodes; j++)
            {
                double net_sum = 0;

                for (int k = 0; k < (data_entry.Count() + 1); k++)
                {
                    if (k == 0)
                    {
                        net_sum += 1 * weights[Tuple.Create(0, j, k)];
                    }
                    else
                    {
                        net_sum += data_entry[k - 1] * weights[Tuple.Create(0, j, k)];
                    }
                }

                net[j] = net_sum;
                snet[j] = 1 / (1 + (double)Math.Exp(-net[j]));
            }
        }

        private void learn()
        {

            for (int i = 0; i < no_hidden_nodes; i++)
            {
                for (int j = 0; j < (dataSet[0].Item1.Count() + 1); j++)
                {
                    weights.Add(Tuple.Create(0, i, j), 1);
                }
            }

            for (int i = 0; i < no_hidden_nodes + 1; i++)
            {
                weights.Add(Tuple.Create(1, 0, i), 1);
            }
            //List<double> weights = new List<double>();

            int iteration_count = 0;
            while (iteration_count < no_iterations)
            {
                for (int i = 0; i < dataSet.Count(); i++)
                {

                    List<double> net = new List<double>();
                    List<double> snet = new List<double>();

                    predict(dataSet[i].Item1, ref net, ref snet);

                    double out_net = 0;
                    double out_snet = 0;

                    for (int j = 0; j < no_hidden_nodes + 1; j++)
                    {
                        if (j == 0)
                        {
                            out_net += 1 * weights[Tuple.Create(1, 0, j)];
                        }
                        else
                        {
                            out_net += snet[j - 1] * weights[Tuple.Create(1, 0, j)];
                        }
                    }

                    out_snet = 1 / (1 + (double)Math.Exp(-out_net));

                    error_squared = (dataSet[i].Item2 - out_snet) * (dataSet[i].Item2 - out_snet);
                    //richTextBox1.Text += "|" + error_squared + "|";

                    if (error_squared > mse)
                    {
                        double out_error = out_snet * (1 - out_snet) * (dataSet[i].Item2 - out_snet);
                        List<double> hidden_layer_error = new List<double>();
                        List<double> weight_hidden = new List<double>();

                        for (int j = 0; j < no_hidden_nodes; j++)
                        {
                            hidden_layer_error.Add(snet[j] * (1 - snet[j]) * (out_error * weights[Tuple.Create(1, 0, j)]));
                        }

                        for (int j = 0; j < no_hidden_nodes + 1; j++)
                        {
                            if (j == 0)
                            {
                                weights[Tuple.Create(1, 0, j)] = learning_rate * 1 * out_error + momentum * weights[Tuple.Create(1, 0, j)];
                            }
                            else
                            {
                                weights[Tuple.Create(1, 0, j)] = learning_rate * snet[j - 1] * out_error + momentum * weights[Tuple.Create(1, 0, j)];
                            }
                        }

                        for (int j = 0; j < no_hidden_nodes; j++)
                        {
                            for (int k = 0; k < (dataSet[i].Item1.Count() + 1); k++)
                            {
                                if (k == 0)
                                {
                                    weights[Tuple.Create(0, j, k)] = learning_rate * 1 * hidden_layer_error[j] + momentum * weights[Tuple.Create(0, j, k)];
                                }
                                else
                                {
                                    weights[Tuple.Create(0, j, k)] = learning_rate * dataSet[i].Item1[k - 1] * hidden_layer_error[j] + momentum * weights[Tuple.Create(0, j, k)];
                                }
                            }
                        }
                    }

                    //double net_sum = 0;

                    /*for (int j = 0; j < weights.Count(); j++)
                    {
                        if(x_index < 5)
                        {
                            net_sum += weights[j] * dataSet[i].Item1[x_index];
                        }

                        if((j+1) % no_hidden_nodes == 0)
                        {
                            net_sum = 0;
                            x_index++;
                        }
                    }*/
                }

                iteration_count++;
            }


            //richTextBox1.Text += "Initialization Input= No. of hidden nodes: " + no_nodes_str + ", Learning rate: " + learning_rate_str +
            //        ", MSE: " + mse_str + ", Momentum: " + momentum_str + ", No. of Iterations: " + no_itr_str + "\n";

            richTextBox1.Text += "Final weights 'layer, from, to' :\n";



            foreach (KeyValuePair<Tuple<int, int, int>, double> entry in weights)
            {

                richTextBox1.Text += entry.Key.Item1.ToString() + "," + entry.Key.Item2.ToString() + "," + entry.Key.Item3.ToString() + ": " + entry.Value.ToString() + "\n";
            }

            richTextBox1.Text += "........................................................................................\n";

            textBox5.ReadOnly = false;
            textBox6.ReadOnly = false;

            if (number_of_xs == 4)
            {
                textBox7.ReadOnly = false;
                textBox8.ReadOnly = false;
            }

            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string learning_rate_str = textBox1.Text;
            double learning_rate_tmp = 0;
            string mse_str = textBox2.Text;
            double mse_tmp = 0;
            string no_nodes_str = textBox3.Text;
            int no_nodes_tmp = -9999;
            string momentum_str = textBox9.Text;
            double momentum_tmp = 0;
            string no_itr_str = textBox10.Text;
            int no_itr_tmp = -9999;

            bool input_is_correct = true;
            
            if(learning_rate_str == "" || learning_rate_str == " " || mse_str == "" || mse_str == " " 
                || no_nodes_str == "" || no_nodes_str == " " || momentum_str == "" || momentum_str == " " 
                || no_itr_str == "" || no_itr_str == " " || dataSet.Count == 0)
            {
                input_is_correct = false;
                MessageBox.Show("Error: All initialization inputs must not be blank");
            }
            else if(!int.TryParse(no_nodes_str, out no_nodes_tmp) || !double.TryParse(learning_rate_str, out learning_rate_tmp) || !double.TryParse(mse_str, out mse_tmp)
                || !double.TryParse(momentum_str, out momentum_tmp) || !int.TryParse(no_itr_str, out no_itr_tmp))
            {
                input_is_correct = false;
                MessageBox.Show("Error: Number of nodes & number of iterations must be an int. Learning rate, MSE & Momentum must be a real number");
            }

            if(no_nodes_tmp < 1 || no_itr_tmp < 1)
            {
                input_is_correct = false;
                MessageBox.Show("Error: Number of hidden nodes and number of iterations must be greater than 0");
            }

            if(input_is_correct == true)
            {
                no_hidden_nodes = (uint)no_nodes_tmp;
                learning_rate = learning_rate_tmp;
                mse = mse_tmp;
                momentum = momentum_tmp;
                no_iterations = (uint)no_itr_tmp;
                button1.Enabled = false;
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox9.ReadOnly = true;
                textBox10.ReadOnly = true;
                button2.Enabled = false;

                richTextBox1.Text += "Initialization Input= No. of hidden nodes: " + no_nodes_str + ", Learning rate: " + learning_rate_str +
                    ", MSE: " + mse_str + ", Momentum: " + momentum_str + ", No. of Iterations: " + no_itr_str + "\n";

                richTextBox1.Text += "........................................................................................\n";

                learn();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (number_of_xs == 4)
            {
                bool input_is_correct = true;

                string x1_str = textBox5.Text;
                double x1 = 0;
                string x2_str = textBox6.Text;
                double x2 = 0;
                string x3_str = textBox7.Text;
                double x3 = 0;
                string x4_str = textBox8.Text;
                double x4 = 0;

                if (x1_str == "" || x1_str == " " || x2_str == "" || x2_str == " "
                    || x3_str == "" || x3_str == " " || x4_str == "" || x4_str == " ")
                {
                    input_is_correct = false;
                    MessageBox.Show("Error: All Xs for prediction must not be blank");
                }
                else if (!double.TryParse(x1_str, out x1) || !double.TryParse(x2_str, out x2)
                    || !double.TryParse(x3_str, out x3) || !double.TryParse(x4_str, out x4))
                {
                    input_is_correct = false;
                    MessageBox.Show("Error: All Xs for prediction must be a number");
                }

                if (input_is_correct == true)
                {
                    List<double> predict_xs = new List<double>();
                    predict_xs.Add(x1);
                    predict_xs.Add(x2);
                    predict_xs.Add(x3);
                    predict_xs.Add(x4);

                    List<double> net = new List<double>();
                    List<double> snet = new List<double>();

                    predict(predict_xs, ref net, ref snet);

                    double out_net = 0;
                    double out_snet = 0;

                    for (int j = 0; j < no_hidden_nodes + 1; j++)
                    {
                        if (j == 0)
                        {
                            out_net += 1 * weights[Tuple.Create(1, 0, j)];
                        }
                        else
                        {
                            out_net += snet[j - 1] * weights[Tuple.Create(1, 0, j)];
                        }
                    }

                    out_snet = 1 / (1 + (double)Math.Exp(-out_net));
                    //MessageBox.Show(out_snet.ToString());

                    richTextBox1.Text += x1_str + ", " + x2_str + ", " + x3_str + ", " + x4_str + ": " + out_snet.ToString() + ", rounded: " + Math.Round(out_snet).ToString() + "\n";

                    richTextBox1.Text += "........................................................................................\n";

                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    textBox8.Text = "";
                }
            }
            else if (number_of_xs == 2)
            {
                bool input_is_correct = true;

                string x1_str = textBox5.Text;
                double x1 = 0;
                string x2_str = textBox6.Text;
                double x2 = 0;

                if (x1_str == "" || x1_str == " " || x2_str == "" || x2_str == " ")
                {
                    input_is_correct = false;
                    MessageBox.Show("Error: All Xs for prediction must not be blank");
                }
                else if (!double.TryParse(x1_str, out x1) || !double.TryParse(x2_str, out x2))
                {
                    input_is_correct = false;
                    MessageBox.Show("Error: All Xs for prediction must be a number");
                }

                if (input_is_correct == true)
                {
                    List<double> predict_xs = new List<double>();
                    predict_xs.Add(x1);
                    predict_xs.Add(x2);

                    List<double> net = new List<double>();
                    List<double> snet = new List<double>();

                    predict(predict_xs, ref net, ref snet);

                    double out_net = 0;
                    double out_snet = 0;

                    for (int j = 0; j < no_hidden_nodes + 1; j++)
                    {
                        if (j == 0)
                        {
                            out_net += 1 * weights[Tuple.Create(1, 0, j)];
                        }
                        else
                        {
                            out_net += snet[j - 1] * weights[Tuple.Create(1, 0, j)];
                        }
                    }

                    out_snet = 1 / (1 + (double)Math.Exp(-out_net));
                    //MessageBox.Show(out_snet.ToString());

                    richTextBox1.Text += x1_str + ", " + x2_str + ": " + out_snet.ToString() + ", rounded: " + Math.Round(out_snet).ToString() + "\n";

                    richTextBox1.Text += "........................................................................................\n";

                    textBox5.Text = "";
                    textBox6.Text = "";
                    textBox7.Text = "";
                    textBox8.Text = "";
                }
            }
        }
    }
}
