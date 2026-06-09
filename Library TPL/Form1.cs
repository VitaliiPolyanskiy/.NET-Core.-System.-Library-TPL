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
                MessageBox.Show("Файл із масивом чисел успішно створено!");
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
            // Task - асинхронна операція - елементарна одиниця виконання
            Task tsk1 = new Task(Task1);
            Task tsk2 = new Task(Task2, "../../array2.dat");
            try
            {
                // Start запускає Task
                tsk1.Start();
                tsk2.Start();
                // Wait очікує завершення виконання об'єкта Task
                tsk1.Wait();
                tsk2.Wait();
                MessageBox.Show("Обидві задачі виконано!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Dispose - звільнення ресурсів, що використовуються задачами
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
                MessageBox.Show("Максимальний елемент масиву чисел " + ar[max].ToString() + " має індекс " +
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
                MessageBox.Show("Ідентифікатор задачі: " + Task.CurrentId);
                MessageBox.Show("Ідентифікатор попередньої задачі: " + t.Id);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Task tsk = new Task(Task2, "../../array3.dat"); // створимо першу задачу
            Task taskCont = tsk.ContinueWith(ContTask); // створимо продовження задачі
            try
            {
                tsk.Start(); // запустимо першу задачу на виконання
                taskCont.Wait(); // очікуємо закінчення виконання другої задачі
                MessageBox.Show("Обидві задачі виконано!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Dispose - звільнення ресурсів, що використовуються задачами
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
                // Застосуємо TaskFactory для запуску задачі
                tsk1 = Task.Factory.StartNew(Task1);

                // TaskFactory надає методи, що спрощують створення та керування задачами
                tsk2 = Task.Factory.StartNew(() =>
                {
                    GeneratorOfNumbers("../../array2.dat");
                });
                tsk3 = Task.Factory.StartNew(() =>
                {
                    Task.WaitAll(tsk1, tsk2); // очікуємо завершення задач
                    // Dispose - звільнення ресурсів, що використовуються задачами
                    tsk1.Dispose();
                    tsk2.Dispose();
                });

                MessageBox.Show("Обидві задачі виконано!");
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
                    // Кінець файлу досягнуто
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
            MessageBox.Show("Визначимо середнє арифметичне елементів масиву ...");
            try
            {
                // запустимо на виконання задачу, що повертає значення типу double
                Task<double> tsk = Task<double>.Factory.StartNew(Task3, "../../array3.dat");
                tsk.Wait();
                MessageBox.Show("Середнє арифметичне елементів масиву: " + tsk.Result.ToString());
                tsk.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        void MyTask(Object ct)
        {
            // Структура CancellationToken поширює повідомлення про те, що операції слід скасувати.
            CancellationToken cancelTok = (CancellationToken)ct;

            // завершимо задачу, якщо вона була скасована ще до запуску
            cancelTok.ThrowIfCancellationRequested();
            // ThrowIfCancellationRequested створює виняток OperationCanceledException, 
            // якщо для цієї ознаки є запит на скасування.

            FileStream file = null;
            BinaryWriter writer = null;
            try
            {
                file = new FileStream("../../array.dat", FileMode.Create, FileAccess.Write);
                writer = new BinaryWriter(file);
                for (int i = 0; i < 100000000; i++)
                {
                    // IsCancellationRequested отримує значення, що вказує,
                    // чи є для цього об'єкта CancellationTokenSource запит на скасування.
                    if (cancelTok.IsCancellationRequested)
                    {
                        MessageBox.Show("Отримано запит на скасування задачі!");
                        // викидаємо виняток, якщо встановлено ознаку скасування задачі
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
            MessageBox.Show("Генеруються дані ...");
            // створимо об'єкт джерела ознак скасування      
            CancellationTokenSource cancelTokSrc = new CancellationTokenSource();

            // отримаємо ознаку скасування з джерела і передамо її задачі та делегату
            Task tsk = Task.Factory.StartNew(MyTask,
                cancelTokSrc.Token, /* Ознака скасування CancellationToken, що передається в задачу */
                cancelTokSrc.Token /* Ознака скасування CancellationToken, яка буде призначена новій задачі Task */ );
            Thread.Sleep(1000);
            try
            {
                // після 3-секундної затримки скасуємо задачу, використовуючи ознаку скасування
                cancelTokSrc.Cancel();
                // очікуємо завершення задачі
                tsk.Wait();
            }
            catch (AggregateException)
            {
                if (tsk.IsCanceled) // перевіримо факт скасування задачі
                    MessageBox.Show("Задача скасована!");
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
                // Клас Parallel також є частиною TPL і призначений для спрощення паралельного виконання коду.
                // Одним із методів, що дозволяють паралельне виконання задач, є метод Invoke.
                // За наявності кількох ядер на цільовій машині ці методи будуть виконуватися паралельно на різних ядрах.
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
            string str = string.Format("Факторіал числа {0} дорівнює {1}", x, result);
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
            // Stopwatch надає набір методів та засобів,
            // які можна використовувати для точного вимірювання витраченого часу.
            Stopwatch sw = new Stopwatch();
            data = new int[100000000];
            sw.Start();

            // Parallel.For виконує цикл for, забезпечуючи можливість паралельного виконання ітерацій,
            // а також контролю стану циклу та керування цим станом.
            Parallel.For(0, data.Length, (i) => data[i] = i);
            sw.Stop();
            MessageBox.Show("Паралельно виконуваний цикл ініціалізації: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            for (int i = 0; i < data.Length; i++) data[i] = i;
            sw.Stop();
            MessageBox.Show("Послідовно виконуваний цикл ініціалізації: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            Parallel.For(0, data.Length, MyTransform);
            sw.Stop();
            MessageBox.Show("Паралельно виконуваний цикл перетворення: " +
                            sw.Elapsed.TotalSeconds + " секунд");
            sw.Reset();
            sw.Start();
            for (int i = 0; i < data.Length; i++) MyTransform(i);
            sw.Stop();
            MessageBox.Show("Послідовно виконуваний цикл перетворення: " +
                          sw.Elapsed.TotalSeconds + " секунд");
        }

        public class SharedState
        {
            public int State { get; set; }
        }
        SharedState sharedState = null;
        void MyTransform2(int i,
            ParallelLoopState pls /* Дозволяє ітераціям циклів Parallel взаємодіяти з іншими ітераціями. */)
        {
            lock (sharedState)
            {
                ++sharedState.State;
            }
            if (data[i] < 0)
                pls.Break();
            // Повідомляє, що цикл Parallel повинен припинити виконання у перший зручний для системи момент в ітераціях після поточної.

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
            // Структура ParallelLoopResult надає стан виконання циклу Parallel.
            ParallelLoopResult loopResult =
                    Parallel.For(0, data.Length, MyTransform2);

            // IsCompleted отримує значення, що вказує, чи дійшов цикл до завершення,
            // тобто всі ітерації циклу виконані і він не отримав запиту на передчасне переривання роботи.
            if (!loopResult.IsCompleted)
                MessageBox.Show("Цикл завершився передчасно на кроці " +
                                loopResult.LowestBreakIteration);
            MessageBox.Show("Кількість ітерацій " + sharedState.State);

        }

        private void button10_Click(object sender, EventArgs e)
        {
            ParallelLoopResult result = Parallel.ForEach<int>(new List<int>() { 1, 2, 3, 4, 5, 6 },
                Factorial);
        }

    }
}