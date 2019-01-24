namespace PiggyRuntime
{
    using System;
    using System.IO;

    public class Redirect : IDisposable
    {
        private StreamWriter output;
        private TextWriter output_old;

        public Redirect(string file_name)
        {
            output_old = Console.Out;
            output = new StreamWriter(new FileStream(file_name, FileMode.Create));
            output.AutoFlush = true;
            Console.SetOut(output);
        }

        public void Dispose()
        {
            Console.SetOut(output_old);
            output.Close();
        }
    }
}

