using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Library_TPL
{
    public partial class Form1 : Form
    {
        Random rnd = new Random();
        public Form1()
        {
            InitializeComponent();
        }
        void GeneratorOfNumbers(string filename)
        {
            try
            {
                FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                BinaryWriter writer = new BinaryWriter(file);
                for (int i = 0; i < 1000000; i++)
                {
                    int n = rnd.Next(-99, 99);
                    writer.Write(n);
                }
                writer.Close();
                file.Close();
                MessageBox.Show("Файл с массивом чисел успешно создан!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw e;
            }
        }

        void Task1()
        {
            GeneratorOfNumbers("../../array1.dat");
        }

        void Task2(object path)
        {
            GeneratorOfNumbers(path as string);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Task - асинхронная операция - элементарная единица исполнения
            Task tsk1 = new Task(Task1);
            Task tsk2 = new Task(Task2, "../../array2.dat");
            try
            {
                // Start запускает Task.
                tsk1.Start();
                tsk2.Start();
                // Wait ожидает завершения выполнения объекта Task.
                tsk1.Wait();
                tsk2.Wait();
                MessageBox.Show("Обе задачи выполнены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Dispose - освобождение ресурсов, используемых задачами
                tsk1.Dispose();
                tsk2.Dispose();
            }

        }

        void MaxOfNumbers(string filename)
        {
            try
            {
                FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file);
                int max = 0;
                int[] ar = new int[file.Length / 4];
                for (int i = 0; i < ar.Length; i++)
                {
                    ar[i] = reader.ReadInt32();
                    if (ar[i] > ar[max])
                        max = i;
                    i++;
                }
                reader.Close();
                file.Close();
                MessageBox.Show("Максимальный элемент массива чисел " + ar[max].ToString() + " имеет индекс " +
                                max.ToString() + "\n");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw e;
            }
        }

        void ContTask(Task t)
        {
            if (t.Exception == null)
            {
                MaxOfNumbers("../../array3.dat");
                MessageBox.Show("Идентификатор задачи: " + Task.CurrentId);
                MessageBox.Show("Идентификатор предыдущей задачи: " + t.Id);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Task tsk = new Task(Task2, "../../array3.dat"); // создадим первую задачу
            Task taskCont = tsk.ContinueWith(ContTask); // создадим продолжение задачи
            try
            {
                tsk.Start(); // запустим первую задачу на выполнение
                taskCont.Wait(); // ожидаем окончания выполнения второй задачи
                MessageBox.Show("Обе задачи выполнены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Dispose - освобождение ресурсов, используемых задачами
                tsk.Dispose();
                taskCont.Dispose();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Task tsk1 = null;
            Task tsk2 = null;
            Task tsk3 = null;
            try
            {
                // Применим TaskFactory для запуска задачи
                tsk1 = Task.Factory.StartNew(Task1);

                // TaskFactory предоставляет методы, упрощающие создание и управление задачами
                tsk2 = Task.Factory.StartNew(() =>
                {
                    GeneratorOfNumbers("../../array2.dat");
                });
                tsk3 = Task.Factory.StartNew(() =>
                {
                    Task.WaitAll(tsk1, tsk2); // ожидаем завершение задач
                    // Dispose - освобождение ресурсов, используемых задачами
                    tsk1.Dispose();
                    tsk2.Dispose();
                });
                
                MessageBox.Show("Обе задачи выполнены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                tsk3.Dispose();
            }
        }

        double Task3(object v)
        {
            string filename = (string)v;
            double sum = 0;
            try
            {
                FileStream file1 = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(file1);
                try
                {
                    while (true)
                    {
                        sum += reader.ReadInt32();
                    }
                }
                catch (EndOfStreamException)
                {
                    //Достигнут конец файла
                }
                sum = sum / (file1.Length / 4);
                reader.Close();
                file1.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw e;
            }
            return sum;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Определим среднее арифметическое элементов массива ...");
            try
            {
                // запустим на выполнение задачу, возвращающую значение типа double
                Task<double> tsk = Task<double>.Factory.StartNew(Task3, "../../array3.dat");
                tsk.Wait();     
                MessageBox.Show("Среднее арифметическое элементов массива: " + tsk.Result.ToString()); 
                tsk.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        void MyTask(Object ct)
        {
            // Структура CancellationToken распространяет уведомление о том, что операции следует отменить.
            CancellationToken cancelTok = (CancellationToken)ct;

            // завершим задачу, если она была отменена ещё до запуска
            cancelTok.ThrowIfCancellationRequested();
            // ThrowIfCancellationRequested создает исключение OperationCanceledException, 
            // если для данного признака есть запрос на отмену.

            FileStream file = null;
            BinaryWriter writer = null;
            try
            {
                file = new FileStream("../../array.dat", FileMode.Create, FileAccess.Write);
                writer = new BinaryWriter(file);
                for (int i = 0; i < 100000000; i++)
                {
                    // IsCancellationRequested получает значение, указывающее, 
                    // есть ли для данного объекта CancellationTokenSource запрос на отмену.
                    if (cancelTok.IsCancellationRequested)
                    {
                        MessageBox.Show("Получен запрос на отмену задачи!");
                        // выбрасываем исключение, если установлен признак отмены задачи
                        cancelTok.ThrowIfCancellationRequested();
                    }
                    int n = rnd.Next(100);
                    writer.Write(n);
                }
            }
            finally
            {
                writer.Close();
                file.Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Генерируются данные ...");
            // создадим объект источника признаков отмены      
            CancellationTokenSource cancelTokSrc = new CancellationTokenSource();

            // получим признак отмены из источника и передадим его задаче и делегату
            Task tsk = Task.Factory.StartNew(MyTask,
                cancelTokSrc.Token, /* Признак отмены CancellationToken, передаваемый в задачу */
                cancelTokSrc.Token /* Признак отмены CancellationToken, который будет назначен новой задаче Task */ );
            Thread.Sleep(1000);
            try
            {
                // после 3-х секундной задержки отменим задачу, используя признак отмены
                cancelTokSrc.Cancel();
                // ожидаем завершения задачи
                tsk.Wait();
            }
            catch (AggregateException)
            {
                if (tsk.IsCanceled) // проверим факт отмены задачи
                    MessageBox.Show("Задача отменена!");
            }
            finally
            {
                tsk.Dispose();
                cancelTokSrc.Dispose();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                // Класс Parallel также является частью TPL и предназначен для упрощения параллельного выполнения кода.
                // Одним из методов, позволяющих параллельное выполнение задач, является метод Invoke.
                // При наличии нескольких ядер на целевой машине данные методы будут выполняться параллельно на различных ядрах.
                Parallel.Invoke(Task1, () =>
                {
                    GeneratorOfNumbers("../../array2.dat");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        static void Factorial(int x)
        {
            int result = 1;

            for (int i = 1; i <= x; i++)
            {
                result *= i;
            }
            string str = string.Format("Факториал числа {0} равен {1}", x, result);
            MessageBox.Show(str);
            Thread.Sleep(3000);
        }
        private void button9_Click(object sender, EventArgs e)
        {
            Parallel.For(1, 7, Factorial);
        }

        int[] data;

        void MyTransform(int i)
        {
            if (data[i] < 1000) data[i] = 0;
            if (data[i] > 1000 & data[i] < 2000) data[i] = 100;
            if (data[i] > 2000 & data[i] < 3000) data[i] = 200;
            if (data[i] > 3000 & data[i] < 4000) data[i] = 300;
            if (data[i] > 4000 & data[i] < 5000) data[i] = 400;
            if (data[i] > 5000 & data[i] < 6000) data[i] = 500;
            if (data[i] > 6000 & data[i] < 7000) data[i] = 600;
            if (data[i] > 7000 & data[i] < 8000) data[i] = 700;
            if (data[i] > 8000 & data[i] < 9000) data[i] = 800;
            if (data[i] > 9000 & data[i] < 10000) data[i] = 900;
            if (data[i] > 10000) data[i] = 1000;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Stopwatch предоставляет набор методов и средств, 
            // которые можно использовать для точного измерения затраченного времени.
            Stopwatch sw = new Stopwatch();
            data = new int[100000000];
            sw.Start();

            // Parallel.For выполняет цикл for, обеспечивая возможность параллельного выполнения итераций, 
            // а также контроля состояния цикла и управления этим состоянием.
            Parallel.For(0, data.Length, (i) => data[i] = i);
            sw.Stop();
            MessageBox.Show("Параллельно выполняемый цикл инициализации: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            for (int i = 0; i < data.Length; i++) data[i] = i;
            sw.Stop();
            MessageBox.Show("Последовательно выполняемый цикл инициализации: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            Parallel.For(0, data.Length, MyTransform);
            sw.Stop();
            MessageBox.Show("Параллельно выполняемый цикл преобразования: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            for (int i = 0; i < data.Length; i++) MyTransform(i);
            sw.Stop();
            MessageBox.Show("Последовательно выполняемый цикл преобразования: " +
                          sw.Elapsed.TotalSeconds + " секунд");
        }

        public class SharedState
        {
            public int State { get; set; }
        }
        SharedState sharedState = null;
        void MyTransform2(int i,
            ParallelLoopState pls /* Позволяет итерациям циклов Parallel взаимодействовать с другими итерациями.*/)
        {
            lock (sharedState)
            {
                ++sharedState.State;
            }
            if (data[i] < 0)
                pls.Break();
            // Сообщает, что цикл Parallel должен прекратить выполнение в первый удобный для системы момент в итерациях после текущей.

            if (data[i] < 1000) data[i] = 0;
            if (data[i] > 1000 & data[i] < 2000) data[i] = 100;
            if (data[i] > 2000 & data[i] < 3000) data[i] = 200;
            if (data[i] > 3000 & data[i] < 4000) data[i] = 300;
            if (data[i] > 4000 & data[i] < 5000) data[i] = 400;
            if (data[i] > 5000 & data[i] < 6000) data[i] = 500;
            if (data[i] > 6000 & data[i] < 7000) data[i] = 600;
            if (data[i] > 7000 & data[i] < 8000) data[i] = 700;
            if (data[i] > 8000 & data[i] < 9000) data[i] = 800;
            if (data[i] > 9000 & data[i] < 10000) data[i] = 900;
            if (data[i] > 10000) data[i] = 1000;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            data = new int[100000000];
            for (int i = 0; i < data.Length; i++)
                data[i] = i;
            data[1000] = -10;
            sharedState = new SharedState();
            sharedState.State = 0;
            // Структура ParallelLoopResult предоставляет состояние выполнения цикла Parallel.
            ParallelLoopResult loopResult =
                        Parallel.For(0, data.Length, MyTransform2);

            // IsCompleted получает значение, указывающее, дошел ли цикл до завершения, 
            // то есть все итерации цикла выполнены и он не получил запроса на преждевременное прерывание работы.
            if (!loopResult.IsCompleted)
                MessageBox.Show("Цикл завершился преждевременно на шаге " +
                                   loopResult.LowestBreakIteration);
            MessageBox.Show("Количество итераций " + sharedState.State);

        }

        private void button10_Click(object sender, EventArgs e)
        {
            ParallelLoopResult result = Parallel.ForEach<int>(new List<int>() { 1, 2, 3, 4, 5, 6 },
                Factorial);
        }

    }
}
